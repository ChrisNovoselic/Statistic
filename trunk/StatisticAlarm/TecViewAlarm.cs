using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color

using HClassLibrary;

namespace StatisticAlarm
{
    public class TecViewAlarm : StatisticCommon.TecView
    {
        /// <summary>
        /// ����� ��� �������� ��������� ��� ������������� ������� - ������������
        /// </summary>
        public class AlarmTecViewEventArgs : AlarmNotifyEventArgs
        {
            public struct EventDetail
            {
                public int id;
                public float value;
                public DateTime last_changed_at;
                public int id_tm;
            }

            public int Id { get { return m_id_tg < 0 ? m_id_gtp : m_id_tg; } }

            public DateTime m_dtRegistred;
            public List<EventDetail> m_listEventDetail;

            //public EventRegEventArgs() : base ()
            //{
            //    m_id_gtp = -1;
            //    m_id_tg = -1;
            //    m_situation = 0;
            //}

            public AlarmTecViewEventArgs(int id_gtp, int id_tg, int s, List<EventDetail> listEventDetail)
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
        
        public delegate void AlarmTecViewEventHandler (AlarmTecViewEventArgs ev);
        public event AlarmTecViewEventHandler EventReg;

        public TecViewAlarm(StatisticCommon.TecView.TYPE_PANEL typePanel, int indxTEC, int indx_comp)
            : base(typePanel, indxTEC, indx_comp)
        {
        }

        //public delegate int EventAlarmRegistredHandler(int id, int hour, int min);
        //public event EventAlarmRegistredHandler EventAlarmRegistred;
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
            int iRes = (int)HHandler.INDEX_WAITHANDLE_REASON.SUCCESS
                , iDebug = 1; //-1 - ��� �������, 0 - ���./�������, 1 - ������������
            //���������
            double TGTURNONOFF_VALUE = -1F //�������� ��� ������������ "�� ���./����."
                , NOT_VALUE = -2F //��� ��������
                , power_TM = NOT_VALUE;
            //������� ��������� ��� ������������ "�� ���./����." - ��������
            StatisticCommon.TG.INDEX_TURNOnOff curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;
            //������ ��������, �������������� ������� ������������
            List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail> listEventDetail = new List<TecViewAlarm.AlarmTecViewEventArgs.EventDetail>();

            //��� �������
            if (!(iDebug < 0))
                Console.WriteLine(@" - curHour=" + curHour.ToString() + @"; curMinute=" + curMinute.ToString());
            else
                ;

            //if (((lastHour == 24) || (lastHourError == true)) || ((lastMin == 0) || (lastMinError == true)))
            if (((curHour == 24) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_HOUR) == true))
                || ((curMinute == 0) || (m_markWarning.IsMarked((int)INDEX_WARNING.LAST_MIN) == true)))
            {
                Logging.Logg().Error(@"TecView::AlarmEventRegistred (" + m_tec.name_shr + @"[ID_COMPONENT=" + m_ID + @"])"
                                    + @" - curHour=" + curHour + @"; curMinute=" + curMinute, Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {
                foreach (StatisticCommon.TG tg in allTECComponents[indxTECComponents].m_listTG)
                {
                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;

                    //��� �������
                    if (!(iDebug < 0))
                        Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                    else
                        ;

                    if (m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 1)
                        if (!(m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 0))
                            curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                        else
                            ;
                    else
                    {//������ ��� ����� 1F
                        curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;

                        if (power_TM == NOT_VALUE) power_TM = 0F; else ;
                        power_TM += m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                    }
                    //??? ����������� ������������� ��������� �������� �������� (id_tm = -1)
                    listEventDetail.Add(new TecViewAlarm.AlarmTecViewEventArgs.EventDetail()
                    {
                        id = tg.m_id
                        , value = (float)m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                        , last_changed_at = m_dictValuesTG[tg.m_id].m_dtCurrent_TM
                        , id_tm = -1
                    });

                    //������������ - �������� ���������
                    if (iDebug == 1)
                        if (!(tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        {
                            if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                            {// �������� - �� ����.
                                //������ �������� ������������ �� � �������� ��� ��� � �����
                                power_TM -= m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                                //��������� �������� ��� "�������" (< 1)
                                m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 0.666;
                                //�������� ���������
                                curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                            }
                            else
                                if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                                {
                                    //��������� �������� ��� "�������" (> 1)
                                    m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 66.6;
                                    //�������� ���������
                                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                                }
                                else
                                    ;

                            //��� �������
                            if (!(iDebug < 0))
                                Console.Write(Environment.NewLine + @"�������:: " + tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM + Environment.NewLine);
                            else
                                ;
                        }
                        else
                            ;
                    else
                        ;

                    if (tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN)
                    {
                        tg.m_TurnOnOff = curTurnOnOff;
                    }
                    else
                    {
                        if (!(tg.m_TurnOnOff == curTurnOnOff))
                        {
                            EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, tg.m_id, (int)curTurnOnOff, listEventDetail));

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
                        if ((TECComponentCurrent.m_listTG.IndexOf(tg) + 1) < TECComponentCurrent.m_listTG.Count)
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
                            EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, -1, situation, listEventDetail)); //������
                            Console.WriteLine(@"; ::AlarmEventRegistred () - EventReg [ID=" + TECComponentCurrent.m_id + @"] ...");
                        }
                        else
                            if (Math.Abs(power_TM - m_valuesHours[curHour].valuesUDGe) > m_valuesHours[curHour].valuesUDGe * ((double)TECComponentCurrent.m_dcKoeffAlarmPcur / 100))
                            {
                                //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                                if (power_TM < m_valuesHours[curHour].valuesUDGe)
                                    situation = -1; //������
                                else
                                    situation = 1; //������

                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, -1, situation, listEventDetail));
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

        public void OnEventConfirm(int id_tg)
        {
            foreach (StatisticCommon.TECComponent tc in allTECComponents)
            {
                if (tc.m_id == id_tg)
                {
                    if (tc.m_listTG[0].m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                        tc.m_listTG[0].m_TurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                    else
                        if (tc.m_listTG[0].m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                            tc.m_listTG[0].m_TurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                        else
                            ;

                    break;
                }
                else
                    ;
            }
        }
    }
}
