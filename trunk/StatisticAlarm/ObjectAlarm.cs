using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace StatisticAlarm
{
    public enum INDEX_ACTION { ERROR = -1, NOTHING, ADD, RETRY }
    /// <summary>
    /// Перечисление - индексы для типов сигнализаций
    /// </summary>
    public enum INDEX_TYPE_ALARM { CUR_POWER, TGTurnOnOff }
    /// <summary>
    /// Перечисление - индексы для состояний события сигнализации
    /// </summary>
    public enum INDEX_STATES_ALARM
    {
        /// <summary>
        /// Событие поставлено в очередь для оповещения
        /// </summary>
        QUEUEDED = -1
        /// <summary>
        /// Событие зафиксировано
        /// </summary>
        , PROCESSED
        /// <summary>
        /// Событие подтверждено
        /// </summary>
        , CONFIRMED
    }
    /// <summary>
    /// Класс для учета событий сигнализации и их состояний
    /// </summary>
    public class DictAlarmObject : object
    {
        private Dictionary<KeyValuePair<int, int>, ALARM_OBJECT> _dictAlarmObject;

        /// <summary>
        /// Класс объекта события сигнализации
        /// </summary>
        private class ALARM_OBJECT
        {
            /// <summary>
            /// Индекс типа сигнализации
            /// </summary>
            public INDEX_TYPE_ALARM type; //{ get { return id_tg > 0 ? INDEX_TYPE_ALARM.CUR_POWER : INDEX_TYPE_ALARM.TGTurnOnOff; } }
            /// <summary>
            /// Признак подтверждения события сигнализации
            /// </summary>
            public bool CONFIRM
            {
                get
                {
                    return dtConfirm.CompareTo(dtReg) > 0; //? true : false;
                }
            }

            public AdminAlarm.EventRegEventArgs m_evReg;
            /// <summary>
            /// Дата/время регистрации
            /// </summary>
            public DateTime dtReg
                /// <summary>
                /// Дата/время подтверждения
                /// </summary>
                , dtConfirm;
            /// <summary>
            /// Текущее состояние события сигнализации
            /// </summary>
            public INDEX_STATES_ALARM state;

            public ALARM_OBJECT(AdminAlarm.EventRegEventArgs ev) { m_evReg = ev; dtReg = dtConfirm = DateTime.Now; state = INDEX_STATES_ALARM.QUEUEDED; }
        }
        /// <summary>
        /// Конструктор основной (без параметров)
        /// </summary>
        public DictAlarmObject()
        {
            _dictAlarmObject = new Dictionary<KeyValuePair<int, int>, ALARM_OBJECT>();
        }
        /// <summary>
        /// Найти объект сигнализации по указанным частям составного ключа
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ТГ</param>
        /// <returns>Объект сигнализации</returns>
        private ALARM_OBJECT find(int id_comp, int id_tg)
        {
            ALARM_OBJECT objRes = null;

            KeyValuePair<int, int> cKey = new KeyValuePair<int, int>(id_comp, id_tg);
            if (_dictAlarmObject.ContainsKey(cKey) == true)
            {
                objRes = _dictAlarmObject[cKey];
            }
            else
                Logging.Logg().Error(@"DictAlarmObject::find (id=" + id_comp.ToString() + @", id_tg=" + id_tg.ToString() + @") - НЕ НАЙДЕН!", Logging.INDEX_MESSAGE.NOT_SET);

            return objRes;
        }
        /// <summary>
        /// Получить дату/время регистрации события сигнализации для ТГ
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ТГ</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, int id_tg)
        {
            DateTime dtRes = DateTime.Now;

            ALARM_OBJECT objAlarm = find(id_comp, id_tg);
            if (!(objAlarm == null))
            {
                dtRes = objAlarm.dtReg;
            }
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

        public int Confirmed(int id_gtp, int id_tg)
        {
            int iRes = -1;

            ALARM_OBJECT objAlarm = find(id_gtp, id_tg);
            if (!(objAlarm == null))
            {
                objAlarm.dtConfirm = DateTime.Now;
                objAlarm.state = INDEX_STATES_ALARM.CONFIRMED;

                iRes = 0;
            }
            else
                ;

            return iRes;
        }

        public bool IsConfirmed(int id_gtp, int id_tg)
        {
            bool bRes = false;
            ALARM_OBJECT objAlarm = find(id_gtp, id_tg);
            if (!(objAlarm == null))
                bRes = objAlarm.CONFIRM;
            else
                bRes = false;

            return bRes;
        }
        /// <summary>
        /// Зарегистрировать событие
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        /// <returns>Результат регистрации (-1 - ошибка, 0 - ничего_не_делать, 1 - новый_объект, 2 - повторное_событие)</returns>
        public INDEX_ACTION Registred(AdminAlarm.EventRegEventArgs ev)
        {
            INDEX_ACTION iRes = INDEX_ACTION.NOTHING;
            ALARM_OBJECT alarmObj = find(ev.m_id_gtp, ev.m_id_tg);

            try
            {
                if (alarmObj == null)
                {//Только, если объект события сигнализации НЕ создан
                    // создать объект события сигнализации
                    alarmObj = new ALARM_OBJECT(ev);
                    _dictAlarmObject.Add(new KeyValuePair<int, int>(ev.m_id_gtp, ev.m_id_tg), alarmObj);

                    //if (m_bDestGUIActivated == true) {
                    //Устновить состояние "в_процессе"
                    alarmObj.state = INDEX_STATES_ALARM.PROCESSED;
                    //Сообщить для ГУИ о событии сигнализации
                    iRes = INDEX_ACTION.ADD;
                    //} else ;
                }
                else
                {
                    //Только, если объект события сигнализации создан
                    bool bEventRetry = false;
                    //Проверить состояние
                    if (alarmObj.CONFIRM == false)
                    {
                        // если НЕ подтверждено - установить признак повторного оповещения для ГУИ
                        bEventRetry = true;
                    }
                    else
                    {
                        // если подтверждено - проверить период между датой/временем регистрации события сигнализации и датой/временем его подтверждения
                        if ((alarmObj.dtConfirm - alarmObj.dtReg) > TimeSpan.FromMilliseconds(AdminAlarm.MSEC_ALARM_EVENTRETRY))
                        {
                            // ??? если 
                            bEventRetry = true;
                        }
                        else
                            ;
                    }

                    if (bEventRetry == true)
                    {
                        alarmObj.dtReg = DateTime.Now;

                        //if (m_bDestGUIActivated == true) {
                        // если состояние "в очереди", то изменить состояние на "обрабатывается"
                        if (alarmObj.state < INDEX_STATES_ALARM.PROCESSED) alarmObj.state = INDEX_STATES_ALARM.PROCESSED; else ;
                        // повторить оповещение пользователя о событии сигнализации
                        iRes = INDEX_ACTION.RETRY;
                        //} else ;
                    }
                    else
                        ;
                }
            }
            catch (Exception e)
            {
                iRes = INDEX_ACTION.ERROR;
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"DictAlarmObject::Registred () - ...");
            }

            return iRes;
        }
    }
}
