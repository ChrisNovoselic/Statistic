using System;
using System.Collections.Generic;
using System.Text;
//using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;

using HClassLibrary;
using StatisticCommon;

namespace trans_tg
{
    public class AdminTransTG : AdminTS_NSS
    {        
        public List <RDGStruct[]> m_listCurTimezoneOffsetRDGExcelValues;

        private List <bool> [] m_listTimezoneOffsetHaveDates;

        public AdminTransTG(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues)
        {
            InitializeAdminTransTG();
        }

        private void InitializeAdminTransTG()
        {
            m_listTimezoneOffsetHaveDates = new List<bool>[(int)StatisticCommon.CONN_SETT_TYPE.PBR + 1];
            for (int i = 0; i < (int)StatisticCommon.CONN_SETT_TYPE.PBR + 1; i++)
            {
                m_listTimezoneOffsetHaveDates[i] = new List<bool>();
            }

            m_listCurTimezoneOffsetRDGExcelValues = new List<RDGStruct[]> ();
        }

        protected override bool ImpRDGExcelValuesResponse()
        {
            bool bRes = false;

            bRes = IsCanUseTECComponents();
            int i = -1
                , iTimeZoneOffset = -1;

            if (bRes == true)
            {
                iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc;
                m_listCurTimezoneOffsetRDGExcelValues.Add(new RDGStruct[iTimeZoneOffset]);

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

                if (bRes)
                    for (i = 0; i < iTimeZoneOffset; i++)
                        setRDGExcelValuesItem(out m_listCurTimezoneOffsetRDGExcelValues[m_listCurTimezoneOffsetRDGExcelValues.Count - 1][i], i + 1);
                else
                    ;
            }
            else
                ;

            bRes = base.ImpRDGExcelValuesResponse();

            return bRes;
        }

        protected override void ClearDates(StatisticCommon.CONN_SETT_TYPE type)
        {
            base.ClearDates(type);
            
            int i = 1;

            m_listTimezoneOffsetHaveDates[(int)type].Clear();
            for (i = 0; i < allTECComponents[indxTECComponents].tec.m_timezone_offset_msc; i++)
            {
                m_listTimezoneOffsetHaveDates[(int)type].Add(false);
            }

        }

        protected override bool GetDatesResponse(StatisticCommon.CONN_SETT_TYPE type, DataTable table, DateTime date)
        {
            DateTime dateTimezoneOffsetRDGExcel = date.AddHours(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc);
            //bool bIsHourTimezoneOffsetRDGExcel = false;

            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    //TimeSpan dateDiff = ((DateTime)table.Rows[i][0]) - date;

                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if ((hour == 0) && (!(((DateTime)table.Rows[i][0]).Day == date.Day))/* && (!(dateTimezoneOffsetRDGExcel.Day == date.Day))*/)
                        hour = 24;
                    else
                        ;

                    if ((!(dateTimezoneOffsetRDGExcel.Day == ((DateTime)table.Rows[i][0]).Day)) && (hour > 0))
                        m_arHaveDates[(int)type, hour - 1] = Convert.ToInt32 (table.Rows[i][1]); //true;
                    else {
                        hour = hour == 0 ? 24 : hour;
                        m_listTimezoneOffsetHaveDates[(int)type][hour - 1 - (24 + (-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc))] = true;
                    }
                }
                catch { }
            }

