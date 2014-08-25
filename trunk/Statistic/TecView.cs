using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using StatisticCommon;

namespace Statistic
{
    public class TecView : HAdmin
    {
        public enum StatesMachine
        {
            InitSensors,
            CurrentTime,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            Current_TM_Gen,
            Current_TM_SN,
            LastMinutes_TM,
            AdminValues, //Получение административных данных
            PPBRValues,
        }

        //private volatile string sensorsString_TM;
        //private List<TG> m_listSensorId2TG;

        DateTime m_dtLastChangedAt;

        private AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected DataTable m_tablePPBRValuesResponse
                    //, m_tableRDGExcelValuesResponse
                    ;

        private StatisticCommon.TEC m_tec {
            get { return m_list_tec [0]; }
        }

        protected override void Initialize () {
            base.Initialize ();
        }

        public TecView(HReports rep, bool[] arMarkSavePPBRValues)
            : base(rep)
        {
        }

        public void ChangeState_AdminAlarm () {
            DateTime dtChangeState = DateTime.Now;

            foreach (TECComponent tc in allTECComponents) {
                if ((tc.m_id > 100) && (tc.m_id < 500)) {
                    if (semaState.WaitOne(DbInterface.MAX_WATING) == true)
                    {
                        GetRDGValues((int)s_typeFields, indxTECComponents = allTECComponents.IndexOf (tc), dtChangeState);

                        try
                        {
                            semaState.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, @"TecView::ChangeState () - semaState.Release (1) - ...");
                        }
                    }
                    else
                        ; //Превышено время ожидания - операция не выполнена
                }
                else
                    ; //Это не ГТП
            }
        }

        public override void Activate(bool active)
        {
            base.Activate(active);
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
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
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents() == true)
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request (m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetPBRDatesQuery(date, s_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        protected override void GetPPBRValuesRequest(StatisticCommon.TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
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
            m_prevDate = m_curDate;
            m_curDate = date.Date;

            newState = true;
            states.Clear();

            if ((m_tec.list_TECComponents[indx].m_SensorsString_SOTIASSO.Equals(string.Empty) == true))
            {
                states.Add((int)StatesMachine.InitSensors);
                states.Add((int)StatesMachine.CurrentTime);
            }
            else ;

            states.Add((int)StatesMachine.Current_TM_Gen);
            states.Add((int)StatesMachine.PPBRDates);
            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminDates);
            states.Add((int)StatesMachine.AdminValues);
        }

        private bool GetSensorsTEC()
        {
            if (m_tec.m_bSensorsStrings == false) {
                m_tec.initSensorsTEC ();
            }
            else
                ;

            return m_tec.m_bSensorsStrings;
        }

        private void GetCurrentTMGenRequest()
        {
            //Request(allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], allTECComponents[indxTECComponents].tec.currentTMRequest(sensorsString_TM));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMRequest(m_tec.GetSensorsString (-1, CONN_SETT_TYPE.DATA_SOTIASSO)));
        }

        private bool GetCurrentTMGenResponse(DataTable table)
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
                foreach (TG t in g.m_listTG)
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

                tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

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
                    case StatisticCommon.TEC.TEC_TYPE.COMMON:
                        break;
                    case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                        //value *= 20;
                        break;
                    default:
                        break;
                }

