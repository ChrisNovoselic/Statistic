﻿using System.Windows.Forms; //TableLayoutPanel
using System;

using System.Globalization; //DaylightTime

using System.Data; //DataTable

using HClassLibrary; //HStates

namespace StatisticTimeSync
{
    partial class PanelSourceData
    {
        class HGetDate : HStates
        {
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

                register(0, m_ConnSett, m_ConnSett.name, 0);
            }

            public override void ClearValues()
            {
            }

            protected override bool StateRequest(int state)
            {
                bool bRes = true;

                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        getDate();
                        break;
                    default:
                        break;
                }

                return bRes;
            }

            protected override bool StateCheckResponse(int state, out bool error, out DataTable table)
            {
                bool bRes = true;
                error = true;
                table = null;

                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        bRes = Response(m_IdListenerCurrent, out error, out table);
                        break;
                    default:
                        break;
                }

                return bRes;
            }

            protected override bool StateResponse(int state, DataTable table)
            {
                bool bRes = true;

                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        bRes = GetCurrentTimeResponse(table);
                        break;
                    default:
                        break;
                }

                return bRes;
            }

            protected override void StateErrors(int state, bool response)
            {
                string error = string.Empty,
                    reason = string.Empty,
                    waiting = string.Empty;

                switch (state)
                {
                    case (int)StatesMachine.CurrentTime:
                        if (response == true)
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

                Logging.Logg().Error(@"HGetDate::StateErrors () - error=" + error + @" - вЫход...");
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

                    Logging.Logg().Debug("HGetDate::GetCurrentTime () - states.Clear()");

                    states.Add((int)StatesMachine.CurrentTime);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "HGetDate::GetCurrentTime () - semaState.Release(1)");
                    }
                }
            }

            protected bool GetCurrentTimeResponse(DataTable table)
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

                return true;
            }
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

        PanelGetDate[] m_arPanels;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            int i = -1;

            m_arPanels = new PanelGetDate[INDEX_SOURCE_GETDATE.Length];
            for (i = 0; i < m_arPanels.Length; i++)
                m_arPanels [i] = new PanelGetDate ();

            //Только для панели с эталонным серверои БД
            m_arPanels[0].DelegateEtalonGetDate = new HClassLibrary.DelegateDateFunc(recievedEtalonDate);
            //Для панелей с любыми серверами БД
            for (i = 0; i < m_arPanels.Length; i++) {
                EvtGetDate += new HClassLibrary.DelegateObjectFunc(m_arPanels[i].OnEvtGetDate);
                EvtEtalonDate += new HClassLibrary.DelegateDateFunc(m_arPanels[i].OnEvtEtalonDate);
            }

            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.ColumnCount = 3; this.RowCount = 7;
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100 / this.ColumnCount));
            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100 / this.RowCount));

            this.Controls.Add(m_arPanels[0], 0, 0);

            int indx = -1
                , col = -1
                , row = -1;
            for (i = 1; i < m_arPanels.Length; i++) {
                indx = i;
                //if (! (indx < this.RowCount))
                    indx += (int)(indx / this.RowCount);
                //else ;

                col = (int)(indx / this.RowCount);
                row = indx % (this.RowCount - 0);
                if (row == 0) row = 1; else ;
                this.Controls.Add(m_arPanels[i], col, row); 
            }

            this.ResumeLayout();
        }

        #endregion
    }
}