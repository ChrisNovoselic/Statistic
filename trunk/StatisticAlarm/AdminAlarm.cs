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
        List<StatisticAlarm.TecViewAlarm> m_listTecView;

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
        /// �������-������������ ��� 
        /// </summary>
        public event TecViewAlarm.AlarmTecViewEventHandler EventReg;
        /// <summary>
        /// ������� ��� �������� � 'TecView' - ��������� ��������� ������� �� (���./����)
        ///  ����� �������������!!! �������������
        /// </summary>
        public event DelegateIntFunc EventConfirm;
        /// <summary>
        /// ���������� ������� - ������� ������������ ������������ (�������������)
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        public void OnEventConfirm(int id_comp, int id_tg)
        {
            Logging.Logg().Debug(@"AdminAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            //�������� ��������� �� (���./����.)
            //??? ������� ������������ ���� 'TecView', ���� ���, � ������� ����� ����� �� ���
            EventConfirm(id_tg);
        }
        /// <summary>
        /// ���������� ������� - ����������� ������� ������������ �� 'TecView'
        /// </summary>
        /// <param name="obj">������, ������������������ ������� ������������</param>
        /// <param name="ev">�������� ������� ������������</param>
        private void OnEventReg_TecView(TecViewAlarm.AlarmTecViewEventArgs ev)
        {
            EventReg (ev);
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
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new IntDelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].AlarmRegistred);
                    m_listTecView[m_listTecView.Count - 1].EventReg += new TecViewAlarm.AlarmTecViewEventHandler (OnEventReg_TecView);

                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.MINUTES] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;
                    m_listTecView[m_listTecView.Count - 1].m_arTypeSourceData[(int)StatisticCommon.TG.ID_TIME.HOURS] = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;

                    m_listTecView[m_listTecView.Count - 1].m_bLastValue_TM_Gen = true;

                    EventConfirm += m_listTecView[m_listTecView.Count - 1].OnEventConfirm;
                } else ;
            }
        }

        public AdminAlarm()
        {
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

        //private TecViewAlarm getTecView (int id)
        //{
        //    foreach (TecViewAlarm tv in m_listTecView)
        //        if (tv.m_tec.m_id == id)
        //            return tv;
        //        else
        //            ;

        //    throw new Exception(@"AdminAlarm::getTecView (id_tec=" + id + @") - �� ������ ������ 'TecView' ...");
        //}

        //private int OnEventAlarmRegistred_TecView(int id_tec, int curHour, int curMinute)
        //{
        //    int iRes = -1;
        //    TecView tecView = getTecView (id_tec);

        //    return iRes;
        //}
    }
}
