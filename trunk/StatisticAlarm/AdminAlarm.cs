using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    partial class AdminAlarm
    {
        private List<StatisticAlarm.TecViewAlarm> m_listTecView;

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
        /// <summary>
        /// Обработчик события - регистрация события сигнализации от 'TecView'
        /// </summary>
        /// <param name="obj">Объект, зарегистрировавший событие сигнализации</param>
        /// <param name="ev">Аргумент события сигнализации</param>
        private void OnEventReg_TecView(TecViewAlarm.AlarmTecViewEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ev);
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"AdminAlarm::OnEventReg_TecView () - ...");
            else
                if (iAction == INDEX_ACTION.NEW)
                    push (new object []
                        {
                            new object []
                            {
                                StatesMachine.Insert
                                , ev
                            }
                        });
                else
                    ; // неизвестное/необрабатываемое днйствие
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            m_listTecView = new List<StatisticAlarm.TecViewAlarm> ();

            HMark markQueries = new HMark ();
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO);
            //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN);
            //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN);

            //Отладка ???!!!
            int indxTecView = -1
                , DEBUG_ID_TEC = -1;
            foreach (StatisticCommon.TEC t in listTEC) {
                if ((DEBUG_ID_TEC == -1) || (DEBUG_ID_TEC == t.m_id)) {
                    m_listTecView.Add(new StatisticAlarm.TecViewAlarm(StatisticCommon.TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    indxTecView = m_listTecView.Count - 1;
                    m_listTecView[indxTecView].InitTEC(new List<StatisticCommon.TEC> { t }, markQueries);
                    m_listTecView[indxTecView].updateGUI_Fact = new IntDelegateIntIntFunc(m_listTecView[indxTecView].AlarmRegistred);
                    m_listTecView[indxTecView].EventReg += new TecViewAlarm.AlarmTecViewEventHandler(OnEventReg_TecView);

                    m_listTecView[indxTecView].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.MINUTES] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;
                    m_listTecView[indxTecView].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.HOURS] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;

                    m_listTecView[m_listTecView.Count - 1].m_bLastValue_TM_Gen = true;
                } else ;
            }
        }

        private void ChangeState () {
            foreach (TecViewAlarm tv in m_listTecView)
                tv.ChangeState ();
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

                if (m_bAlarmDbEventUpdated == false)
                    m_timerAlarm.Change(PanelStatistic.POOL_TIME * 1000, System.Threading.Timeout.Infinite);
                else
                    if (IsStarted == true)
                    {
                        ChangeState();

                        m_timerAlarm.Change (MSEC_ALARM_TIMERUPDATE, System.Threading.Timeout.Infinite);
                    }
                    else
                        ;
            }
        }

        //private TecViewAlarm getTecView (int id)
        //{
        //    foreach (TecViewAlarm tv in m_listTecView)
        //        if (tv.m_tec.m_id == id)
        //            return tv;
        //        else
        //            ;

        //    throw new Exception(@"AdminAlarm::getTecView (id_tec=" + id + @") - не найден объект 'TecView' ...");
        //}

        //private int OnEventAlarmRegistred_TecView(int id_tec, int curHour, int curMinute)
        //{
        //    int iRes = -1;
        //    TecView tecView = getTecView (id_tec);

        //    return iRes;
        //}
        /// <summary>
        /// Изменить состояние ТГ (вкл./выкл.)
        /// </summary>
        /// <param name="id_tg">Идентификатор ТГ</param>
        private void tgConfirm(int id_tg)
        {
            TECComponent tc = null;

            foreach (TecView tv in m_listTecView)
            {
                tc = tv.FindTECComponent(id_tg);

                if ((!(tc == null))
                    && (tc.IsTG == true))
                {
                    if (tc.m_listTG[0].m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                        tc.m_listTG[0].m_TurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                    else
                        if (tc.m_listTG[0].m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                            tc.m_listTG[0].m_TurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                        else
                            ;
                }
                else
                    ;
            }
        }
    }
}
