using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color
using System.Globalization; //...CultureInfo

//using HClassLibrary;
using StatisticCommon;
using ASUTP;
using ASUTP.Core;

namespace StatisticAlarm
{
    /// <summary>
    /// ����� ������ ��� ��������� � ������
    /// </summary>
    public class TecViewAlarm : StatisticCommon.TecView
    {
        /// <summary>
        /// ����� ��� �������� ��������� ��� ������������� ������� - ������������
        /// </summary>
        public class AlarmTecViewEventArgs : AlarmNotifyEventArgs
        {
            /// <summary>
            /// ��������� ��� �������� �������� ��� �����������
            ///  ������ ���������� ������� ������������
            /// </summary>
            public struct EventDetail
            {
                public int id;
                public double value;
                public DateTime last_changed_at;
                public int id_tm;

                public string ValuesToString()
                {
                    return @"value=" + value.ToString(@"F3", CultureInfo.InvariantCulture)
                        + @", last_changed_at=" + last_changed_at.ToString(@"dd.MM.yyyy HH.mm.ss.fff");
                }
            }

            public int Id { get { return
                //m_id_tg < 0 ? m_id_gtp : m_id_tg
                m_id_comp
                ; } }

            public List<EventDetail> m_listEventDetail;

            public AlarmTecViewEventArgs(int id_comp, EventReason r, DateTime dtReg, int s, List<EventDetail> listEventDetail)
                : base(id_comp, r, dtReg, s)
            {
                m_listEventDetail = listEventDetail;
            }
        }
        /// <summary>
        /// ������� - ��� ����������� ������� 'EventReg'
        ///  ����������� ������ ���������� ������� ������������
        /// </summary>
        /// <param name="ev">�������� ��� ������������� �������</param>        
        public delegate void AlarmTecViewEventHandler (AlarmTecViewEventArgs ev);
        /// <summary>
        /// ������� ��� ����������� ������ ���������� ������� ������������
        ///  � ������ "����������_����������"
        /// </summary>
        public event AlarmTecViewEventHandler EventReg;
        /// <summary>
        /// ����������� - �������� (��������� - �������� �����)
        /// </summary>
        /// <param name="key">���� ��������</param>
        public TecViewAlarm (FormChangeMode.KeyDevice key)
            : base(key, TECComponentBase.TYPE.ELECTRO)
        {
            updateGUI_Fact = new IntDelegateIntIntFunc (AlarmRegistred);
        }

        public override void ChangeState()
        {
            new Thread(new ParameterizedThreadStart(threadGetRDGValues)).Start();

            //base.ChangeState(); //Run
        }

