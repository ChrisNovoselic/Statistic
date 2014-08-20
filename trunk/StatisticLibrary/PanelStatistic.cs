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
        //Копия для 'class TECComponentBase' - из 'PanelStatisticView' класса требуется исключть???
        //protected volatile string sensorsString_TM = string.Empty;
        //protected volatile string[] sensorsStrings_Fact = { string.Empty, string.Empty }; //Только для особенной ТЭЦ (Бийск) - 3-х, 30-ти мин идентификаторы

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        //public TG[] sensorId2TG;

        public volatile TEC tec;
        public volatile int indx_TEC;
        public volatile int indx_TECComponent;
        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public List<TECComponentBase> m_list_TECComponents;

        public List <TG> listTG {
            get {
                if (indx_TECComponent < 0)
                    return tec.m_listTG;
                else
                    return tec.list_TECComponents [indx_TECComponent].m_listTG;
            }
        }

        protected StatusStrip stsStrip;
        protected HReports m_report;

        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

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
