﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
//using HClassLibrary;
using ASUTP.Database;
using ASUTP;
using ASUTP.Core;

namespace StatisticDiagnostic
{
    public partial class FormMain : FormMainStatistic
    {
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
            : base (ID_APPLICATION.DIAGNOSTIC)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            //this.notifyIconMain.Icon =
            this.Icon = resources.GetObject(@"StatisticDiagnostic") as System.Drawing.Icon;

            _state = 1;
            InitializeComponent();
        }

        public void FormDiagnostic_Load(object obj, EventArgs ev)
        {
            //Отобразить окно для визуализации выполнения длительной операции
            delegateStartWait();

            string msg = string.Empty;
            bool bAbort = true;
            CreatefileConnSett(@"connsett.ini");
            Start();
            bAbort = initialize(out msg);
            this.m_panel = new PanelStatisticDiagnostic(PanelStatisticDiagnostic.Mode.DEFAULT, SystemColors.ControlText, SystemColors.Control);
            this.m_panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            #region Добавить рабочую панель на форму
            this._panelMain.SuspendLayout ();
            _panelMain.Controls.Add (this.m_panel);
            this._panelMain.ResumeLayout (false);
            this._panelMain.PerformLayout ();
            #endregion
            this.m_panel.Start();

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
        /// Обработчик события - активация главного окна приложения
        /// </summary>
        /// <param name="obj">Инициатор события - окно</param>
        /// <param name="ev">Аргумент события</param>
        private void FormDiagnostic_Activate(object obj, EventArgs ev)
        {
            if (_state == 0)
                m_panel.Activate(true);
            else
                ;
        }

        /// <summary>
        /// Обработчик события - деактивация главного окна приложения
        /// </summary>
        /// <param name="obj">Инициатор события - окно</param>
        /// <param name="ev">Аргумент события</param>
        private void FormDiagnostic_Deactivate(object obj, EventArgs ev)
        {
            m_panel.Activate(false);
        }

        /// <summary>
        /// Инициализация панели
        /// </summary>
        public bool initialize(out string msgError)
        {
            bool bRes = true;
            msgError = string.Empty;
            //handlerCmd(Environment.GetCommandLineArgs());

            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                //!!! Один экземпляр для всего приложения на весь срок выполнения
                new DbTSQLConfigDatabase (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ());
                DbTSQLConfigDatabase.DbConfig ().Register ();

                _state = validateUser(out msgError);
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
                        formParameters = new FormParameters_DB();
                        updateParametersSetup();
                        s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);
                        break;
                }

                DbTSQLConfigDatabase.DbConfig ().UnRegister ();
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
        private int validateUser(out string msgError)
        {
            int iRes = 0;
            msgError = string.Empty;

            //Проверить наличие пользователя в БД_конфигурации
            try
            {
                //Создать И удалить объект с пользовательскими настройками (заполнить статические члены)
                using (HStatisticUsers users = new HStatisticUsers(DbTSQLConfigDatabase.DbConfig().ListenerId, ASUTP.Helper.HUsers.MODE_REGISTRATION.MIXED)) { ; }
            }
            catch (Exception e)
            {
                if (e is ASUTP.Helper.HException)
                    iRes = ((ASUTP.Helper.HException)e).m_code; //-2, -3, -4
                else
                    iRes = -1; // общая (неизвестная) ошибка

                msgError = e.Message;
            }

            if (iRes == 0)
                if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_DIAGNOSTIC) == false)
                {
                    msgError = @"Пользователю не разрешено использовать задачу";
                    iRes = -6;
                }
                else
                    //Успех...
                    ;
            else
                ;

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

            PanelStatisticDiagnostic.UPDATE_TIME = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.DIAGNOSTIC_TIMER_UPDATE]);
            PanelStatisticDiagnostic.VALIDATE_ASKUE_TM = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_ASKUE_VALUE]);

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
                m_panel?.Stop();

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

        protected override int UpdateStatusString()
        {
            int have_msg = 0;

            have_msg = base.UpdateStatusString();

            return have_msg;
        }

        /* protected override void timer_Start()
         {
             //int i = -1;
         }*/
    }
}
