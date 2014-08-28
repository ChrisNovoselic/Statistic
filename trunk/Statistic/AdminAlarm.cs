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
        List<TecView> m_listTecView;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;
        private Int32 m_msecTimerUpdate = 30 * 1000; //5 * 60 * 1000;

        private bool m_bIsActive;

        protected void Initialize () {
        }

        private void AdminAlarm_EventAlarmCurPower (int indx, int id_tg) {
        }

        private void AdminAlarm_EventAlarmTGTurnOnOff(int indx, int id_tg)
        {
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            m_listTecView = new List<TecView> ();

            foreach (StatisticCommon.TEC t in listTEC) {
                if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id)) {
                    m_listTecView.Add(new TecView(null, TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    m_listTecView [m_listTecView.Count - 1].InitTEC (new List <StatisticCommon.TEC> { t });
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new DelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].SuccessThreadRDGValues);
                    m_listTecView[m_listTecView.Count - 1].EventAlarmCurPower += new DelegateIntIntFunc(AdminAlarm_EventAlarmCurPower);
                    m_listTecView[m_listTecView.Count - 1].EventAlarmTGTurnOnOff += new DelegateIntIntFunc(AdminAlarm_EventAlarmTGTurnOnOff);
                }
                else
                    ;
            }
        }

        public AdminAlarm()
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
