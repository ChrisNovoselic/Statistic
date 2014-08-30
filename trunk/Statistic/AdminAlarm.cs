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
            public bool CONFIRM { get { return dtConfirm.CompareTo (dtReg) > 0 ? true : false; } }

            public DateTime dtReg, dtConfirm;

            public ALARM_OBJECT(int tg) { dtReg = dtConfirm = DateTime.Now; }
        }

        List<TecView> m_listTecView;
        private Dictionary <KeyValuePair <int, int>, ALARM_OBJECT> m_dictAlarmObject;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;
        private Int32 m_msecTimerUpdate = 30 * 1000; //5 * 60 * 1000;

        private bool m_bIsActive;

        protected void Initialize () {
        }

        public event DelegateIntIntFunc EventAdd, EventRetry;

        public void OnEventConfirm(int id, int id_tg)
        {
            Logging.Logg().LogDebugToFile(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString () + @"; id_tg=" + id_tg.ToString ());

            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                m_dictAlarmObject [cKey].dtConfirm = DateTime.Now;
            }
            else
                Logging.Logg().LogErrorToFile(@"AdminAlarm::OnEventConfirm () - id=" + id.ToString() + @"; id_tg=" + id_tg.ToString() + @", Õ≈ Õ¿…ƒ≈Õ!");
        }

        private void OnAdminAlarm_EventReg(int id, int id_tg)
        {
            ALARM_OBJECT alarmObj = new ALARM_OBJECT(id_tg);
            KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == false)
            {
                m_dictAlarmObject.Add(cKey, alarmObj);

                EventAdd (id, id_tg);
            }
            else {
                m_dictAlarmObject[cKey].dtReg = DateTime.Now;

                EventRetry(id, id_tg);
            }
        }

        public bool IsEnabledButtonAlarm(int id, int id_tg)
        {
            bool bRes = false;

            KeyValuePair<int, int> cKey = new KeyValuePair<int, int>(id, id_tg);
            if (m_dictAlarmObject.ContainsKey(cKey) == true)
            {
                bRes = ! m_dictAlarmObject[cKey].CONFIRM;
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
                    m_listTecView[m_listTecView.Count - 1].EventReg += new DelegateIntIntFunc(OnAdminAlarm_EventReg);
                //} else ;
            }
        }

        public AdminAlarm()
        {
            m_dictAlarmObject = new Dictionary<KeyValuePair <int, int>,ALARM_OBJECT> ();
            
            lockValue = new object ();
        }

        public void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                m_bIsActive = active;

            if (m_bIsActive == true)
                m_timerAlarm.Change(0, m_msecTimerUpdate);
            else
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
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
                if (m_bIsActive == true)
                {
                    ChangeState();
                }
                else
                    ;
            }
        }
    }
}
