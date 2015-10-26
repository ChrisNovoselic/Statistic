using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media; //...SoundPlayer
using System.IO; //...File
using System.Threading; //...Thread

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    /// <summary>
    /// Класс главной формы приложения "Журнал событий сигнализаций"
    /// </summary>
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        /// <summary>
        /// Состояние формы (признак выполнения очередной операции: 0 - без ошибок)
        /// </summary>
        private int _state;
        /// <summary>
        /// Объект с параметрами приложения (из БД_конфигурации)
        /// </summary>
        public static FormParameters formParameters;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public FormMain()
        {
            //Установить состояние ("загружается")
            _state = 1;

            InitializeComponent();
        }
        /// <summary>
        /// Обработчик события - загрузка формы завершена
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormMain_Load(object obj, EventArgs ev)
        {
            string msg = string.Empty;
            bool bAbort = true;
            //Создать объект - чтение зашифрованного файла с параметрами соединения
            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            //Отобразить окно для визуализации выполнения длительной операции
            delegateStartWait();
            //Создать список форм для редактирования параметров соединения
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            //Добавить элемент с параметрами соединения из объекта 'FIleConnSett' 
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);

            bAbort = Initialize(out msg);
            //Снять с отображения окно для визуализации выполнения длительной операции
            delegateStopWait();

            if (msg.Equals(string.Empty) == false)
                //Прекратить/выдать сообщение об ошибке
                Abort(msg, bAbort);
            else
            {
                //Продолжить выполнение приложения
                this.Activate();
            }
        }
        /// <summary>
        /// Инициализация данных формы
        /// </summary>
        /// <param name="msgError">Сообщение об ошибке (при наличии)</param>
        /// <returns>Признак выполнения функции</returns>
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
                        formParameters = new FormParameters_DB(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());

                        updateParametersSetup ();

                        s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                        //Создать "рабочую" панель
                        m_panelAlarm = new PanelAlarm(MODE.SERVICE);
                        _panelMain.Controls.Add(m_panelAlarm);

                        m_formAlarmEvent = new MessageBoxAlarmEvent (this);
                        m_panelAlarm.EventGUIReg += new AlarmNotifyEventHandler(OnPanelAlarmEventGUIReg);
                        m_formAlarmEvent.EventActivateTabPage += new DelegateBoolFunc(activateTabPage);
                        m_formAlarmEvent.EventFixed += new DelegateObjectFunc(m_panelAlarm.OnEventFixed);

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

        private void updateParametersSetup()
        {
            ////Параметры записи сообщений лог-а...
            //Logging.UpdateMarkDebugLog();

            //Параметры обновления "основной панели"...
            PanelStatistic.POOL_TIME = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);
            PanelStatistic.ERROR_DELAY = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]);

            //Параметры перехода на сезонное времяисчисление...
            HAdmin.SeasonDateTime = DateTime.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]);
            HAdmin.SeasonAction = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_ACTION]);

            //Параметры обработки запросов к БД...
            DbInterface.MAX_RETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAX_ATTEMPT]);
            DbInterface.MAX_WAIT_COUNT = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_COUNT]);
            DbInterface.WAIT_TIME_MS = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_TIME]);

            //Параметры валидности даты/времени получения данных СОТИАССО...
            TecViewAlarm.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

            //Параметрвы для ALARM...
            AdminAlarm.MSEC_ALARM_TIMERUPDATE =
                30
                //Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_UPDATE])
                * 1000;
            AdminAlarm.MSEC_ALARM_EVENTRETRY =
                90
                //Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_EVENT_RETRY])
                * 1000;
            AdminAlarm.MSEC_ALARM_TIMERBEEP = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000;
            AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_SYSTEMMEDIA_TIMERBEEP];

            ////Обновить значения идентификаторов активных источников СОТИАССО
            //FormParameters.UpdateIdLinkTMSources();
        }
        /// <summary>
        /// Инициализация параметров соединения с БД_конфигурации
        /// </summary>
        /// <param name="msgError">Сообщение об ошибке (при наличии)</param>
        /// <returns>Признак выполнения функции</returns>
        private int InitializeConfigDB (out string msgError)
        {
            int iRes = 0;
            msgError = string.Empty;
            //Идентификатор соединения с БД_конфигурации
            int idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            //Проверить наличие пользователя в БД_конфигурации
            try
            {
                //Создать И удалить объект с пользовательскими настройками (заполнить статические члены)
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
            //Отменить регистрацию соединения с БД_конфигурации
            DbSources.Sources().UnRegister(idListenerConfigDB);

            return iRes;
        }
        /// <summary>
        /// Обработчик события - активация формы
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormMain_Activated(object obj, EventArgs ev)
        {
            if (_state == 0)
                m_panelAlarm.Activate(true);
            else
                ;
        }
        /// <summary>
        /// ??? - обязательно - Обработчик события - деактивация формы
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormMain_Deactivated(object obj, EventArgs ev)
        {
            if (_state == 0)
                m_panelAlarm.Activate(false);
            else
                ;
        }
        /// <summary>
        /// Обработчик события выбора п. главного меню "Помощь-О программе"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fMenuItemAbout_Click(object obj, EventArgs ev)
        {
            using (FormAbout formAbout = new FormAbout(this.Icon.ToBitmap() as Image))
            {
                formAbout.ShowDialog(this);
            }
        }
        /// <summary>
        /// Обработчик события - перед закрытием формы
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormMain_FormClosing(object obj, EventArgs ev)
        {
            if (! (m_panelAlarm == null))
            //if (!(_state < 0))
            {
                //Деактивировать панель
                m_panelAlarm.Activate(false);
                //Остановить панель
                m_panelAlarm.Stop();
            }
            else
                ;
        }
        /// <summary>
        /// ??? Функция при 1-м событии запуска таймера
        /// </summary>
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
        /// <summary>
        /// Обработчик события выбора п. главного меню "Файл-Выход"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fMenuItemExit_Click (object obj, EventArgs ev)
        {
            Close ();
        }
        /// <summary>
        /// Обработчик события выбора п. главного меню "Настройка-БД_конфигурации"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fMenuItemDBConfig_Click(object obj, EventArgs ev)
        {
            bool bAbort = false;
            string msg = string.Empty; ;
            //Получить рез-т отображения окна с настройками параметров соединения
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
            //Проверить наличие сообщения об ошибке
            if (msg.Equals(string.Empty) == false)
                //Отобразить сообщение/завершить работу приложения (в ~ от 'bAbort')
                Abort(msg, bAbort);
            else
                //Продолжить работу
                this.Activate();
        }

        #region Код для отображения сообщения о событии сигнализации

        private class MessageBoxAlarmEvent
        {
            private Queue <AlarmNotifyEventArgs> _queueArgs;

            public event DelegateBoolFunc EventActivateTabPage;
            public event DelegateObjectFunc EventFixed;
            
            private int m_iAlarmEventCounter;
            private int AlarmEventCounter { get { return m_iAlarmEventCounter; } set { m_iAlarmEventCounter = value; } }
            private SoundPlayer m_sndAlarmEvent;
            private
                //System.Threading.Timer
                System.Windows.Forms.Timer
                    m_timerAlarmEvent;
            private Form _owner;

            public MessageBoxAlarmEvent(Form owner)
            {
                _owner = owner;
                _queueArgs = new Queue<AlarmNotifyEventArgs> ();
            }

            //private void timerAlarmEvent (object obj)
            private void timerAlarmEvent(object obj, EventArgs ev)
            {
                //System.Media.SystemSounds.Question.Play();
                if (m_sndAlarmEvent == null)
                    Console.Beep();
                else
                    m_sndAlarmEvent.Play();
            }

            private void messageBoxShow(object text)
            {
                ////Вариант №1
                //MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Вариант №2
                MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                _owner.BeginInvoke(new DelegateFunc(messageBoxHide));
            }

            private void messageBoxHide()
            {
                //bool bContinue = false;

                lock (this)
                {
                    m_iAlarmEventCounter--;

                    if (m_iAlarmEventCounter == 0)
                    {
                        //m_timerAlarmEvent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        m_timerAlarmEvent.Stop();
                        m_timerAlarmEvent.Dispose();
                        m_timerAlarmEvent = null;

                        if (!(m_sndAlarmEvent == null))
                        {
                            m_sndAlarmEvent.Stop();
                            m_sndAlarmEvent.Dispose();
                            m_sndAlarmEvent = null;
                        }
                        else
                            ;

                        //bContinue = true;

                        //Активация текущей вкладки
                        EventActivateTabPage (true);
                    }
                    else
                        ;

                    EventFixed(_queueArgs.Dequeue());
                }
            }

            public void MessageBoxShow(object ev)
            {
                //int selIndxTabPage = -1;
                //string text = string.Empty;

                lock (this)
                {
                    //selIndxTabPage = (int)(obj as object[])[0];
                    //text = (string)(obj as object[])[1];

                    if (m_timerAlarmEvent == null)
                    {
                        //Деактивация текущей вкладки
                        EventActivateTabPage(false);

                        string strPathSnd = Environment.GetEnvironmentVariable("windir") + @"\Media\" + AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP;
                        if (File.Exists(strPathSnd) == true)
                            m_sndAlarmEvent = new SoundPlayer(strPathSnd);
                        else
                            ;

                        m_timerAlarmEvent =
                            //new System.Threading.Timer(new TimerCallback(timerAlarmEvent), null, 0, AdminAlarm.MSEC_ALARM_TIMERBEEP) //Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000
                            new System.Windows.Forms.Timer();
                        m_timerAlarmEvent.Tick += new EventHandler(timerAlarmEvent);
                        m_timerAlarmEvent.Interval = AdminAlarm.MSEC_ALARM_TIMERBEEP;
                        m_timerAlarmEvent.Start();

                        m_iAlarmEventCounter = 1;
                    }
                    else
                        m_iAlarmEventCounter++;

                    _queueArgs.Enqueue (ev as AlarmNotifyEventArgs);
                }

                //Поверх остальных окон
                bool bPrevTopMost = _owner.TopMost;
                _owner.TopMost = true;
                //Диалоговое окно
                new Thread(new ParameterizedThreadStart(messageBoxShow)).Start((ev as AlarmNotifyEventArgs).m_messageGUI);
                //Востановить значение по умолчанию
                _owner.TopMost = bPrevTopMost;
            }
        }

        MessageBoxAlarmEvent m_formAlarmEvent;

        private void activateTabPage(bool active)
        {
            activateTabPage (-1, active);
        }
        
        private void activateTabPage (int indx, bool active)
        {            
            m_panelAlarm.Activate (active);
        }

        private void OnPanelAlarmEventGUIReg(AlarmNotifyEventArgs ev)
        {
            try
            {
                //panelAdminKomDispEventGUIReg(text);
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new AlarmNotifyEventHandler(m_formAlarmEvent.MessageBoxShow), ev);
                else
                    Logging.Logg().Error(@"FormMain::OnPanelAlarmEventGUIReg () - ... BeginInvoke (m_formAlarmEvent.Show) - ...", Logging.INDEX_MESSAGE.D_001);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::OnPanelAlarmEventGUIReg (string) - text=" + ev.m_message_shr);
            }
        }

        #endregion
    }

    partial class FormMain
    {
        PanelAlarm m_panelAlarm;
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
            this.Text = "Регистрация событий сигнализаций";
            this.MinimumSize = new Size (800, 600); // установить минимальный размер формы
            this.Icon = ((System.Drawing.Icon)(resources.GetObject(@"StatisticAlarm"))); // назаначить пиктонграмму окна
            this.StartPosition = FormStartPosition.CenterScreen;
            //Сформировать/добавить главное меню
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
            //Создать панель для размещения "рабочих" панелей
            _panelMain = new Panel ();
            _panelMain.Location = new Point(0, this.MainMenuStrip.Height);
            _panelMain.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - this.MainMenuStrip.Height - this.m_statusStripMain.Height);
            _panelMain.Anchor = (AnchorStyles)(((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right) | AnchorStyles.Bottom);            
            _panelMain.Controls.Add (m_panelAlarm);

            this.SuspendLayout ();
            //Добавить элементы управления
            this.Controls.Add(this.MainMenuStrip);
            this.Controls.Add (_panelMain);
            //Возобновить формирование макета с дочерними элементами управления
            this.ResumeLayout (false);
            this.PerformLayout ();
            //Добавить обработчики событий
            this.Load += new EventHandler(FormMain_Load);
            this.Activated += new EventHandler(FormMain_Activated);
            this.Deactivate += new EventHandler(FormMain_Deactivated);
            this.FormClosing += new FormClosingEventHandler(FormMain_FormClosing);
        }

        #endregion
    }
}
