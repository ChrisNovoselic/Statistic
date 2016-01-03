﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace StatisticAlarm
{
    public enum INDEX_ACTION { ERROR = -1, NOTHING, NEW, RETRY }
    /// <summary>
    /// Перечисление - индексы для типов сигнализаций
    /// </summary>
    public enum INDEX_TYPE_ALARM { UNKNOWN = -1, CUR_POWER = 1, TGTurnOnOff }
    /// <summary>
    /// Перечисление - индексы для состояний события сигнализации
    /// </summary>
    public enum INDEX_STATES_ALARM
    {
        /// <summary>
        /// Событие поставлено в очередь для оповещения
        /// </summary>
        REGISTRING = -1
        /// <summary>
        /// Событие зафиксировано
        /// </summary>
        , REGISTRED
        , FIXING
        , FIXED
        , CONFIRMING
        /// <summary>
        /// Событие подтверждено
        /// </summary>
        , CONFIRMED
    }
    /// <summary>
    /// Тип функции - для обработки события оповещения панели о событии сигнализаций (новое/повтор)
    /// </summary>
    /// <param name="ev">Аргумент события оповещения</param>
    public delegate void AlarmNotifyEventHandler (AlarmNotifyEventArgs ev);
    /// <summary>
    /// Класс для описания события оповещения
    /// </summary>
    public class AlarmNotifyEventArgs : EventArgs
    {
        public int m_id_comp;
        public DateTime? m_dtRegistred;        
        public int m_situation;
        public string m_message_shr;
        public string m_messageGUI;

        public StatisticCommon.FormChangeMode.MODE_TECCOMPONENT Mode { get { return StatisticCommon.TECComponent.Mode(m_id_comp); } } 
        /// <summary>
        /// Индекс типа сигнализации
        /// </summary>
        public INDEX_TYPE_ALARM type { get { return Mode == StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.GTP ? INDEX_TYPE_ALARM.CUR_POWER :
            Mode == StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.TG ? INDEX_TYPE_ALARM.TGTurnOnOff
                : INDEX_TYPE_ALARM.UNKNOWN; }
        }

        public AlarmNotifyEventArgs(int id_comp, DateTime dtReg, int situation)
            : base()
        {
            m_id_comp = id_comp;
            m_dtRegistred = dtReg;
            m_situation = situation;
            m_message_shr = GetMessage(id_comp, situation);
        }

        public static int GetSituation(string message)
        {
            int iRes = 0;

            switch (message)
            {
                case @"вверх":
                case @"вкл.":
                    iRes = 1;
                    break;
                case @"вниз":
                case @"выкл.":
                    iRes = -1;
                    break;
                default:
                    break;
            }

            return iRes;
        }

        public static string GetMessage(int id_comp, int situation)
        {
            string strRes = string.Empty;
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = StatisticCommon.TECComponent.Mode(id_comp);

            if (mode == StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.GTP)
                if (situation == 1)
                    strRes = @"вверх";
                else
                    if (situation == -1)
                        strRes = @"вниз";
                    else
                        strRes = @"нет";
            else
                if (mode == StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.TG)
                    if (situation == (int)StatisticCommon.TG.INDEX_TURNOnOff.ON) //TGTurnOnOff = ON
                        strRes = @"вкл.";
                    else
                        if (situation == (int)StatisticCommon.TG.INDEX_TURNOnOff.OFF) //TGTurnOnOff = OFF
                            strRes = @"выкл.";
                        else
                            strRes = @"нет";
                else
                    ; // неизвестный тип компонента

            return strRes;
        }
    }
    /// <summary>
    /// Класс для учета событий сигнализации и их состояний
    /// </summary>
    public class DictAlarmObject : object
    {
        private
            Dictionary<
                KeyValuePair<int, DateTime>
                //int
                    , ALARM_OBJECT> _dictAlarmObject;

        /// <summary>
        /// Класс объекта события сигнализации
        /// </summary>
        private class ALARM_OBJECT
        {
            public DateTime m_dtRegistred { get { return _dtRegistred.GetValueOrDefault(); } }

            public void Fixing()
            {
                _dtFixed = null;
                _state = INDEX_STATES_ALARM.FIXING;
            }

            public void Fixed(DateTime? dt)
            {
                _dtFixed = dt;
                _state = INDEX_STATES_ALARM.FIXED;
            }

            public bool FIXED
            {
                get
                {
                    return !(_dtFixed == null); //? true : false;
                }
            }

            public void Confirming ()
            {
                _dtConfirmed = null;
                _state = INDEX_STATES_ALARM.CONFIRMING;
            }

            public void Confirmed(DateTime? dt)
            {
                _dtConfirmed = dt;
                _state = INDEX_STATES_ALARM.CONFIRMED;
            }
            /// <summary>
            /// Признак подтверждения события сигнализации
            /// </summary>
            public bool CONFIRMED
            {
                get
                {
                    return ! (_dtConfirmed == null); //? true : false;
                }
            }

            public bool RETRY
            {
                get
                {
                    DateTime? dt;
                    if (_dtFixed == null)
                        dt = _dtRegistred;
                    else
                        dt = _dtFixed;

                    return (! (_state == INDEX_STATES_ALARM.FIXING)) && ((DateTime.UtcNow - dt) > TimeSpan.FromMilliseconds(AdminAlarm.MSEC_ALARM_TIMERUPDATE));
                }
            }
            
            public bool HISTORY
            {
                get
                {
                    return (CONFIRMED == true) && (! ((DateTime.UtcNow - _dtConfirmed) < TimeSpan.FromMilliseconds(AdminAlarm.MSEC_ALARM_EVENTRETRY)));
                }
            }
            /// <summary>
            /// Дата/время регистрации
            /// </summary>
            private DateTime? _dtRegistred
                , _dtFixed
                /// <summary>
                /// Дата/время подтверждения
                /// </summary>
                , _dtConfirmed;
            /// <summary>
            /// Текущее состояние события сигнализации
            /// </summary>
            private INDEX_STATES_ALARM _state;
            //public INDEX_STATES_ALARM State { get { return _state; } }

            public void New(TecViewAlarm.AlarmTecViewEventArgs ev)
            {
                init (ev.m_dtRegistred.GetValueOrDefault());
                // если состояние "в очереди", то изменить состояние на "обрабатывается"
                _state = INDEX_STATES_ALARM.REGISTRING;
            }

            private void init(DateTime dt)
            {
                _dtRegistred = dt;
                _dtConfirmed =
                _dtFixed =
                    null;
            }

            private ALARM_OBJECT(INDEX_STATES_ALARM state) { _state = state; }

            public ALARM_OBJECT(TecViewAlarm.AlarmTecViewEventArgs ev) : this (INDEX_STATES_ALARM.REGISTRING)
            {
                init(ev.m_dtRegistred.GetValueOrDefault());
            }

            public ALARM_OBJECT(AdminAlarm.AlarmDbEventArgs ev)
                : this(INDEX_STATES_ALARM.REGISTRED)
            {
                init(ev.m_dtRegistred.GetValueOrDefault());
            }
        }
        /// <summary>
        /// Конструктор основной (без параметров)
        /// </summary>
        public DictAlarmObject()
        {
            _dictAlarmObject =
                new Dictionary<
                    KeyValuePair<int, DateTime>
                    //int
                        , ALARM_OBJECT>();
        }
        /// <summary>
        /// Найти объект сигнализации по указанным частям составного ключа
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ТГ</param>
        /// <returns>Объект сигнализации</returns>
        private ALARM_OBJECT find(int id_comp, DateTime dtReg)
        {
            ALARM_OBJECT objRes = null;

            KeyValuePair<int, DateTime> cKey = new KeyValuePair<int, DateTime>(id_comp, dtReg);
            //int cKey = id_comp;
            if (_dictAlarmObject.ContainsKey(cKey) == true)
            {
                objRes = _dictAlarmObject[cKey];
            }
            else
                Logging.Logg().Error(@"DictAlarmObject::find (id_comp=" + id_comp.ToString() + @") - НЕ НАЙДЕН!", Logging.INDEX_MESSAGE.NOT_SET);

            return objRes;
        }

        private ALARM_OBJECT find(int id_comp)
        {
            ALARM_OBJECT objRes = null;
            DateTime? partKey;
            List <DateTime> listPartKeys = new List<DateTime> ();

            foreach (KeyValuePair <int, DateTime> cKey in _dictAlarmObject.Keys)
                if (cKey.Key.Equals(id_comp) == true)
                    listPartKeys.Add(cKey.Value);
                else
                    ;

            listPartKeys.Sort ();
            partKey = listPartKeys.LastOrDefault ();

            if (! (partKey == null))
                objRes = _dictAlarmObject [new KeyValuePair<int,DateTime> (id_comp, partKey.GetValueOrDefault())];

            return objRes;
        }
        /// <summary>
        /// Получить дату/время регистрации события сигнализации для ТГ
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ТГ</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, DateTime dtReg)
        {
            DateTime dtRes = DateTime.Now;

            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
                dtRes = objAlarm.m_dtRegistred;
            else
                ;

            return dtRes;
        }
        ///// <summary>
        ///// Установить состояние
        ///// </summary>
        ///// <param name="id_gtp">Составная часть ключа: идентификатор ГТП</param>
        ///// <param name="id_tg">Составная часть ключа: идентификатор ТГ</param>
        ///// <param name="state">Новое состояние</param>
        ///// <returns></returns>
        //public int SetState (int id_gtp, int id_tg, INDEX_STATES_ALARM state)
        //{
        //    int iRes = -1;

        //    ALARM_OBJECT objAlarm = find(id_gtp, id_tg);
        //    if (!(objAlarm == null))
        //    {
        //        switch (state)
        //        {
        //            case INDEX_STATES_ALARM.PROCESSED:
        //                break;
        //            case INDEX_STATES_ALARM.CONFIRMED:
        //                objAlarm.dtConfirm = DateTime.Now;
        //                break;
        //            default:
        //                break;
        //        }

        //        objAlarm.state = state;

        //        iRes = 0;
        //    }
        //    else
        //        ;

        //    return iRes;
        //}
        /// <summary>
        /// Установить признак - событие сигнализаций "ожидание фиксации"
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификтор компонента ТЭЦ (ГТП, ТГ)</param>
        /// <param name="dtReg">Часть составного ключа: дата/время регистрации события сигнализаций</param>
        /// <returns>Результат выполнения операции</returns>
        public int Fixing(int id_comp, DateTime dtReg)
        {
            int iRes = -1;

            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
            {
                objAlarm.Fixing ();

                iRes = 0;
            }
            else
                ;

            return iRes;
        }

        public int Fixed(int id_comp, DateTime dtReg, DateTime dtFixed)
        {
            int iRes = -1;

            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
            {
                objAlarm.Fixed(dtFixed);

                iRes = 0;
            }
            else
                ;

            return iRes;
        }

        public int Confirmed(int id_comp, DateTime dtReg, DateTime dtConfirmed)
        {
            int iRes = -1;

            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
            {
                objAlarm.Confirmed(dtConfirmed);

                iRes = 0;
            }
            else
                ;

            return iRes;
        }
        /// <summary>
        /// Установить признак - событие сигнализаций "ожидается подтверждение"
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификтор компонента ТЭЦ (ГТП, ТГ)</param>
        /// <param name="dtReg">Часть составного ключа: дата/время регистрации события сигнализаций</param>
        /// <returns>Результат выполнения операции</returns>
        public int Confirming(int id_comp, DateTime dtReg)
        {
            int iRes = -1;

            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
            {
                objAlarm.Confirming ();

                iRes = 0;
            }
            else
                ;

            return iRes;
        }

        public bool IsConfirmed(int id_comp, DateTime dtReg)
        {
            bool bRes = false;
            ALARM_OBJECT objAlarm = find(id_comp, dtReg);
            if (!(objAlarm == null))
                bRes = objAlarm.CONFIRMED;
            else
                bRes = false;

            return bRes;
        }
        /// <summary>
        /// Зарегистрировать событие в режиме "выполнение_приложения"
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        /// <returns>Результат регистрации (-1 - ошибка, 0 - ничего_не_делать, 1 - новый_объект, 2 - повторное_событие)</returns>
        public INDEX_ACTION Registred(TecViewAlarm.AlarmTecViewEventArgs ev)
        {
            INDEX_ACTION iRes = INDEX_ACTION.NOTHING;
            ALARM_OBJECT alarmObj = null;

            lock (this)
            {
                try
                {
                    alarmObj =
                        //find(ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault())
                        find(ev.m_id_comp)
                        ;

                    if (alarmObj == null)
                    {//Только, если объект события сигнализации НЕ создан
                        // создать объект события сигнализации
                        alarmObj = new ALARM_OBJECT(ev);
                        _dictAlarmObject.Add(new KeyValuePair<int, DateTime> (ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault ()), alarmObj);
                        //Сообщить для сохранения в БД
                        iRes = INDEX_ACTION.NEW;
                    }
                    else
                        //Только, если объект события сигнализации создан
                        //Проверить состояние
                        if (alarmObj.CONFIRMED == true)
                            // если подтверждено - проверить период между датой/временем регистрации события сигнализации и датой/временем его подтверждения
                            if (alarmObj.HISTORY == true)
                            {
                                // ??? если 
                                alarmObj.New (ev);
                                // повторить оповещение пользователя о событии сигнализации
                                iRes = INDEX_ACTION.NEW;
                            }
                            else
                                ;
                        else
                            ;
                }
                catch (Exception e)
                {
                    iRes = INDEX_ACTION.ERROR;
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"DictAlarmObject::Registred (" + ev.GetType().Name + @") - ...");
                }
            }

            return iRes;
        }

        private bool isNotify (ALARM_OBJECT obj)
        {
            bool bRes = false;

            if (obj.CONFIRMED == false)
                if (obj.RETRY == true)
                {
                    obj.Fixing();
                    bRes = true;
                }
                else
                    ; // событие не требует оповещения
            else
                ; // событие подтверждено

            return bRes;
        }
        /// <summary>
        /// Зарегистрировать событие от БД
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        /// <returns>Результат регистрации (см. пред. обработчик для 'TecViewAlarm.AlarmTecViewEventArgs')</returns>
        public INDEX_ACTION Registred(AdminAlarm.AlarmDbEventArgs ev)
        {
            INDEX_ACTION iRes = INDEX_ACTION.NOTHING;
            ALARM_OBJECT alarmObj = find(ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault ());

            lock (this)
            {
                try
                {
                    if (alarmObj == null)
                    {//Только, если объект события сигнализации НЕ создан
                        // создать объект события сигнализации
                        alarmObj = new ALARM_OBJECT(ev);
                        _dictAlarmObject.Add(new KeyValuePair<int, DateTime>(ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault()), alarmObj);
                        alarmObj.Fixed (ev.m_dtFixed);
                        alarmObj.Confirmed(ev.m_dtConfirm);

                        if (isNotify (alarmObj) == true)
                            iRes = INDEX_ACTION.NEW;
                        else
                            ;
                    }
                    else
                    {
                        if (isNotify (alarmObj) == true)
                            iRes = INDEX_ACTION.RETRY;
                        else
                            ;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"DictAlarmObject::Registred (" + ev.GetType().Name + @") - ...");
                }
            }

            return iRes;
        }
    }
}