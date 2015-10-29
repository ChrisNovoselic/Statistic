using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color
using System.Globalization; //...CultureInfo

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
            /// <summary>
            /// ��������� ��� �������� �������� ��� �����������
            ///  ������ ���������� ������� ������������
            /// </summary>
            public struct EventDetail
            {
                public int id;
                public float value;
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

            public AlarmTecViewEventArgs(int id_comp, float value, DateTime dtReg, int s, List<EventDetail> listEventDetail)
                : base(id_comp, value, dtReg, s)
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
        /// <param name="typePanel">��� ������ � ������� ����������� ������ �����</param>
        /// <param name="indxTEC">������ ��� � ������</param>
        /// <param name="indx_comp">??? ������ ����������</param>
        public TecViewAlarm(StatisticCommon.TecView.TYPE_PANEL typePanel, int indxTEC, int indx_comp)
            : base(typePanel, indxTEC, indx_comp)
        {
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
            int iRes = (int)HHandler.INDEX_WAITHANDLE_REASON.SUCCESS
                , iDebug = -1; //-1 - ��� �������, 0 - ���./�������, 1 - ������������
            //���������
            float TGTURNONOFF_VALUE = -1F //�������� ��� ������������ "�� ���./����."
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
                Logging.Logg().Error(@"TecView::AlarmEventRegistred (" + m_tec.name_shr + @"[ID_COMPONENT=" + m_ID + @"])"
                                    + @" - curHour=" + curHour + @"; curMinute=" + curMinute, Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            {
                foreach (StatisticCommon.TG tg in allTECComponents[indxTECComponents].m_listTG)
                {
                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN;

                    #region ��� ��� �������
                    if (!(iDebug < 0))
                        Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                    else
                        ;
                    #endregion ��������� ����� ���� ��� �������

                    if (m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 1F)
                        //??? ��������� �� �������� �� '< 0F'
                        if (!(m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 0F))
                            curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                        else
                            //Console.WriteLine(@"�������������� (Value < 0): id_tg=" + tg.m_id + @", value=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
                            Logging.Logg().Warning(@"TecViewAlarm::AlarmRegistred (id_tg=" + tg.m_id + @") - "
                                 + @"value=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                                , Logging.INDEX_MESSAGE.NOT_SET);
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

                    #region ��� ��� �������
                    //������������ - �������� ���������
                    if (iDebug == 1)
                        if (!(tg.m_TurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.UNKNOWN))
                        {
                            if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.ON)
                            {// �������� - �� ����.
                                //������ �������� ������������ �� � �������� ��� ��� � �����
                                power_TM -= m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                                //��������� �������� ��� "�������" (< 1)
                                m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 0.666F;
                                //�������� ���������
                                curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.OFF;
                            }
                            else
                                if (curTurnOnOff == StatisticCommon.TG.INDEX_TURNOnOff.OFF)
                                {
                                    //��������� �������� ��� "�������" (> 1)
                                    m_dictValuesTG[tg.m_id].m_powerCurrent_TM = 66.6F;
                                    //�������� ���������
                                    curTurnOnOff = StatisticCommon.TG.INDEX_TURNOnOff.ON;
                                }
                                else
                                    ;

                            Console.Write(Environment.NewLine + @"�������:: "
                                + tg.m_id_owner_gtp + @":" + tg.m_id + @"="
                                + m_dictValuesTG[tg.m_id].m_powerCurrent_TM
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
                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(tg.m_id, listEventDetail[0].value, DateTime.UtcNow, (int)curTurnOnOff, listEventDetail));

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

                    #region ��� ��� �������
                    if (!(iDebug < 0))
                        if ((TECComponentCurrent.m_listTG.IndexOf(tg) + 1) < TECComponentCurrent.m_listTG.Count)
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
                            EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, listEventDetail[0].value, DateTime.UtcNow, situation, listEventDetail)); //������
                            Console.WriteLine(@"; ::AlarmEventRegistred () - EventReg [ID=" + TECComponentCurrent.m_id + @"] ...");
                        }
                        else
                        #endregion ��������� ����� ���� ��� �������
                            if (Math.Abs(power_TM - m_valuesHours[curHour].valuesUDGe) > m_valuesHours[curHour].valuesUDGe * ((double)TECComponentCurrent.m_dcKoeffAlarmPcur / 100))
                            {
                                //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                                if (power_TM < m_valuesHours[curHour].valuesUDGe)
                                    situation = -1; //������
                                else
                                    situation = 1; //������

                                EventReg(new TecViewAlarm.AlarmTecViewEventArgs(TECComponentCurrent.m_id, power_TM, DateTime.UtcNow, situation, listEventDetail));
                            }
                            else
                                ; //EventUnReg...
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
