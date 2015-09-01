﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mt
{
    public class AdminMT : AdminModes
    {
        protected enum StatesMachine
        {
            PPBRValues,
            PPBRDates,
        }

        public AdminMT () : base ()
        {
        }        

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            string query = string.Empty;
            int i = -1;
            TimeSpan ts = GetUTCOffsetOfMoscowTimeZone();

            query += @"SELECT [objName], [idFactor], [PBR_NUMBER], [Datetime], [Value_MBT] as VALUE FROM [dbo].[v_ALL_PARAM_MODES_BIYSK]" +
                @" WHERE [ID_Type_Data] = 3" +
                @" AND [objName] = '" + comp.m_listMTermId [0] + @"'" +
                @" AND [Datetime] > " + @"'" + date.Date.Add(- ts).ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                + @" AND [PBR_NUMBER] > 0"
                + @" ORDER BY [Datetime], [PBR_NUMBER]"
                ;

            DbMCSources.Sources().Request(m_IdListenerCurrent, query);

            Logging.Logg().Debug("AdminMT::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1,
                hour = -1,
                PBRNumber = -1;
            TimeSpan ts = GetUTCOffsetOfMoscowTimeZone();
            DataRow []hourRows;

            for (hour = 1; hour < 25; hour++)
            {
                try
                    {
                        //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"'");
                        //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours) + @"'");
                        //hourRows = table.Select(@"Datetime=#" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"#");
                        hourRows = table.Select(@"Datetime=#" + date.Date.AddHours(hour - ts.Hours).ToString(@"yyyy-MM-dd HH:00:00.000") + @"#");

                        PBRNumber = -1;

                        m_curRDGValues[hour - 1].pbr = -1.0F;
                        m_curRDGValues[hour - 1].pmin = -1.0F;
                        m_curRDGValues[hour - 1].pmax = -1.0F;

                        if (hourRows.Length > 0) {
                            for (i = 0; i < hourRows.Length; i ++) {
                                if (!(PBRNumber > Int32.Parse(hourRows[i][@"PBR_NUMBER"].ToString())))
                                {
                                    PBRNumber = Int32.Parse(hourRows[i][@"PBR_NUMBER"].ToString());

                                    for (j = 0; j < hourRows [i].Table.Columns.Count; j ++) {
                                        Console.Write(@"[" + hourRows[i].Table.Columns[j].ColumnName + @"] = " + hourRows[i][hourRows[i].Table.Columns[j].ColumnName] + @"; ");
                                    }
                                    Console.WriteLine(@"");

                                    switch (Int32.Parse(hourRows[i][@"idFactor"].ToString()))
                                    {
                                        case 0:
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                m_curRDGValues[hour - 1].pbr = (double)hourRows[i][@"VALUE"];
                                            else
                                                m_curRDGValues[hour - 1].pbr = 0;
                                            break;
                                        case 1:
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                m_curRDGValues[hour - 1].pmin = (double)hourRows[i][@"VALUE"];
                                            else
                                                m_curRDGValues[hour - 1].pmin = 0;
                                            break;
                                        case 2:
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                m_curRDGValues[hour - 1].pmax = (double)hourRows[i][@"VALUE"];
                                            else
                                                m_curRDGValues[hour - 1].pmax = 0;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    ;
                            }
                        }
                        else {
                            if (hour > 1) {
                                m_curRDGValues[hour - 1].pbr = m_curRDGValues[hour - 2].pbr;
                                m_curRDGValues[hour - 1].pmin = m_curRDGValues[hour - 2].pmin;
                                m_curRDGValues[hour - 1].pmax = m_curRDGValues[hour - 2].pmax;

                                m_curRDGValues[hour - 1].pbr_number = m_curRDGValues[hour - 2].pbr_number;
                                m_curRDGValues[hour - 1].dtRecUpdate = m_curRDGValues[hour - 2].dtRecUpdate;
                            }
                            else {
                            }
                        }

                        if (PBRNumber > 0) {
                            m_curRDGValues[hour - 1].pbr_number = @"ПБР" + PBRNumber;

                            if (hour > 1) {
                                if (m_curRDGValues[hour - 1].pbr < 0)
                                    m_curRDGValues[hour - 1].pbr = m_curRDGValues[hour - 2].pbr;
                                else
                                    ;

                                if (m_curRDGValues[hour - 1].pmin < 0)
                                    m_curRDGValues[hour - 1].pmin = m_curRDGValues[hour - 2].pmin;
                                else
                                    ;

                                if (m_curRDGValues[hour - 1].pmax < 0)
                                    m_curRDGValues[hour - 1].pmax = m_curRDGValues[hour - 2].pmax;
                                else
                                    ;
                            }
                            else
                                ;

                            int hh = -1;
                            for (hh = hour; hh > 0; hh --) {
                                if (m_curRDGValues[hh - 1].pbr_number.Equals (string.Empty) == false)
                                    if (PBRNumber < Int32.Parse(m_curRDGValues[hh - 1].pbr_number.Substring (3)))
                                    {
                                        PBRNumber = Int32.Parse(m_curRDGValues[hh - 1].pbr_number.Substring (3));

                                        m_curRDGValues[hour - 1].pbr = m_curRDGValues[hh - 1].pbr;
                                        m_curRDGValues[hour - 1].pmin = m_curRDGValues[hh - 1].pmin;
                                        m_curRDGValues[hour - 1].pmax = m_curRDGValues[hh - 1].pmax;

                                        m_curRDGValues[hour - 1].pbr_number = m_curRDGValues[hh - 1].pbr_number;

                                        //break;
                                    } else {
                                    }
                                else
                                    ;
                            }
                        }
                        else
                            ; //m_curRDGValues[hour - 1].pbr_number = GetPBRNumber (hour);

                        m_curRDGValues[hour - 1].dtRecUpdate = DateTime.MinValue;
                        
                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
                    }
                    catch (Exception e) {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"AdminMT::GetPPBRValuesResponse () - ...");
                    }
            }

            return iRes;
        }

        protected override bool InitDbInterfaces () {
            bool bRes = true;
            int i = -1;

            if (m_list_tec.Count > 0) {
                m_IdListenerCurrent = DbMCSources.Sources().Register(m_list_tec[0].connSetts[(int)StatisticCommon.CONN_SETT_TYPE.MTERM], true, @"Modes-Terminale");

                bRes = false;
            }
            else
                ;

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
                case (int)StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    if (indxTECComponents < allTECComponents.Count)
                        GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS);
                    else
                        result = -1;
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

            Logging.Logg().Debug(@"AdminMT::StateRequest () - state=" + state.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            switch (state)
            {
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
                ReportClear (false);
            else
                ;

            Logging.Logg().Debug(@"AdminMT::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

            bool bClear = false;

            delegateStopWait();

            switch (state)
            {
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