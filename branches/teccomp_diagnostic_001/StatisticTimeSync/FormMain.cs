﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

//using HClassLibrary;
using StatisticCommon;
using ASUTP.Database;
using ASUTP.Forms;
using ASUTP.Helper;
using ASUTP.Core;
using ASUTP;

namespace StatisticTimeSync
{
    public partial class FormMain : ASUTP.Forms.FormMainBaseWithStatusStrip {
        /// <summary>
        /// Панель - единственная дочерняя по отношению к главной форме
        ///  , и единственная родительская по отношению к рабочей панели
        /// </summary>
        private Panel _panelMain;
        /// <summary>
        /// Объект с параметрами приложения (из БД_конфигурации)
        /// </summary>
        public static FormParameters formParameters;
        /// <summary>
        /// Состояние формы (признак выполнения очередной операции: 0 - без ошибок)
        /// </summary>
        private int _state;

        public FormMain()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            //this.notifyIconMain.Icon =
            this.Icon = resources.GetObject(@"StatisticTimeSync") as System.Drawing.Icon;

            _state = 1;
            InitializeComponent(); 
        }

        /// <summary>
        /// Обработчик события - загрузка панели завершена (...запуск старта панели...)
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void FormStatisticTimeSync_Load(object obj, EventArgs ev)
        {
            string msg = string.Empty;
            bool bAbort = true;
            //Отобразить окно для визуализации выполнения длительной операции
            delegateStartWait();

            //Создать объект - чтение зашифрованного файла с параметрами соединения
            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            //Создать список форм для редактирования параметров соединения
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            //Добавить элемент с параметрами соединения из объекта 'FIleConnSett' 
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);

            bAbort = initialize(out msg);
            //Снять с отображения окно для визуализации выполнения длительной операции
            delegateStopWait();

            if (msg.Equals(string.Empty) == false)
                //Прекратить/выдать сообщение об ошибке
                Abort(msg, bAbort);
            else
                //Продолжить выполнение приложения
                this.Activate();
        }

        /// <summary>
        /// Активация формы
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие(форма)</param>
        /// <param name="ev">Аргумент события</param>
        private void FormStatisticTimeSync_Activate(object obj, EventArgs ev)
        {
           m_panel?.Activate(true);
        }

        /// <summary>
        /// Деактивация формы
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие(форма)</param>
        /// <param name="ev">Аргумент события</param>
        private void FormStatisticTimeSync_Deactivate(object obj, EventArgs ev)
        {
           m_panel?.Activate(false);  
        }

        /// <summary>
        /// Инициализация панели
        /// </summary>
        public bool initialize(out string msgError)
        {
            bool bRes = true;
            msgError = string.Empty;

            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                _state = InitializeConfigDB(out msgError);
                switch (_state)
                {
                    case -1:
                        msgError = FormMainStatistic.MSG_ERROR_INIT[(int)FormMainStatistic.INDEX_ERROR_INIT.UNKNOWN];
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
                        //!!! Один экземпляр для всего приложения на весь срок выполнения
                        new DbTSQLConfigDatabase (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ());
                        formParameters = new FormParameters_DB();
                        updateParametersSetup();
                        s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                        m_panel = new PanelSourceData (SystemColors.ControlText, SystemColors.Control);
                        m_panel.SetDelegateReport (ErrorReport, WarningReport, ActionReport, ReportClear);
                        m_panel.Start ();

                        #region Добавить рабочую панель на форму
                        this._panelMain.SuspendLayout ();
                        _panelMain.Controls.Add (this.m_panel);
                        this._panelMain.ResumeLayout (false);
                        this._panelMain.PerformLayout ();
                        #endregion

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

        /// <summary>
        /// Инициализация параметров соединения с БД_конфигурации
        /// </summary>
        /// <param name="msgError">Сообщение об ошибке (при наличии)</param>
        /// <returns>Признак выполнения функции</returns>
        private int InitializeConfigDB(out string msgError)
        {
            int iRes = 0;
            msgError = string.Empty;
            //Идентификатор соединения с БД_конфигурации
            int idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            //Проверить наличие пользователя в БД_конфигурации
            try
            {
                //Создать И удалить объект с пользовательскими настройками (заполнить статические члены)
                using (HStatisticUsers users = new HStatisticUsers(idListenerConfigDB, HUsers.MODE_REGISTRATION.MIXED)) { ; }
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
                if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB) == false)
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
        /// 
        /// </summary>
        private void updateParametersSetup()
        {
            //Параметры записи сообщений лог-а...
            Logging.UpdateMarkDebugLog();

            //Параметры обновления "основной панели"...
            PanelStatistic.POOL_TIME = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);
            PanelStatistic.ERROR_DELAY = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]);

            //Параметры перехода на сезонное времяисчисление...
            HAdmin.SeasonDateTime = DateTime.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]);
            HAdmin.SeasonAction = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_ACTION]);

            //Параметры обработки запросов к БД...
            Constants.MAX_RETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAX_ATTEMPT]);
            Constants.MAX_WAIT_COUNT = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_COUNT]);
            Constants.WAIT_TIME_MS = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_TIME]);

            //Параметры валидности даты/времени получения данных СОТИАССО...
            TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

            //Параметрвы для ALARM...
        }

        /// <summary>
        /// Обработчик события выбора п. главного меню "Файл-Выход"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fMenuItemExit_Click(object obj, EventArgs ev)
        {
            Close();
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
            DialogResult dlgRes = s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].ShowDialog();

            if ((dlgRes == DialogResult.OK)
                || (dlgRes == DialogResult.Yes))
            {
                //??? Остановить панель
                m_panel.Stop();

                bAbort = initialize(out msg);
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

        protected override void UpdateActiveGui(int type)
        {
            throw new NotImplementedException();
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
    }
}
