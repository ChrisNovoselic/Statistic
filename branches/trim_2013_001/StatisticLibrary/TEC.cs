using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

namespace StatisticCommon
{
    public class TEC
    {
        public enum INDEX_NAME_FIELD { ADMIN_DATETIME, REC, IS_PER, DIVIAT,
                                        PBR_DATETIME, PBR, PBR_NUMBER, 
                                        COUNT_INDEX_NAME_FIELD};
        public enum TEC_TYPE { COMMON, BIYSK };

        public int m_id;
        public string name
                    , prefix_admin, prefix_pbr;
        public string [] m_arNameTableAdminValues, m_arNameTableUsedPPBRvsPBR;
        public List <string> m_strNamesField;

        public int m_timezone_offset_msc { get; set; }
        public string m_path_rdg_excel { get; set;}
        public string m_strTemplateNameSgnDataTM,
                    m_strTemplateNameSgnDataFact;

        public List<TECComponent> list_TECComponents;

        public TEC_TYPE type() { if (name.IndexOf("�����") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;
        //�������������� ������� �������� ������������ � 'DbInterface' � 'tec.cs' ��� 'Data' � 'PanelAdmin.cs' ��� 'AdminValues', 'PBR'
        //������, ����� ������, �� ������� ��������, ��� ��� ������� ��������� ������ ��� ��� ���� ������
        public /*List <int>*/int [] m_arListenerIds;
        //������� 'DbInterface' � 'PanelAdmin.cs' ��� 'AdminValues', 'PBR' (1-�� ������� ������ = -1, �.�. �� ������������, �.�. ��� 'Data' ���� 'm_dbInterface')
        public int[] m_arIndxDbInterfaces;

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface  [] m_arDBInterfaces; //��� ������ (SQL ������)

        public FormParametersTG parametersTGForm;

        public TEC (int id, string name, string table_name_admin, string table_name_pbr, string prefix_admin, string prefix_pbr, bool bUseData) {
            list_TECComponents = new List<TECComponent>();

            this.m_id = id;
            this.name = name;
            this.m_arNameTableAdminValues = new string[(int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS]; this.m_arNameTableUsedPPBRvsPBR = new string[(int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS];

            this.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] = table_name_admin; this.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] = table_name_pbr;
            //this.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = "AdminValuesOfID"; this.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = "PPBRvsPBROfID";

            this.prefix_admin = prefix_admin; this.prefix_pbr = prefix_pbr;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_arListenerIds = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            m_arIndxDbInterfaces = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = (int)CONN_SETT_TYPE.DATA_FACT; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                m_arListenerIds [i] =
                m_arIndxDbInterfaces [i] =
                -1;
            }

            m_strNamesField = new List<string> ();

            if ((type () == TEC_TYPE.BIYSK) && (bUseData == true))
                parametersTGForm = new FormParametersTG ("setup.ini");
            else
                ;

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void SetNamesField (string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat,
                                    string pbr_datetime, string ppbr_vs_pbr, string pbr_number) {
            m_strNamesField.Add(admin_datetime); //INDEX_NAME_FIELD.ADMIN_DATETIME
            m_strNamesField.Add(admin_rec); //INDEX_NAME_FIELD.REC
            m_strNamesField.Add(admin_is_per); //INDEX_NAME_FIELD.IS_PER
            m_strNamesField.Add(admin_diviat); //INDEX_NAME_FIELD.DIVIAT

            m_strNamesField.Add(pbr_datetime); //INDEX_NAME_FIELD.PBR_DATETIME
            m_strNamesField.Add(ppbr_vs_pbr); //INDEX_NAME_FIELD.PBR

            m_strNamesField.Add(pbr_number); //INDEX_NAME_FIELD.PBR_NUMBER
        }

        public void Request(CONN_SETT_TYPE indx_src, string request)
        {
            m_arDBInterfaces[(int)indx_src].Request(m_arListenerIds[(int)indx_src], request);
        }

        public bool GetResponse(CONN_SETT_TYPE indx_src, out bool error, out DataTable table)
        {
            return m_arDBInterfaces[(int)indx_src].GetResponse(m_arListenerIds[(int)indx_src], out error, out table);
        }

        public void StartDbInterfaces()
        {
            if (used == 0)
            {
                m_arDBInterfaces = new DbTSQLInterface[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

                for (int i = (int)CONN_SETT_TYPE.DATA_FACT; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    m_arDBInterfaces[i] = new DbTSQLInterface(DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, "��������� MSSQL-��: " + name);
                    m_arListenerIds[i] = m_arDBInterfaces[i].ListenerRegister();
                    m_arDBInterfaces[i].Start();
                    m_arDBInterfaces[i].SetConnectionSettings(connSetts[i]);
                }
            }
            else
                ;

            used++;

            if (used > list_TECComponents.Count)
                used = list_TECComponents.Count;
            else
                ;
        }

        private void stopDbInterfaces () {
            for (int i = (int)CONN_SETT_TYPE.DATA_FACT; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arDBInterfaces[i].Stop ();
                //for (int j = 0; j < m_arListenerIds[i].Count; j ++)
                    m_arDBInterfaces[i].ListenerUnregister(m_arListenerIds[i]/*[j]*/);
            }
        }

        public void StopDbInterfaces()
        {
            used--;
            if (used == 0)
            {
                stopDbInterfaces ();
            }
            else
                ;

            if (used < 0)
                used = 0;
            else
                ;
        }

        public void StopDbInterfacesForce()
        {
            if (used > 0)
            {
                stopDbInterfaces ();
            }
            else
                ;
        }

        public void RefreshConnectionSettings()
        {
            if (used > 0)
            {
                for (int i = (int)CONN_SETT_TYPE.DATA_FACT; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    if (! (m_arDBInterfaces[i] == null))
                        m_arDBInterfaces[i].SetConnectionSettings(connSetts [i]);
                    else
                        ;
                }                
            }
            else
                ;
        }

        public int connSettings (DataTable source, int type) {
            int iRes = 0;

            if (source.Rows.Count == 1) {
                connSetts[type] = new ConnectionSettings();
                connSetts[type].server = source.Rows[0]["IP"].ToString();
                connSetts[type].port = Int32.Parse(source.Rows[0]["PORT"].ToString());
                connSetts[type].dbName = source.Rows[0]["DB_NAME"].ToString();
                connSetts[type].userName = source.Rows[0]["UID"].ToString();
                connSetts[type].password = source.Rows[0]["PASSWORD"].ToString();
                connSetts[type].ignore = Boolean.Parse(source.Rows[0]["IGNORE"].ToString()); //== "1";
            }
            else {
                if (source.Rows.Count == 0)
                    iRes = -1;
                else
                    iRes = -2;
            }

            return iRes;
        }

        private string idComponentValueQuery (int num_comp) {
            string strRes = string.Empty;

            if (num_comp < 0)
            {
                foreach (TECComponent g in list_TECComponents)
                //foreach (TG tg in list_TECComponents [num_comp].TG)
                {
                    if ((g.m_id > 100) && (g.m_id < 500)) {
                        strRes += ", ";

                        strRes += (g.m_id).ToString();
                        //selectAdmin += (tg.m_id).ToString();
                    }
                    else
                        ;
                }
                strRes = strRes.Substring(2);
            }
            else
            {
                if ((list_TECComponents[num_comp].m_id > 100) && (list_TECComponents[num_comp].m_id < 500))
                    strRes += (list_TECComponents[num_comp].m_id).ToString();
                else {
                    foreach (TG tg in list_TECComponents[num_comp].TG)
                    {
                        strRes += ", ";

                        strRes += (tg.m_id).ToString();
                    }
                    strRes = strRes.Substring(2);
                }
            }

            return strRes;
        }

        //���������� ������ ��� �� �� ������������ ������ (������ ������)
        private string selectPBRValueQuery(TECComponent g)
        {
            string strRes = string.Empty;

            if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR].Length > 0)
                if (g.prefix_admin.Length > 0)
                {
                    //selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                    //            strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                    //            strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                    strRes += m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_pbr + @"_" + g.prefix_pbr + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR] + " AS PBR";
                    strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_pbr + @"_" + g.prefix_pbr + @"_" + "Pmin";
                    strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_pbr + @"_" + g.prefix_pbr + @"_" + "Pmax";
                }
                else
                {
                    //selectAdmin += strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                    //            strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                    //            strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                    strRes += m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + prefix_pbr + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR] + " AS PBR";
                    strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_pbr + @"_" + "Pmin";
                    strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_pbr + @"_" + "Pmax";
                }
            else
                ;

            return strRes;
        }

        private string pbrValueQuery(string selectPBR, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT " +
                        //strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR";

                    if (selectPBR.Length > 0)
                        strRes += @", " + selectPBR;
                    else
                        ; //��� ������ ��� ���

                    //if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR].Length > 0)
                    //    strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
                    //else
                    //    ;
                    
                    if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];
                    else
                        ;
                    strRes += @" " + @"FROM " +
                        /*strUsedAdminValues*/ m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] +
                        @" WHERE " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" AND " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +
                        //@" AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " IS NULL" +
                        @" ORDER BY DATE_PBR" +
                        @" ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string sensorsFactRequest()
        {
            return @"SELECT DISTINCT SENSORS.NAME, SENSORS.ID " +
                    @"FROM DEVICES " +
                    @"INNER JOIN SENSORS ON " +
                    @"DEVICES.ID = SENSORS.STATIONID " +
                    @"INNER JOIN DATA ON " +
                    @"DEVICES.CODE = DATA.OBJECT AND " +
                    @"SENSORS.CODE = DATA.ITEM " +
                    @"WHERE DATA.PARNUMBER = 12 AND " +
                    //@"SENSORS.NAME LIKE '��%P%+'";
                    @"SENSORS.NAME LIKE '" + m_strTemplateNameSgnDataFact + @"'";
        }

