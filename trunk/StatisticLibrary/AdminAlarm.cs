using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using HClassLibrary;

namespace StatisticCommon
{
    public class AdminAlarm
    {
        List<TecView> m_listTecView;
        
        /// <summary>
        /// Объект для учета событий сигнализации и их состояний
        /// </summary>
        private DictAlarmObject m_dictAlarmObject;

        private object lockValue;

        private
            System.Threading.Timer
            //System.Windows.Forms.Timer
                m_timerAlarm;

        /// <summary>
        /// Интервал времени (милисекунды) между опросами по проверке выполнения условий сигнализаций
        /// </summary>
        public static volatile int MSEC_ALARM_TIMERUPDATE = -1;
        /// <summary>
        /// Период времени (милисекунды) от даты/времени регистрации события сигнализации
        ///  , в течение которого (только в случае подтверждения) выполнение условия сигнализации
        ///  не является основанием для его регистрации с новым идентификатором.
        ///  В противном случае (НЕподтверждения) пользователь оповещается повторно.
        /// </summary>
        public static volatile int MSEC_ALARM_EVENTRETRY = -1;
        /// <summary>
        /// Интервал времени (милисекунды) при периодическом воспроизведении звукового файла
        /// </summary>
        public static volatile int MSEC_ALARM_TIMERBEEP = -1;
        /// <summary>
        /// Строка - наименование (звукового) файла
        ///  , воспроизводящегося при оповещении пользователя оо событии сигнализации
        /// </summary>
        public static string FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = string.Empty;

        private int m_iActiveCounter;

