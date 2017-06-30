using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;

using HClassLibrary;
using StatisticCommon;
using System.Linq;
using System.Data.Common;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    partial class PanelSOTIASSODay
    {
        private class HandlerSignalQueue : HHandlerQueue
        {
            /// <summary>
            /// Структура для хранения значения
            /// </summary>
            public struct VALUE
            {
                /// <summary>
                /// Метка времени значения
                /// </summary>
                public DateTime stamp;
                /// <summary>
                /// Индекс 30-ти мин интервала
                /// </summary>
                public int index_stamp;
                /// <summary>
                /// Значение
                /// </summary>
                public float value;
                /// <summary>
                /// Статус или качество/достоверность значения
                /// </summary>
                public short quality;
            }

            public /*struct*/ class VALUES
            {
                public DateTime serverTime;

                public IList<VALUE> m_valuesHours;

                //public void SetServerTime(DateTime serverTime)
                //{
                //    this.serverTime = serverTime;
                //}
            }

            public struct USER_DATE
            {
                public int UTC_OFFSET;

                public DateTime Value;
            }

            public USER_DATE UserDate { get { return UserDate; } set { _handlerDb.UserDate = value; } }

            private HandlerDbSignalValue _handlerDb;

            public HandlerSignalQueue(int iListenerConfigDbId, IEnumerable<TEC> listTEC)
                : base ()
            {
                _handlerDb = new HandlerDbSignalValue(iListenerConfigDbId, listTEC);
                _handlerDb.UpdateGUI_Fact += new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);
            }
            /// <summary>
            /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
            /// </summary>
            /// <param name="hour">Номер часа в запросе</param>
            /// <param name="min">Номер минуты в звпросе</param>
            /// <returns>Признак результата выполнения функции</returns>
            private int onEvtHandlerStatesCompleted(int conn_sett_type, int state_machine)
            {
                int iRes = ((!((CONN_SETT_TYPE)conn_sett_type == CONN_SETT_TYPE.UNKNOWN)) // тип источника данных известен
                    && (!(state_machine < 0))) // кол-во строк "не меньше 0"
                    ? 0
                        : -1;

                ///*IAsyncResult iar = Begin*/
                //Invoke(new Action<CONN_SETT_TYPE, int>(onStatesCompleted), (CONN_SETT_TYPE)conn_sett_type, state_machine);

                return iRes;
            }

            protected override int StateCheckResponse(int state, out bool error, out object outobj)
            {
                throw new NotImplementedException();
            }

            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                throw new NotImplementedException();
            }

            protected override int StateRequest(int state)
            {
                throw new NotImplementedException();
            }

            protected override int StateResponse(int state, object obj)
            {
                throw new NotImplementedException();
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }
    }
}