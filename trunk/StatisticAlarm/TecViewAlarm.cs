using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color
using System.Globalization; //...CultureInfo

using HClassLibrary;

namespace StatisticAlarm
{
    public class TecViewAlarm : StatisticCommon.TecView
    {
        /// <summary>
        /// Класс для описания аргумента при возникновении события - сигнализация
        /// </summary>
        public class AlarmTecViewEventArgs : AlarmNotifyEventArgs
        {
            /// <summary>
            /// Структура для описания элемента при детализации
            ///  случая выполнения условий сигнализаций
            /// </summary>
            public struct EventDetail
            {
                public int id;
                public float value;
                public DateTime last_changed_at;
                public int id_tm;

                public string ValuesToString()
                {
                    return @"value=" + value.ToString(@"F3", CultureInfo.InvariantCulture)
                        + @", last_changed_at=" + last_changed_at.ToString(@"dd.MM.yyyy HH.mm.ss.fff");
                }
            }

            public int Id { get { return
                //m_id_tg < 0 ? m_id_gtp : m_id_tg
                m_id_comp
                ; } }

            public List<EventDetail> m_listEventDetail;

            public AlarmTecViewEventArgs(int id_comp, float value, DateTime dtReg, int s, List<EventDetail> listEventDetail)
                : base(id_comp, value, dtReg, s)
            {
                m_listEventDetail = listEventDetail;
            }
        }
        /// <summary>
        /// Делегат - тип обработчика события 'EventReg'
        ///  регистрации случая выполнения условия сигнализаций
        /// </summary>
        /// <param name="ev">Аргумент при возникновении события</param>        
        public delegate void AlarmTecViewEventHandler (AlarmTecViewEventArgs ev);
        /// <summary>
        /// Событие для регистрации случая выполнения условия сигнализаций
        ///  в режиме "выполнения_приложения"
        /// </summary>
        public event AlarmTecViewEventHandler EventReg;
        /// <summary>
        /// Конструктор - основной (параметры - базового класа)
        /// </summary>
        /// <param name="typePanel">Тип панели к которой принадлежит объект класс</param>
        /// <param name="indxTEC">Индекс ТЭЦ в списке</param>
        /// <param name="indx_comp">??? Индекс компонента</param>
        public TecViewAlarm(StatisticCommon.TecView.TYPE_PANEL typePanel, int indxTEC, int indx_comp)
            : base(typePanel, indxTEC, indx_comp)
        {
        }
        /// <summary>
        /// Функция проверки выполнения условий сигнализаций (для одного ГТП)
        /// </summary>
        /// <param name="curHour">Текущий час</param>
        /// <param name="curMinute">Текущий интервал (1-мин) - текущая минута указанного часа</param>
        /// <returns>Признак выполнения функции</returns>
        public int AlarmRegistred(int curHour, int curMinute)
        {
            //return EventAlarmDetect(m_tec.m_id, curHour, curMinute);

            //Признак выполнения функции
            int iRes = (int)HHandler.INDEX_WAITHANDLE_REASON.SUCCESS
                , iDebug = -1; //-1 - нет отладки, 0 - раб./отладка, 1 - имитирование
            //Константы
            float TGTURNONOFF_VALUE = -1F //Значения для сигнализации "ТГ вкл./откл."
                , NOT_VALUE = -2F //НЕТ значения
                , power_TM = NOT_VALUE;
            //Признак состояния для сигнализации "ТГ вкл./откл." - исходный
            StatisticCommon.TG.INDEX_TURNOnOff curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;
            //Список объектов, детализирующих событие сигнализации
            List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail> listEventDetail = new List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail>();

            #region Код для отладки
            if (!(iDebug < 0))
                Console.WriteLine(@" - curHour=" + curHour.ToString() + @"; curMinute=" + curMinute.ToString());
            else
                ;
            #endregion Окончание блока кода для отладки

            //if (((lastHour == 24) || (lastHourError == true)) || ((lastMin == 0) || (lastMinError == true)))
            if (((curHour == 24) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_HOUR) == true))
                || ((curMinute == 0) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_MIN) == true)))
            {
                Logging.Logg().Error(@"TecView::AlarmEventRegistred (" + m_tec.name_shr + @"[ID_COMPONENT=" + m_ID + @"])"
                                    + @" - curHour=" + curHour + @"; curMinute=" + curMinute, Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {
                foreach (StatisticCommon.TG tg in allTECComponents[indxTECComponents].m_listTG)
                {
                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;

                    #region Код для отладки
                    if (!(iDebug < 0))
                        Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                    else
                        ;
                    #endregion Окончание блока кода для отладки

                    if (m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 1F)
                        //??? проверять ли значение на '< 0F'
                        if (!(m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 0F))
                            curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                        else
                            //Console.WriteLine(@"Предупреждение (Value < 0): id_tg=" + tg.m_id + @", value=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                            Logging.Logg().Warning(@"TecViewAlarm::AlarmRegistred (id_tg=" + tg.m_id + @") - "
                                 + @"value=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    else
                    {//Больше ИЛИ равно 1F
                        curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;

                        if (power_TM == NOT_VALUE) power_TM = 0F; else ;
                        power_TM += m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                    }
                    //??? неизвестный идентификатор источника значений СОТИАССО (id_tm = -1)
                    listEventDetail.Add(new TecViewAlarm.AlarmTecViewEventArgs.EventDetail()
                    {
                        id = tg.m_id
                        , value = (float)m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                        , last_changed_at = m_dictValuesTG[tg.m_id].m_dtCurrent_TM
                        , id_tm = -1
                    });

                    #region Код для отладки
                    //Имитирование - изменяем состояние
                    if (iDebug == 1)
                        if (!(tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        {
                            if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                            {// имитация - ТГ выкл.
                                //Учесть мощность выключенного ТГ в значении для ГТП в целом
                                power_TM -= m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                                //Присвоить значение для "отладки" (< 1)
                                m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 0.666F;
                                //Изменить состояние
                                curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                            }
                            else
                                if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                                {
                                    //Присвоить значение для "отладки" (> 1)
                                    m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 66.6F;
                                    //Изменить состояние
                                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                                }
                                else
                                    ;

                            Console.Write(Environment.NewLine + @"Отладка:: "
                                + tg.m_id_owner_gtp + @":" + tg.m_id + @"="
                                + m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                                + Environment.NewLine);                            
                        }
                        else
                            ;
                    else
                        ;
                    #endregion Окончание блока кода для отладки

                    if (! (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        if (tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN)
                            tg.m_TurnOnOff = curTurnOnOff;
                        else
                            if (!(tg.m_TurnOnOff == curTurnOnOff))
                            {
                                //
                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(tg.m_id, listEventDetail[0].value, DateTime.UtcNow, (int)curTurnOnOff, listEventDetail));

                                //Прекращаем текущий цикл...
                                //Признак досрочного прерывания цикла для сигн. "Текущая P"
                                power_TM = TGTURNONOFF_VALUE;

                                break;
                            }
                            else
                                ; //Состояние ТГ не изменилось
                    else
                        //Текущее состояние ТГ не удалось определить
                        Logging.Logg().Warning (@"TecViewAlarm::AlarmRegistred (id_tg=" + tg.m_id + @") - Detail: "
                            + listEventDetail[listEventDetail.Count - 1].ValuesToString ()
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    #region Код для отладки
                    if (!(iDebug < 0))
                        if ((TECComponentCurrent.m_listTG.IndexOf(tg) + 1) < TECComponentCurrent.m_listTG.Count)
                            Console.Write(@", ");
                        else
                            ;
                    else
                        ;
                    #endregion Окончание блока кода для отладки
                }

                if (!(power_TM == TGTURNONOFF_VALUE))
                    if ((!(power_TM == NOT_VALUE)) && (!(power_TM < 1)))
                    {
                        int situation = 0;

                        #region Код для отладки
                        if (!(iDebug < 0))
                        {
                            situation = HMath.GetRandomNumber() % 2 == 1 ? -1 : 1;
                            EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, listEventDetail[0].value, DateTime.UtcNow, situation, listEventDetail)); //Меньше
                            Console.WriteLine(@"; ::AlarmEventRegistred () - EventReg [ID=" + TECComponentCurrent.m_id + @"] ...");
                        }
                        else
                        #endregion Окончание блока кода для отладки
                            if (Math.Abs(power_TM - m_valuesHours[curHour].valuesUDGe) > m_valuesHours[curHour].valuesUDGe * ((double)TECComponentCurrent.m_dcKoeffAlarmPcur / 100))
                            {
                                //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                                if (power_TM < m_valuesHours[curHour].valuesUDGe)
                                    situation = -1; //Меньше
                                else
                                    situation = 1; //Больше

                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, power_TM, DateTime.UtcNow, situation, listEventDetail));
                            }
                            else
                                ; //EventUnReg...
                    }
                    else
                        ; //Нет значений ИЛИ значения ограничены 1 МВт
                else
                    iRes = -102; //(int)INDEX_WAITHANDLE_REASON.BREAK;

                #region Код для отладки
                if (!(iDebug < 0))
                    Console.WriteLine();
                else
                    ;

                ////Отладка
                //for (int i = 0; i < m_valuesHours.valuesFact.Length; i ++)
                //    Console.WriteLine(@"valuesFact[" + i.ToString() + @"]=" + m_valuesHours.valuesFact[i]);
                #endregion Окончание блока кода для отладки
            }

            return iRes;
        }
    }
}
