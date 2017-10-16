﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;
using StatisticCommon;
using StatisticTransModes;
using System.Collections.ObjectModel;

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

        protected override void getPPBRDatesRequest(DateTime date)
        {
        }

        protected override void getPPBRValuesRequest(TEC t, TECComponent comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
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

            Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.D_002);
        }

        protected override int getPPBRDatesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;

            return iRes;
        }

        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1,
                hour = -1,
                offsetPBR = 2
                , offset = 0;
            string msgDebug = string.Format(@"Модес-Центр, получено строк={0}, [{1}] на {2}: ", table.Rows.Count, @"PBR_NUMBER", date);

            for (i = 0; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                    if ((hour == 0)
                        && (!(((DateTime)table.Rows[i]["DATE_PBR"]).Day == date.Day))) {
                    // это крайний час текущих суток
                        hour = 24;

                        msgDebug = msgDebug.Replace(@"PBR_NUMBER", table.Rows[i][@"PBR_NUMBER"].ToString());
                    } else
                        if (hour == 0)
                        // это предыдущие сутки
                            continue;
                        else
                            ;

                    hour += offset;

                    m_curRDGValues[hour - 1].pbr_number = table.Rows[i][@"PBR_NUMBER"].ToString();
                    // проблема решается ранее, в 'DbMCInterface::GetData'
                    //if (m_curRDGValues[hour - 1].pbr_number.IndexOf(@"ПБР") < 0)
                    //    m_curRDGValues[hour - 1].pbr_number = @"ПБР" + m_curRDGValues[hour - 1].pbr_number;
                    //else
                    //    ;

                    //Logging.Logg().Debug(string.Format(@"AdminMC::getPPBRValuesResponse () - hour={0}, PBRNumber={1}...", hour, m_curRDGValues[hour - 1].pbr_number), Logging.INDEX_MESSAGE.NOT_SET);

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
                    if ((m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true) && (hour == (HAdmin.SeasonDateTime.Hour - 0))) {
                        m_curRDGValues[hour].From(m_curRDGValues[hour - 1]);

                        offset++;
                    } else {
                    }

                    msgDebug += string.Format(@"[Час={0}, знач={1}],", hour, m_curRDGValues[hour - 1].pbr);
                } catch(Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"AdminMC::getPPBRValuesResponse () - строка={0}", i), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            msgDebug = msgDebug.Substring(0, msgDebug.Length - 1); // удалить лишнюю запятую
            Logging.Logg().Debug(msgDebug, Logging.INDEX_MESSAGE.D_002);

            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
            bool bRes = true;
            int i = -1;

            DbMCSources.Sources().SetMCApiHandler(dbMCSources_OnEventHandler);
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

        private void dbMCSources_OnEventHandler(object obj)
        {
            DbMCInterface.ID_MC_EVENT id_event = (DbMCInterface.ID_MC_EVENT)(obj as object[])[0];

            if (id_event == DbMCInterface.ID_MC_EVENT.GENOBJECT_MODIFIED) {
                Dictionary<DateTime, List<int>> equipments;

                string msg = string.Empty
                    , listEquipment = string.Empty;

                equipments = (obj as object[])[1] as Dictionary<DateTime, List<int>>;

                msg = string.Format(@"::mcApi_OnData53500Modified() - обработчик события - изменения[кол-во={1}]{0}для оборудования {2}..."
                        , Environment.NewLine, equipments.Count, @"СПИСОК");

                foreach (KeyValuePair<DateTime, List<int>> pair in equipments) {
                    listEquipment += string.Format(@"[{Дата=0}, список=({1})],", pair.Key.ToString(), string.Join(", ", pair.Value));
                }

                listEquipment = listEquipment.Remove(listEquipment.Length - 1);

                msg = msg.Replace(@"СПИСОК", listEquipment);

                Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);
            } else if (id_event == DbMCInterface.ID_MC_EVENT.RELOAD_PLAN_VALUES) {
                DateTime dateTarget;
                ReadOnlyCollection<Guid>  makets;
                string abbr = string.Empty
                    , taskModes = string.Empty;

                dateTarget = (DateTime)(obj as object[])[1];
                makets = (obj as object[])[2] as ReadOnlyCollection<Guid>;
                abbr = (string)(obj as object[])[3];
                taskModes = (string)(obj as object[])[4];

                Logging.Logg().Action(string.Format(@"::mcApi_OnMaket53500Changed() - обработчик события - переопубликация[на дату={0}, кол-во макетов={1}], Аббр={2}, описание={3}..."
                    , dateTarget.ToString(), makets.Count, abbr, taskModes)
                , Logging.INDEX_MESSAGE.NOT_SET);
            } else if (id_event == DbMCInterface.ID_MC_EVENT.NEW_PLAN_VALUES) {
                DateTime day
                    , version;
                string pbr_number = string.Empty
                    , id_mc_tec = string.Empty;
                int id_gate = -1;

                day = (DateTime)(obj as object[])[1];
                pbr_number = (string)(obj as object[])[2];
                version = (DateTime)(obj as object[])[3];
                id_mc_tec = (string)(obj as object[])[4];
                id_gate = (int)(obj as object[])[5];

                Logging.Logg().Action(string.Format(@"::mcApi_OnPlanDataChanged() - обработчик события - новый план[на дату={0}, номер={1}, от={2}, для подразделения={3}, IdGate={4}]..."
                    , day.ToString(), pbr_number, version.ToString(), id_mc_tec, id_gate)
                , Logging.INDEX_MESSAGE.NOT_SET);
            } else
                ;
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
                    getPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date/*, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS*/);
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
                    iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
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

                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate, true);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    clearPPBRDates();
                    result = getPPBRDatesResponse(table as DataTable, m_curDate);
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
                ClearValues();


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

        public override void GetRDGValues(/*int TYPE_FIELDS mode,*/ int indx, DateTime date)
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
