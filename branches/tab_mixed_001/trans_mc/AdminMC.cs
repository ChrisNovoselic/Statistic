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
        List<string> m_listMCId;

        protected enum StatesMachine
        {
            InitIGO,
            PPBRValues,
            PPBRDates,
        }

        public AdminMC () : base ()
        {
            //m_listIGO = new List<Modes.BusinessLogic.IGenObject> ();
            m_listMCId = new List<string> ();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override void Resume()
        {
            base.Resume ();
        }

        public override bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
        {
            return m_listDbInterfaces[0].GetResponse(0, out error, out table);
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


            ((DbMCInterface)m_listDbInterfaces[0]).Request(0, query); //
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

        protected override bool InitDbInterfaces () {
            bool bRes = true;
            int i = -1;

            m_listDbInterfaces.Clear();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            m_listDbInterfaces.Add(new DbMCInterface("Интерфейс доступа к сервисам Modes-Centre"));

            m_listDbInterfaces[0].Start();

            m_listDbInterfaces[0].SetConnectionSettings("ne1843");

            //Для всех один - идентификатор
            m_indxDbInterfaceCurrent = 0;
            m_listDbInterfaces[0].ListenerRegister();

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

        public override void StartThreadSourceData()
        {
            InitDbInterfaces();

            base.StartThreadSourceData();
        }

        public override void StopThreadSourceData()
        {
            base.StopThreadSourceData();

            if (m_listDbInterfaces.Count > 0)
            {
                m_listDbInterfaces[0].ListenerUnregister(0);
                m_listDbInterfaces[0].Stop();
            }
            else
                ;
        }

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            switch (state)
            {
                case (int)StatesMachine.InitIGO:
                    ActionReport("Инициализация объектов Modes-Centre.");
                    //InitIGO ();
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
                    bRes = GetResponse(0, 0, out error, out table/*, false*/);
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
                errored_state = actioned_state = false;
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

            ((DbMCInterface)m_listDbInterfaces[0]).Request(0, query); //List IGO FROM Modfes-Centre

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
            }
        }

        public override void CopyCurToPrevRDGValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].pbr = m_curRDGValues[i].pbr;
                m_prevRDGValues[i].pmin = m_curRDGValues[i].pmin;
                m_prevRDGValues[i].pmax = m_curRDGValues[i].pmax;
            }
        }

        public override void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = m_curRDGValues[i].pmin = m_curRDGValues[i].pbr = 0.0;
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
