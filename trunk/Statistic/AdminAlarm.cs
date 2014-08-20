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
        List<StatisticCommon.TEC> m_listTEC;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;
        private Int32 m_msecTimerUpdate = 30 * 1000; //5 * 60 * 1000;

        private bool m_bIsActive;

        protected void Initialize () {
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            m_listTEC = listTEC;
        }

        public AdminAlarm(HReports rep)
        {
            lockValue = new object ();
        }

        public void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                m_bIsActive = active;

            if (m_bIsActive == true)
                m_timerAlarm.Change(m_msecTimerUpdate, m_msecTimerUpdate);
            else
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            foreach (StatisticCommon.TEC t in m_listTEC)
            {
                t.StartDbInterfaces (CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            }

            //m_evTimerCurrent = new ManualResetEvent(true);
            m_timerAlarm = new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Stop()
        {
            foreach (StatisticCommon.TEC t in m_listTEC)
            {
                t.StopDbInterfaces ();
            }

            m_timerAlarm.Dispose ();
            m_timerAlarm = null;
        }

        private void ChangeState () {
            foreach (StatisticCommon.TEC t in m_listTEC)
            {
                t.ChangeState (TEC.TYPE_PANEL.ADMIN_ALARM);
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