                tgTmp.power_TM = value;
            }

            try { m_tec.m_dtLastChangedAt_TM_Gen = HAdmin.ToCurrentTimeZone(m_tec.m_dtLastChangedAt_TM_Gen); }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, @"TecView::GetCurrentTMGenResponse () - HAdmin.ToCurrentTimeZone () - ...");
            }

            return bRes;
        }

        private void GetCurrentTMSNRequest()
        {
            m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
        }

        private bool GetCurrentTMSNResponse(DataTable table)
        {
            bool bRes = true;
            int id = -1;

            m_tec.m_dtLastChangedAt_TM_SN = DateTime.Now;

            if (table.Rows.Count == 1)
            {
                if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
                    return false;
                else
                    ;

                if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
                    if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_tec.m_dblTotalPower_TM_SN) == false)
                        return false;
                    else
                        ;
                else
                    m_tec.m_dblTotalPower_TM_SN = 0.0;

                if ((!(m_tec.m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_tec.m_dtLastChangedAt_TM_SN) == false))
                    return false;
                else
                    ;
            }
            else
            {
                bRes = false;
            }

            return bRes;
        }
            
        protected override bool StateCheckResponse(int state, out bool error, out System.Data.DataTable table)
        {
            bool bRes = true;
            
            error = false;
            table = null;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    //Ответ без запроса к БД в 'StateResponse'
                    //Запрос не требуется, т.к. сведеения получены при вызове 'InitTEC'
                    break;
                case (int)StatesMachine.CurrentTime:
                case (int)StatesMachine.Current_TM_Gen:
                case (int)StatesMachine.Current_TM_SN:
                case (int)StatesMachine.LastMinutes_TM:
                case (int)StatesMachine.PPBRDates:
                case (int)StatesMachine.PPBRValues:
                case (int)StatesMachine.AdminDates:
                case (int)StatesMachine.AdminValues:
                    bRes = Response(m_IdListenerCurrent, out error, out table);
                    break;
                default:
                    bRes = false;
                    break;
            }

            if (bRes == false)
                error = true;
            else
                ;

            return bRes;
        }

        protected override void StateErrors(int state, bool response)
        {
            string reason = string.Empty,
                    waiting = string.Empty,
                    msg = string.Empty;
            
            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    reason = @"получения идентификаторов датчиков";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.CurrentTime:
                    if (response == true)
                    {
                        reason = @"разбора";
                    }
                    else
                    {
                        reason = @"получения";
                    }

                    reason += @" текущего времени сервера";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (response == true)
                    {
                        reason = @"разбора";
                    }
                    else
                    {
                        reason = @"получения";
                    }

                    reason += @" сохранённых часовых значений (PPBR)";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    msg = @"Неизвестная команда...";
                    break;
            }

            Logging.Logg().LogErrorToFile(@"AdminAlarm::StateErrors () - ошибка " + reason + @". " + waiting + @". ");
        }

        protected override bool StateRequest(int state)
        {
            bool bRes = true;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    //Запрос не требуется...
                    break;
                case (int)StatesMachine.CurrentTime:
                    GetCurrentTimeRequest();
                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    GetCurrentTMGenRequest ();
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    GetCurrentTMSNRequest();
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRDates:
                    //ActionReport("Получение списка сохранённых часовых значений.");
                    GetPPBRDatesRequest(m_curDate);
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    bRes = false;
                    break;
            }

            return bRes;
        }

        protected override bool StateResponse(int state, System.Data.DataTable table)
        {
            bool result = true;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    switch (m_tec.type())
                        {
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
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
                case (int)StatesMachine.CurrentTime:
                    result = GetCurrentTimeResponse(table);
                    if (result == true)
                    {
                        if (using_date == true) {
                            m_prevDate = serverTime.Date;
                            m_curDate = m_prevDate;

                            if (!(setDatetime == null)) setDatetime(m_curDate); else;
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    result = GetCurrentTMGenResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    result = GetCurrentTMSNResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, m_curDate);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        public void GetCurPower()
        {
            ChangeState_CurPower ();

            try
            {
                semaState.Release(1);
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, @"TecView::GetCurPower () - semaState.Release (1) - ...");
            }
        }

        private void ChangeState_CurPower () {
            lock (m_lockObj) {
                newState = true;
                states.Clear();

                if (m_tec.m_SensorsString_SOTIASSO.Equals(string.Empty) == false)
                    states.Add((int)StatesMachine.InitSensors);
                else ;

                states.Add((int)TecView.StatesMachine.Current_TM_Gen);
                states.Add((int)TecView.StatesMachine.Current_TM_SN);
            }
        }
    }
}