        public string sensorsTMRequest()
        {
            return @"SELECT [NAME], [ID] FROM [dbo].[reals_rv] WHERE [NAME] LIKE '" + m_strTemplateNameSgnDataTM + @"'";
        }

        public string minsRequest(DateTime usingDate, int hour, string sensors)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            usingDate = usingDate.Date.AddHours(hour);
            string request = string.Empty;

            switch (type())
            {
                case TEC.TEC_TYPE.COMMON:
                    //request = @"SELECT DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER, DATA.VALUE0, DATA.DATA_DATE, SENSORS.ID, DATA.SEASON " +
                    request = @"SELECT SENSORS.ID, DATA.DATA_DATE, DATA.SEASON, DATA.VALUE0 " + //, DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER " +
                             @"FROM DEVICES " +
                             @"INNER JOIN SENSORS ON " +
                             @"DEVICES.ID = SENSORS.STATIONID " +
                             @"INNER JOIN DATA ON " +
                             @"DEVICES.CODE = DATA.OBJECT AND " +
                             @"SENSORS.CODE = DATA.ITEM AND " +
                             @"DATA.DATA_DATE >= '" + usingDate.ToString("yyyy.MM.dd HH:00:00") +
                             @"' AND " +
                             @"DATA.DATA_DATE <= '" + usingDate.AddHours(1).ToString("yyyy.MM.dd HH:00:00") +
                             @"' " +
                             @"WHERE DATA.PARNUMBER = 2 AND (" + sensors +
                             @") " +
                             @"ORDER BY DATA.DATA_DATE, DATA.SEASON";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                    request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.TIME, IZM_TII.WINTER_SUMMER, IZM_TII.VALUE_UNIT " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
                             @"FROM IZM_TII " +
                             @"INNER JOIN CHANNELS ON " +
                             @"IZM_TII.IDCHANNEL = CHANNELS.IDCHANNEL " +
                             @"INNER JOIN DEVICES ON " +
                             @"CHANNELS.IDDEVICE = DEVICES.IDDEVICE AND " +
                             @"IZM_TII.TIME >= '" + usingDate.ToString("yyyyMMdd HH:00:00") +
                             @"' AND " +
                             @"IZM_TII.TIME <= '" + usingDate.AddHours(1).ToString("yyyyMMdd HH:00:00") +
                             @"' WHERE IZM_TII.PERIOD = 180 AND " +
                             @"IZM_TII.IDCHANNEL IN(" + sensors +
                             @") " +
                             @"ORDER BY IZM_TII.TIME";
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            return request;
        }

