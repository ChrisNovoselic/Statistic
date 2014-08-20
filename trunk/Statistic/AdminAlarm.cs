using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

namespace StatisticCommon
{
    public class AdminAlarm
    {
        //List <TEC> m_listTEC;
        List <AdminAlarmTEC> m_listAdminAlarmTEC;

        private object lockValue;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerAlarm;

        private bool m_bIsActive;

        protected void Initialize () {
        }

        public void InitTEC(List <TEC> listTEC)
        {
            foreach (TEC t in listTEC)
            {
                if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id))
                {
                    m_listAdminAlarmTEC.Add (new AdminAlarmTEC (null, null));
                    m_listAdminAlarmTEC [m_listAdminAlarmTEC.Count - 1].InitTEC (new List <TEC> () {t});
                }
                else
                    ;
            }
        }

        public AdminAlarm(HReports rep)
        {
            m_listAdminAlarmTEC = new List<AdminAlarmTEC> ();

            lockValue = new object ();
        }

        public void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                m_bIsActive = active;

            foreach (AdminAlarmTEC aat in m_listAdminAlarmTEC)
            {
                aat.Activate(active);
            }

            if (active == true)
                m_timerAlarm.Change (5 * 60 * 60, 5 * 60 * 60);
            else
                m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            foreach (AdminAlarmTEC aat in m_listAdminAlarmTEC)
            {
                aat.Start();
            }

            //m_evTimerCurrent = new ManualResetEvent(true);
            m_timerAlarm = new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Stop()
        {
            foreach (AdminAlarmTEC aat in m_listAdminAlarmTEC)
            {
                aat.Stop();
            }

            m_timerAlarm.Dispose ();
            m_timerAlarm = null;
        }

        private void ChangeState () {
            foreach (AdminAlarmTEC aat in m_listAdminAlarmTEC)
            {
                aat.Ask ();
            }
        }

        private void TimerAlarm_Tick(Object stateInfo)
        {
            lock (lockValue)
            {
                if (m_bIsActive == true)
                {
                    ChangeState();
                }
                else
                    ;
            }
        }
    }

    public class AdminAlarmTEC : HAdmin
    {
        private enum StatesMachine
        {
            Init_TM,
            CurrentTime,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            Current_TM,
            LastMinutes_TM,
            AdminValues, //Получение административных данных
            PPBRValues,
        }

        private volatile string sensorsString_TM;
        private List<TG> m_listSensorId2TG;

        DateTime m_dtLastChangedAt;

        private AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected DataTable m_tablePPBRValuesResponse
                    //, m_tableRDGExcelValuesResponse
                    ;

        private TEC m_tec {
            get { return m_list_tec [0]; }
        }

        protected override void Initialize () {
            base.Initialize ();

            sensorsString_TM = string.Empty;
            m_listSensorId2TG = new List<TG> ();
        }

        public AdminAlarmTEC(HReports rep, bool[] arMarkSavePPBRValues)
            : base(rep)
        {
        }

        public void Ask () {
            GetRDGValues((int)s_typeFields, indxTECComponents, DateTime.Now.Date);

            try {
                semaState.Release (1);
            }
            catch (Exception e) {
                Logging.Logg().LogExceptionToFile(e, @"AdminAlarmTEC::Ask () - semaState.Release (1) - ...");
            }
        }

        public override void Activate(bool active)
        {
            base.Activate(active);

            if ((active == true) && (threadIsWorking == 1))
                Ask ();
            else
                ;
        }

        public override void Start()
        {
            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StartDbInterfaces(CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminAlarm::Start () - m_list_tec == null");

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StopDbInterfaces();
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminAlarm::Stop () - m_list_tec == null");
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            return bRes;
        }

        public override void  ClearValues()
        {
        }

        public override void CopyCurToPrevRDGValues()
        {
        }

        public override void getCurRDGValues(HAdmin source)
        {
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
        }

        protected override bool GetPPBRDatesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool GetPPBRValuesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        public override void GetRDGValues(int mode, int indx, DateTime date)
        {
            newState = true;
            states.Clear();

            if ((sensorsString_TM.Equals(string.Empty) == true))
                states.Add((int)StatesMachine.Init_TM);
            else ;

            states.Add((int)StatesMachine.Current_TM);
        }

        public override bool Response(int idListener, out bool error, out System.Data.DataTable table)
        {
            bool bRes = true;

            error = false;
            table = null;

            return bRes;
        }

        private TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
        {
            for (int i = 0; i < m_listSensorId2TG.Count; i++)
                switch (indxVal)
                {
                    case TG.INDEX_VALUE.FACT:
                        if (m_listSensorId2TG[i].ids_fact[(int)id_type] == id)
                            return m_listSensorId2TG[i];
                        else
                            ;
                        break;
                    case TG.INDEX_VALUE.TM:
                        if (m_listSensorId2TG[i].id_tm == id)
                            return m_listSensorId2TG[i];
                        else
                            ;
                        break;
                    default:
                        break;
                }

            return null;
        }

        private bool GetSensorsTEC()
        {
            bool bRes = true;

            int j = -1;
            for (j = 0; j < m_tec.list_TECComponents.Count; j++)
                if (m_tec.list_TECComponents[j].m_id > 1000) m_listSensorId2TG.Add(m_tec.list_TECComponents[j].TG[0]); else ;

            sensorsString_TM = string.Empty;

            for (int i = 0; i < m_listSensorId2TG.Count; i++)
            {
                if (!(m_listSensorId2TG[i] == null))
                {
                    if (sensorsString_TM.Equals(string.Empty) == false)
                        switch (m_tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
                        {
                            case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                                //Общий источник для всех ТЭЦ
                                sensorsString_TM += @", "; //@" OR ";
                                break;
                            case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                                //Источник для каждой ТЭЦ свой
                                sensorsString_TM += @" OR ";
                                break;
                            default:
                                break;
                        }
                    else
                        ;

                    switch (m_tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
                    {
                        case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                            //Общий источник для всех ТЭЦ
                            sensorsString_TM += m_listSensorId2TG[i].id_tm.ToString();
                            break;
                        case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                            //Источник для каждой ТЭЦ свой
                            sensorsString_TM += @"[dbo].[NAME_TABLE].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
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

            return bRes;
        }

        private void GetCurrentTMRequest()
        {
            m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMRequest(sensorsString_TM));
        }

        private bool GetCurrentTMResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1,
                id = -1;
            double value = -1;
            DateTime dtLastChangedAt =
                m_dtLastChangedAt = DateTime.Now;
            TG tgTmp;

            foreach (TECComponent g in m_tec.list_TECComponents)
            {
                foreach (TG t in g.TG)
                {
                    t.power_TM = 0;
                }
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                    return false;
                else
                    ;

                tgTmp = FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                if (tgTmp == null)
                    return false;
                else
                    ;

                if (!(table.Rows[i]["value"] is DBNull))
                    if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                        return false;
                    else
                        ;
                else
                    value = 0.0;

                if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                    return false;
                else
                    ;

                if (m_dtLastChangedAt > dtLastChangedAt)
                    m_dtLastChangedAt = dtLastChangedAt;
                else
                    ;

                switch (m_tec.type())
                {
                    case TEC.TEC_TYPE.COMMON:
                        break;
                    case TEC.TEC_TYPE.BIYSK:
                        //value *= 20;
                        break;
                    default:
                        break;
                }

                tgTmp.power_TM = value;
            }

            return bRes;
        }

        protected override bool StateCheckResponse(int state, out bool error, out System.Data.DataTable table)
        {
            error = false;
            table = null;

            switch (state)
            {
                case (int)StatesMachine.Init_TM:
                    return true;
                case (int)StatesMachine.Current_TM:
                case (int)StatesMachine.LastMinutes_TM:
                    return m_tec.Response(CONN_SETT_TYPE.DATA_SOTIASSO, out error, out table);
                case (int)StatesMachine.PPBRValues:
                    return DbSources.Sources().Response(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], out error, out table);
                case (int)StatesMachine.AdminValues:
                    return DbSources.Sources().Response(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], out error, out table);
                default:
                    break;
            }

            error = true;

            return false;
        }

        protected override void StateErrors(int state, bool response)
        {
            switch (state)
            {
                case (int)StatesMachine.Init_TM:
                    break;
                case (int)StatesMachine.Current_TM:
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    break;
            }
        }

        protected override bool StateRequest(int state)
        {
            bool bRes = true;

            switch (state)
            {
                case (int)StatesMachine.Init_TM:
                    break;
                case (int)StatesMachine.Current_TM:
                    GetCurrentTMRequest ();
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    break;
            }

            return bRes;
        }

        protected override bool StateResponse(int state, System.Data.DataTable table)
        {
            bool result = true;

            switch (state)
            {
                case (int)StatesMachine.Init_TM:
                    switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                            case TEC.TEC_TYPE.BIYSK:
                                result = GetSensorsTEC();
                                break;
                            default:
                                break;
                        }
                        if (result == true)
                        {
                        }
                        else
                            ;
                    break;
                case (int)StatesMachine.Current_TM:
                    result = GetCurrentTMResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