            return true;
        }

        protected override void GetAdminDatesRequest(DateTime date)
        {
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents())
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date));
                Request(m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)StatisticCommon.CONN_SETT_TYPE.ADMIN], GetAdminDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents () == true)
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request(m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)StatisticCommon.CONN_SETT_TYPE.ADMIN], GetPBRDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        //�� 'TEC.cs'
        private string GetAdminDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE, ID FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)mode] + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyyMMdd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE, ID FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)mode] + " WHERE" +
                            @" ID_COMPONENT = " + comp.m_id +
                          @" AND DATE > '" + dt.AddHours(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc).ToString("yyyyMMdd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        //�� 'TEC.cs'
        private string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE_TIME, ID FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)mode] +
                            @" WHERE " +
                            @"DATE_TIME > '" + dt.ToString("yyyyMMdd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE_TIME, ID FROM " + @"[" + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)mode] + @"]" +
                            @" WHERE" +
                            @" ID_COMPONENT = " + comp.m_id + "" +
                            @" AND DATE_TIME > '" + dt.AddHours(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc).ToString("yyyyMMdd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        private int GetCountGTP ()
        {
            int iRes = 0;

            foreach (int indx in m_listTECComponentIndexDetail)
            {
                if (modeTECComponent (indx) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                    iRes ++;
                else
                    ;
            }

            return iRes;
        }

        protected override string [] setAdminValuesQuery(TEC t, TECComponent comp, DateTime date)
        {
            string [] resQuery = base.setAdminValuesQuery(t, comp, date);

            int currentHour = -1
                , offset = -1;

            date = date.Date;

            currentHour = 0;

            int indx = m_listTECComponentIndexDetail.IndexOf (GetIndexTECComponent (t.m_id, comp.m_id)) - GetCountGTP ();

            if (indx < m_listCurTimezoneOffsetRDGExcelValues.Count)
            {
                for (int i = currentHour; i < m_listTimezoneOffsetHaveDates[(int)StatisticCommon.CONN_SETT_TYPE.ADMIN].Count; i++)
                {
                    offset = GetSeasonHourOffset(i + 1);

                    // ������ ��� ����� ���� �������, ������������ �
                    if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.ADMIN][i] == true)
                    {
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)m_typeFields] + " SET " +
                                            @"REC='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + @"IS_PER=" + (m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviationPercent ? "1" : "0") +
                                            @", " + "DIVIAT='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + "SEASON=" + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                                            @", " + "FC=" + (m_curRDGValues[i].fc ? 1 : 0) +
                                            @" WHERE" +
                                            @" DATE = '" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyyMMdd HH:mm:ss") +
                                            @"'" +
                                            @" AND ID_COMPONENT = " + comp.m_id + "; ";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // ������ �����������, ���������� ��������
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyyMMdd HH:mm:ss") +
                                            @"', '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + (m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviationPercent ? "1" : "0") +
                                            @", '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + (comp.m_id) +
                                            @", " + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                                            @", " + (m_curRDGValues[i].fc ? 1 : 0) +
                                            @"),";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                Logging.Logg().Debug("AdminTransTG::setAdminValuesQuery () - m_listCurTimezoneOffsetRDGExcelValues.Count = " + m_listCurTimezoneOffsetRDGExcelValues.Count, Logging.INDEX_MESSAGE.NOT_SET);
            }

            return resQuery;
        }

        protected override string [] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string [] resQuery = base.setPPBRQuery(t, comp, date);

            int currentHour = -1;

            date = date.Date;

            currentHour = 0;

            int indx = m_listTECComponentIndexDetail.IndexOf(GetIndexTECComponent(t.m_id, comp.m_id)) - GetCountGTP();
            if (indx < m_listCurTimezoneOffsetRDGExcelValues.Count)
            {
                for (int i = currentHour; i < m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.PBR].Count; i++)
                {
                    // ������ ��� ����� ���� �������, ������������ �
                    if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.PBR][i])
                    {
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + @"[" + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + @"]" +
                                            " SET " +
                                            @"PBR='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmin='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pmin.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmax='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @" WHERE " +
                                            t.m_strNamesField [(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @" = '" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyyMMdd HH:mm:ss") +
                                            @"'" +
                                            @" AND ID_COMPONENT = " + comp.m_id + "; ";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // ������ �����������, ���������� ��������
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyyMMdd HH:mm:ss") +
                                            @"', '" + serverTime.ToString("yyyyMMdd HH:mm:ss") +
                                            @"', '" + GetPBRNumber((i + 0) + (-1 * t.m_timezone_offset_msc)) +
                                            @"', " + comp.m_id +
                                            @", '" + "0" + "'" +
                                            @", '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pbr.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @", '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pmin.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @", '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].pmax.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @"),";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else {
                Logging.Logg().Debug("AdminTransTG::setPPBRQuery () - m_listCurTimezoneOffsetRDGExcelValues.Count = " + m_listCurTimezoneOffsetRDGExcelValues.Count, Logging.INDEX_MESSAGE.NOT_SET);
            }

            resQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = @"";

            Logging.Logg().Debug("AdminTransTG::setPPBRQuery ()", Logging.INDEX_MESSAGE.NOT_SET);

            return resQuery;
        }

        public void getCurTimezoneOffsetRDGExcelValues (AdminTransTG source) {
            m_listCurTimezoneOffsetRDGExcelValues = new List<RDGStruct[]> ();

            for (int i = 0; i < source.m_listCurTimezoneOffsetRDGExcelValues.Count; i ++)
            {
                m_listCurTimezoneOffsetRDGExcelValues.Add(new AdminTS.RDGStruct[source.m_listCurTimezoneOffsetRDGExcelValues [i].Length]);
                source.m_listCurTimezoneOffsetRDGExcelValues [i].CopyTo(m_listCurTimezoneOffsetRDGExcelValues [i], 0);
            }            
        }

        public override void getCurRDGValues(HAdmin source)
        {
            m_listCurRDGValues = new List<RDGStruct[]>();

            foreach (int indx in m_listTECComponentIndexDetail)
            {
                int j = m_listTECComponentIndexDetail.IndexOf(indx);

                m_listCurRDGValues.Add(new RDGStruct[((AdminTS_NSS)source).m_listCurRDGValues[j].Length]);
                ((AdminTS_NSS)source).m_listCurRDGValues[j].CopyTo(m_listCurRDGValues[j], 0);
            }

            getCurTimezoneOffsetRDGExcelValues((AdminTransTG)source);
        }

        public override void SaveRDGValues(int id, DateTime date, bool bCallback)
        {
            m_prevDate = date.Date;

            SaveChanges ();
        }

        public override void ClearRDGValues(DateTime date) {
            m_prevDate = date.Date;
            
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                if (modeTECComponent (indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                {
                    indxTECComponents = indx;

                    base.ClearRDG();
                }
                else
                    ;
            }
            
        }        
    }
}
