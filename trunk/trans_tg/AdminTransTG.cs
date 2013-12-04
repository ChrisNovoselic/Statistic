using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;

using StatisticCommon;

namespace trans_tg
{
    public class AdminTransTG : AdminTS_NSS
    {        
        public List <RDGStruct[]> m_listCurTimezoneOffsetRDGExcelValues;

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

            m_listCurTimezoneOffsetRDGExcelValues = new List<RDGStruct[]> ();
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
                m_listCurTimezoneOffsetRDGExcelValues.Add (new RDGStruct[iTimeZoneOffset]);

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

                if (bRes)
                {
                    for (i = 1 /*rowRDGExcelStart*/; i < 24 + 1; i++)
                    {
                        if (i < rowRDGExcelStart)
                        {
                            setRDGExcelValuesItem(out m_listCurTimezoneOffsetRDGExcelValues[m_listCurTimezoneOffsetRDGExcelValues.Count - 1][i - 1], i);
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

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];

            m_curRDGValues.CopyTo(curRDGValues, 0);

            m_listCurRDGValues.Add(curRDGValues);

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
                        m_arHaveDates[(int)type, hour - 1] = true;
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
        private string GetAdminDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + " WHERE" +
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
        private string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE_TIME FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] +
                            @" WHERE " +
                            @"DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE_TIME FROM " + allTECComponents[indxTECComponents].tec.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] +
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

            int currentHour = -1;

            date = date.Date;

            currentHour = 0;

            int indx = m_listTECComponentIndexDetail.IndexOf (GetIndexTECComponent (t.m_id, comp.m_id)) - GetCountGTP ();

            if (indx < m_listCurTimezoneOffsetRDGExcelValues.Count)
            {
                for (int i = currentHour; i < m_listTimezoneOffsetHaveDates [(int)CONN_SETT_TYPE.ADMIN].Count; i++)
                {
                    // запись для этого часа имеется, модифицируем её
                    if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.ADMIN][i] == true)
                    {
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + " SET " +
                                            @"REC='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + @"IS_PER=" + (m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviationPercent ? "1" : "0") +
                                            @", " + "DIVIAT='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
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
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                            @"', '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + (m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviationPercent ? "1" : "0") +
                                            @", '" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                            @"', " + (comp.m_id) +
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
                Logging.Logg ().LogLock ();
                Logging.Logg().LogToFile("AdminTransTG::setAdminValuesQuery () - m_listCurTimezoneOffsetRDGExcelValues.Count = " + m_listCurTimezoneOffsetRDGExcelValues.Count, true, true, false);
                Logging.Logg().LogUnlock();
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
                    // запись для этого часа имеется, модифицируем её
                    if (m_listTimezoneOffsetHaveDates[(int)CONN_SETT_TYPE.PBR][i])
                    {
                        switch (m_typeFields)
                        {
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] +
                                            " SET " +
                                            @"PBR='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].ppbr[0].ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmin='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].ppbr[1].ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmax='" + m_listCurTimezoneOffsetRDGExcelValues[indx][i].ppbr[2].ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @" WHERE " +
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
                            case AdminTS.TYPE_FIELDS.STATIC:
                                break;
                            case AdminTS.TYPE_FIELDS.DYNAMIC:
                                resQuery[(int)DbInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours((i + 1) + (-1 * t.m_timezone_offset_msc)).ToString("yyyy-MM-dd HH:mm:ss") +
                                            @"', '" + serverTime.ToString("yyyy-MM-dd HH:mm:ss") +
                                            @"', '" + "ПБР" + getPBRNumber((i + 0) + (-1 * t.m_timezone_offset_msc)) +
                                            @"', " + comp.m_id +
                                            @", '" + "0" +
                                            @"', " + m_listCurTimezoneOffsetRDGExcelValues[indx][i].ppbr[0].ToString("F1", CultureInfo.InvariantCulture) +
                                            @"),";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else{
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("AdminTransTG::setPPBRQuery () - m_listCurTimezoneOffsetRDGExcelValues.Count = " + m_listCurTimezoneOffsetRDGExcelValues.Count, true, true, false);
                Logging.Logg().LogUnlock();
            }

            resQuery[(int)DbInterface.QUERY_TYPE.DELETE] = @"";

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("AdminTransTG - setPPBRQuery", true, true, false);
            Logging.Logg().LogUnlock();

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

        protected override void InitializeSyncState()
        {
            m_waitHandleState = new WaitHandle[2] { new AutoResetEvent(true), new ManualResetEvent (false) };
        }        
    }
}
