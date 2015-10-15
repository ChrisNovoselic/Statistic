using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        private int _state;
        public static FormParameters formParameters;

        public FormMain()
        {
            _state =1;

            InitializeComponent();
        }

        private void FormMain_Load(object obj, EventArgs ev)
        {
            delegateStartWait ();
            
            string msg = string.Empty;
            //bool bAbort = false;

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);            

            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                _state = Initialize(out msg);
                switch (_state)
                {
                    case -1:
                        msg = @"Неизвестная причина";
                        break;
                    case -3: //Не найден пользователь
                        //Остальные п.п. меню блокируются в 'сменитьРежимToolStripMenuItem_EnabledChanged'
                        // этот п. блокируется только при конкретной ошибке "-3"
                        (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems[0].Enabled = false;
                        break;
                    case -2:
                    case -5:
                    case -4: //@"Необходимо изменить параметры соединения с БД"
                        //Сообщение получено из 'Initialize'
                        break;
                    case -6: //пользователю не разрешено использовать задачу
                        msg = @"Пользователю не разрешено использовать задачу"; 
                        break;
                    default:
                        //Успех...
                        m_panelAlarm.Start();
                        break;
                }
            }
            else
            {//Файла с параметрами соединения нет совсем или считанные параметры соединения не валидны
                msg = @"Необходимо изменить параметры соединения с БД конфигурации";
            }

            delegateStopWait();

            if (msg.Equals(string.Empty) == false)
                Abort(msg, /*bAbort*/ true);
            else
                this.Activate();            
        }

        private int Initialize (out string msgError)
        {
            int iRes = 0;
            msgError = string.Empty;

            int idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            //Проверить наличие пользователя в БД_конфигурации
            try
            {
                using (HStatisticUsers users = new HStatisticUsers(idListenerConfigDB)) { ; }
            }
            catch (Exception e)
            {
                if (e is HException)
                    iRes = ((HException)e).m_code; //-2, -3, -4
                else
                    iRes = -1; // общая (неизвестная) ошибка

                msgError = e.Message;
            }

            if (iRes == 0)
                if (HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP) == true)
                {//Успех...
                    m_panelAlarm.Start ();
                }
                else
                    iRes = -6;
            else
                ;

            DbSources.Sources().UnRegister(idListenerConfigDB);

            return iRes;
        }

        private void FormMain_Activated(object obj, EventArgs ev)
        {
            if (_state == 0)
                m_panelAlarm.Activate(true);
            else
                ;
        }

        private void FormMain_Deactivated(object obj, EventArgs ev)
        {
            if (_state == 0)
                m_panelAlarm.Activate(false);
            else
                ;
        }

        private void fMenuItemAbout_Click(object obj, EventArgs ev)
        {
            using (FormAbout formAbout = new FormAbout ())
            {
                formAbout.ShowDialog ();
            }
        }

        private void FormMain_FormClosing(object obj, EventArgs ev)
        {
            m_panelAlarm.Activate (false);
            m_panelAlarm.Stop ();
        }

        protected override void timer_Start()
        {
            throw new NotImplementedException();
        }

        protected override int UpdateStatusString()
        {
            throw new NotImplementedException();
        }

        protected override void HideGraphicsSettings()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateActiveGui(int type)
        {
            throw new NotImplementedException();
        }

        private void fMenuItemExit_Click (object obj, EventArgs ev)
        {
            Close ();
        }

        private void fMenuItemDBConfig_Click(object obj, EventArgs ev)
        {
            s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].ShowDialog ();
        }
    }

    partial class FormMain
    {
        PanelAlarmJournal m_panelAlarm;
        Panel _panelMain;
        
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));            
            this.components = new System.ComponentModel.Container();

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "FormStatisticAlarm";
            this.MinimumSize = new Size (800, 600);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject(@"StatisticAlarm")));
            this.StartPosition = FormStartPosition.CenterScreen;

            this.MainMenuStrip = new MenuStrip ();
            this.MainMenuStrip.Items.AddRange (
                new ToolStripMenuItem [] {
                    new ToolStripMenuItem (@"Файл")
                    , new ToolStripMenuItem (@"Настройка")
                    , new ToolStripMenuItem (@"О программе")
                }
            );
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"Выход"));
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemExit_Click);
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"БД конфигурации"));
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemDBConfig_Click);
            (this.MainMenuStrip.Items[2] as ToolStripMenuItem).Click += new EventHandler(fMenuItemAbout_Click);

            m_panelAlarm = new PanelAlarmJournal(new ConnectionSettings(@"CONFIG_DB", @"10.100.104.18", 1433, @"techsite_cfg-2.X.X", @"client", @"client"), PanelAlarmJournal.MODE.SERVICE);

            _panelMain = new Panel ();
            _panelMain.Location = new Point(0, this.MainMenuStrip.Height);
            _panelMain.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - this.MainMenuStrip.Height - this.m_statusStripMain.Height);
            _panelMain.Anchor = (AnchorStyles)(((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right) | AnchorStyles.Bottom);            
            _panelMain.Controls.Add (m_panelAlarm);

            this.SuspendLayout ();

            this.Controls.Add(this.MainMenuStrip);
            this.Controls.Add (_panelMain);

            this.ResumeLayout (false);
            this.PerformLayout ();

            this.Load += new EventHandler(FormMain_Load);
            this.Activated += new EventHandler(FormMain_Activated);
            this.FormClosing += new FormClosingEventHandler(FormMain_FormClosing);
        }

        #endregion
    }
}
