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
        public enum INDEX_TYPE_ALARM { CUR_POWER, TGTurnOnOff }
        public enum INDEX_STATES_ALARM { QUEUEDED = -1, PROCESSED, CONFIRMED }

        //private bool m_bDestGUIActivated;
        //public bool DestGUIActivated { get { return m_bDestGUIActivated; } set { if (! (m_bDestGUIActivated == value)) OnChangedDestGUIActivated (value); else ; } }
        //private void OnChangedDestGUIActivated(bool newVal) {
        //    m_bDestGUIActivated = newVal;

        //    if (m_bDestGUIActivated == true) {
        //        //Передать все накопленные события "родителю"
        //        foreach (KeyValuePair <int, int> cKey in m_dictAlarmObject.Keys) {
        //            if (m_dictAlarmObject [cKey].state < INDEX_STATES_ALARM.PROCESSED) {
        //                m_dictAlarmObject [cKey].state = INDEX_STATES_ALARM.PROCESSED;
        //                EventAdd (m_dictAlarmObject [cKey].m_evReg);
        //            }
        //            else
        //                ;
        //        }
        //    }
        //    else
        //        ;
        //}

        private class ALARM_OBJECT
        {
            public INDEX_TYPE_ALARM type; //{ get { return id_tg > 0 ? INDEX_TYPE_ALARM.CUR_POWER : INDEX_TYPE_ALARM.TGTurnOnOff; } }
            public bool CONFIRM { get {
                    return dtConfirm.CompareTo (dtReg) > 0 ? true : false;
                }
            }

            public TecView.EventRegEventArgs m_evReg;
            public DateTime dtReg, dtConfirm;
            public INDEX_STATES_ALARM state;

            public ALARM_OBJECT(TecView.EventRegEventArgs ev) { m_evReg = ev; dtReg = dtConfirm = DateTime.Now; state = INDEX_STATES_ALARM.QUEUEDED; }
        }

        List<TecView> m_listTecView;
        private Dictionary <KeyValuePair <int, int>, ALARM_OBJECT> m_dictAlarmObject;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;
        public static volatile int MSEC_ALARM_TIMERUPDATE = -1;
        public static volatile int MSEC_ALARM_EVENTRETRY = -1;
        public static volatile int MSEC_ALARM_TIMERBEEP = -1;
        public static string FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = string.Empty;
        //private Int32 m_msecTimerUpdate;
        //private Int32 m_msecEventRetry;

        private int m_iActiveCounter;

        protected void Initialize () {
        }

        public DateTime TGAlarmDatetimeReg(int id_comp, int id_tg)
        {
            DateTime dtRes = DateTime.Now;

            KeyValuePair<int, int> cKey = new KeyValuePair<int, int>(id_comp, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                dtRes = m_dictAlarmObject[cKey].dtReg;
            }
            else
                ;

            return dtRes;
        }

        public delegate void DelegateOnEventReg(TecView.EventRegEventArgs e);
        public event DelegateOnEventReg EventAdd, EventRetry;
        public event DelegateIntFunc EventConfirm;

        public void OnEventConfirm(int id, int id_tg)
        {
            Logging.Logg().Debug(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString() + @"; id_tg=" + id_tg.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                m_dictAlarmObject [cKey].dtConfirm = DateTime.Now;
                m_dictAlarmObject [cKey].state = INDEX_STATES_ALARM.CONFIRMED;

                if (!(id_tg < 0))
                    EventConfirm(id_tg);
                else
                    ;
            }
            else
                Logging.Logg().Error(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString() + @"; id_tg=" + id_tg.ToString() + @", НЕ НАЙДЕН!", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void OnAdminAlarm_EventReg(TecView obj, TecView.EventRegEventArgs ev)
        {
            ALARM_OBJECT alarmObj = null;
            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (ev.m_id_gtp, ev.m_id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == false)
            {
                alarmObj = new ALARM_OBJECT(ev);
                m_dictAlarmObject.Add(cKey, alarmObj);

                //if (m_bDestGUIActivated == true) {
                    m_dictAlarmObject [cKey].state = INDEX_STATES_ALARM.PROCESSED;
                    
                    EventAdd(ev);
                //} else ;
            }
            else {
                bool bEventRetry = false;
                if (m_dictAlarmObject[cKey].CONFIRM == false) {
                    bEventRetry = true;
                }
                else {
                    if ((m_dictAlarmObject[cKey].dtConfirm - m_dictAlarmObject[cKey].dtReg) > TimeSpan.FromMilliseconds (MSEC_ALARM_EVENTRETRY)) {
                        bEventRetry = true;
                    }
                    else
                        ;
                }

                if (bEventRetry == true) {
                    m_dictAlarmObject[cKey].dtReg = DateTime.Now;

                    //if (m_bDestGUIActivated == true) {
                        if (m_dictAlarmObject [cKey].state < INDEX_STATES_ALARM.PROCESSED) m_dictAlarmObject [cKey].state = INDEX_STATES_ALARM.PROCESSED; else ;

                        EventRetry(ev);
                    //} else ;
                }
                else
                    ;
            }
        }

        public bool Confirm (int id_comp, int id_tg) {
            bool bRes = false;
            KeyValuePair<int, int>  cKey = new KeyValuePair<int, int>(id_comp, id_tg);
            if (m_dictAlarmObject.ContainsKey (cKey) == true)
                bRes = m_dictAlarmObject[cKey].CONFIRM;
            else
                bRes = false;

            return bRes;
        }

        public bool IsEnabledButtonAlarm(int id, int id_tg)
        {
            bool bRes = false;

            KeyValuePair<int, int> cKey = new KeyValuePair<int, int>(id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                bRes = ! m_dictAlarmObject[cKey].CONFIRM;
                //if (m_dictAlarmObject [cKey].dtConfirm.CompareTo (m_dictAlarmObject [cKey].dtReg) > 0)
                //    bRes = false;
                //else
                //    bRes = true;
            }
            else
                ;

            return bRes;
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
            m_dictAlarmObject = new Dictionary<KeyValuePair <int, int>,ALARM_OBJECT> ();
            
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
                    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                else
                    if (m_iActiveCounter > 0)
                        m_timerAlarm.Change(MSEC_ALARM_TIMERUPDATE, System.Threading.Timeout.Infinite);
                    else
                        ;

                ////Немедленный запуск ВСЕГДА
                //if (!(m_iActiveCounter < 0))
                //    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                //else
                //    ;
            }
            else
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);

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
            m_timerAlarm = new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite);
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
        {
            lock (lockValue)
            {
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
