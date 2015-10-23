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
    public class AdminAlarm
    {
        /// <summary>
        /// ����� ��� �������� ��������� ��� ������������� ������� - ������������
        /// </summary>
        public class EventRegEventArgs : EventArgs
        {
            public struct EventDetail
            {
                public int id;
                public float value;
                public DateTime last_changed_at;
                public int id_tm;
            }

            public int Id { get { return m_id_tg < 0 ? m_id_gtp : m_id_tg; } }

            public int m_id_gtp;
            public int m_id_tg;
            public DateTime m_dtRegistred;
            public int m_situation;
            public List<EventDetail> m_listEventDetail;
            public string m_message;

            //public EventRegEventArgs() : base ()
            //{
            //    m_id_gtp = -1;
            //    m_id_tg = -1;
            //    m_situation = 0;
            //}

            public static int GetSituation(string message)
            {
                int iRes = 0;

                switch (message)
                {
                    case @"�����":
                    case @"���.":
                        iRes = 1;
                        break;
                    case @"����":
                    case @"����.":
                        iRes = -1;
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            public static string GetMessage(int id_gtp, int id_tg, int situation)
            {
                string strRes = string.Empty;

                if (id_tg < 0)
                    if (situation == 1)
                        strRes = @"�����";
                    else
                        if (situation == -1)
                            strRes = @"����";
                        else
                            strRes = @"���";
                else
                    if (situation == (int)StatisticCommon.TG.INDEX_TURNOnOff.ON) //TGTurnOnOff = ON
                        strRes = @"���.";
                    else
                        if (situation == (int)StatisticCommon.TG.INDEX_TURNOnOff.OFF) //TGTurnOnOff = OFF
                            strRes = @"����.";
                        else
                            strRes = @"���";

                return strRes;
            }

            public EventRegEventArgs(int id_gtp, int id_tg, int s, List<EventDetail> listEventDetail)
                : base()
            {
                m_id_gtp = id_gtp;
                m_id_tg = id_tg;
                m_dtRegistred = DateTime.UtcNow;
                m_situation = s;
                m_listEventDetail = listEventDetail;

                m_message = GetMessage(m_id_gtp, m_id_tg, m_situation);
            }
        }

        List<StatisticAlarm.TecViewAlarm> m_listTecView;
        
        /// <summary>
        /// ������ ��� ����� ������� ������������ � �� ���������
        /// </summary>
        private DictAlarmObject m_dictAlarmObject;

        private object lockValue;

        private
            System.Threading.Timer
            //System.Windows.Forms.Timer
                m_timerAlarm;

        /// <summary>
        /// �������� ������� (�����������) ����� �������� �� �������� ���������� ������� ������������
        /// </summary>
        public static volatile int MSEC_ALARM_TIMERUPDATE = -1;
        /// <summary>
        /// ������ ������� (�����������) �� ����/������� ����������� ������� ������������
        ///  , � ������� �������� (������ � ������ �������������) ���������� ������� ������������
        ///  �� �������� ���������� ��� ��� ����������� � ����� ���������������.
        ///  � ��������� ������ (���������������) ������������ ����������� ��������.
        /// </summary>
        public static volatile int MSEC_ALARM_EVENTRETRY = -1;
        /// <summary>
        /// �������� ������� (�����������) ��� ������������� ��������������� ��������� �����
        /// </summary>
        public static volatile int MSEC_ALARM_TIMERBEEP = -1;
        /// <summary>
        /// ������ - ������������ (���������) �����
        ///  , ������������������ ��� ���������� ������������ �� ������� ������������
        /// </summary>
        public static string FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = string.Empty;

        private int m_iActiveCounter;

        protected void Initialize () {
        }
        /// <summary>
        /// �������� ����/����� ����������� ������� ������������ ��� ��
        /// </summary>
        /// <param name="id_comp">��������� ����� �����: ������������� ���</param>
        /// <param name="id_tg">��������� ����� �����: ������������� ���</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, int id_tg)
        {
            return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, id_tg);
        }

        public delegate void DelegateOnEventReg(EventRegEventArgs e);
        public event DelegateOnEventReg EventAdd, EventRetry;
        public event DelegateIntFunc EventConfirm;
        /// <summary>
        /// ���������� ������� - ������� ������������ ������������ (�������������)
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        public void OnEventConfirm(int id_comp, int id_tg)
        {
            Logging.Logg().Debug(@"AdminAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            if (! (m_dictAlarmObject.Confirmed (id_comp, id_tg) < 0))
                if (!(id_tg < 0))
                    //�������� ��������� �� (���./����.)
                    EventConfirm(id_tg);
                else
                    ;
            else
                Logging.Logg().Error(@"AdminAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString() + @", �� ������!", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// ���������� ������� - ����������� ������� ������������ �� 'TecView'
        /// </summary>
        /// <param name="obj">������, ������������������ ������� ������������</param>
        /// <param name="ev">�������� ������� ������������</param>
        private void OnEventReg_TecView(EventRegEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ev);
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"AdminAlarm::OnAdminAlarm_EventReg () - ...");
            else
                if (iAction == INDEX_ACTION.ADD)
                    EventAdd(ev);
                else
                    if (iAction == INDEX_ACTION.RETRY)
                        EventRetry(ev);
                    else
                        ;
        }
        /// <summary>
        /// ���������� ������� "������������" ��� ������� ������������
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        /// <returns>���������: ������� ����������/��_����������)</returns>
        public bool IsConfirmed (int id_comp, int id_tg) {
            return m_dictAlarmObject.IsConfirmed (id_comp, id_tg);
        }

        public bool IsEnabledButtonAlarm(int id, int id_tg)
        {
            return ! m_dictAlarmObject.IsConfirmed(id, id_tg);
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

            //������� ???!!!
            int DEBUG_ID_TEC = -1;
            foreach (StatisticCommon.TEC t in listTEC) {
                if ((DEBUG_ID_TEC == -1) || (DEBUG_ID_TEC == t.m_id)) {
                    m_listTecView.Add(new StatisticAlarm.TecViewAlarm(StatisticCommon.TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    m_listTecView [m_listTecView.Count - 1].InitTEC (new List <StatisticCommon.TEC> { t }, markQueries);
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new IntDelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].AlarmEventRegistred);
                    m_listTecView[m_listTecView.Count - 1].EventReg += new AdminAlarm.DelegateOnEventReg (OnEventReg_TecView);

                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.MINUTES] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;
                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.HOURS] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;

                    m_listTecView[m_listTecView.Count - 1].m_bLastValue_TM_Gen = true;

                    EventConfirm += m_listTecView[m_listTecView.Count - 1].OnEventConfirm;
                } else ;
            }
        }

        public AdminAlarm()
        {
            m_dictAlarmObject = new DictAlarmObject ();

            lockValue = new object ();

            m_iActiveCounter = -1; //��� ������������ 1-� �� ����� "���������"
            //m_bDestGUIActivated = false; //������� �� ������� (��������) ��� ����������� ������� ������������ 
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
                //����������� ������ ������ ��� 1-�� ���������
                //!!! ���� ���������� ����������� ������ ������, ��
                //!!! ��������� �� ����� ������� ����� ������������ �� ��� ���, ���� ������� ��� ���� ����� �����
                //!!! �.�. ��� ����������� ��������� ��������������� �����������: this.Activate(false) -> this.Activate(true)
                if (m_iActiveCounter == 0)
                {
                    //������� �0
                    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                    ////������� �1
                    //m_timerAlarm.Interval = MSEC_ALARM_TIMERUPDATE;
                    //m_timerAlarm.Start ();
                }
                else
                    if (m_iActiveCounter > 0)
                    {
                        //������� �0
                        m_timerAlarm.Change(MSEC_ALARM_TIMERUPDATE, System.Threading.Timeout.Infinite);
                        ////������� �1
                        //m_timerAlarm.Interval = ProgramBase.TIMER_START_INTERVAL; // �� ����� �������� ��������� �������� ��������� ��������
                        //m_timerAlarm.Start();
                    }
                    else
                        ;

                ////����������� ������ ������
                //if (!(m_iActiveCounter < 0))
                //    m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                //else
                //    ;
            }
            else
                //������� �0
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
                ////������� �1
                //m_timerAlarm.Stop ();

            foreach (TecViewAlarm tv in m_listTecView)
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
            foreach (TecViewAlarm tv in m_listTecView)
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
            foreach (TecViewAlarm tv in m_listTecView)
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
            foreach (TecViewAlarm tv in m_listTecView)
            {
                tv.ChangeState ();
            }
        }

        private void TimerAlarm_Tick(Object stateInfo)
        //private void TimerAlarm_Tick(Object stateInfo, EventArgs ev)
        {
            lock (lockValue)
            {
                ////�������� ���������� ��������
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

        private TecViewAlarm getTecView (int id)
        {
            foreach (TecViewAlarm tv in m_listTecView)
                if (tv.m_tec.m_id == id)
                    return tv;
                else
                    ;

            throw new Exception(@"AdminAlarm::getTecView (id_tec=" + id + @") - �� ������ ������ 'TecView' ...");
        }

        //private int OnEventAlarmRegistred_TecView(int id_tec, int curHour, int curMinute)
        //{
        //    int iRes = -1;
        //    TecView tecView = getTecView (id_tec);

        //    return iRes;
        //}
    }
}