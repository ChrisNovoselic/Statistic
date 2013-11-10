using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Globalization;

using StatisticCommon;

namespace trans_tg
{
    public class AdminTransTG : Admin
    {        
        public RDGStruct[] m_curTimezoneOffsetRDGExcelValues;

        private List <bool> [] m_listTimezoneOffsetHaveDates;

        public AdminTransTG()
        {
            InitializeAdminTransTG();
        }

        private void InitializeAdminTransTG()
        {
            m_listTimezoneOffsetHaveDates = new List<bool>[(int)CONN_SETT_TYPE.PBR + 1];
            for (int i = 0; i < (int)CONN_SETT_TYPE.PBR + 1; i ++) {
                m_listTimezoneOffsetHaveDates[i] = new List<bool>();
            }
        }

        protected override bool GetRDGExcelValuesResponse()
        {
            bool bRes = IsCanUseTECComponents();

            if (bRes)
            {
                int i = -1,
                    iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc,
                    rowRDGExcelStart = 1 + iTimeZoneOffset,
                    hour = -1;
                m_curTimezoneOffsetRDGExcelValues = new RDGStruct[iTimeZoneOffset];

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

                if (bRes)
                {
                    for (i = 1 /*rowRDGExcelStart*/; i < 24 + 1; i++)
                    {
                        if (i < rowRDGExcelStart)
                        {
                            setRDGExcelValuesItem(out m_curTimezoneOffsetRDGExcelValues[i - 1], i);
                        }
                        else
                        {
                            hour = i - iTimeZoneOffset;
                            setRDGExcelValuesItem(out m_curRDGValues[hour - 1], i);
                        }
                    }
                }
                else
                    ;
            }
            else
                ;

            return bRes;
        }

        protected override void ClearDates(CONN_SETT_TYPE type)
        {
            base.ClearDates(type);
            
            int i = 1;

            m_listTimezoneOffsetHaveDates[(int)type].Clear();
            for (i = 0; i < allTECComponents[indxTECComponents].tec.m_timezone_offset_msc; i++)
            {
                m_listTimezoneOffsetHaveDates[(int)type].Add(false);
            }

        }

        protected override bool GetDatesResponse(CONN_SETT_TYPE type, DataTable table, DateTime date)
        {
            DateTime dateTimezoneOffsetRDGExcel = date.AddDays(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc);
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

                    if (!(dateTimezoneOffsetRDGExcel.Day == ((DateTime)table.Rows[i][0]).Day))
                        m_arHaveDates[(int)type, hour - 1] = true;
                    else
                        m_listTimezoneOffsetHaveDates[(int)type][hour - 1] = true;
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
                Request(allTECComponents[indxTECComponents].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], GetAdminDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
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

            if (IsCanUseTECComponents())
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request(allTECComponents[indxTECComponents].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], GetPBRDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        //Из 'TEC.cs'
        private string GetAdminDatesQuery(DateTime dt, Admin.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case Admin.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.STATIC] + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                case Admin.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.DYNAMIC] + " WHERE" +
                            @" ID_COMPONENT = " + comp.m_id +
                          @" AND DATE > '" + dt.AddHours(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        //Из 'TEC.cs'
        private string GetPBRDatesQuery(DateTime dt, Admin.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case Admin.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE_TIME FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.STATIC] +
                            @" WHERE " +
                            @"DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                case Admin.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE_TIME FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.DYNAMIC] +
                            @" WHERE" +
                            @" ID_COMPONENT = " + comp.m_id + "" +
                            @" AND DATE_TIME > '" + dt.AddHours(-1 * allTECComponents[indxTECComponents].tec.m_timezone_offset_msc).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        protected override string [] setAdminValuesQuery(TEC t, TECComponent comp, DateTime date)
        {
            string [] resQuery = base.setAdminValuesQuery(t, comp, date);
            
            int currentHour = -1;

            date = date.Date;

            currentHour = 0;

            for (int i = currentHour; i < m_listTimezoneOffsetHaveDates [(int)CONN_SETT_TYPE.ADMIN].Count; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.ADMIN][i])
                {
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.DYNAMIC] + " SET " + @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (m_curTimezoneOffsetRDGExcelValues [i].deviationPercent ? "1" : "0") +
                                        @", " + "DIVIAT='" + m_curTimezoneOffsetRDGExcelValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curTimezoneOffsetRDGExcelValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curTimezoneOffsetRDGExcelValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curTimezoneOffsetRDGExcelValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (comp.m_id) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            return resQuery;
        }

        protected override string [] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string [] resQuery = base.setPPBRQuery(t, comp, date);

            int currentHour = -1;

            date = date.Date;

            currentHour = 0;

            for (int i = currentHour; i < m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.PBR].Count; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.PBR][i])
                {
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.DYNAMIC] +
                                        " SET " + @"PBR='" + m_curTimezoneOffsetRDGExcelValues[i].plan.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber((i + 0) + (-1 * t.m_timezone_offset_msc)) +
                                        @"', " + comp.m_id +
                                        @", '" + "0" +
                                        @"', " + m_curTimezoneOffsetRDGExcelValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            resQuery[(int)DbInterface.QUERY_TYPE.DELETE] = @"";

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("SetPPBRRequest", true, true, false);
            Logging.Logg().LogUnlock();

            return resQuery;
        }
    }
}
