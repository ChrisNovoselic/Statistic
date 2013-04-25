using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

namespace Statistic
{
    public class TEC
    {
        public enum TEC_TYPE { COMMON, BIYSK };

        public string name,
                    prefix_admin, prefix_pbr,
                    m_strUsedAdminValues, m_strUsedPPBRvsPBR;

        public List<GTP> list_GTP;

        public TEC_TYPE type() { if (name.IndexOf("Бийск") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;
        public int[] m_arListenerIds; //Идентификаторы номеров клиентов подключенных к 'DbInterface' в 'tec.cs' для 'Data' и 'Admin.cs' для 'AdminValues', 'PBR'
        public int[] m_arIndxDbInterfaces; //Индексы 'DbInterface' в 'Admin.cs' для 'AdminValues', 'PBR' (1-ый элемент ВСЕГДА = -1, т.е. не используется, т.к. для 'Data' есть 'm_dbInterface')

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface m_dbInterface; //Для данных (SQL сервер)

        public ParametersTG parametersTGForm;

        public TEC (string name, string table_name_admin, string table_name_pbr, string prefix_admin, string prefix_pbr) {
            list_GTP = new List<GTP>();

            this.name = name;
            this.m_strUsedAdminValues = table_name_admin; this.m_strUsedPPBRvsPBR = table_name_pbr;
            this.prefix_admin = prefix_admin; this.prefix_pbr = prefix_pbr;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_arListenerIds = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            m_arIndxDbInterfaces = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = (int)CONN_SETT_TYPE.DATA; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                m_arListenerIds [i] =
                m_arIndxDbInterfaces [i] =
                -1;
            }

            if (type () == TEC_TYPE.BIYSK)
                parametersTGForm = new ParametersTG ();
            else
                ;

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void Request(string request)
        {
            m_dbInterface.Request(m_arListenerIds[(int)CONN_SETT_TYPE.DATA], request);
        }

        public bool GetResponse(out bool error, out DataTable table)
        {
            return m_dbInterface.GetResponse(m_arListenerIds[(int)CONN_SETT_TYPE.DATA], out error, out table);
        }

        public void StartDbInterface()
        {
            if (used == 0)
            {
                m_dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL);
                m_arListenerIds[(int)CONN_SETT_TYPE.DATA] = m_dbInterface.ListenerRegister();
                m_dbInterface.Start();
                m_dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            used++;
            if (used > list_GTP.Count)
                used = list_GTP.Count;
            else
                ;
        }

        public void StopDbInterface()
        {
            used--;
            if (used == 0)
            {
                m_dbInterface.Stop();
                m_dbInterface.ListenerUnregister(m_arListenerIds[(int)CONN_SETT_TYPE.DATA]);
            }
            if (used < 0)
                used = 0;
        }

        public void StopDbInterfaceForce()
        {
            if (used > 0)
            {
                m_dbInterface.Stop();
                m_dbInterface.ListenerUnregister(m_arListenerIds[(int)CONN_SETT_TYPE.DATA]);
            }
        }

        public void RefreshConnectionSettings()
        {
            if (used > 0)
            {
                m_dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            else
                ;
        }

        public int connSettings (DataTable source, int type) {
            int iRes = 0;

            connSetts[type] = new ConnectionSettings();
            connSetts[type].server = source.Rows[0]["IP"].ToString();
            connSetts[type].port = Int32.Parse(source.Rows[0]["PORT"].ToString());
            connSetts[type].dbName = source.Rows[0]["DB_NAME"].ToString();
            connSetts[type].userName = source.Rows[0]["UID"].ToString();
            connSetts[type].password = source.Rows[0]["PASSWORD"].ToString();
            connSetts[type].ignore = Boolean.Parse(source.Rows[0]["IGNORE"].ToString()); //== "1";

            return iRes;
        }

        public string GetAdminValueQuery (GTP gtp, DateTime dt) {
            string strRes = string.Empty;

            string select1 = "";
            string select2 = "";

            select1 = prefix_admin;

            if (gtp.prefix.Length > 0)
            {
                select1 += "_" + gtp.prefix;
                select2 += select1 + "_PBR";
            }
            else
            {
                select2 += select1 + "_PBR";
            }

            strRes = @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedAdminValues + "." + select1 + @"_REC, " +
                    m_strUsedAdminValues + "." + @select1 + @"_IS_PER, " +
                    m_strUsedAdminValues + "." + select1 + @"_DIVIAT, " +
                    m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + m_strUsedPPBRvsPBR + "." + select2 +
                    @" FROM " + m_strUsedAdminValues + " LEFT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                    @"WHERE " + m_strUsedAdminValues + ".DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                    @"' AND " + m_strUsedAdminValues + ".DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                    @"'" +
                    @" UNION " +
                    @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedAdminValues + "." + select1 + @"_REC, " +
                    m_strUsedAdminValues + "." + select1 + @"_IS_PER, " +
                    m_strUsedAdminValues + "." + select1 + @"_DIVIAT, " +
                    m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + m_strUsedPPBRvsPBR + "." + select2 +
                    @" FROM " + m_strUsedAdminValues + " RIGHT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                    @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                    @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                    @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 AND " + m_strUsedAdminValues + ".DATE IS NULL ORDER BY DATE1, DATE2 ASC";

            return strRes;
        }

        public string GetAdminValueQuery (int num_gtp, DateTime dt) {
            string strRes = string.Empty;

            string name1 = string.Empty, name2 = string.Empty,
                    select1 = string.Empty, select2 = string.Empty;

            name1 = prefix_admin;

            switch (type())
            {
                case TEC.TEC_TYPE.COMMON:
                    name2 = name1 + "_PBR";

                    if (num_gtp < 0)
                    {
                        foreach (GTP g in list_GTP)
                        {
                            select1 += ", ";
                            select2 += ", ";
                            if (g.prefix.Length > 0)
                            {
                                select1 += m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_REC, " +
                                            m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_IS_PER, " +
                                            m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_DIVIAT";
                                select2 += m_strUsedPPBRvsPBR + "." + name1 + "_" + g.prefix + "_PBR";
                            }
                            else
                            {
                                select1 += m_strUsedAdminValues + "." + name1 + @"_REC, " +
                                            m_strUsedAdminValues + "." + name1 + @"_IS_PER, " +
                                            m_strUsedAdminValues + "." + name1 + @"_DIVIAT";
                                select2 += m_strUsedPPBRvsPBR + "." + name1 + "_PBR";
                            }
                        }
                        select1 = select1.Substring(2);
                        select2 = select2.Substring(2);
                    }
                    else
                    {
                        GTP g = list_GTP[num_gtp];
                        if (g.prefix.Length > 0)
                        {
                            select1 += m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_REC, " +
                                        m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_IS_PER, " +
                                        m_strUsedAdminValues + "." + name1 + "_" + g.prefix + "_DIVIAT";
                            select2 += m_strUsedPPBRvsPBR + "." + name1 + "_" + g.prefix + "_PBR";
                        }
                        else
                        {
                            select1 += m_strUsedAdminValues + "." + name1 + @"_REC, " +
                                        m_strUsedAdminValues + "." + name1 + @"_IS_PER, " +
                                        m_strUsedAdminValues + "." + name1 + @"_DIVIAT";
                            select2 += m_strUsedPPBRvsPBR + "." + name1 + "_PBR";
                        }
                    }

                    strRes = @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + select1 +
                                @", " + select2 +
                                @", " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedAdminValues + " LEFT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                                @"WHERE " + m_strUsedAdminValues + ".DATE >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND " + m_strUsedAdminValues + ".DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                @"'" +
                                @" UNION " +
                                @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + select1 +
                                @", " + select2 +
                                @", " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedAdminValues + " RIGHT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                                @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 AND " + m_strUsedAdminValues + ".DATE IS NULL ORDER BY DATE1, DATE2 ASC";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    name2 = "_PBR" + name1;

                    if (num_gtp < 0)
                    {
                        foreach (GTP g in list_GTP)
                        {
                            select1 += ", ";
                            select2 += ", ";
                            switch (g.name)
                            {
                                case "ГТП ТГ3-8":
                                    select1 += m_strUsedAdminValues + @"." + name1 + @"_" + g.prefix + "_REC, " +
                                                m_strUsedAdminValues + "." + name1 + @"_" + g.prefix + "_IS_PER, " +
                                                m_strUsedAdminValues + "." + name1 + @"_" + g.prefix + "_DIVIAT";
                                    select2 += m_strUsedPPBRvsPBR + @"." + name2 +
                                               @"_110";
                                    break;
                                case "ГТП ТГ1,2":
                                    select1 += m_strUsedAdminValues + @"." + name1 + @"_" + g.prefix + "_REC, " +
                                                m_strUsedAdminValues + "." + name1 + @"_" + g.prefix + "_IS_PER, " +
                                                m_strUsedAdminValues + "." + name1 + @"_" + g.prefix + "_DIVIAT";
                                    select2 += @"PPBRVSPBR." + name2 +
                                               @"_35";
                                    break;
                                default:
                                    select1 += m_strUsedAdminValues + @"." + name1 + @"_REC, " +
                                                m_strUsedAdminValues + "." + name1 + @"_IS_PER, " +
                                                m_strUsedAdminValues + "." + name1 + @"_DIVIAT";
                                    select2 += m_strUsedPPBRvsPBR + @"." + name2;
                                    break;
                            }
                        }
                        select1 = select1.Substring(2);
                        select2 = select2.Substring(2);
                    }
                    else
                    {
                        switch (list_GTP[num_gtp].name)
                        {
                            case "ГТП ТГ3-8":
                                select1 += m_strUsedAdminValues + @"." + name1 + @"_" + list_GTP[num_gtp].prefix + "_REC, " +
                                            m_strUsedAdminValues + "." + name1 + @"_" + list_GTP[num_gtp].prefix + "_IS_PER, " +
                                            m_strUsedAdminValues + "." + name1 + @"_" + list_GTP[num_gtp].prefix + "_DIVIAT";
                                select2 += m_strUsedPPBRvsPBR + @"." + name2 +
                                           @"_110";
                                break;
                            case "ГТП ТГ1,2":
                                select1 += m_strUsedAdminValues + @"." + name1 + @"_" + list_GTP[num_gtp].prefix + "_REC, " +
                                            m_strUsedAdminValues + "." + name1 + @"_" + list_GTP[num_gtp].prefix + "_IS_PER, " +
                                            m_strUsedAdminValues + @"." + name1 + @"_" + list_GTP[num_gtp].prefix + "_DIVIAT";
                                select2 += m_strUsedPPBRvsPBR + @"." + name2 +
                                           @"_35";
                                break;
                            default:
                                select1 += m_strUsedAdminValues + @"." + name1 + @"_REC, " +
                                            m_strUsedAdminValues + "." + name1 + @"_IS_PER, " +
                                            m_strUsedAdminValues + "." + name1 + @"_DIVIAT";
                                select2 += m_strUsedPPBRvsPBR + @"." + name2;
                                break;
                        }
                    }

                    strRes = @"SELECT DATE, " + select1 +
                             @" FROM " + m_strUsedAdminValues + " " +
                             @"WHERE DATE >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' ORDER BY DATE";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetAdminDatesQuery (DateTime dt) {
            return @"SELECT DATE FROM " + m_strUsedAdminValues + " WHERE " +
                      @"DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' ORDER BY DATE ASC";
        }

        public string GetPBRDatesQuery (DateTime dt) {
            return @"SELECT DATE_TIME FROM " + m_strUsedPPBRvsPBR + " WHERE " +
                      @"DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' ORDER BY DATE_TIME ASC";
        }
    }
}
