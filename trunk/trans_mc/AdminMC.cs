using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;
using StatisticCommon;
using StatisticTransModes;

namespace trans_mc
{
    public class AdminMC : AdminModes
    {
        string m_strMCServiceHost;

        protected enum StatesMachine
        {
            InitIGO,
            PPBRValues,
            PPBRDates,
        }

        public AdminMC (string strMCServiceHost) : base ()
        {
            m_strMCServiceHost = strMCServiceHost;
        }

        protected override void GetPPBRDatesRequest(DateTime date) {
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            string query = "PPBR";
            int i = -1;

            Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вХод...: query=" + query);

            query += ";";
            for (i = 0; i < comp.m_listMCentreId.Count; i++)
            {
                query += comp.m_listMCentreId[i];

                if ((i + 1) < comp.m_listMCentreId.Count) query += ","; else ;
            }

            query += ";";
            query += date.ToOADate ().ToString ();

            DbMCSources.Sources().Request(m_IdListenerCurrent, query); //

            Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вЫход...: query=" + query);
        }

        protected override bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;
            int i = -1, j = -1,
                hour = -1,
                offsetPBR = 2
                , offset = 0;

            for (i = 0; i < table.Rows.Count; i ++)
            {
                try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                        if ((hour == 0) && (! (((DateTime)table.Rows[i]["DATE_PBR"]).Day == date.Day)))
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

                        hour += offset;

                        m_curRDGValues[hour - 1].pbr_number = table.Rows[i][@"PBR_NUMBER"].ToString();

                        //for (j = 0; j < 3 /*4 для SN???*/; j ++)
                        //{
                            j = 0;
                            if (!(table.Rows[i][offsetPBR + j] is DBNull))
                                m_curRDGValues[hour - 1].pbr = (double)table.Rows[i][offsetPBR + j];
                            else
                                m_curRDGValues[hour - 1].pbr = 0;
                        //}

                        j = 1;
                        if (!(table.Rows[i][offsetPBR + j] is DBNull))
                            m_curRDGValues[hour - 1].pmin = (double)table.Rows[i][offsetPBR + j];
                        else
                            m_curRDGValues[hour - 1].pmin = 0;

                        j = 2;
                        if (!(table.Rows[i][offsetPBR + j] is DBNull))
                            m_curRDGValues[hour - 1].pmax = (double)table.Rows[i][offsetPBR + j];
                        else
                            m_curRDGValues[hour - 1].pmax = 0;

                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;

                        if ((!(HourSeason < 0)) && (hour == (HourSeason - 0)))
                        {
                            m_curRDGValues[hour].pbr_number = m_curRDGValues[hour - 1].pbr_number;

                            m_curRDGValues[hour].pbr = m_curRDGValues[hour - 1].pbr;

                            m_curRDGValues[hour].pmin = m_curRDGValues[hour - 1].pmin;

                            m_curRDGValues[hour].pmax = m_curRDGValues[hour - 1].pmax;

                            m_curRDGValues[hour].recomendation = m_curRDGValues[hour - 1].recomendation;
                            m_curRDGValues[hour].deviationPercent = m_curRDGValues[hour - 1].deviationPercent;
                            m_curRDGValues[hour].deviation = m_curRDGValues[hour - 1].deviation;

                            offset ++;
                        }
                        else
                        {
                        }
                    }
                    catch { }
            }

            return bRes;
        }

        protected override bool InitDbInterfaces () {
            bool bRes = true;
            int i = -1;

            m_IdListenerCurrent = DbMCSources.Sources().Register(m_strMCServiceHost, true, @"Modes-Centre");

            //for (i = 0; i < allTECComponents.Count; i ++)
            //{
            //    if (modeTECComponent (i) == FormChangeMode.MODE_TECCOMPONENT.GTP)
            //    {
            //        m_listMCId.Add (allTECComponents [i].m_MCId.ToString ());
            //        //m_listDbInterfaces[0].ListenerRegister();
            //    }
            //    else
            //        ;
            //}

            //List <Modes.BusinessLogic.IGenObject> listIGO = (((DbMCInterface)m_listDbInterfaces[0]).GetListIGO(listMCId));

            return bRes;
        }

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            string msg = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    msg = @"Инициализация объектов Modes-Centre";
                    ActionReport(msg);
                    Logging.Logg().Debug(@"AdminMC::StateResponse () - " + msg);
                    break;
                case (int)StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS);
                    break;
                case (int)StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date) && (m_ignore_date == false))
                    {
                        result = false;
                        break;
                    }
                    else
                        ;
                    ActionReport("Получение списка сохранённых часовых значений.");
                    //GetPPBRDatesRequest(m_curDate);
                    break;
                default:
                    break;
            }

            Logging.Logg().Debug(@"AdminMC::StateRequest () - state=" + state.ToString() + @" - вЫход...");

            return result;
        }

        protected override bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table)
        {
            bool bRes = false;

            error = true;
            table = null;

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                case (int)StatesMachine.PPBRValues:
                case (int)StatesMachine.PPBRDates:
                    //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    bRes = Response(0, out error, out table/*, false*/);
                    break;
                default:
                    break;
            }

            return bRes;
        }

        protected override bool StateResponse(int /*StatesMachine*/ state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    result = true;
                    if (result == true)
                    {
                        Logging.Logg().Debug(@"AdminMC::StateResponse () - Инициализация объектов Modes-Centre");
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = GetPPBRValuesResponse(table, m_curDate);
                    if (result == true)
                    {
                        readyData(m_curDate);
                    }
                    else
                        ;
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
                default:
                    break;
            }

            if (result == true)
                FormMainBaseWithStatusStrip.m_report.ClearStates ();
            else
                ;

            Logging.Logg().Debug(@"AdminMC::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...");

            return result;
        }

        protected override void StateErrors(int /*StatesMachine*/ state, bool response)
        {
            bool bClear = false;

            delegateStopWait();

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    ErrorReport("Ошибка инициализации объектов Modes-Centre. Переход в ожидание.");
                    break;
                case (int)StatesMachine.PPBRValues:
                    if (response)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else
                    {
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора сохранённых часовых значений (PPBR). Переход в ожидание.");
                        //saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений (PPBR). Переход в ожидание.");
                        //saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        //semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                default:
                    break;
            }

            if (bClear)
            {
                ClearValues();
                //ClearTables();
            }
            else
                ;

            if (! (errorData == null)) errorData(); else ;
        }

        private bool InitIGO ()
        {
            bool bRes = false;

            string query = "InitIGO;";

            int i = -1;
            for (i = 0; i < m_listModesId.Count; i ++)
            {
                query += m_listModesId[i];

                if ((i + 1) < m_listModesId.Count)
                    query += ", ";
                else
                    ;
            }

            DbMCSources.Sources ().Request(m_IdListenerCurrent, query); //List IGO FROM Modfes-Centre

            bRes = true;
            return bRes;
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
            delegateStartWait();
            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //if (m_listIGO.Count == 0)
                //{
                //    states.Add((int)StatesMachine.InitIGO);
                //}
                //else
                //    ;

                states.Add((int)StatesMachine.PPBRValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminMC::GetRDGValues () - semaState.Release(1)");
                }
            }
        }
    }
}
