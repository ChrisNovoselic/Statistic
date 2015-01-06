using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;

using HClassLibrary;

namespace StatisticCommon
{
    public abstract class PanelStatistic : TableLayoutPanel
    {
        public abstract void Start();
        public abstract void Stop();

        public abstract void Activate(bool active);
    }

    public abstract class PanelStatisticWithTableHourRows : PanelStatistic
    {
        protected abstract void initTableHourRows();
    }

    public abstract class PanelStatisticView : PanelStatisticWithTableHourRows
    {
        //Копия для 'class TECComponentBase' - из 'PanelStatisticView' класса требуется исключть???
        //protected volatile string sensorsString_TM = string.Empty;
        //protected volatile string[] sensorsStrings_Fact = { string.Empty, string.Empty }; //Только для особенной ТЭЦ (Бийск) - 3-х, 30-ти мин идентификаторы

        //protected StatusStrip stsStrip;
        //protected HReports m_report;

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

        public void SetDelegate(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }
    }
}
