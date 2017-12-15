﻿using System.Windows.Forms; //TableLayoutPanel, DockStyle
using System;
using System.Globalization; //DaylightTime
using System.Data; //DataTable
using System.Threading;
//using HClassLibrary; //HHandlerDb
using ASUTP.Core;
using ASUTP.Database;
using ASUTP;

namespace StatisticTimeSync
{    
    partial class PanelSourceData
    {
        /// <summary>
        /// Класс для работы с БД
        /// </summary>
        class HGetDate : ASUTP.Helper.HHandlerDb {
            ConnectionSettings m_ConnSett;
            DateTime m_serverTime;
            DelegateDateFunc delegateGetDate;
            DelegateFunc delegateError;

            protected enum StatesMachine
            {
                CurrentTime,
            }

            public HGetDate(ConnectionSettings connSett, DelegateDateFunc fGetDate, DelegateFunc fError)
            {
                m_ConnSett = connSett;
                delegateGetDate = fGetDate;
                delegateError = fError;
            }

            public override void StartDbInterfaces()
            {
                m_dictIdListeners.Add(0, new int[] { -1 });

                register(0, 0, m_ConnSett, m_ConnSett.name);
            }

            public override void ClearValues()
            {
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine)
                {
                    case StatesMachine.CurrentTime:
                        getDate();
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            protected override int StateCheckResponse(int state, out bool error, out object table)
            {
                int iRes = 0;
                error = true;
                table = null;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine)
                {
                    case StatesMachine.CurrentTime:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            protected override int StateResponse(int state, object table)
            {
                int iRes = 0;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine)
                {
                    case StatesMachine.CurrentTime:
                        iRes = GetCurrentTimeResponse(table as DataTable);
                        break;
                    default:
                        break;
                }

                return 0;
            }

            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int request, int result)
            {
                INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;
                
                string error = string.Empty,
                    reason = string.Empty,
                    waiting = string.Empty;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine)
                {
                    case StatesMachine.CurrentTime:
                        if (request == 0)
                        {
                            reason = @"разбора";
                        }
                        else
                        {
                            reason = @"получения";
                        }

                        reason += @" текущего времени сервера";
                        waiting = @"Переход в ожидание";
                        break;
                    default:
                        break;
                }

                error = "Ошибка " + reason + ".";

                if (waiting.Equals(string.Empty) == false)
                    error += " " + waiting + ".";
                else
                    ;

                ErrorReport(error);

                //if (! (errorData == null)) errorData (); else ;

                Logging.Logg().Error(@"HGetDate::StateErrors () - error=" + error + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

                return reasonRes;
            }

            protected override void StateWarnings(int /*StatesMachine*/ state, int request, int result)
            {
            }

            protected void getDate()
            {
                DbInterface.DB_TSQL_INTERFACE_TYPE type = DbInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
                switch (m_ConnSett.port)
                {
                    case 1433:
                    default:
                        type = DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL;
                        break;
                }

                m_IdListenerCurrent = m_dictIdListeners[0][0];
                GetCurrentTimeRequest(type, m_IdListenerCurrent);
            }

            public void GetDate()
            {
                lock (m_lockState)
                {
                    ClearStates();

                    Logging.Logg().Debug("HGetDate::GetCurrentTime () - states.Clear()", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);

                    Run(@"HGetDate::GetCurrentTime ()");
                }
            }

            protected int GetCurrentTimeResponse(DataTable table)
            {
                if (table.Rows.Count == 1)
                {
                    m_serverTime = (DateTime)table.Rows[0][0];
                }
                else
                {
                    m_serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);

                    ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
                }

                if (!(delegateGetDate == null))
                    delegateGetDate(m_serverTime);
                else
                    ;

                return 0;
            }
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly (cols, rows);
        }

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

        #region Код, автоматически созданный конструктором компонентов

        //PanelGetDate[] m_arPanels;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте;;
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            
            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            initializeLayoutStyle (3, 7);

            this.ResumeLayout();
        }

        #endregion
        
    }
}
