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
            Initialize ();
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

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
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
                states.Add((int)StatesMachine.PPBRValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - AdminMC::GetRDGValues () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogToFile("Исключение обращения к переменной (semaState)", false, false, false);
                    Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                    Logging.Logg().LogToFile(e.ToString(), false, false, false);
                    Logging.Logg().LogUnlock();
                }
            }
        }

        public override void getCurRDGValues(HAdmin source)
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].ppbr[0] = ((AdminMC)source).m_curRDGValues[i].ppbr[0];
                m_curRDGValues[i].ppbr[1] = ((AdminMC)source).m_curRDGValues[i].ppbr[1];
                m_curRDGValues[i].ppbr[2] = ((AdminMC)source).m_curRDGValues[i].ppbr[2];
            }
        }

        public override void CopyCurToPrevRDGValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].ppbr[0] = m_curRDGValues[i].ppbr[0];
                m_prevRDGValues[i].ppbr[1] = m_curRDGValues[i].ppbr[1];
                m_prevRDGValues[i].ppbr[2] = m_curRDGValues[i].ppbr[2];
            }
        }

        public override void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].ppbr[0] = m_curRDGValues[i].ppbr[1] = m_curRDGValues[i].ppbr[2] = 0.0;
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
                    if (! (m_prevRDGValues[i].ppbr [j] == m_curRDGValues[i].ppbr [j]))
                        return true;
                    else
                        ;
                }
            }

            return false;
        }
    }
}
