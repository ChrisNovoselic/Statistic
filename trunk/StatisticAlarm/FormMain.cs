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
            string msg = string.Empty;
            bool bAbort = true;

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);

            delegateStartWait();

            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);

            bAbort = Initialize(out msg);

            delegateStopWait();

            if (msg.Equals(string.Empty) == false)
                Abort(msg, bAbort);
            else
                this.Activate();            
        }

        private bool Initialize(out string msgError)
        {
            bool bRes = true;
            msgError = string.Empty;

            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                _state = InitializeConfigDB(out msgError);
                switch (_state)
                {
                    case -1:
                        msgError = @"Неизвестная причина";
                        break;
                    case -3: //@"Не найден пользователь@
                        break;
                    case -2:
                    case -5:
                    case -4: //@"Необходимо изменить параметры соединения с БД" - получено из 'Initialize'
                        bRes = false;
                        break;
                    case -6: //@"Пользователю не разрешено использовать задачу" - получено из 'Initialize'
                        break;
                    default:
                        //Успех... пост-инициализация
                        s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                        m_panelAlarm.Start();
                        break;
                }
            }
            else
            {//Файла с параметрами соединения нет совсем или считанные параметры соединения не валидны
                msgError = @"Необходимо изменить параметры соединения с БД конфигурации";

                bRes = false;
            }

            return bRes;
        }

        private int InitializeConfigDB (out string msgError)
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
                if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP) == false)
                {
                    msgError = @"Пользователю не разрешено использовать задачу";
                    iRes = -6;
                }
                else
                    //Успех...
                    ;
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
            using (FormAbout formAbout = new FormAbout(this.Icon.ToBitmap() as Image))
            {
                formAbout.ShowDialog(this);
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
            bool bAbort = false;
            string msg = string.Empty; ;
            DialogResult dlgRes = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].ShowDialog ();

            if ((dlgRes == DialogResult.OK)
                || (dlgRes == DialogResult.Yes))
            {
                //??? Остановить панель
                m_panelAlarm.Stop();

                bAbort = Initialize(out msg);
            }
            else
                ;

            if (msg.Equals(string.Empty) == false)
                Abort(msg, bAbort);
            else
                this.Activate();
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

            m_panelAlarm = new PanelAlarmJournal(PanelAlarmJournal.MODE.SERVICE);

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