        /// <summary>
        /// ����� ������� �������� ��� 'TecViewAlarm'
        /// </summary>
        /// <param name="synch">������ ��� �������������</param>
        private void threadGetRDGValues(object synch)
        {
            int indxEv = -1;

            //if (m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS].WaitOne (0, true) == false)
            ((AutoResetEvent)m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS]).Set();
            //else ;

            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            foreach (TECComponent tc in allTECComponents)
            {
                if (tc.IsGTP == true)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == (int)INDEX_WAITHANDLE_REASON.BREAK)
                        break;
                    else
                    {
                        if (!(indxEv == (int)INDEX_WAITHANDLE_REASON.SUCCESS))
                            ((ManualResetEvent)m_waitHandleState[indxEv]).Reset();
                        else
                            ;

                        CurrentKey = new FormChangeMode.KeyDevice () { Id = tc.m_id, Mode = tc.Mode };

                        getRDGValues();
                    }
                }
                else
                    ; //��� �� ���
            }
        }
        /// <summary>
        /// ��������� �������� ��� 'TecViewAlarm'
        /// </summary>
        private void getRDGValues()
        {
            GetRDGValues(CurrentKey, DateTime.Now);

            Run(@"TecView::GetRDGValues () - ...");
        }
        /// <summary>
        /// �������� ��������� ��� ������� ��������� �������� 'TecViewAlarm'
        /// </summary>
        /// <param name="key">���� ����������</param>
        /// <param name="date">���� ������������� ��������</param>
        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            m_prevDate = m_curDate;
            m_curDate = date.Date;

            ClearStates();

            adminValuesReceived = false;

            if ((m_tec.GetReadySensorsStrings (key.Mode) == false))
            {
                AddState((int)StatesMachine.InitSensors);
            }
            else ;

            if (currHour == true)
                AddState((int)StatesMachine.CurrentTimeView);
            else
                ;

            //??? � ��� AISKUE+SOTIASSO
            if (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                AddState((int)StatesMachine.Hours_Fact);
            else
                if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO))
                    AddState((int)StatesMachine.Hours_TM);
                else
                    ;

            if (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                AddState((int)StatesMachine.CurrentMins_Fact);
            else
                if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO))
                    AddState((int)StatesMachine.CurrentMins_TM);
                else
                    ;

            if (m_bLastValue_TM_Gen == true)
                AddState((int)StatesMachine.LastValue_TM_Gen);
            else
                ;

            AddState((int)StatesMachine.PPBRValues);
            AddState((int)StatesMachine.AdminValues);
        }
        /// <summary>
        /// ������� �������� ���������� ������� ������������ (��� ������ ���)
        /// </summary>
        /// <param name="curHour">������� ���</param>
        /// <param name="curMinute">������� �������� (1-���) - ������� ������ ���������� ����</param>
        /// <returns>������� ���������� �������</returns>
        public int AlarmRegistred(int curHour, int curMinute)
        {
            //return EventAlarmDetect(m_tec.m_id, curHour, curMinute);

            //������� ���������� �������
            int iRes = (int)ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.SUCCESS
                , iDebug = -1 //-1 - ��� �������, 0 - ���./�������, 1 - ������������
                , cntTGTurnOn = 0 // ���-�� ���. ��
                , cntTGTurnUnknown = allTECComponents.Find(comp => comp.m_id == CurrentKey.Id).ListLowPointDev.Count // ���-�� �� � ����������� ����������
                , cntPower_TMValues = 0; //������� ���-�� �������� ���./����. �� � ����� �������� �������� ��� ���
            //���������
            double TGTURNONOFF_VALUE = -1F //�������� ��� ������������ "�� ���./����."
                , NOT_VALUE = -2F //��� ��������
                , power_TM = NOT_VALUE;
            //������� ��������� ��� ������������ "�� ���./����." - ��������
            StatisticCommon.TG.INDEX_TURNOnOff curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;
            //������ ��������, �������������� ������� ������������
            List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail> listEventDetail = new List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail>();

            #region ��� ��� �������
            if (!(iDebug < 0))
                Console.WriteLine(@" - curHour=" + curHour.ToString() + @"; curMinute=" + curMinute.ToString());
            else
                ;
            #endregion ��������� ����� ���� ��� �������

            //if (((lastHour == 24) || (lastHourError == true)) || ((lastMin == 0) || (lastMinError == true)))
            if (((curHour == 24) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_HOUR) == true))
                || ((curMinute == 0) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_MIN) == true)))
            {
                Logging.Logg().Error(@"TecView::AlarmEventRegistred (" + m_tec.name_shr + @"[KeyComponent=" + CurrentKey + @"])"
                        + @" - curHour=" + curHour + @"; curMinute=" + curMinute
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {
                foreach (StatisticCommon.TG tg in allTECComponents.Find(comp => comp.m_id == CurrentKey.Id).ListLowPointDev)
                {
                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;

                    #region ��� ��� �������
                    if (!(iDebug < 0))
                        Console.Write($"{tg.m_keys_owner.Find (key => key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP).Id}:{tg.m_id}={m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM}");
                    else
                        ;
                    #endregion ��������� ����� ���� ��� �������

                    if (m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM < 1F)
                        //??? ��������� �� �������� �� '< 0F'
                        if (!(m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM < 0F))
                            curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                        else
                            ; //??? �������������� ��������� ��
                    else
                    {//������ ��� ����� 1F
                        curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                        // ����������� ��������                        
                        if (power_TM == NOT_VALUE) power_TM = 0F; else ;
                        // ������ � ����� �������� �������� ���, ������� �������� ��
                        power_TM += m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM;
                        // ��������� ������� 
                        cntPower_TMValues ++;
                    }
                    //??? ����������� ������������� ��������� �������� �������� (id_tm = -1)
                    listEventDetail.Add(new TecViewAlarm.AlarmTecViewEventArgs.EventDetail()
                    {
                        id = tg.m_id
                        , value = m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM
                        , last_changed_at = m_dictValuesLowPointDev[tg.m_id].m_dtCurrent_TM
                        , id_tm = m_dictValuesLowPointDev[tg.m_id].m_id_TM
                    });

                    #region ��� ��� �������
                    //������������ - �������� ���������
                    if (iDebug == 1)
                        if (!(tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        {
                            if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                            {// �������� - �� ����.
                                //������ �������� ������������ �� � �������� ��� ��� � �����
                                power_TM -= m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM;
                                //��������� �������� ��� "�������" (< 1)
                                m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM = 0.666F;
                                //�������� ���������
                                curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                            }
                            else
                                if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                                {
                                    //��������� �������� ��� "�������" (> 1)
                                    m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM = 66.6F;
                                    //�������� ���������
                                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                                }
                                else
                                    ;

                            Console.Write(Environment.NewLine + @"�������:: "
                                + tg.m_keys_owner.Find (key => key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP).Id + @":" + tg.m_id + @"="
                                + m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM
                                + Environment.NewLine);
                        }
                        else
                            ;
                    else
                        ;
                    #endregion ��������� ����� ���� ��� �������

                    if (! (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        if (tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN)
                            tg.m_TurnOnOff = curTurnOnOff;
                        else
                            if (!(tg.m_TurnOnOff == curTurnOnOff))
                            {
                                //
                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(tg.m_id, new AlarmNotifyEventArgs.EventReason() { value = listEventDetail[listEventDetail.Count - 1].value
                                        , UDGe = m_valuesHours[curHour].valuesUDGe
                                        , koeff = TECComponentCurrent.m_dcKoeffAlarmPcur }
                                    , DateTime.UtcNow
                                    , (int)curTurnOnOff
                                    , listEventDetail));

                                //���������� ������� ����...
                                //������� ���������� ���������� ����� ��� ����. "������� P"
                                power_TM = TGTURNONOFF_VALUE;

                                break;
                            }
                            else
                                ; //��������� �� �� ����������
                    else
                        //������� ��������� �� �� ������� ����������
                        Logging.Logg().Warning (@"TecViewAlarm::AlarmRegistred (id_tg=" + tg.m_id + @") - Detail: "
                            + listEventDetail[listEventDetail.Count - 1].ValuesToString ()
                            , Logging.INDEX_MESSAGE.NOT_SET);

                    if (! (tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                    {
                        cntTGTurnUnknown --;

                        if (tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                            cntTGTurnOn ++;
                        else
                            ;
                    }
                    else
                        ;

                    #region ��� ��� �������
                    if (!(iDebug < 0))
                        if ((TECComponentCurrent.ListLowPointDev.IndexOf(tg) + 1) < TECComponentCurrent.ListLowPointDev.Count)
                            Console.Write(@", ");
                        else
                            ;
                    else
                        ;
                    #endregion ��������� ����� ���� ��� �������
                }

                if (!(power_TM == TGTURNONOFF_VALUE))
                    if ((!(power_TM == NOT_VALUE)) && (!(power_TM < 1)))
                    {
                        int situation = 0;

                        #region ��� ��� �������
                        if (!(iDebug < 0))
                        {
                            situation = HMath.GetRandomNumber() % 2 == 1 ? -1 : 1;
                            EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id
                                , new AlarmNotifyEventArgs.EventReason() { value = listEventDetail[0].value
                                    , UDGe = m_valuesHours[curHour].valuesUDGe
                                    , koeff = TECComponentCurrent.m_dcKoeffAlarmPcur }
                                , DateTime.UtcNow
                                , situation
                                , listEventDetail)); //������
                            Console.WriteLine(@"; ::AlarmEventRegistred () - EventReg [ID=" + TECComponentCurrent.m_id + @"] ...");
                        }
                        else
                        #endregion ��������� ����� ���� ��� �������
                        {
                            if ((cntTGTurnUnknown == 0) // ���-�� �� � ����������� ���������� = 0
                                && (cntTGTurnOn == cntPower_TMValues)) // ���-�� ��, �������� ��� �������� ������ ����. ���./����. ��� = ���-�� ���. ��
                            {
                                double absDiff = Math.Abs(power_TM - m_valuesHours[curHour].valuesUDGe)
                                 , lim = m_valuesHours[curHour].valuesUDGe * ((double)TECComponentCurrent.m_dcKoeffAlarmPcur / 100);
                                if (absDiff > lim)
                                {
                                    //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                                    if (power_TM < m_valuesHours[curHour].valuesUDGe)
                                        situation = -1; //������
                                    else
                                        situation = 1; //������

                                    EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id
                                        , new AlarmNotifyEventArgs.EventReason () { value = power_TM 
                                            , UDGe = m_valuesHours[curHour].valuesUDGe
                                            , koeff = TECComponentCurrent.m_dcKoeffAlarmPcur }
                                        , DateTime.UtcNow
                                        , situation
                                        , listEventDetail));
                                }
                                else
                                    ; //EventUnReg...
                            }
                            else
                                // ���������� �� ��� �������� ���./�������� ��_�_������ �� ������� ���
                                Logging.Logg().Warning(@"TecViewAlarm::AlarmRegistred (id=" + CurrentKey.Id + @") - ���������� �� ��� �������� ���./�������� ��_�_������ �� ������� ���", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        ; //��� �������� ��� �������� ���������� 1 ���
                else
                    iRes = -102; //(int)INDEX_WAITHANDLE_REASON.BREAK;

                #region ��� ��� �������
                if (!(iDebug < 0))
                    Console.WriteLine();
                else
                    ;

                ////�������
                //for (int i = 0; i < m_valuesHours.valuesFact.Length; i ++)
                //    Console.WriteLine(@"valuesFact[" + i.ToString() + @"]=" + m_valuesHours.valuesFact[i]);
                #endregion ��������� ����� ���� ��� �������
            }

            return iRes;
        }
    }
}
