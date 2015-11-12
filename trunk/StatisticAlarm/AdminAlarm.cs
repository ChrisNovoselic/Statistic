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
        /// <summary>
        /// ���������� ������� - ����������� ������� ������������ �� 'TecView'
        /// </summary>
        /// <param name="obj">������, ������������������ ������� ������������</param>
        /// <param name="ev">�������� ������� ������������</param>
        private void OnEventReg_TecView(TecViewAlarm.AlarmTecViewEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ref ev);
            StatesMachine state = StatesMachine.Unknown;
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"AdminAlarm::OnEventReg_TecView () - ...");
            else
            {
                switch (iAction)
                {
                    case INDEX_ACTION.NEW:
                        state = StatesMachine.Insert;
                        break;
                    case INDEX_ACTION.RETRY:
                        state = StatesMachine.Retry;
                        break;
                    default: // �����������/���������������� ��������
                        break;
                }

                push(new object[]
                    {
                        new object []
                        {
                            state
                            , ev
                        }
                    }
                );
            }
        }

        public void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_handlerDb.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        public void InitTEC(List<StatisticCommon.TEC> listTEC, HMark markQueries)
        {
            m_listTecView = new List<StatisticAlarm.TecViewAlarm> ();

            //HMark markQueries = new HMark ();
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO);
            ////markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN);
            ////markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN);

            //������� ???!!!
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
            int iListenerId = DbSources.Sources().Register (FormMain.s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett (), false, @"CONFIG_DB");
            foreach (TecViewAlarm tv in m_listTecView)
            {
                tv.m_tec.PerformUpdate (iListenerId);
                tv.ChangeState ();
            }
            DbSources.Sources().UnRegister (iListenerId);
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

        //    throw new Exception(@"AdminAlarm::getTecView (id_tec=" + id + @") - �� ������ ������ 'TecView' ...");
        //}

        //private int OnEventAlarmRegistred_TecView(int id_tec, int curHour, int curMinute)
        //{
        //    int iRes = -1;
        //    TecView tecView = getTecView (id_tec);

        //    return iRes;
        //}
        /// <summary>
        /// �������� ��������� �� (���./����.)
        /// </summary>
        /// <param name="id_tg">������������� ��</param>
        private void tgConfirm(int id_tg, StatisticCommon.TG.INDEX_TURNOnOff state)
        {
            TECComponent tc = null;

            if (! (state == TG.INDEX_TURNOnOff.UNKNOWN))
                foreach (TecView tv in m_listTecView)
                {
                    tc = tv.FindTECComponent(id_tg);

                    if ((!(tc == null))
                        && (tc.IsTG == true))
                        if (! (tc.m_listTG[0].m_TurnOnOff == state))
                            tc.m_listTG[0].m_TurnOnOff = state;
                        else
                            ;
                    else
                        ;
                }
            else
                Logging.Logg().Error(@"AdminAlarm::tgConfirm (id=" + id_tg + @") - ������� ������������� ��������� �� ��� �����������...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
