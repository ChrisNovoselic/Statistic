﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using HClassLibrary;
using StatisticCommon;

namespace StatisticTrans
{
    public abstract partial class FormMainTrans : FormMainBaseWithStatusStrip
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const Int32 TIMER_SERVICE_MIN_INTERVAL = 66666;

        protected enum MODE_MASHINE : ushort { INTERACTIVE, TO_DATE, SERVICE, UNKNOWN };
        public enum CONN_SETT_TYPE : short {SOURCE, DEST, COUNT_CONN_SETT_TYPE};
        protected enum INDX_UICONTROLS { SERVER_IP, PORT, NAME_DATABASE, USER_ID, PASS, COUNT_INDX_UICONTROLS };

        protected System.Windows.Forms.Control[,] m_arUIControls;

        protected static FileINI m_sFileINI;

        protected System.Windows.Forms.Timer timerService;

        protected HAdmin[] m_arAdmin;
        protected GroupBox[] m_arGroupBox;

        protected DataGridViewAdmin m_dgwAdminTable;

        protected List<int> m_listTECComponentIndex;

        protected DateTime m_arg_date;
        protected Int32 m_arg_interval;

        protected FormChangeMode.MODE_TECCOMPONENT m_modeTECComponent;

        protected MODE_MASHINE m_modeMashine = MODE_MASHINE.INTERACTIVE;

        protected CheckBox m_checkboxModeMashine;

        //protected HMark m_markQueries;

