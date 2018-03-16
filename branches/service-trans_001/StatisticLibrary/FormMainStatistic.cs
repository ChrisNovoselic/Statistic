﻿using ASUTP.Database;
using ASUTP.Forms;
using ASUTP.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace StatisticCommon
{
    /// <summary>
    /// Класс общей формы для всех приложений в решении 'Statistic'
    /// </summary>
    public partial class FormMainStatistic : FormMainBaseWithStatusStrip
    {
        protected static FileINI m_sFileINI; //setup.ini
        protected static FIleConnSett m_sFileCS; //connsett.ini

        //TODO: а где описание остальных ошибок
        public enum ID_ERROR_INIT { UNKNOWN = -1, }
        public enum INDEX_ERROR_INIT { UNKNOWN = 0, }
        public static string[] MSG_ERROR_INIT = { @"Неизвестная причина" };

        /// <summary>
        /// вызов класса работы с командной строкой
        /// </summary>
        /// <param name="args">параметры командной строки</param>
        /// <returns>класс</returns>
        protected virtual HCmd_Arg createHCmdArg(string[] args)
        {
            return new HCmd_Arg(args);
        }

        /// <summary>
        /// Контруктор класса
        /// </summary>
        public FormMainStatistic()
        {
            //string str = Environment.CommandLine;
            createHCmdArg(Environment.GetCommandLineArgs());
        }        

        /// <summary>
        /// Обновить содержание строки состояния
        /// </summary>
        /// <returns>Признак необходимости удержания сообщения в строке статуса</returns>
        protected override int UpdateStatusString()
        {
            int have_msg = 0;

            have_msg = (m_report.errored_state == true) ? -1 : (m_report.warninged_state == true) ? 1 : 0;

            if (((!(have_msg == 0)) || (m_report.actioned_state == true)))
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

            return have_msg;
        }

        /// <summary>
        /// создание ConnSett
        /// </summary>
        /// <param name="connSettFileName">connsett.ini</param>
        public void CreatefileConnSett(string connSettFileName)
        {
            m_sFileCS = new FIleConnSett(connSettFileName, FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, m_sFileCS.ReadSettingsFile, m_sFileCS.SaveSettingsFile));
        }

        protected override void UpdateActiveGui (int type)
        {
            throw new NotImplementedException ();
        }

        protected override void HideGraphicsSettings ()
        {
            throw new NotImplementedException ();
        }

        protected override void timer_Start ()
        {
            //??? ничего не делать
        }
    }
}
