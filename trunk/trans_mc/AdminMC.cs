using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;

namespace trans_mc
{
    class AdminMC : HAdmin
    {
        string m_strMCServiceHost;
        List<string> m_listMCId;

        protected enum StatesMachine
        {
            InitIGO,
            PPBRValues,
            PPBRDates,
        }

        public AdminMC (string strMCServiceHost, HReports rep) : base (rep)
        {
            m_strMCServiceHost = strMCServiceHost;
            
            //m_listIGO = new List<Modes.BusinessLogic.IGenObject> ();
            m_listMCId = new List<string> ();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override bool Response(int idListener, out bool error, out DataTable table/*, bool isTec*/)
        {
            return DbMCSources.Sources ().Response(m_IdListenerCurrent, out error, out table);
        }

        protected override void GetPPBRDatesRequest(DateTime date) {
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            string query = "PPBR";
            int i = -1;

            query += ";";
            for (i = 0; i < comp.m_listMCId.Count; i ++)
            {
                query += comp.m_listMCId [i];

                if ((i + 1) < comp.m_listMCId.Count) query += ","; else ;
            }

            query += ";";
            query += date.ToOADate ().ToString ();

            Logging.Logg().LogDebugToFile("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS): query=" + query);

            DbMCSources.Sources().Request(m_IdListenerCurrent, query); //
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
                offsetPBR = 2;

            for (i = 0; i < table.Rows.Count; i ++)
            {
                try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i]["DATE_PBR"]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

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
                    }
                    catch { }
            }

            return bRes;
        }

        protected bool InitDbInterfaces () {
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

        public override void Start()
        {
            InitDbInterfaces();

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            DbMCSources.Sources().UnRegister(m_IdListenerCurrent);
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
                    Logging.Logg().LogDebugToFile(@"AdminMC::StateResponse () - " + msg);
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
                        Logging.Logg().LogDebugToFile(@"AdminMC::StateResponse () - Инициализация объектов Modes-Centre");
                    }
                    else
                        ;
                    break;
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
        }

        private bool InitIGO ()
        {
            bool bRes = false;

            string query = "InitIGO;";

            int i = -1;
            for (i = 0; i < m_listMCId.Count; i ++)
            {
                query += m_listMCId [i];

                if ((i + 1) < m_listMCId.Count)
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

        public override void getCurRDGValues(HAdmin source)
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = ((AdminMC)source).m_curRDGValues[i].pbr;
                m_curRDGValues[i].pmin = ((AdminMC)source).m_curRDGValues[i].pmin;
                m_curRDGValues[i].pmax = ((AdminMC)source).m_curRDGValues[i].pmax;

                m_curRDGValues[i].pbr_number = ((AdminMC)source).m_curRDGValues[i].pbr_number;
            }
        }

        public override void CopyCurToPrevRDGValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].pbr = m_curRDGValues[i].pbr;
                m_prevRDGValues[i].pmin = m_curRDGValues[i].pmin;
                m_prevRDGValues[i].pmax = m_curRDGValues[i].pmax;

                m_prevRDGValues[i].pbr_number = m_curRDGValues[i].pbr_number;
            }
        }

        public override void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = m_curRDGValues[i].pmin = m_curRDGValues[i].pbr = 0.0;
                m_curRDGValues[i].pbr_number = string.Empty;
            }

            //CopyCurToPrevRDGValues();
        }

        public override bool WasChanged()
        {
            int i = -1, j = -1;
            
            for (i = 0; i < 24; i++)
            {
                for (j = 0; j < 3 /*4 для SN???*/; j++)
                {
                    if (! (m_prevRDGValues[i].pbr == m_curRDGValues[i].pbr))
                        return true;
                    else
                        ;
                }
            }

            return false;
        }
    }
}