        protected bool m_bTransAuto {
            get
            {
                //return WindowState == FormWindowState.Minimized ? true : false;
                //return ((WindowState == FormWindowState.Minimized) && (ShowInTaskbar == false) && (notifyIconMain.Visible == true));

                //return !timerMain.Enabled;

                return m_modeMashine == MODE_MASHINE.TO_DATE ? true : false;
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

        //private string GetINIParametersOfID(int param)
        //{
        //    return m_sFileINI.GetValueOfID(param);
        //}

        private string GetINIParametersOfKey(string keyParam)
        {
            return m_sFileINI.GetMainValueOfKey(keyParam);
        }

        public FormMainTrans(int id_app, string[] par, string[] val)
        //public FormMainTrans(int id_app, string []par, string [] val)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //ru-Ru

            InitializeComponent();

            m_report = new HReports();

            //DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);
            Logging.DelegateGetINIParametersOfKEY = new StringDelegateStringFunc(GetINIParametersOfKey);

            m_sFileINI = new FileINI(@"setup.ini", false, par, val);

            string keyPar = string.Empty
                , valDefPar = string.Empty;

            keyPar = @"Main DataSource"; valDefPar = @"671";
            m_sFileINI.AddMainPar(keyPar, valDefPar);
            s_iMainSourceData = Int32.Parse(m_sFileINI.GetMainValueOfKey(keyPar));

            //keyPar = @"Season DateTime"; valDefPar = @"21.10.2014 03:00";
            //m_fileINI.Add(keyPar, valDefPar);
            //keyPar = @"Season Action"; valDefPar = @"-1";
            //m_fileINI.Add(keyPar, valDefPar);

            //Ошибка для отладки
            //System.Threading.Timer tm = null;
            //tm.Dispose ();
            
            //Вариант №1
            keyPar = @"iapp"; valDefPar = id_app.ToString ();
            m_sFileINI.AddMainPar(keyPar, valDefPar);
            ProgramBase.s_iAppID = Int32.Parse(m_sFileINI.GetMainValueOfKey(keyPar));
            //Вариант №2
            //ProgramBase.s_iAppID = id_app;

            ////Если ранее тип логирования не был назанчен...
            //if (Logging.s_mode == Logging.LOG_MODE.UNKNOWN)
            //{
            //    //назначить тип логирования - БД
            //    Logging.s_mode = Logging.LOG_MODE.DB;
            //}
            //else { }

            //if (Logging.s_mode == Logging.LOG_MODE.DB)
            //{
            //    //Инициализация БД-логирования
            //    int err = -1;
            //    StatisticCommon.Logging.ConnSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, idListenerConfigDB, s_iMainSourceData, -1, out err).Rows[0]);
            //}
            //else { }

            m_sFileINI.AddMainPar(@"ОкноНазначение", @"Конвертер (...)");
            m_sFileINI.AddMainPar(@"ID_TECNotUse", string.Empty);

            m_sFileINI.AddMainPar(@"ОпросСохранениеППБР", false.ToString() + @"," + false.ToString());
            m_sFileINI.AddMainPar(@"ОпросСохранениеАдминЗнач", false.ToString() + @"," + false.ToString());

            keyPar = @"Season DateTime";
            m_sFileINI.AddMainPar(keyPar, @"26.10.2014 02:00");
            keyPar = @"Season Action";
            m_sFileINI.AddMainPar(keyPar, @"-1");

            keyPar = @"SetPBRQuery LogPBRNumber";
            m_sFileINI.AddMainPar(keyPar, false.ToString());

            keyPar = @"SetPBRQuery LogQuery";
            m_sFileINI.AddMainPar(keyPar, false.ToString());            

            this.Text =
            this.notifyIconMain.Text = @"Статистика: " + m_sFileINI.GetMainValueOfKey(@"ОкноНазначение");

            m_listID_TECNotUse = new List<int>();
            string[] arStrID_TECNotUse = m_sFileINI.GetMainValueOfKey(@"ID_TECNotUse").Split(',');
            foreach (string str in arStrID_TECNotUse)
            {
                if (str.Equals (string.Empty) == false)
                    m_listID_TECNotUse.Add(Int32.Parse(str));
                else
                    ;
            }

            try
            {
                string strChecked = string.Empty;
                bool bRes = false
                    , bChecked = false;
                strChecked = m_sFileINI.GetMainValueOfKey(@"ОпросСохранениеППБР");
                if (bool.TryParse(strChecked.Split(',')[0], out bChecked) == true)
                    ОпросППБРToolStripMenuItem.Checked = bChecked;
                else
                    ОпросППБРToolStripMenuItem.Checked = false;
                if (bool.TryParse(strChecked.Split(',')[1], out bChecked) == true)
                    СохранППБРToolStripMenuItem.Checked = bChecked;
                else
                    СохранППБРToolStripMenuItem.Checked = false;

                bRes =
                bChecked =
                    false;
                strChecked = m_sFileINI.GetMainValueOfKey(@"ОпросСохранениеАдминЗнач");
                if (bool.TryParse(strChecked.Split(',')[0], out bChecked) == true)
                    ОпросАдминЗначенияToolStripMenuItem.Checked = bChecked;
                else
                    ОпросАдминЗначенияToolStripMenuItem.Checked = false;
                if (bool.TryParse(strChecked.Split(',')[1], out bChecked) == true)
                    СохранАдминЗначенияToolStripMenuItem.Checked = bChecked;
                else
                    СохранАдминЗначенияToolStripMenuItem.Checked = false;
            }
            catch (Exception e)
            {
                throw new Exception(@"FormMainTrans::нет возможности установить перечень опрашиваемых/сохранямых параметров...");
            }

            if ((ОпросАдминЗначенияToolStripMenuItem.Checked == false) &&
                (ОпросППБРToolStripMenuItem.Checked == false))
            {
                throw new Exception(@"FormMainTrans::не определн перечень опрашиваемых/сохранямых параметров...");
            }
            else
                ;

            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateMessage.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescMessage.Size = new System.Drawing.Size(463, 17);

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
                        m_modeMashine = MODE_MASHINE.TO_DATE;

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
                if (m_modeMashine == MODE_MASHINE.TO_DATE)
                    enabledUIControl(false);
                else
                    enabledUIControl(true);
        }

        private void InitializeComponentTrans () {
            int i = -1;

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i])).BeginInit();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                this.groupBoxDest.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]);
            }

            i = (Int16)INDX_UICONTROLS.PORT + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestPort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(11, 57);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestPort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(32, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 31;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Порт";

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // nudnDestPort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 55);
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "nudnDestPort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(69, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 26;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});

            i = (Int16)INDX_UICONTROLS.PASS + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestPass
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 136);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestPass";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(45, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 34;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Пароль";

            i = (Int16)INDX_UICONTROLS.USER_ID + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestUserID
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 110);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestUserID";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(103, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 33;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Имя пользователя";

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 84);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(98, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 32;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Имя базы данных";

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 30);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 30;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "IP адрес сервера";

            i = (Int16)INDX_UICONTROLS.PASS;
            // 
            // mtbxDestPass
            //
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 133);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "mtbxDestPass";
            ((MaskedTextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).PasswordChar = '#';
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 29;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.USER_ID;
            // 
            // tbxDestUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 107);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 28;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE;
            // 
            // tbxDestNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 81);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 27;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxDestServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 27);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 25;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i])).EndInit();
        }
        
        protected void InitializeComponentTransDB()
        {
            int i = -1;

            //m_arUIControls = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, 2 * (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROLS]
            m_arUIControls = new System.Windows.Forms.Control[,]
            { { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() },
            { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() } };

            InitializeComponentTrans ();

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i])).BeginInit();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                this.groupBoxSource.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]);
            }

            i = (Int16)INDX_UICONTROLS.PORT + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(12, 55);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourcePort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(32, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 21;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Порт";

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // nudnSourcePort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 53);
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "nudnSourcePort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(69, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 16;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});

            i = (Int16)INDX_UICONTROLS.PASS + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePass
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 134);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourcePass";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(45, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 24;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Пароль";

            i = (Int16)INDX_UICONTROLS.USER_ID + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 108);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(103, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 23;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Имя пользователя";

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 82);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(98, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 22;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Имя базы данных";

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 28);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 20;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "IP адрес сервера";

            i = (Int16)INDX_UICONTROLS.PASS;
            // 
            // mtbxSourcePass
            //
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 131);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "mtbxSourcePass";
            ((MaskedTextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).PasswordChar = '#';
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 19;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(component_Changed);

            i = (Int16)INDX_UICONTROLS.USER_ID;
            // 
            // tbxSourceUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 105);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 18;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(component_Changed);

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE;
            // 
            // tbxSourceNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 79);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 17;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxSourceServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 25);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 15;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i])).EndInit();
        }

        protected abstract void buttonSaveSourceSett_Click(object sender, EventArgs e);

        protected override void UpdateActiveGui(int type) {}
        protected override void HideGraphicsSettings() { }

        protected void InitializeComponentTransSrc(string text)
        {
            int i = -1;
            
            m_arUIControls = new System.Windows.Forms.Control[,]
            { { new System.Windows.Forms.TextBox(), new Button (), null, null, null,
                new System.Windows.Forms.Label(), null, null, null, null },
            { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() } };

            InitializeComponentTrans ();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                if (!(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i] == null))
                    this.groupBoxSource.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]);
                else
                    ;
            }

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 28);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 20;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = text;

            i =  (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxSourcePathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 55);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(243, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 15;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);
            ((TextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).ReadOnly = true;

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // buttonPathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(257, 53);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "buttonSaveSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(29, 23);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 2;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "...";
            ((Button)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).UseVisualStyleBackColor = true;
            //m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Click += new System.EventHandler(...);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;

            //Идентичный код с панелью Modes-Centre
            buttonSourceExport.Location = new System.Drawing.Point(8, 86);

            buttonSourceSave.Location = new System.Drawing.Point(151, 86);
            buttonSourceSave.Click -= buttonSourceSave_Click;
            buttonSourceSave.Click += new EventHandler(this.buttonSaveSourceSett_Click);
            buttonSourceSave.Enabled = false;

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();

            this.groupBoxSource.Size = new System.Drawing.Size(300, 120);

            this.groupBoxDest.Location = new System.Drawing.Point(3, 196);

            panelMain.Size = new System.Drawing.Size(822, 404);

            //base.buttonClose.Anchor = AnchorStyles.Left;
            buttonClose.Location = new System.Drawing.Point(733, 434);

            this.Size = new System.Drawing.Size(849, 514);

            this.m_checkboxModeMashine.Location = new System.Drawing.Point(13, 434);
        }

        protected List<int> m_listID_TECNotUse;

        protected abstract void start();

        protected override void Start() {
            initTableHourRows();
            
            base.Start ();
        }

        protected void RemoveTEC (HAdmin admin) {
            foreach (int id in m_listID_TECNotUse)
            {
                admin.RemoveTEC(id);
            }
        }

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
                (m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex] < m_arAdmin[i].allTECComponents.Count))
            {
                ConnectionSettings connSett = m_arAdmin[i].allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].tec.connSetts[(int)StatisticCommon.CONN_SETT_TYPE.PBR];
                for (int j = 0; j < (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; j++)
                {
                    switch (j)
                    {
                        case (Int16)FormMainTrans.INDX_UICONTROLS.SERVER_IP:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.server;
                            break;
                        case (Int16)INDX_UICONTROLS.PORT:
                            //if (m_arUIControlDB[i, j].Enabled == true)
                                ((NumericUpDown)m_arUIControls[i, j]).Text = connSett.port.ToString();
                            //else
                            //    ;
                            break;
                        case (Int16)INDX_UICONTROLS.NAME_DATABASE:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.dbName;
                            break;
                        case (Int16)INDX_UICONTROLS.USER_ID:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.userName;
                            break;
                        case (Int16)INDX_UICONTROLS.PASS:
                            ((MaskedTextBox)m_arUIControls[i, j]).Text = connSett.password;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                ;
        }

        protected abstract void setUIControlSourceState();

        protected void enabledButtonSourceExport(bool enabled)
        {
            buttonSourceExport.Enabled = enabled;
        }

        /// <summary>
        /// Создание формы для редактирования параметров соединения с БД конфигурации
        /// </summary>
        /// <param name="connSettFileName">наименование файла с параметрами соединения</param>
        /// <param name="bCheckAdminLength">признак проверки кол-ва параметров соединения по кол-ву объекьлв 'HAdmin'</param>
        protected void CreateFormConnectionSettings(string connSettFileName, bool bCheckAdminLength)
        {
            bool bShowFormConnSett = false;
            
            s_fileConnSett = new FIleConnSett(connSettFileName, FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List <FormConnectionSettings> ();
            s_listFormConnectionSettings.Add (new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));

            if (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
                ;
            else
                bShowFormConnSett = true;

            if (bCheckAdminLength == true)
            {
                if (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count < m_arAdmin.Length)
                {
                    while (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count < m_arAdmin.Length)
                        s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].addConnSett(new ConnectionSettings());

                    if (bShowFormConnSett == false) bShowFormConnSett = true; else ;
                }
                else
                    ;
            }
            else
            {
            }

            if (bShowFormConnSett == true)
                конфигурацияБДToolStripMenuItem.PerformClick();
            else
                ;
        }

        /// <summary>
        /// Заполнение списка компонентов
        /// </summary>
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

        /// <summary>
        /// Заполнение таблицы полученными данными (при [авто]режиме экспорт данных + переход к следующему элементу списка компонентов)
        /// </summary>
        /// <param name="date">дата</param>
        protected virtual void setDataGridViewAdmin(DateTime date)
        {
            //if (m_IndexDB == (short)CONN_SETT_TYPE.SOURCE) {
            //    string strDatetimeSeason = m_fileINI.GetValueOfKey(@"Season DateTime");
            //    if (strDatetimeSeason.Equals(string.Empty) == false)
            //    {
            //        DateTime dtSeason = DateTime.Parse(strDatetimeSeason);

            //        if ((date == dtSeason.Date))
            //        {
            //            //Преобразовать массивы m_arAdmin
            //            for (CONN_SETT_TYPE type = (CONN_SETT_TYPE)0; type < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; type++)
            //            {
            //                m_arAdmin[(int)type].ToSummerWinter(dtSeason.Hour - 1);
            //            }
            //        }
            //        else
            //        {
            //        }
            //    }
            //    else
            //    {
            //    }
            //} else {
            //}

            //if (WindowState == FormWindowState.Minimized)
            //if (m_bTransAuto == true)
            //if (m_modeMashine == MODE_MASHINE.AUTO || m_modeMashine == MODE_MASHINE.SERVICE)
            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                //Копирование данных из массива одного объекта (SOURCE) в массив другого объекта (DEST)
                m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);
                //((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_bSavePPBRValues = true;

                //SaveRDGValues (false);
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateBoolFunc(SaveRDGValues), false);
                else
                    Logging.Logg().Error(@"FormMainTrans::setDataGridViewAdmin () - ... BeginInvoke (SaveRDGValues) - ...", Logging.INDEX_MESSAGE.D_001);

                //this.BeginInvoke(new DelegateFunc(trans_auto_next));
            }
            else
            {
                updateDataGridViewAdmin (date);
            }
        }

        /// <summary>
        /// При [авто]режиме переход к следующему элементу списка компонентов
        /// </summary>
        protected virtual void errorDataGridViewAdmin()
        {
            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                IAsyncResult asyncRes;
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    asyncRes = this.BeginInvoke(new DelegateFunc(trans_auto_next));
                else
                    Logging.Logg().Error(@"FormMainTrans::errorDataGridViewAdmin () - ... BeginInvoke (trans_auto_next) - ...", Logging.INDEX_MESSAGE.D_001);
            }
            else
                ;
        }

        protected abstract void updateDataGridViewAdmin (DateTime date);

        protected void initTableHourRows(/*int indx = индекс для m_arAdmin*/)
        {
            for (CONN_SETT_TYPE type = (CONN_SETT_TYPE)0; type < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; type++)
                m_arAdmin[(int)type].m_curDate = dateTimePickerMain.Value;

            if (dateTimePickerMain.Value.Date.Equals(HAdmin.SeasonDateTime.Date) == false) {
                m_dgwAdminTable.InitRows(24, false);
            }
            else {
                m_dgwAdminTable.InitRows(25, true);
            }
        }

        /// <summary>
        /// При [авто]режиме переход к следующему элементу списка компонентов
        /// </summary>
        protected virtual void saveDataGridViewAdminComplete()
        {
            Logging.Logg().Debug(@"FormMainTrans::saveDataGridViewAdminComplete () - m_bTransAuto=" + m_bTransAuto + @", m_modeMashine=" + m_modeMashine.ToString () + @", - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                IAsyncResult asyncRes;
                //if (IsHandleCreated/*InvokeRequired*/ == true)
                    asyncRes = this.BeginInvoke(new DelegateFunc(trans_auto_next));
                //else
                    //Logging.Logg().Error(@"FormMainTrans::saveDataGridViewAdminComplete () - ... BeginInvoke (trans_auto_next) - ...");
                ////this.EndInvoke (asynchRes);
                //////trans_auto_next ();
            }
            else
                ;

            Logging.Logg().Debug(@"FormMainTrans::saveDataGridViewAdminComplete () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected void setDatetimePickerMain(DateTime date)
        {
            dateTimePickerMain.Value = date;
        }

        protected void setDatetimePicker(DateTime date)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateDateFunc(setDatetimePickerMain), date);
            else
                Logging.Logg().Error(@"FormMainTrans::setDatetimePicker () - ... BeginInvoke (setDatetimePickerMain) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        protected override void Stop()
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

            base.Stop();
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

                if (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count > 1)
                    s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].SelectedIndex = m_IndexDB;
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
            //buttonClose_Click (null, null);
            buttonClose.PerformClick();
        }

        private void конфигурацияБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_formConnectionSettings.StartPosition = FormStartPosition.CenterParent;
            s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].ShowDialog(this);

            //Эмуляция нажатия кнопки "Ок"
            /*
            m_formConnectionSettings.btnOk_Click(null, null);
            */

            DialogResult dlgRes = s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].DialogResult;
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

        //protected override void ErrorReport (string msg) {
        //    Logging.Logg().Error(@"FormMainTrans::ErrorReport () - сообщение: " + msg);
            
        //    m_statusStripMain.BeginInvoke(delegateEvent);

        //    m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].AbortRDGExcelValues();

        //    this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), false);

        //    if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
        //        this.BeginInvoke(new DelegateFunc(trans_auto_next));
        //    else
        //        ;
        //}

        //protected override void ActionReport(string msg)
        //{
        //    m_statusStripMain.BeginInvoke(delegateEvent);

        //    this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), true);
        //}

        protected override int UpdateStatusString()
        {
            int have_msg = -1;

            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)))
            {
                have_msg = (m_report.errored_state == true) ? -1 : (m_report.warninged_state == true) ? 1 : 0;

                if (((! (have_msg == 0)) || (m_report.actioned_state == true)) && (m_arAdmin[m_IndexDB].IsStarted == true))
                {
                    if (m_report.actioned_state == true)
                    {
                        m_lblDescMessage.Text = m_report.last_action;
                        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    }
                    else
                        ;

                    if (have_msg == 1)
                    {
                        m_lblDescMessage.Text = m_report.last_warning;
                        m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                    }
                    else
                        ;

                    if (have_msg == -1)
                    {
                        m_lblDescMessage.Text = m_report.last_error;
                        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    }
                    else
                        ;
                }
                else
                {
                    m_lblDescMessage.Text = string.Empty;
                    m_lblDateMessage.Text = string.Empty;
                }
            }
            else
                ;

            return have_msg;
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
                if (m_bTransAuto == true) buttonClose.PerformClick(); else enabledUIControl(true);
        }

        protected void trans_auto_next () {
            Logging.Logg().Debug(@"FormMainTrans::trans_auto_next () - comboBoxTECComponent.SelectedIndex=" + comboBoxTECComponent.SelectedIndex, Logging.INDEX_MESSAGE.NOT_SET);

            if (comboBoxTECComponent.SelectedIndex + 1 < comboBoxTECComponent.Items.Count)
            {
                comboBoxTECComponent.SelectedIndex ++;
                //Обработчик отключен - вызов "программно"
                comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
                if (m_bTransAuto == true)
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

        protected override void timer_Start()
        {
            m_listTECComponentIndex = ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).GetListIndexTECComponent(m_modeTECComponent);

            string keyPar = @"Season DateTime";
            HAdmin.SeasonDateTime = DateTime.Parse (m_sFileINI.GetMainValueOfKey (keyPar));
            keyPar = @"Season Action";
            HAdmin.SeasonAction = Int32.Parse(m_sFileINI.GetMainValueOfKey(keyPar));

            if (m_modeMashine == MODE_MASHINE.TO_DATE)
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

        private void timerService_Tick(object sender, EventArgs e)
        {
            if (!(m_modeMashine == MODE_MASHINE.TO_DATE))
            switch (m_modeMashine) {
                case MODE_MASHINE.SERVICE:
                    if (timerService.Interval == ProgramBase.TIMER_START_INTERVAL)
                    {
                        //Первый запуск
                        if (m_arg_interval == timerService.Interval) m_arg_interval++; else ; //??? случайное совпадение...
                        timerService.Interval = m_arg_interval;

                        FillComboBoxTECComponent();
                    }
                    else
                        ;

                    dateTimePickerMain.Value = DateTime.Now;

                    trans_auto_start();
                    break;
                case MODE_MASHINE.TO_DATE:
                    if (timerService.Interval == ProgramBase.TIMER_START_INTERVAL)
                    {
                        //Первый запуск
                        if (m_arg_interval == timerService.Interval) m_arg_interval++; else ; //??? случайное совпадение...
                        timerService.Interval = m_arg_interval;
                    }
                    else
                        выходToolStripMenuItem.PerformClick ();
                    break;
                default:
                    break;
            }
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

        protected /*virtual*/ void buttonDestSave_Click(object sender, EventArgs e) { throw new NotImplementedException (); }

        protected /*virtual*/ void buttonSourceSave_Click(object sender, EventArgs e) { throw new NotImplementedException(); }

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
            initTableHourRows();

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
            if (!(m_modeMashine == MODE_MASHINE.TO_DATE))
                if (m_checkboxModeMashine.Checked == true)
                {
                    //if (m_modeMashine == MODE_MASHINE.INTERACTIVE) m_modeMashine = MODE_MASHINE.SERVICE; else ;
                    //То же самое
                    if (!(m_modeMashine == MODE_MASHINE.SERVICE)) m_modeMashine = MODE_MASHINE.SERVICE; else ;

                    enabledUIControl(false);
                    m_dgwAdminTable.Enabled = false;

                    InitializeTimerService();
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
            {
                InitializeTimerService();
                timerService.Start();
            }
        }

        private void InitializeTimerService () {
            if (timerService == null) {
                timerService = new System.Windows.Forms.Timer(this.components);
                timerService.Interval = ProgramBase.TIMER_START_INTERVAL; //Пеавый запуск
                timerService.Tick += new System.EventHandler(this.timerService_Tick);
            }
            else
                ;
        }

        private struct PARAMToSaveRDGValues
        {
            public int listIndex;
            public DateTime date;
            public bool bCallback;

            public PARAMToSaveRDGValues (int li, DateTime dt, bool cb) {
                listIndex = li;
                date = dt;
                bCallback = cb;
            }
        };

        private void saveRDGValues (object bCallback) {
            ((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(((PARAMToSaveRDGValues)bCallback).listIndex, ((PARAMToSaveRDGValues)bCallback).date, ((PARAMToSaveRDGValues)bCallback).bCallback);
        }

        protected virtual void SaveRDGValues (bool bCallback) {
            //((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
            PARAMToSaveRDGValues paramToSaveRDGValues = new PARAMToSaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
            new Thread(new ParameterizedThreadStart(saveRDGValues)).Start(paramToSaveRDGValues);
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
