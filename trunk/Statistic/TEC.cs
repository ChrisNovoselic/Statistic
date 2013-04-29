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
        public enum INDEX_NAME_FIELD { ADMIN_DATETIME, REC, IS_PER, DIVIAT,
                                        PBR_DATETIME, PBR, 
                                        COUNT_INDEX_NAME_FIELD};
        public enum TEC_TYPE { COMMON, BIYSK };

        public string name,
                    prefix_admin, prefix_pbr,
                    m_strUsedAdminValues, m_strUsedPPBRvsPBR;
        public List <string> m_strNamesField;

        public List<GTP> list_GTP;

        public TEC_TYPE type() { if (name.IndexOf("�����") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;
        public int[] m_arListenerIds; //�������������� ������� �������� ������������ � 'DbInterface' � 'tec.cs' ��� 'Data' � 'Admin.cs' ��� 'AdminValues', 'PBR'
        public int[] m_arIndxDbInterfaces; //������� 'DbInterface' � 'Admin.cs' ��� 'AdminValues', 'PBR' (1-�� ������� ������ = -1, �.�. �� ������������, �.�. ��� 'Data' ���� 'm_dbInterface')

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface m_dbInterface; //��� ������ (SQL ������)

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

            m_strNamesField = new List<string> ();

            if (type () == TEC_TYPE.BIYSK)
                parametersTGForm = new ParametersTG ();
            else
                ;

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void SetNamesField (string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat,
                                    string pbr_datetime, string ppbr_vs_pbr) {
            m_strNamesField.Add(admin_datetime); //INDEX_NAME_FIELD.ADMIN_DATETIME
            m_strNamesField.Add(admin_rec); //INDEX_NAME_FIELD.REC
            m_strNamesField.Add(admin_is_per); //INDEX_NAME_FIELD.IS_PER
            m_strNamesField.Add(admin_diviat); //INDEX_NAME_FIELD.DIVIAT

            m_strNamesField.Add(pbr_datetime); //INDEX_NAME_FIELD.PBR_DATETIME
            m_strNamesField.Add(ppbr_vs_pbr); //INDEX_NAME_FIELD.PBR
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
            else
                ;

            if (used < 0)
                used = 0;
            else
                ;
        }

        public void StopDbInterfaceForce()
        {
            if (used > 0)
            {
                m_dbInterface.Stop();
                m_dbInterface.ListenerUnregister(m_arListenerIds[(int)CONN_SETT_TYPE.DATA]);
            }
            else
                ;
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

        public string GetPBRValueQuery (GTP gtp, DateTime dt) {
            string strRes = string.Empty;

            string /*selectAdmin,*/ selectPBR,
                    /*strUsedAdminValues,*/ strUsedPPBRvsPBR;

            //selectAdmin = prefix_admin;
            selectPBR = prefix_pbr;

            //strUsedAdminValues = m_strUsedAdminValues;
            strUsedPPBRvsPBR = m_strUsedPPBRvsPBR;

            if (gtp.prefix_pbr.Length > 0)
            {
                //selectAdmin += "_" + gtp.prefix;
                selectPBR += "_" + gtp.prefix_pbr + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }
            else
            {
                selectPBR += "_PBR";
            }

            strRes = @"SELECT " + strUsedPPBRvsPBR + "." + m_strNamesField [(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " + strUsedPPBRvsPBR + "." + selectPBR + " AS PBR" + 
                    @" FROM " + strUsedPPBRvsPBR +
                    @" WHERE " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                    @"' AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                    @"' AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0 " + "ORDER BY DATE_PBR ASC";

            return strRes;
        }

        public string GetAdminValueQuery (GTP gtp, DateTime dt) {
            string strRes = string.Empty;

            string selectAdmin/*, selectPBR*/,
                    strUsedAdminValues/*, strUsedPPBRvsPBR*/;

            selectAdmin = prefix_admin;
            //selectPBR = prefix_pbr;

            strUsedAdminValues = m_strUsedAdminValues;
            //strUsedPPBRvsPBR = m_strUsedPPBRvsPBR;

            if (gtp.prefix_admin.Length > 0)
            {
                selectAdmin += "_" + gtp.prefix_admin;
                //selectPBR += "_" + gtp.prefix + "_PBR";
            }
            else
            {
                //selectPBR += "_PBR";
            }

            strRes = @"SELECT " + strUsedAdminValues + ".DATE AS DATE_ADMIN, " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT] +
                    @" FROM " + strUsedAdminValues +
                    @" WHERE " + strUsedAdminValues + ".DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                    @" AND " + strUsedAdminValues + ".DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                    @" UNION " +
                    @"SELECT " + strUsedAdminValues + ".DATE AS DATE_ADMIN, " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                    strUsedAdminValues + "." + selectAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT] +
                    @" FROM " + strUsedAdminValues +
                    @" WHERE " + strUsedAdminValues + ".DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                    @" AND " + strUsedAdminValues + ".DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                    @" AND " + strUsedAdminValues + ".DATE IS NULL ORDER BY DATE_ADMIN ASC";

            return strRes;
        }

        public string GetPBRValueQuery (int num_gtp, DateTime dt) {
            string strRes = string.Empty;

            return strRes;
        }

        public string GetAdminValueQuery (int num_gtp, DateTime dt) {
            string strRes = string.Empty;

            string nameAdmin = string.Empty, namePBR = string.Empty,
                    selectAdmin = string.Empty, selectPBR = string.Empty;

            nameAdmin = prefix_admin;
            namePBR = prefix_pbr;

            switch (type())
            {
                case TEC.TEC_TYPE.COMMON:
                    if (num_gtp < 0)
                    {
                        foreach (GTP g in list_GTP)
                        {
                            selectAdmin += ", ";
                            selectPBR += ", ";

                            if (g.prefix_admin.Length > 0)
                            {
                                selectAdmin += m_strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                            m_strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                            m_strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                                selectPBR += m_strUsedPPBRvsPBR + @"." + namePBR + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
                            }
                            else
                            {
                                selectAdmin += m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                            m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                            m_strUsedAdminValues + "." + namePBR + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                                selectPBR += m_strUsedPPBRvsPBR + "." + namePBR + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
                            }
                        }
                        selectAdmin = selectAdmin.Substring(2);
                        selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        GTP g = list_GTP[num_gtp];
                        if (g.prefix_admin.Length > 0)
                        {
                            selectAdmin += m_strUsedAdminValues + "." + nameAdmin + "_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                        m_strUsedAdminValues + "." + nameAdmin + "_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                        m_strUsedAdminValues + "." + nameAdmin + "_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.DIVIAT];
                            selectPBR += m_strUsedPPBRvsPBR + "." + namePBR + "_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.PBR];
                        }
                        else
                        {
                            selectAdmin += m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                        m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                        m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                            selectPBR += m_strUsedPPBRvsPBR + "." + namePBR + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
                        }
                    }

                    strRes = @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE_ADMIN, " + m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE_PBR, " + selectAdmin +
                                @", " + selectPBR +
                                @", " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedAdminValues + " LEFT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                                @"WHERE " + m_strUsedAdminValues + ".DATE >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND " + m_strUsedAdminValues + ".DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                @"'" +
                                @" UNION " +
                                @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE_ADMIN, " + m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE_PBR, " + selectAdmin +
                                @", " + selectPBR +
                                @", " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedAdminValues + " RIGHT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                                @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 AND " + m_strUsedAdminValues + ".DATE IS NULL ORDER BY DATE_ADMIN, DATE_PBR ASC";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    if (num_gtp < 0)
                    {
                        foreach (GTP g in list_GTP)
                        {
                            selectAdmin += ", ";
                            selectPBR += ", ";

                            if (g.prefix_admin.Length > 0)
                            {
                                selectAdmin += m_strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                            m_strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                            m_strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                                selectPBR += m_strUsedPPBRvsPBR + @"." + namePBR + g.prefix_pbr;
                            }
                            else {
                                selectAdmin += m_strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                                m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                                m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                                selectPBR += m_strUsedPPBRvsPBR + @"." + namePBR;
                            }
                        }
                        selectAdmin = selectAdmin.Substring(2);
                        selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        GTP g = list_GTP[num_gtp];
                        if (g.prefix_admin.Length > 0)
                        {
                            selectAdmin += m_strUsedAdminValues + @"." + nameAdmin + @"_" + list_GTP[num_gtp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                        m_strUsedAdminValues + "." + nameAdmin + @"_" + list_GTP[num_gtp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                        m_strUsedAdminValues + @"." + nameAdmin + @"_" + list_GTP[num_gtp].prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                            selectPBR += m_strUsedPPBRvsPBR + @"." + namePBR + list_GTP[num_gtp].prefix_pbr;
                        }
                        else {
                            selectAdmin += m_strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
                                            m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                                            m_strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                            selectPBR += m_strUsedPPBRvsPBR + @"." + namePBR;
                        }
                    }

                    strRes = @"SELECT DATE, " + selectAdmin +
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

        public string NameFieldOfAdminRequest(GTP gtp)
        {
            string strRes = @"" + this.prefix_admin;

            if (gtp.prefix_admin.Length > 0)
            {
                strRes += "_" + gtp.prefix_admin;
            }
            else
                ;

            return strRes;
        }

        public string NameFieldOfPBRRequest(GTP gtp)
        {
            string strRes = @"" + this.prefix_pbr;

            if (gtp.prefix_pbr.Length > 0)
            {
                strRes += "_" + gtp.prefix_pbr;
            }
            else
                ;

            return strRes;
        }
    }
}
