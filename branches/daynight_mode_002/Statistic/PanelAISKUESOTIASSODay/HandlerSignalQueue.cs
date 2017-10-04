﻿using System;
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
    partial class PanelAISKUESOTIASSODay
    {
        private partial class HandlerSignalQueue : HHandlerQueue
        {
            public enum EVENT { /*SET_TEC,*/ LIST_SIGNAL = 11, CUR_VALUES, CHECK_VALUES }
            ///// <summary>
            ///// Делегат по окончанию обработки очередного обработанного из очереди события
            /////  (наименование по традиции из HHandlerDB)
            ///// </summary>
            //public Action<CONN_SETT_TYPE, EVENT> UpdateGUI_Fact;
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

                public static VALUES Copy(VALUES src)
                {
                    return new VALUES() { serverTime = src.serverTime, m_valuesHours = new List<VALUE>(src.m_valuesHours) };
                }
            }

            public struct USER_DATE
            {
                public int UTC_OFFSET;

                public DateTime Value;
            }

            public USER_DATE UserDate { get { return UserDate; } set { _handlerDb.UserDate = value; } }

            private HandlerDbSignalValue _handlerDb;

            public Dictionary<CONN_SETT_TYPE, IList<SIGNAL>> Signals;

            public Dictionary<CONN_SETT_TYPE, VALUES> Values;

            public Action<DelegateStringFunc, DelegateStringFunc, DelegateStringFunc, DelegateBoolFunc> SetDelegateReport;
            public Action<bool> ReportClear;

            public HandlerSignalQueue(int iListenerConfigDbId, IEnumerable<TEC> listTEC)
                : base ()
            {
                Signals = new Dictionary<CONN_SETT_TYPE, IList<SIGNAL>>();
                Values = new Dictionary<CONN_SETT_TYPE, VALUES>();

                _handlerDb = new HandlerDbSignalValue(iListenerConfigDbId, listTEC, _types);                
                SetDelegateReport += new Action<DelegateStringFunc, DelegateStringFunc, DelegateStringFunc, DelegateBoolFunc>(_handlerDb.SetDelegateReport);
                ReportClear += new Action<bool>(_handlerDb.ReportClear);
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

            protected override int StateCheckResponse(int state, out bool error, out object outobj)
            {
                int iRes = 0;

                error = false;
                outobj = null;

                HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE indxSync = HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE.UNKNOWN;
                
                indxSync = (HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE)WaitHandle.WaitAny(_handlerDb.m_arSyncStateCheckResponse);
                switch (indxSync) {
                    case HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE.RESPONSE:
                        switch ((EVENT)state) {
                            case EVENT.LIST_SIGNAL:
                                outobj = _handlerDb.Signals;
                                break;
                            case EVENT.CUR_VALUES:
                            case EVENT.CHECK_VALUES:
                                outobj = _handlerDb.Values;
                                break;
                            default:
                                iRes = -2;
                                break;
                        }
                        break;
                    case HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE.ERROR:
                        iRes = -1;
                        break;
                    case HandlerDbSignalValue.INDEX_SYNC_STATECHECKRESPONSE.WARNING:
                        iRes = 1;
                        break;
                    default:
                        iRes = -3;
                        break;
                }                

                error = !(iRes == 0);

                return iRes;
            }

            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                throw new NotImplementedException();
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;
                ItemQueue itemQueue = Peek;

                CONN_SETT_TYPE type = CONN_SETT_TYPE.UNKNOWN;

                switch ((EVENT)state) {
                    case EVENT.LIST_SIGNAL:
                        type = (CONN_SETT_TYPE)itemQueue.Pars[1];

                        _handlerDb.GetListSignals((int)itemQueue.Pars[0], type);
                        break;
                    case EVENT.CUR_VALUES:
                    case EVENT.CHECK_VALUES:
                        type = (CONN_SETT_TYPE)itemQueue.Pars[0];

                        _handlerDb.Request(type, Signals[type].ElementAt((int)itemQueue.Pars[1]).kks_code);
                        break;
                    default:
                        Logging.Logg().Error(string.Format(@"HandlerSignalQueue::StateRequest (CONN_SETT_TYPE={0}, event={1}) - необработанное событие...", type, (EVENT)state), Logging.INDEX_MESSAGE.NOT_SET);

                        iRes = -1;
                        break;
                }

                return iRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                int iRes = 0;
                ItemQueue itemQueue = Peek;

                CONN_SETT_TYPE type = CONN_SETT_TYPE.UNKNOWN;
                EventArgsDataHost arg;

                switch ((EVENT)state) {
                    case EVENT.LIST_SIGNAL:
                        type = (CONN_SETT_TYPE)itemQueue.Pars[1];

                        if (Signals.ContainsKey(type) == false)
                            Signals.Add(type, new List<SIGNAL>(obj as IList<SIGNAL>));
                        else
                            Signals[type] = new List<SIGNAL>(obj as IList<SIGNAL>);

                        arg = new EventArgsDataHost(null, new object[] { (EVENT)state, type });
                        break;
                    case EVENT.CUR_VALUES:
                    case EVENT.CHECK_VALUES:
                        type = (CONN_SETT_TYPE)itemQueue.Pars[0];

                        if (Values.ContainsKey(type) == false)
                            Values.Add(type, VALUES.Copy(obj as VALUES));
                        else
                            Values[type] = VALUES.Copy(obj as VALUES);
                        // дополнительно передать ккс-код (для поиска столбца)
                        arg = new EventArgsDataHost(null, new object[] { (EVENT)state, type, Signals[type].ElementAt((int)itemQueue.Pars[1]).kks_code });
                        break;
                    default:
                        arg = new EventArgsDataHost(null, new object[] { });
                        break;
                }

                itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(arg);

                return iRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }
    }
}