        public string hoursRequest(DateTime usingDate, string sensors)
        {
            string request;

            switch (type())
            {
                case TEC.TEC_TYPE.COMMON:
                    //request = @"SELECT DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER, DATA.VALUE0, DATA.DATA_DATE, SENSORS.ID, DATA.SEASON " +
                    request = @"SELECT SENSORS.ID, DATA.DATA_DATE, DATA.SEASON, DATA.VALUE0 " + //, DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER " +
                                @"FROM DEVICES " +
                                @"INNER JOIN SENSORS ON " +
                                @"DEVICES.ID = SENSORS.STATIONID " +
                                @"INNER JOIN DATA ON " +
                                @"DEVICES.CODE = DATA.OBJECT AND " +
                                @"SENSORS.CODE = DATA.ITEM AND " +
                                @"DATA.DATA_DATE > '" + usingDate.ToString("yyyy.MM.dd") +
                                @"' AND " +
                                @"DATA.DATA_DATE <= '" + usingDate.AddDays(1).ToString("yyyy.MM.dd") +
                                @"' " +
                                @"WHERE DATA.PARNUMBER = 12 AND (" + sensors +
                                @") " +
                                @"ORDER BY DATA.DATA_DATE, DATA.SEASON";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                    request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.TIME, IZM_TII.WINTER_SUMMER, IZM_TII.VALUE_UNIT " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
                             @"FROM IZM_TII " +
                             @"INNER JOIN CHANNELS ON " +
                             @"IZM_TII.IDCHANNEL = CHANNELS.IDCHANNEL " +
                             @"INNER JOIN DEVICES ON " +
                             @"CHANNELS.IDDEVICE = DEVICES.IDDEVICE AND " +
                             @"IZM_TII.TIME > '" + usingDate.ToString("yyyyMMdd") +
                             @"' AND " +
                             @"IZM_TII.TIME <= '" + usingDate.AddDays(1).ToString("yyyyMMdd") +
                             @"' WHERE IZM_TII.PERIOD = 1800 AND " +
                             @"IZM_TII.IDCHANNEL IN(" + sensors +
                             @") " +
                        //@"ORDER BY IZM_TII.TIME";
                             @"ORDER BY IZM_TII.TIME, IZM_TII.WINTER_SUMMER";
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            return request;
        }

        public static string getNameTG(string templateNameBD, string nameBD)
        {
            //��������� ��� 1-�� '%'
            int pos = -1;
            string strRes = nameBD.Substring(templateNameBD.IndexOf('%'));

            //����� 1-� �� �����
            pos = 0;
            while (pos < strRes.Length)
            {
                if ((strRes[pos] < '0') || (strRes[pos] > '9'))
                    break;
                else
                    ;

                pos++;
            }
            //�������� - ��� ������� ������ �� ����� �����
            if (!(pos < strRes.Length))
                return strRes;
            else
                ;

            strRes = strRes.Substring(0, pos);

            strRes = "��" + strRes;

            return strRes;
        }

        public string GetPBRValueQuery(int num_comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    if (num_comp < 0)
                    {
                        string strDelim = ", ",
                                comp_selectPBRValueQuery = string.Empty;
                        foreach (TECComponent g in list_TECComponents)
                        {
                            comp_selectPBRValueQuery = string.Empty;
                            if ((g.m_id > 100) && (g.m_id < 500))
                            {
                                comp_selectPBRValueQuery = selectPBRValueQuery(g);

                                if (comp_selectPBRValueQuery.Length > 0)
                                    selectPBR += (strDelim + comp_selectPBRValueQuery);
                                else
                                    ;
                            }
                            else
                                ;
                        }
                        if (selectPBR.Length > strDelim.Length)
                            selectPBR = selectPBR.Substring(strDelim.Length);
                        else
                            ;
                    }
                    else
                    {
                        selectPBR = selectPBRValueQuery(list_TECComponents[num_comp]);
                    }
                    break;
                default:
                    break;
            }

            strRes = pbrValueQuery(selectPBR, dt, mode);

            return strRes;
        }

