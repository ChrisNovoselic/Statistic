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
        public event AdminAlarm.DelegateOnEventReg EventReg;

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
        public int AlarmEventRegistred(int curHour, int curMinute)
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
            List<StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail> listEventDetail = new List<StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail>();

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
                    listEventDetail.Add(new StatisticAlarm.AdminAlarm.EventRegEventArgs.EventDetail()
                    {
                        id = tg.m_id
                        ,
                        value = (float)m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                        ,
                        last_changed_at = m_dictValuesTG[tg.m_id].m_dtCurrent_TM,
                        id_tm = -1
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
                            EventReg(new AdminAlarm.EventRegEventArgs(TECComponentCurrent.m_id, tg.m_id, (int)curTurnOnOff, listEventDetail));

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
                            EventReg(new AdminAlarm.EventRegEventArgs(TECComponentCurrent.m_id, -1, situation, listEventDetail)); //������
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

                                EventReg(new AdminAlarm.EventRegEventArgs(TECComponentCurrent.m_id, -1, situation, listEventDetail));
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
