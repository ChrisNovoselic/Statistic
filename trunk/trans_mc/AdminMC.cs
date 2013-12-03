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
        protected enum StatesMachine
        {
            PPBRValues,
            PPBRDates,
        }
        
        public AdminMC () : base ()
        {
        }

        public override void Start()
        {
            if (started)
                ;
            else
                started = true;

            //GetRDGValues(m_typeFields, indxTECComponents);
        }

        public override bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
        {
            bool bRes = false;

            table = null;

            error = bRes;
            return bRes;
        }

        protected override void GetPPBRDatesRequest(DateTime date) {
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
        }

        protected override bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            switch (state)
            {
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
                    GetPPBRDatesRequest(m_curDate);
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
                    bRes = GetResponse(0, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
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
                    result = GetPPBRValuesResponse(table, m_curDate);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, m_curDate);
                    if (result)
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
    }
}
