using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;

namespace StatisticCommon
{
    public abstract class PanelStatistic : TableLayoutPanel
    {
        public abstract void Start();
        public abstract void Stop();

        public abstract void Activate(bool active);
    }

    public abstract class PanelStatisticView : PanelStatistic
    {
        protected volatile string sensorsString_TM = string.Empty;
        protected volatile string[] sensorsStrings_Fact = { string.Empty, string.Empty }; //Только для особенной ТЭЦ (Бийск)

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public TG[] sensorId2TG;

        public volatile TEC tec;
        public volatile int num_TEC;
        public volatile int num_TECComponent;
        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public List<TECComponentBase> m_list_TECComponents;

        protected StatusStrip stsStrip;
        protected HReports m_report;

        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        protected int CountTG { get { return sensorId2TG.Length; } }

        protected bool GetSensors()
        {
            int t = 0;

            if (num_TECComponent < 0)
            {//ТЭЦ в полном составе
                int j, k;
                for (j = 0; j < tec.list_TECComponents.Count; j++)
                {
                    if ((tec.list_TECComponents[j].m_id > 100) && (tec.list_TECComponents[j].m_id < 500)) {
                        for (k = 0; k < tec.list_TECComponents[j].TG.Count; k ++) {
                            sensorId2TG[t] = tec.list_TECComponents[j].TG[k];
                            t++;
                        }
                    }
                    else
                        ;
                }
            }
            else
            {// Для не ТЭЦ в полном составе (ГТП, ЩУ, ТГ)
                for (int k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                {
                    sensorId2TG[t] = tec.list_TECComponents[num_TECComponent].TG[k];

                    t++;
                }
            }

            for (int i = 0; i < sensorId2TG.Length; i++)
            {
                if (!(sensorId2TG[i] == null))
                {
                    //Идентификаторы факт. (АСКУЭ)
                    for (TG.ID_TIME it = TG.ID_TIME.MINUTES; it < TG.ID_TIME.COUNT_ID_TIME; it ++) {
                        switch (tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_ASKUE - (int)CONN_SETT_TYPE.DATA_ASKUE])
                        {
                            case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                                //Общий источник для всех ТЭЦ
                                if (sensorsStrings_Fact[(int)it].Equals(string.Empty) == true)
                                {
                                    sensorsStrings_Fact[(int)it] = sensorId2TG[i].ids_fact[(int)it].ToString();
                                }
                                else
                                {
                                    sensorsStrings_Fact[(int)it] += ", " + sensorId2TG[i].ids_fact[(int)it].ToString();
                                }
                                break;
                            case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                                //Источник для каждой ТЭЦ свой
                                if (tec.type () == TEC.TEC_TYPE.BIYSK) {
                                    //Как для "Общий источник для всех ТЭЦ"
                                    if (sensorsStrings_Fact[(int)it].Equals(string.Empty) == true)
                                    {
                                        sensorsStrings_Fact[(int)it] = sensorId2TG[i].ids_fact[(int)it].ToString();
                                    }
                                    else
                                    {
                                        sensorsStrings_Fact[(int)it] += ", " + sensorId2TG[i].ids_fact[(int)it].ToString();
                                    }
                                }
                                else {
                                    if (sensorsStrings_Fact[(int)it].Equals(string.Empty) == true)
                                    {
                                        sensorsStrings_Fact[(int)it] = "SENSORS.ID = " + sensorId2TG[i].ids_fact[(int)it].ToString();
                                    }
                                    else
                                    {
                                        sensorsStrings_Fact[(int)it] += " OR SENSORS.ID = " + sensorId2TG[i].ids_fact[(int)it].ToString();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    //Идентификаторв ТМ (СОТИАССО)
                    switch (tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
                    {
                        case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                            //Общий источник для всех ТЭЦ
                            if (sensorsString_TM.Equals(string.Empty) == true)
                            {
                                sensorsString_TM = sensorId2TG[i].id_tm.ToString();
                            }
                            else
                            {
                                sensorsString_TM += ", " + sensorId2TG[i].id_tm.ToString();
                            }
                            break;
                        case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                            //Источник для каждой ТЭЦ свой
                            if (sensorsString_TM.Equals(string.Empty) == true)
                            {
                                sensorsString_TM = "[NAME_TABLE].ID = " + sensorId2TG[i].id_tm.ToString();
                            }
                            else
                            {
                                sensorsString_TM += " OR [NAME_TABLE].ID = " + sensorId2TG[i].id_tm.ToString();
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //ErrorReportSensors(ref table);

                    return false;
                }
            }

            return true;
        }

        //protected void ErrorReportSensors(ref DataTable src)
        //{
        //    string error = "Ошибка определения идентификаторов датчиков в строке ";
        //    for (int j = 0; j < src.Rows.Count; j++)
        //        error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

        //    error = error.Substring(0, error.LastIndexOf(","));
        //    ErrorReport(error);
        //}

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public void ErrorReport(string error_string)
        {
            m_report.last_error = error_string;
            m_report.last_time_error = DateTime.Now;
            m_report.errored_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        protected void ActionReport(string action_string)
        {
            m_report.last_action = action_string;
            m_report.last_time_action = DateTime.Now;
            m_report.actioned_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        public void SetDelegate(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }
    }
}
