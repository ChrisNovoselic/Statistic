using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;
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

        public AdminMT (HReports rep) : base (rep)
        {
        }        

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            string query = string.Empty;
            int i = -1;

            query += @"SELECT [objName], [idFactor], [PBR_NUMBER], [Datetime], [Value_MBT] as VALUE FROM [dbo].[v_ALL_PARAM_MODES_BIYSK]" +
                @" WHERE [ID_Type_Data] = 3" +
                @" AND [objName] = '" + comp.m_listMTermId [0] + @"'" +
                @" AND [Datetime] > " + @"'" + date.Date.Add(- GetUTCOffsetOfCurrentTimeZone()).ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                + @" AND [PBR_NUMBER] > 0"
                //+ @" ORDER BY [Datetime]"
                ;

            Logging.Logg().LogDebugToFile("AdminMT::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS): query=" + query);

            DbMCSources.Sources().Request(m_IdListenerCurrent, query); //
        }

        protected override bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;
            int i = -1, j = -1,
                hour = -1,
                PBRNumber = -1;
            TimeSpan ts = GetUTCOffsetOfCurrentTimeZone();
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
                            }
                            else {
                            }
                        }

                        if (PBRNumber > 0)
                            m_curRDGValues[hour - 1].pbr_number = @"ПБР" + PBRNumber;
                        else
                            ; //m_curRDGValues[hour - 1].pbr_number = GetPBRNumber (hour);

                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
                    }
                    catch (Exception e) {
                        Logging.Logg().LogExceptionToFile(e, @"AdminMT::GetPPBRValuesResponse () - ...");
                    }
            }

            return bRes;
        }

        protected override bool InitDbInterfaces () {
            bool bRes = true;
            int i = -1;

            if (m_list_tec.Count > 0) {
                m_IdListenerCurrent = DbMCSources.Sources().Register(m_list_tec [0].connSetts [(int)CONN_SETT_TYPE.MTERM], true, @"Modes-Terminale");

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

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            string msg = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    if (indxTECComponents < allTECComponents.Count)
                        GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS);
                    else
                        result = false;
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

            return result;
        }

        protected override bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table)
        {
            bool bRes = false;

            error = true;
            table = null;

            switch (state)
            {
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
                case (int)StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = GetPPBRValuesResponse(table, m_curDate);
                    if (result == true)
                    {
                        fillData(m_curDate);
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
                m_report.errored_state = m_report.actioned_state = false;
            else
                ;

            return result;
        }

        protected override void StateErrors(int /*StatesMachine*/ state, bool response)
        {
            bool bClear = false;

            delegateStopWait();

            switch (state)
            {
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
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
            delegateStartWait();
            lock (m_lockObj)
            {
                indxTECComponents = indx;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();

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
                    Logging.Logg().LogExceptionToFile(e, "AdminMC::GetRDGValues () - semaState.Release(1)");
                }
            }
        }
    }
}
