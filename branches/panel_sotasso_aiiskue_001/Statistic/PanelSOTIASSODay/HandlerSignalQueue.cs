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
        private partial class HandlerSignalQueue : HHandlerQueue
        {
            public enum EVENT { SET_TEC, LIST_SIGNAL, VALUES }
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

            public Dictionary<CONN_SETT_TYPE, IList<SIGNAL>> Signals { get { return _handlerDb.Signals; } }

            public Dictionary<CONN_SETT_TYPE, VALUES> Values { get { return _handlerDb.Values; } }

            public HandlerSignalQueue(int iListenerConfigDbId, IEnumerable<TEC> listTEC)
                : base ()
            {
                _handlerDb = new HandlerDbSignalValue(iListenerConfigDbId, listTEC);
                _handlerDb.UpdateGUI_Fact += new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);
            }
            /// <summary>
            /// Переопределение наследуемой функции - запуск объекта
            /// </summary>
            public override void Start()
            {
                base.Start();

                _handlerDb.Start();
            }
            /// <summary>
            /// Переопределение наследуемой функции - останов объекта
            /// </summary>
            public override void Stop()
            {
                //Проверить актуальность объекта обработки запросов
                if (!(_handlerDb == null)) {
                    if (_handlerDb.Actived == true)
                        //Если активен - деактивировать
                        _handlerDb.Activate(false);
                    else
                        ;

                    if (_handlerDb.IsStarted == true)
                        //Если выполняется - остановить
                        _handlerDb.Stop();
                    else
                        ;

                    //m_tecView = null;
                } else
                    ;

                //Остановить базовый объект
                base.Stop();
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

            public void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
            {
                _handlerDb.SetDelegateReport(ferr, fwar,  fact, fclr);
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