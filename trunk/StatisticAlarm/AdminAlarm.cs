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

        List<StatisticCommon.TecView> m_listTecView;
        
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
        public event DelegateOnEventReg EventReg, EventAdd, EventRetry;
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
        private void OnEventReg(EventRegEventArgs ev)
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
            m_listTecView = new List<StatisticCommon.TecView> ();

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
                    m_listTecView.Add(new StatisticCommon.TecView(StatisticCommon.TecView.TYPE_PANEL.ADMIN_ALARM, -1, -1));
                    m_listTecView [m_listTecView.Count - 1].InitTEC (new List <StatisticCommon.TEC> { t }, markQueries);
                    m_listTecView[m_listTecView.Count - 1].updateGUI_Fact = new IntDelegateIntIntFunc (m_listTecView[m_listTecView.Count - 1].AlarmEventDetect);
                    m_listTecView[m_listTecView.Count - 1].EventAlarmDetect += new TecView.EventAlarmRegistredHandler(OnEventAlarmRegistred_TecView);

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

            //m_msecTimerUpdate = msecTimerUpdate;
            //m_msecEventRetry = msecEventRetry;
            EventReg += new DelegateOnEventReg(OnEventReg); 
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

        private TecView getTecView (int id)
        {
            foreach (TecView tv in m_listTecView)
                if (tv.m_tec.m_id == id)
                    return tv;
                else
                    ;

            throw new Exception(@"AdminAlarm::getTecView (id_tec=" + id + @") - �� ������ ������ 'TecView' ...");
        }

        private int OnEventAlarmRegistred_TecView(int id_tec, int curHour, int curMinute)
        {
            TecView tecView = getTecView (id_tec);
            
            //������� ���������� �������
            int iRes = (int)HHandler.INDEX_WAITHANDLE_REASON.SUCCESS
                , iDebug = -1; //-1 - ��� �������, 0 - ���./�������, 1 - ������������
            //���������
            double TGTURNONOFF_VALUE = -1F //�������� ��� ������������ "�� ���./����."
                , NOT_VALUE = -2F //��� ��������
                , power_TM = NOT_VALUE;
            //������� ��������� ��� ������������ "�� ���./����." - ��������
            TG.INDEX_TURNOnOff curTurnOnOff = TG.INDEX_TURNOnOff.UNKNOWN;
            //������ ��������, �������������� ������� ������������
            List<StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail> listEventDetail = new List<StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail>();

            //��� �������
            if (!(iDebug < 0))
                Console.WriteLine(@" - curHour=" + curHour.ToString() + @"; curMinute=" + curMinute.ToString());
            else
                ;

            //if (((lastHour == 24) || (lastHourError == true)) || ((lastMin == 0) || (lastMinError == true)))
            if (((curHour == 24) || (tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_HOUR) == true))
                || ((curMinute == 0) || (tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_MIN) == true)))
            {
                Logging.Logg().Error(@"TecView::SuccessThreadRDGValues (" + tecView.m_tec.name_shr + @"[ID_COMPONENT=" + tecView.m_ID + @"])"
                                    + @" - curHour=" + curHour + @"; curMinute=" + curMinute, Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {
                foreach (TG tg in tecView.allTECComponents[tecView.indxTECComponents].m_listTG)
                {
                    curTurnOnOff = TG.INDEX_TURNOnOff.UNKNOWN;

                    //��� �������
                    if (!(iDebug < 0))
                        Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                    else
                        ;

                    if (tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 1)
                        if (!(tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 0))
                            curTurnOnOff = TG.INDEX_TURNOnOff.OFF;
                        else
                            ;
                    else
                    {//������ ��� ����� 1F
                        curTurnOnOff = TG.INDEX_TURNOnOff.ON;

                        if (power_TM == NOT_VALUE) power_TM = 0F; else ;
                        power_TM += tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                    }
                    //??? ����������� ������������� ��������� �������� �������� (id_tm = -1)
                    listEventDetail.Add(new StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail()
                    {
                        id = tg.m_id
                        , value = (float)tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                        , last_changed_at = tecView.m_dictValuesTG[tg.m_id].m_dtCurrent_TM, id_tm = -1
                    });

                    //������������ - �������� ���������
                    if (iDebug == 1)
                        if (!(tg.m_TurnOnOff == TG.INDEX_TURNOnOff.UNKNOWN))
                        {
                            if (curTurnOnOff == TG.INDEX_TURNOnOff.ON)
                            {// �������� - �� ����.
                                //������ �������� ������������ �� � �������� ��� ��� � �����
                                power_TM -= tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                                //��������� �������� ��� "�������" (< 1)
                                tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 0.666;
                                //�������� ���������
                                curTurnOnOff = TG.INDEX_TURNOnOff.OFF;
                            }
                            else
                                if (curTurnOnOff == TG.INDEX_TURNOnOff.OFF)
                                {
                                    //��������� �������� ��� "�������" (> 1)
                                    tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 66.6;
                                    //�������� ���������
                                    curTurnOnOff = TG.INDEX_TURNOnOff.ON;
                                }
                                else
                                    ;

                            //��� �������
                            if (!(iDebug < 0))
                                Console.Write(Environment.NewLine + @"�������:: " + tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM + Environment.NewLine);
                            else
                                ;
                        }
                        else
                            ;
                    else
                        ;

                    if (tg.m_TurnOnOff == TG.INDEX_TURNOnOff.UNKNOWN)
                    {
                        tg.m_TurnOnOff = curTurnOnOff;
                    }
                    else
                    {
                        if (!(tg.m_TurnOnOff == curTurnOnOff))
                        {
                            EventReg(new EventRegEventArgs(tecView.TECComponentCurrent.m_id, tg.m_id, (int)curTurnOnOff, listEventDetail));

                            //���������� ������� ����...
                            //������� ���������� ���������� ����� ��� ����. "������� P"
                            power_TM = TGTURNONOFF_VALUE;

                            break;
                        }
                        else
                            ; //EventUnReg...
                    }

                    //��� �������
                    if (!(iDebug < 0))
                        if ((tecView.TECComponentCurrent.m_listTG.IndexOf(tg) + 1) < tecView.TECComponentCurrent.m_listTG.Count)
                            Console.Write(@", ");
                        else
                            ;
                    else
                        ;
                }

                if (!(power_TM == TGTURNONOFF_VALUE))
                    if ((!(power_TM == NOT_VALUE)) && (!(power_TM < 1)))
                    {
                        int situation = 0;

                        //��� �������
                        if (!(iDebug < 0))
                        {
                            situation = HMath.GetRandomNumber() % 2 == 1 ? -1 : 1;
                            EventReg(new AdminAlarm.EventRegEventArgs(tecView.TECComponentCurrent.m_id, -1, situation, listEventDetail)); //������
                            Console.WriteLine(@"; ::SuccessThreadRDGValues () - EventReg [ID=" + tecView.TECComponentCurrent.m_id + @"] ...");
                        }
                        else
                            if (Math.Abs(power_TM - tecView.m_valuesHours[curHour].valuesUDGe) > tecView.m_valuesHours[curHour].valuesUDGe * ((double)tecView.TECComponentCurrent.m_dcKoeffAlarmPcur / 100))
                            {
                                //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                                if (power_TM < tecView.m_valuesHours[curHour].valuesUDGe)
                                    situation = -1; //������
                                else
                                    situation = 1; //������

                                EventReg(new AdminAlarm.EventRegEventArgs(tecView.TECComponentCurrent.m_id, -1, situation, listEventDetail));
                            }
                            else
                                ; //EventUnReg...
                    }
                    else
                        ; //��� �������� ��� �������� ���������� 1 ���
                else
                    iRes = -102; //(int)INDEX_WAITHANDLE_REASON.BREAK;

                //��� �������
                if (!(iDebug < 0))
                    Console.WriteLine();
                else
                    ;

                ////�������
                //for (int i = 0; i < m_valuesHours.valuesFact.Length; i ++)
                //    Console.WriteLine(@"valuesFact[" + i.ToString() + @"]=" + m_valuesHours.valuesFact[i]);
            }

            return iRes;
        }
    }
}