        public string GetPBRValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    selectPBR = selectPBRValueQuery(comp);
                    break;
                default:
                    break;
            }

            strRes = pbrValueQuery(selectPBR, dt, mode);

            return strRes;
        }

        private string selectAdminValueQuery(TECComponent g)
        {
            string strRes = String.Empty;

            if (g.prefix_admin.Length > 0)
            {
                strRes += m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + g.tec.prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                            m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                            m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }
            else {
                strRes += m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                            m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                            m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                //selectPBR += strUsedPPBRvsPBR + "." + namePBR + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }

            return strRes;
        }

        private string adminValueQuery(string selectAdmin, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty;
            
            switch (mode) {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
                                selectAdmin + //" AS ADMIN_VALUES" +
                        //@", " + selectPBR +
                        //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +
                                @" " + @"FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +

                                @" " + @"WHERE " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                                @" " + @"AND " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +

                                @" " + @"UNION " +
                                @"SELECT " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +

                                //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
                                selectAdmin +
                        //@", " + selectPBR +
                        //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +

                                @" " + @"FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +

                                //" RIGHT JOIN " + strUsedPPBRvsPBR +
                        //" ON " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " = " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " " +

                                @" " + @"WHERE " +

                                //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                        //@"' AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                        //@"' AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +

                                //@" AND " +
                                m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL" +

                                @" " + @"ORDER BY DATE_ADMIN" +
                        //@", DATE_PBR" +
                                @" " + @"ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string currentTMRequest (string sensors) {
            return @"SELECT [dbo].[states_real_his].[id], [dbo].[states_real_his].[last_changed_at], [dbo].[states_real_his].[value] " +
                            @"FROM [dbo].[states_real_his] " +
                            @"INNER JOIN " +
                                @"(SELECT [id], MAX([last_changed_at]) AS last_changed_at " +
                                @"FROM [dbo].[states_real_his] " +
                                @"GROUP BY [id]) AS t2 " +
                            @"ON ([dbo].[states_real_his].[id] = t2.[id] AND [dbo].[states_real_his].[last_changed_at] = t2.last_changed_at AND (" +
                            sensors +
                            @"))";
        }

        public string GetAdminValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectAdmin = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    selectAdmin = selectAdminValueQuery(comp);
                    break;
                default:
                    break;
            }

            strRes = adminValueQuery(selectAdmin, dt, mode);

            return strRes;
        }

        public string GetAdminValueQuery(int num_comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                selectAdmin = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    if (num_comp < 0)
                    {
                        foreach (TECComponent g in list_TECComponents)
                        {
                            if ((g.m_id > 100) && (g.m_id < 500))
                            {
                                selectAdmin += ", ";
                                //selectPBR += ", ";

                                selectAdmin += selectAdminValueQuery(g);
                            }
                            else
                                ;
                        }
                        selectAdmin = selectAdmin.Substring(2);
                        //selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        selectAdmin = selectAdminValueQuery(list_TECComponents[num_comp]);
                    }
                    break;
                default:
                    break;
            }

            strRes = adminValueQuery(selectAdmin, dt, mode);

            //switch (type())
            //{
            //    case TEC.TEC_TYPE.COMMON:

            //        break;
            //    case TEC.TEC_TYPE.BIYSK:
            //        if (num_comp < 0)
            //        {
            //            foreach (TECComponent g in list_TECComponents)
            //            {
            //                selectAdmin += ", ";
            //                //selectPBR += ", ";

            //                if (g.prefix_admin.Length > 0)
            //                {
            //                    selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                    //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + g.prefix_pbr;
            //                }
            //                else {
            //                    selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                    strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                    strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                    //selectPBR += strUsedPPBRvsPBR + @"." + namePBR;
            //                }
            //            }
            //            selectAdmin = selectAdmin.Substring(2);
            //            //selectPBR = selectPBR.Substring(2);
            //        }
            //        else
            //        {
            //            TECComponent g = list_TECComponents[num_comp];
            //            if (g.prefix_admin.Length > 0)
            //            {
            //                selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                            strUsedAdminValues + "." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                            strUsedAdminValues + @"." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + list_TECComponents[num_comp].prefix_pbr;
            //            }
            //            else {
            //                selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR;
            //            }
            //        }

            //        strRes = @"SELECT " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + ", " + selectAdmin +
            //                 @" FROM " + strUsedAdminValues + " " +
            //                 @"WHERE " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
            //                 @"' AND " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
            //                 @"' ORDER BY DATE";
            //        break;
            //    default:
            //        break;
            //}

            return strRes;
        }

        public string GetAdminDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE, ID FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT DATE_TIME, ID FROM " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] +
                            @" WHERE " +
                            @"DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string NameFieldOfAdminRequest(TECComponent comp)
        {
            string strRes = @"" + this.prefix_admin;

            if (comp.prefix_admin.Length > 0)
            {
                strRes += "_" + comp.prefix_admin;
            }
            else
                ;

            return strRes;
        }

        public string NameFieldOfPBRRequest(TECComponent comp)
        {
            string strRes = @"" + this.prefix_pbr;

            if (comp.prefix_pbr.Length > 0)
            {
                strRes += "_" + comp.prefix_pbr;
            }
            else
                ;

            return strRes;
        }
    }
}