        protected void Initialize () {
        }
        /// <summary>
        /// Получить дату/время регистрации события сигнализации для ТГ
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ГТП</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, int id_tg)
        {
            return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, id_tg);
        }

        public delegate void DelegateOnEventReg(TecView.EventRegEventArgs e);
        public event DelegateOnEventReg EventAdd, EventRetry;
        public event DelegateIntFunc EventConfirm;
        /// <summary>
        /// Обработчик события - событие сигнализации подтверждено (пользователем)
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Часть составного ключа: идентификатор ТГ</param>
        public void OnEventConfirm(int id_comp, int id_tg)
        {
            Logging.Logg().Debug(@"AdminAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            if (! (m_dictAlarmObject.Confirmed (id_comp, id_tg) < 0))
                if (!(id_tg < 0))
                    //Изменить состояние ТГ (вкл./выкл.)
                    EventConfirm(id_tg);
                else
                    ;
            else
                Logging.Logg().Error(@"AdminAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString() + @", НЕ НАЙДЕН!", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Обработчик события - регистрация события сигнализации от 'TecView'
        /// </summary>
        /// <param name="obj">Объект, зарегистрировавший событие сигнализации</param>
        /// <param name="ev">Аргумент события сигнализации</param>
        private void OnAdminAlarm_EventReg(TecView obj, TecView.EventRegEventArgs ev)
        {
            int iAction = m_dictAlarmObject.Registred (ev);
            if (iAction == 1)
                EventAdd(ev);
            else
                if (iAction == 2)
                    EventRetry(ev);
                else
                    ;
        }
        /// <summary>
        /// Возвратить признак "подтверждено" для события сигнализации
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Часть составного ключа: идентификатор ТГ</param>
        /// <returns>Результат: признак установлен/не_установлен)</returns>
        public bool IsConfirmed (int id_comp, int id_tg) {
            return m_dictAlarmObject.IsConfirmed (id_comp, id_tg);
        }

        public bool IsEnabledButtonAlarm(int id, int id_tg)
        {
            return ! m_dictAlarmObject.IsConfirmed(id, id_tg);
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            m_listTecView = new List<TecView> ();

            HMark markQueries = new HMark ();
            markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)CONN_SETT_TYPE.PBR);
            markQueries.Marked((int)CONN_SETT_TYPE.DATA_AISKUE);
            markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);
            //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN);
            //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN);

            //Отладка ???!!!
            int DEBUG_ID_TEC = -1;
            foreach (StatisticCommon.TEC t in listTEC) {
                if ((DEBUG_ID_TEC == -1) || (DEBUG_ID_TEC == t.m_id)) {
                    m_listTecView.Add(new TecView(TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    m_listTecView [m_listTecView.Count - 1].InitTEC (new List <StatisticCommon.TEC> { t }, markQueries);
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new IntDelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].SuccessThreadRDGValues);
                    m_listTecView[m_listTecView.Count - 1].EventReg += new TecView.DelegateOnEventReg (OnAdminAlarm_EventReg);

                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] = CONN_SETT_TYPE.DATA_SOTIASSO;
                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = CONN_SETT_TYPE.DATA_SOTIASSO;

                    m_listTecView[m_listTecView.Count - 1].m_bLastValue_TM_Gen = true;

                    EventConfirm += m_listTecView[m_listTecView.Count - 1].OnEventConfirm;
                } else ;
            }
        }

        public AdminAlarm()
        {
            m_dictAlarmObject = new DictAlarmObject ();

            lockValue = new object ();

            m_iActiveCounter = -1; //Для отслеживания 1-й по счету "активации"
            //m_bDestGUIActivated = false; //Активна ли вкладка (родитель) для отображения событий сигнализации

            //m_msecTimerUpdate = msecTimerUpdate;
            //m_msecEventRetry = msecEventRetry; 
        }

        public void Activate(bool active)
        {
            if (active == true)
            {
                if (m_iActiveCounter > 1)
                    return;
                else
                    ;

                m_iActiveCounter++;
            }
            else
                if (m_iActiveCounter > 1)
                    m_iActiveCounter--;
                else
                    ; //return;

            //Int32 msecTimerUpdate = m_msecTimerUpdate;
            if (active == true)
            {
                //Немедленный запуск ТОЛЬКО при 1-ой активации
                //!!! если установить немедленный запуск всегда, ТО
                //!!! сообщение об одном событии будет отображаться до тех пор, пока условия для него будут верны
                //!!! т.к. при отображении сообщения последовательно выполняются: this.Activate(false) -> this.Activate(true)
                if (m_iActiveCounter == 0)
                {
                    //Вариант №0
                    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                    ////Вариант №1
                    //m_timerAlarm.Interval = MSEC_ALARM_TIMERUPDATE;
                    //m_timerAlarm.Start ();
                }
                else
                    if (m_iActiveCounter > 0)
                    {
                        //Вариант №0
                        m_timerAlarm.Change(MSEC_ALARM_TIMERUPDATE, System.Threading.Timeout.Infinite);
                        ////Вариант №1
                        //m_timerAlarm.Interval = ProgramBase.TIMER_START_INTERVAL; // по этому признаку определим задержку очередной итерации
                        //m_timerAlarm.Start();
                    }
                    else
                        ;

                ////Немедленный запуск ВСЕГДА
                //if (!(m_iActiveCounter < 0))
                //    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                //else
                //    ;
            }
            else
                //Вариант №0
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
                ////Вариант №1
                //m_timerAlarm.Stop ();

            foreach (TecView tv in m_listTecView)
            {
                tv.Activate(active);
            }
        }

        public bool IsStarted
        {
            get { return ! (m_timerAlarm == null); }
        }

        public void Start()
        {
            foreach (TecView tv in m_listTecView)
            {
                tv.Start (); //StartDbInterfaces (CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            }

            //m_evTimerCurrent = new ManualResetEvent(true);
            m_timerAlarm =
                new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite)
                //new System.Windows.Forms.Timer ()
                ;
            //m_timerAlarm.Tick += new EventHandler(TimerAlarm_Tick);
        }

        public void Stop()
        {
            foreach (TecView tv in m_listTecView)
            {
                tv.Stop ();
            }

            if (! (m_timerAlarm == null))
            {
                m_timerAlarm.Dispose ();
                m_timerAlarm = null;
            }
            else
                ;
        }

        private void ChangeState () {
            foreach (TecView tv in m_listTecView)
            {
                tv.ChangeState ();
            }
        }

        private void TimerAlarm_Tick(Object stateInfo)
        //private void TimerAlarm_Tick(Object stateInfo, EventArgs ev)
        {
            lock (lockValue)
            {
                ////Задержка выполнения итерации
                //if (m_timerAlarm.Interval == ProgramBase.TIMER_START_INTERVAL)
                //{
                //    m_timerAlarm.Interval = MSEC_ALARM_TIMERUPDATE;

                //    return;
                //}
                //else
                //    ;

                if (! (m_iActiveCounter < 0))
                {
                    ChangeState();

                    m_timerAlarm.Change (MSEC_ALARM_TIMERUPDATE, System.Threading.Timeout.Infinite);
                }
                else
                    ;
            }
        }
    }
}
