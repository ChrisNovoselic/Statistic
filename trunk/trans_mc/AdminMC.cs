﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;
using StatisticCommon;
using StatisticTransModes;
using StatisticTrans;

namespace trans_mc
{
    public class AdminMC : AdminModes
    {
        string m_strMCServiceHost;


        //TransPBR tPBR = new TransPBR();

        protected enum StatesMachine
        {
            InitIGO,
            PPBRValues,
            PPBRDates,
        }

        public AdminMC(string strMCServiceHost)
            : base()
        {
            m_strMCServiceHost = strMCServiceHost;
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {

        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            string query = "PPBR";
            int i = -1;

            //Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вХод...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);

            query += ";";
            for (i = 0; i < comp.m_listMCentreId.Count; i++)
            {
                query += comp.m_listMCentreId[i];

                if ((i + 1) < comp.m_listMCentreId.Count) query += ","; else ;
            }

            //tPBR.GetComp(str, "MC");
            query += ";";
            query += date.ToOADate().ToString();

            DbMCSources.Sources().Request(m_IdListenerCurrent, query); //

            //Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;

            return iRes;
        }

        protected override int GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1,
                hour = -1,
                offsetPBR = 2
                , offset = 0;

            for (i = 0; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                    if ((hour == 0) && (!(((DateTime)table.Rows[i]["DATE_PBR"]).Day == date.Day)))
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

                    //Копирование при переходе лето-зима (-1)                        
                    if ((m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true) && (hour == (HAdmin.SeasonDateTime.Hour - 0)))
                    {
                        m_curRDGValues[hour].pbr_number = m_curRDGValues[hour - 1].pbr_number;
                        m_curRDGValues[hour].dtRecUpdate = m_curRDGValues[hour - 1].dtRecUpdate;

                        m_curRDGValues[hour].pbr = m_curRDGValues[hour - 1].pbr;

                        m_curRDGValues[hour].pmin = m_curRDGValues[hour - 1].pmin;

                        m_curRDGValues[hour].pmax = m_curRDGValues[hour - 1].pmax;

                        m_curRDGValues[hour].recomendation = m_curRDGValues[hour - 1].recomendation;
                        m_curRDGValues[hour].deviationPercent = m_curRDGValues[hour - 1].deviationPercent;
                        m_curRDGValues[hour].deviation = m_curRDGValues[hour - 1].deviation;

                        offset++;
                    }
                    else
                    {
                    }
                }
                catch { }
            }
            //tPBR.InsertData();
            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
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

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;
            string msg = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    msg = @"Инициализация объектов Modes-Centre";
                    ActionReport(msg);
                    Logging.Logg().Debug(@"AdminMC::StateResponse () - " + msg, Logging.INDEX_MESSAGE.NOT_SET);
                    break;
                case (int)StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS);
                    break;
                case (int)StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date) && (m_ignore_date == false))
                    {
                        result = -1;
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

            //Logging.Logg().Debug(@"AdminMC::StateRequest () - state=" + state.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        
            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                case (int)StatesMachine.PPBRValues:
                case (int)StatesMachine.PPBRDates:
                    //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    iRes = response(0, out error, out table/*, false*/);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;
            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    result = 0;
                    if (result == 0)
                    {
                        Logging.Logg().Debug(@"AdminMC::StateResponse () - Инициализация объектов Modes-Centre", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = GetPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                default:
                    break;
            }

            if (result == 0)
                ReportClear(false);
            else
                ;

            //Logging.Logg().Debug(@"AdminMC::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
            //tPBR.InsertData();
            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

            bool bClear = false;

            delegateStopWait();

            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    ErrorReport("Ошибка инициализации объектов Modes-Centre. Переход в ожидание.");
                    break;
                case (int)StatesMachine.PPBRValues:
                    if (request == 0)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else
                    {
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (request == 0)
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

            if (!(errorData == null)) errorData(); else ;

            return reasonRes;
        }

        protected override void StateWarnings(int state, int request, int result)
        {
        }

        private bool InitIGO()
        {
            bool bRes = false;

            string query = "InitIGO;";

            int i = -1;
            for (i = 0; i < m_listModesId.Count; i++)
            {
                query += m_listModesId[i];

                if ((i + 1) < m_listModesId.Count)
                    query += ", ";
                else
                    ;
            }

            DbMCSources.Sources().Request(m_IdListenerCurrent, query); //List IGO FROM Modfes-Centre

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
                //    AddState((int)StatesMachine.InitIGO);
                //}
                //else
                //    ;

                AddState((int)StatesMachine.PPBRValues);

                Run(@"AdminMC::GetRDGValues ()");
            }
        }
    }
}
