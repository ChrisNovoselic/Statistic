using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using StatisticCommon;

namespace Statistic
{
    public class AdminAlarm
    {
        public enum INDEX_TYPE_ALARM { CUR_POWER, TGTurnOnOff }
        
        private class ALARM_OBJECT
        {
            public INDEX_TYPE_ALARM type; //{ get { return id_tg > 0 ? INDEX_TYPE_ALARM.CUR_POWER : INDEX_TYPE_ALARM.TGTurnOnOff; } }
            public bool CONFIRM { get {
                    return dtConfirm.CompareTo (dtReg) > 0 ? true : false;
                }
            }

            public DateTime dtReg, dtConfirm;

            public ALARM_OBJECT(int tg) { dtReg = dtConfirm = DateTime.Now; }
        }

        List<TecView> m_listTecView;
        private Dictionary <KeyValuePair <int, int>, ALARM_OBJECT> m_dictAlarmObject;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;
        private Int32 m_msecTimerUpdate = 30 * 1000; //5 * 60 * 1000;

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
            Logging.Logg().LogDebugToFile(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString () + @"; id_tg=" + id_tg.ToString ());

            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                m_dictAlarmObject [cKey].dtConfirm = DateTime.Now;

                if (!(id_tg < 0))
                    EventConfirm(id_tg);
                else
                    ;
            }
            else
                Logging.Logg().LogErrorToFile(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString() + @"; id_tg=" + id_tg.ToString() + @", НЕ НАЙДЕН!");
        }

        private void OnAdminAlarm_EventReg(TecView obj, TecView.EventRegEventArgs ev)
        {
            ALARM_OBJECT alarmObj = new ALARM_OBJECT(ev.m_id_tg);
            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (ev.m_id_gtp, ev.m_id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == false)
            {
                m_dictAlarmObject.Add(cKey, alarmObj);

                EventAdd(ev);
            }
            else {
                m_dictAlarmObject[cKey].dtReg = DateTime.Now;

                EventRetry(ev);
            }
        }

        public bool IsEnabledButtonAlarm(int id, int id_tg)
        {
            bool bRes = false;

            KeyValuePair<int, int> cKey = new KeyValuePair<int, int>(id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                //bRes = ! m_dictAlarmObject[cKey].CONFIRM;
                if (m_dictAlarmObject [cKey].dtConfirm.CompareTo (m_dictAlarmObject [cKey].dtReg) > 0)
                    bRes = false;
                else
                    bRes = true;
            }
            else
                ;

            return bRes;
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            m_listTecView = new List<TecView> ();

            foreach (StatisticCommon.TEC t in listTEC) {
                //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id)) {
                    m_listTecView.Add(new TecView(null, TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    m_listTecView [m_listTecView.Count - 1].InitTEC (new List <StatisticCommon.TEC> { t });
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new DelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].SuccessThreadRDGValues);
                    m_listTecView[m_listTecView.Count - 1].EventReg += new TecView.DelegateOnEventReg (OnAdminAlarm_EventReg);
                    EventConfirm += m_listTecView[m_listTecView.Count - 1].OnEventConfirm;
                //} else ;
            }
        }

        public AdminAlarm()
        {
            m_dictAlarmObject = new Dictionary<KeyValuePair <int, int>,ALARM_OBJECT> ();
            
            lockValue = new object ();

            m_iActiveCounter = -1; //Для отслеживания 1-й по счету "активации"
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
                    ;

            if (active == true)
                if (m_iActiveCounter == 0)
                    m_timerAlarm.Change(0, m_msecTimerUpdate);
                else
                    if (m_iActiveCounter > 0)
                        m_timerAlarm.Change(m_msecTimerUpdate, m_msecTimerUpdate);
                    else
                        ;
            else
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);

            foreach (TecView tv in m_listTecView)
            {
                tv.Activate(active);
            }
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

            m_timerAlarm.Dispose ();
            m_timerAlarm = null;
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
                }
                else
                    ;
            }
        }
    }
}
