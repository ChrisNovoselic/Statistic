using System;
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

        public AdminMT()
            : base()
        {
        }

        protected override void getPPBRDatesRequest(DateTime date) { }

        protected override int getPPBRDatesResponse(DataTable table, DateTime date) { int iRes = 0; return iRes; }

        protected override void getPPBRValuesRequest(TEC t, TECComponent comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string query = string.Empty;
            DateTime dtReq = date.Date.Add(-HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE);
            int i = -1;

            query += @"SELECT [objName], [idFactor], [PBR_NUMBER], [Datetime],"
                    //+ @" SUM([Value_MBT]) as VALUE"
                    + @" [Value_MBT] as VALUE"
                + @" FROM [dbo].[v_ALL_PARAM_MODES_BIYSK]" +
                @" WHERE [ID_Type_Data] = 3" +
                @" AND [objName] IN (" + string.Join (@",", comp.m_listMTermId.ToArray()) + @")" +
                @" AND [Datetime] > " + @"'" + dtReq.ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                    + @" AND [Datetime] <= " + @"'" + dtReq.AddDays(1).ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                + @" AND [PBR_NUMBER] > 0"
                //+ @" GROUP BY [idFactor], [PBR_NUMBER], [Datetime]"
                + @" ORDER BY [Datetime], [PBR_NUMBER]"
                ;

            DbSources.Sources().Request(m_IdListenerCurrent, query);

            //Logging.Logg().Debug("AdminMT::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - �����...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1, c = -1 //���������� �����
                , MTermId = -1 //������������� ���������� ��� � ������� �����-��������
                , hour = -1 //���������� ����� (����� ����)
                , indxFactor = -1 //������ ���� �������� (0 - P, 1 - Pmin, 2 - Pmax)
                //, iMinPBRNumber = -1
                , iMaxPBRNumber = -1;
            //����� ��� ��� ���� ����� (P, Pmin, Pmax) ��������
            int[] arPBRNumber = new int[3];
            DataRow[] hourRows;
            RDGStruct[,] arRDGValues = null;

            TECComponent comp = allTECComponents[indxTECComponents];
            arRDGValues = new RDGStruct[comp.m_listMTermId.Count, m_curRDGValues.Length];

            for (c = 0; c < comp.m_listMTermId.Count; c ++) {
                MTermId = comp.m_listMTermId[c];

                for (hour = 1; hour < 25; hour++) {
                    try {
                        //������� ������ ������ ��� ���� 'hour'
                        //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"'");
                        //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours) + @"'");
                        //hourRows = table.Select(@"Datetime=#" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"#");
                        hourRows = table.Select(string.Format(@"objName={0} AND Datetime=#{1}#", MTermId, date.Date.AddHours(hour - HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE.Hours).ToString(@"yyyy-MM-dd HH:00:00.000")));

                        //��������� �������� ��� ���� ��������
                        //PBRNumber = -1;
                        arPBRNumber[0] =
                        arPBRNumber[1] =
                        arPBRNumber[2] =
                            -1; // ����� ������
                        // �������� �� ����� (0, 1, 2)
                        arRDGValues[c, hour - 1].pbr = -1.0F;
                        arRDGValues[c, hour - 1].pmin = -1.0F;
                        arRDGValues[c, hour - 1].pmax = -1.0F;
                        //���-����� �� ��������
                        arRDGValues[c, hour - 1].pbr_number = string.Empty;
                        //��������� ���������� ����� ��� ����
                        if (hourRows.Length > 0)
                            //������ ��� ������� ����� ��� ���� 'hour'
                            for (i = 0; i < hourRows.Length; i++)
                            {
                                //���������� ��� �������� � ������ ��� ����
                                indxFactor = Int32.Parse(hourRows[i][@"idFactor"].ToString());
                                //�������� ����� ������ ������ � ����� ������������ ������� ������ (� ���������� �������)
                                if (!(arPBRNumber[indxFactor] > Int32.Parse(hourRows[i][@"PBR_NUMBER"].ToString())))
                                {//�������, ���� ����� ������ � ������� ������ ������
                                    //??? ����� ��� ����������� ��� ���� 3-� ����� �������� (P, Pmin, Pmax)
                                    // , �� ����� ��� ������������ ��� ������� �� ���
                                    arPBRNumber[indxFactor] = Int32.Parse(hourRows[i][@"PBR_NUMBER"].ToString());
                                    ////����� �� ������� ���������� ����������
                                    //for (j = 0; j < hourRows [i].Table.Columns.Count; j ++) {
                                    //    Console.Write(@"[" + hourRows[i].Table.Columns[j].ColumnName + @"] = " + hourRows[i][hourRows[i].Table.Columns[j].ColumnName] + @"; ");
                                    //}
                                    //Console.WriteLine(@"");
                                    //��������� �������� � ~ �� ����
                                    switch (indxFactor)
                                    {
                                        case 0: //'P'
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                arRDGValues[c, hour - 1].pbr = (double)hourRows[i][@"VALUE"];
                                            else
                                                arRDGValues[c, hour - 1].pbr = 0;
                                            break;
                                        case 1: //'Pmin'
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                arRDGValues[c, hour - 1].pmin = (double)hourRows[i][@"VALUE"];
                                            else
                                                arRDGValues[c, hour - 1].pmin = 0;
                                            break;
                                        case 2: //'Pmax'
                                            if (!(hourRows[i][@"VALUE"] is DBNull))
                                                arRDGValues[c, hour - 1].pmax = (double)hourRows[i][@"VALUE"];
                                            else
                                                arRDGValues[c, hour - 1].pmax = 0;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    ;
                            }
                        else
                            //���� �� ������� �� ����� ������ ��� ����
                            if (hour > 1) {
                                //���� �� 1-�� ��� - �����������
                                arRDGValues[c, hour - 1].pbr = arRDGValues[c, hour - 2].pbr;
                                arRDGValues[c, hour - 1].pmin = arRDGValues[c, hour - 2].pmin;
                                arRDGValues[c, hour - 1].pmax = arRDGValues[c, hour - 2].pmax;

                                for (j = 0; j < 3; j++)
                                    arPBRNumber[j] = Int32.Parse(arRDGValues[c, hour - 2].pbr_number.Substring(3));

                                arRDGValues[c, hour - 1].pbr_number = arRDGValues[c, hour - 2].pbr_number;
                                arRDGValues[c, hour - 1].dtRecUpdate = arRDGValues[c, hour - 2].dtRecUpdate;
                            }
                            else
                                ;

                        //iMinPBRNumber = 25;
                        iMaxPBRNumber = -1;
                        for (j = 0; j < 3; j++)
                        {
                            if (arPBRNumber[j] > 0)
                            {
                                //???��� ����� ������� ����������� ����� ������
                                //arRDGValues[c, hour - 1].pbr_number = @"���" + PBRNumber;
                                //if (iMinPBRNumber > arPBRNumber[j])
                                if (iMaxPBRNumber < arPBRNumber[j])
                                    //iMinPBRNumber = arPBRNumber[j];
                                    iMaxPBRNumber = arPBRNumber[j];
                                else
                                    ;

                                if (hour > 1)
                                {
                                    switch (j)
                                    {
                                        case 0:
                                            if (arRDGValues[c, hour - 1].pbr < 0)
                                                arRDGValues[c, hour - 1].pbr = arRDGValues[c, hour - 2].pbr;
                                            else
                                                ;
                                            break;
                                        case 1:
                                            if (arRDGValues[c, hour - 1].pmin < 0)
                                                arRDGValues[c, hour - 1].pmin = arRDGValues[c, hour - 2].pmin;
                                            else
                                                ;
                                            break;
                                        case 2:
                                            if (arRDGValues[c, hour - 1].pmax < 0)
                                                arRDGValues[c, hour - 1].pmax = arRDGValues[c, hour - 2].pmax;
                                            else
                                                ;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    ;
                            }
                            else
                                ; //arRDGValues[c, hour - 1].pbr_number = GetPBRNumber (hour);

                            int hh = -1;
                            for (hh = hour; hh > 0; hh--)
                                //??? ���������� �������������� �������� ������ ���
                                // ��� �������� ���� �������� (P, Pmin, Pmax)
                                if (arRDGValues[c, hh - 1].pbr_number.Equals(string.Empty) == false)
                                    if (arPBRNumber[j] < Int32.Parse(arRDGValues[c, hh - 1].pbr_number.Substring(3)))
                                    {
                                        arPBRNumber[j] = Int32.Parse(arRDGValues[c, hh - 1].pbr_number.Substring(3));
                                        //if (iMinPBRNumber > arPBRNumber[j])
                                        if (iMaxPBRNumber < arPBRNumber[j])
                                            //iMinPBRNumber = arPBRNumber[j];
                                            iMaxPBRNumber = arPBRNumber[j];
                                        else
                                            ;

                                        switch (j)
                                        {
                                            case 0:
                                                arRDGValues[c, hour - 1].pbr = arRDGValues[c, hh - 1].pbr;
                                                break;
                                            case 1:
                                                arRDGValues[c, hour - 1].pmin = arRDGValues[c, hh - 1].pmin;
                                                break;
                                            case 2:
                                                arRDGValues[c, hour - 1].pmax = arRDGValues[c, hh - 1].pmax;
                                                break;
                                            default:
                                                break;
                                        }

                                        //arRDGValues[c, hour - 1].pbr_number = arRDGValues[c, hh - 1].pbr_number;

                                        //break;
                                    }
                                    else
                                        ;
                                else
                                    ;
                            // ����-��������� hh
                        } // ����-��������� �� ������� ����� ��������

                        arRDGValues[c, hour - 1].pbr_number = @"���" +
                            //iMinPBRNumber
                            iMaxPBRNumber
                            ;

                        arRDGValues[c, hour - 1].dtRecUpdate = DateTime.MinValue;

                        arRDGValues[c, hour - 1].fc = false;
                        arRDGValues[c, hour - 1].recomendation = 0;
                        arRDGValues[c, hour - 1].deviationPercent = false;
                        arRDGValues[c, hour - 1].deviation = 0;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"AdminMT::GetPPBRValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                } // ����-��������� �� ������ ���� 'hour'
            } // ����-��������� �� �������������� ���������� �������� (������ ���3-6 ����-5)

            for (hour = 1; hour < 25; hour++)
            {
                m_curRDGValues[hour - 1].pbr = -1F;
                m_curRDGValues[hour - 1].pmin = -1F;
                m_curRDGValues[hour - 1].pmax = -1F;

                m_curRDGValues[hour - 1].pbr_number = string.Empty;

                for (c = 0; c < comp.m_listMTermId.Count; c++)
                {
                    MTermId = comp.m_listMTermId[c];

                    if (!(arRDGValues[c, hour - 1].pbr < 0)) {
                        // �������� ����, ���������� �����������, ���������� � "0"
                        if (m_curRDGValues[hour - 1].pbr < 0) m_curRDGValues[hour - 1].pbr = 0F; else ;
                        // ���������
                        m_curRDGValues[hour - 1].pbr += arRDGValues[c, hour - 1].pbr;
                    } else
                        ;
                    if (!(arRDGValues[c, hour - 1].pmin < 0)) {
                        // �������� ����, ���������� �����������, ���������� � "0"
                        if (m_curRDGValues[hour - 1].pmin < 0) m_curRDGValues[hour - 1].pmin = 0F; else ;
                        // ���������
                        m_curRDGValues[hour - 1].pmin += arRDGValues[c, hour - 1].pmin;
                    } else
                        ;
                    if (!(arRDGValues[c, hour - 1].pmax < 0)) {
                        // �������� ����, ���������� �����������, ���������� � "0"
                        if (m_curRDGValues[hour - 1].pmax < 0) m_curRDGValues[hour - 1].pmax = 0F; else ;
                        // ���������
                        m_curRDGValues[hour - 1].pmax += arRDGValues[c, hour - 1].pmax;
                    }
                    else
                        ;
                } // ����-��������� �� �������������� ���������� �������� (������ ���3-6 ����-5)
                //???
                m_curRDGValues[hour - 1].pbr_number = arRDGValues[0, hour - 1].pbr_number;
                //???
                m_curRDGValues[hour - 1].dtRecUpdate = DateTime.MinValue;

                m_curRDGValues[hour - 1].fc = false;
                m_curRDGValues[hour - 1].recomendation = 0;
                m_curRDGValues[hour - 1].deviationPercent = false;
                m_curRDGValues[hour - 1].deviation = 0;
            } // ����-��������� �� ������ ���� 'hour'

            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
            bool bRes = true;
            int i = -1;

            if (m_list_tec.Count > 0)
            {
                m_IdListenerCurrent = DbSources.Sources().Register(m_list_tec[0].connSetts[(int)StatisticCommon.CONN_SETT_TYPE.MTERM], true, @"Modes-Terminale");

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

            //List <Modes.BusinessLogic.IGenObject> listIGO = (((DbInterface)m_listDbInterfaces[0]).GetListIGO(listMCId));

            return bRes;
        }

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;
            string msg = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.PPBRValues:
                    ActionReport("��������� ������ �����.");
                    if (indxTECComponents < allTECComponents.Count)
                        getPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date/*, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS*/);
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
                    ActionReport("��������� ������ ���������� ������� ��������.");
                    //GetPPBRDatesRequest(m_curDate);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"AdminMT::StateRequest () - state=" + state.ToString() + @" - �����...", Logging.INDEX_MESSAGE.NOT_SET);

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
                case (int)StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate);
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

            //Logging.Logg().Debug(@"AdminMT::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - �����...", Logging.INDEX_MESSAGE.NOT_SET);

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
                        ErrorReport("������ ������� ������ �����. ������� � ��������.");
                    else
                    {
                        ErrorReport("������ ��������� ������ �����. ������� � ��������.");

                        bClear = true;
                    }
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (request == 0)
                    {
                        ErrorReport("������ ������� ���������� ������� �������� (PPBR). ������� � ��������.");
                        //saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("������ ��������� ���������� ������� �������� (PPBR). ������� � ��������.");
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

        public override void GetRDGValues(/*int /*TYPE_FIELDS mode,*/ int indx, DateTime date)
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