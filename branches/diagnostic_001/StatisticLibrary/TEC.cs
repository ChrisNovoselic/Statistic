using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
//using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

using HClassLibrary;

namespace StatisticCommon
{
    public enum CONN_SETT_TYPE
    {
        CONFIG_DB = 0, LIST_SOURCE,
        DATA_AISKUE_PLUS_SOTIASSO = -1 /*����+��������. - ���������*/, ADMIN = 0, PBR = 1, DATA_AISKUE = 2 /*����. - �����*/, DATA_SOTIASSO = 3, DATA_SOTIASSO_3_MIN = 4, DATA_SOTIASSO_1_MIN = 5 /*������������ - ��������*/, MTERM = 6 /*�����-��������*/,
        COUNT_CONN_SETT_TYPE = 7
    };

    public class TEC
    {
        public enum INDEX_TYPE_SOURCE_DATA { COMMON, INDIVIDUAL, COUNT_TYPE_SOURCEDATA }; //�������������� ��������� ��� ������ ���
        public INDEX_TYPE_SOURCE_DATA[] m_arTypeSourceData = new INDEX_TYPE_SOURCE_DATA [(int)INDEX_TYPE_SOURCE_DATA.COUNT_TYPE_SOURCEDATA];
        public DbInterface.DB_TSQL_INTERFACE_TYPE[] m_arInterfaceType = new DbInterface.DB_TSQL_INTERFACE_TYPE[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

        public enum INDEX_NAME_FIELD { ADMIN_DATETIME, REC, IS_PER, DIVIAT,
                                        PBR_DATETIME, PBR, PBR_NUMBER, 
                                        COUNT_INDEX_NAME_FIELD};
        public enum TEC_TYPE { COMMON, BIYSK };

        public int m_id;
        public string name_shr,
                    prefix_admin, prefix_pbr;
        public string [] m_arNameTableAdminValues, m_arNameTableUsedPPBRvsPBR;
        public List <string> m_strNamesField;

        public int m_timezone_offset_msc { get; set; }
        public string m_path_rdg_excel { get; set;}
        public string m_strTemplateNameSgnDataTM,
                    m_strTemplateNameSgnDataFact;

        public List<TECComponent> list_TECComponents;

        public List<TG> m_listTG;
        protected volatile string m_SensorsString_SOTIASSO = string.Empty;
        protected volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������

        public enum SOURCE_SOTIASSO { AVERAGE, INSATANT_APP, INSATANT_TSQL };
        public static SOURCE_SOTIASSO s_SourceSOTIASSO = SOURCE_SOTIASSO.AVERAGE;

        public TEC_TYPE type() { if (name_shr.IndexOf("�����") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;        

        //private DbInterface [] m_arDBInterfaces; //��� ������ (SQL ������)

        //public FormParametersTG parametersTGForm;

        /// <summary>
        /// ������� ������������� ������ � ���������������� ��
        /// </summary>
        public bool m_bSensorsStrings {
            get {
                bool bRes = false;
                if ((m_SensorsString_SOTIASSO.Equals (string.Empty) == false) && (! (m_SensorsStrings_ASKUE == null))) {
                    if ((m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS].Equals(string.Empty) == false) && (m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES].Equals(string.Empty) == false))
                        bRes = true;
                    else
                        ;
                }
                else
                    ;

                return bRes;
            }
        }

        public string GetSensorsString (int indx, CONN_SETT_TYPE connSettType, TG.ID_TIME indxTime = TG.ID_TIME.UNKNOWN) {
            string strRes = string.Empty;

            if (indx < 0) {
                switch ((int)connSettType) {
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO:
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                        strRes = m_SensorsString_SOTIASSO;
                        break;
                    case (int)CONN_SETT_TYPE.DATA_AISKUE:
                        strRes = m_SensorsStrings_ASKUE[(int)indxTime];
                        break;
                    default:
                        Logging.Logg().Error(@"TEC::GetSensorsString (CONN_SETT_TYPE=" + connSettType.ToString ()
                                        + @"; TG.ID_TIME=" + indxTime.ToString() + @")", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
            }
            else {
                switch ((int)connSettType) {
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO:
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                    case (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                        strRes = list_TECComponents [indx].m_SensorsString_SOTIASSO;
                        break;
                    case (int)CONN_SETT_TYPE.DATA_AISKUE:
                        strRes = list_TECComponents[indx].m_SensorsStrings_ASKUE[(int)indxTime];
                        break;
                    default:
                        Logging.Logg().Error(@"TEC::GetSensorsString (CONN_SETT_TYPE=" + connSettType.ToString()
                                        + @"; TG.ID_TIME=" + indxTime.ToString() + @")", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
            }

            return strRes;
        }

        public TEC (int id, string name_shr, string table_name_admin, string table_name_pbr, string prefix_admin, string prefix_pbr, bool bUseData) {
            list_TECComponents = new List<TECComponent>();

            this.m_id = id;
            this.name_shr = name_shr;

            //���������  ������ ��� ������������ ������ � �����, -��� ����������
            this.m_arNameTableAdminValues = new string[(int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS]; this.m_arNameTableUsedPPBRvsPBR = new string[(int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS];

            //��������� ������������ ������ � �����, -��� ���������� �� ������������ �������������� �����
            this.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] = table_name_admin;
            this.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] = table_name_pbr;
            //��������� ������������ ������ � �����, -��� ���������� �� ������������� �������������� �����
            //������� �1 (� �.�. � ��� ��� ������������ �������� ������ �� CSV-�����)
            this.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = @"AdminValuesOfID"; //@"AdminValuesOfID_20141026";
            this.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = @"PPBRvsPBROfID"; // @"PPBRvsPBROfID-Test";
            ////������� �2 (������������ ������ �� �� ������������, �������� �������� ������� ���!!! )
            //this.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = table_name_admin;
            //this.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] = table_name_pbr;

            //��������� ��������� ����� ����������� �����
            this.prefix_admin = prefix_admin; this.prefix_pbr = prefix_pbr;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_strNamesField = new List<string>((int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD);
            for (int i = 0; i < (int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD; i++) m_strNamesField.Add(string.Empty);
            
        }

        public void SetNamesField (string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat,
                                    string pbr_datetime, string ppbr_vs_pbr, string pbr_number) {
            //INDEX_NAME_FIELD.ADMIN_DATETIME
            m_strNamesField [(int)INDEX_NAME_FIELD.ADMIN_DATETIME] = admin_datetime;
            m_strNamesField[(int)INDEX_NAME_FIELD.REC] = admin_rec; //INDEX_NAME_FIELD.REC
            m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] = admin_is_per; //INDEX_NAME_FIELD.IS_PER
            m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT] = admin_diviat; //INDEX_NAME_FIELD.DIVIAT

            m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] = pbr_datetime; //INDEX_NAME_FIELD.PBR_DATETIME
            m_strNamesField[(int)INDEX_NAME_FIELD.PBR] = ppbr_vs_pbr; //INDEX_NAME_FIELD.PBR

            m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER] = pbr_number; //INDEX_NAME_FIELD.PBR_NUMBER
        }

        public static string AddSensor(string prevSensors, int sensor, TEC.TEC_TYPE typeTEC, TEC.INDEX_TYPE_SOURCE_DATA typeSourceData)
        {
            string strRes = prevSensors;

            if (prevSensors.Equals(string.Empty) == false)
                switch (typeSourceData)
                {
                    case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                        //����� �������� ��� ���� ���
                        strRes += @", "; //@" OR ";
                        break;
                    case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                        //�������� ��� ������ ��� ����
                        strRes += @" OR ";
                        break;
                    default:
                        break;
                }
            else
                ;

            switch (typeSourceData)
            {
                case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                    //����� �������� ��� ���� ���
                    strRes += sensor.ToString();
                    break;
                case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    //�������� ��� ������ ��� ����
                    switch (typeTEC)
                    {
                        case TEC.TEC_TYPE.BIYSK:
                            strRes += @"[dbo].[IZM_TII].[IDCHANNEL] = " + sensor.ToString();
                            break;
                        case TEC.TEC_TYPE.COMMON:
                        default:
                            strRes += @"[dbo].[SENSORS].[ID] = " + sensor.ToString();
                            break;
                    }
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
        {
            int i = -1;
            
            for (i = 0; i < list_TECComponents.Count; i++) {
                if ((list_TECComponents [i].m_id > 1000) && (list_TECComponents [i].m_id < 10000)) {
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if (list_TECComponents[i].m_listTG [0].ids_fact[(int)id_type] == id)
                                return list_TECComponents[i].m_listTG[0];
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if (list_TECComponents[i].m_listTG[0].id_tm == id)
                                return list_TECComponents[i].m_listTG[0];
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

            return null;
        }

        public void InitSensorsTEC () {
            int i = -1
                , j = -1;

            if (m_listTG == null)
                m_listTG = new List<TG> ();
            else
                m_listTG.Clear ();

            if (m_SensorsStrings_ASKUE == null)
                m_SensorsStrings_ASKUE = new string [(int)TG.ID_TIME.COUNT_ID_TIME];
            else
                m_SensorsStrings_ASKUE [(int)TG.ID_TIME.HOURS] = m_SensorsStrings_ASKUE [(int)TG.ID_TIME.MINUTES] = string.Empty;

            m_SensorsString_SOTIASSO = string.Empty;

            for (i = 0; i < list_TECComponents.Count; i++) {
                if ((list_TECComponents [i].m_id > 1000) && (list_TECComponents [i].m_id < 10000)) {
                    m_listTG.Add(list_TECComponents[i].m_listTG[0]);

                    m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS], list_TECComponents[i].m_listTG[0].ids_fact[(int)TG.ID_TIME.HOURS], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES], list_TECComponents[i].m_listTG[0].ids_fact[(int)TG.ID_TIME.MINUTES], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    m_SensorsString_SOTIASSO = AddSensor(m_SensorsString_SOTIASSO, list_TECComponents[i].m_listTG[0].id_tm, type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);

                    list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS], list_TECComponents[i].m_listTG[0].ids_fact[(int)TG.ID_TIME.HOURS], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES], list_TECComponents[i].m_listTG[0].ids_fact[(int)TG.ID_TIME.MINUTES], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    list_TECComponents[i].m_SensorsString_SOTIASSO = AddSensor(list_TECComponents[i].m_SensorsString_SOTIASSO, list_TECComponents[i].m_listTG[0].id_tm, type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                }
                else
                {
                    for (j = 0; j < list_TECComponents[i].m_listTG.Count; j++) {
                        list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS], list_TECComponents[i].m_listTG[j].ids_fact[(int)TG.ID_TIME.HOURS], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                        list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES], list_TECComponents[i].m_listTG[j].ids_fact[(int)TG.ID_TIME.MINUTES], type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                        list_TECComponents[i].m_SensorsString_SOTIASSO = AddSensor(list_TECComponents[i].m_SensorsString_SOTIASSO, list_TECComponents[i].m_listTG[j].id_tm, type(), m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    }
                }
            }
        }

        public int connSettings (DataTable source, int type) {
            int iRes = 0;

            connSetts[type] = new ConnectionSettings(source.Rows[0], -1);

            if ((!((int)type < (int)CONN_SETT_TYPE.DATA_AISKUE)) && (!((int)type > (int)CONN_SETT_TYPE.DATA_SOTIASSO)))
                if (FormMainBase.s_iMainSourceData == connSetts[(int)type].id)
                {
                    m_arTypeSourceData[(int)type - (int)CONN_SETT_TYPE.DATA_AISKUE] = TEC.INDEX_TYPE_SOURCE_DATA.COMMON;
                }
                else
                    m_arTypeSourceData[(int)type - (int)CONN_SETT_TYPE.DATA_AISKUE] = TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL;
            else
                ;

            m_arInterfaceType[(int)type] = DbTSQLInterface.getTypeDB(connSetts[(int)type].port);

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
                    foreach (TG tg in list_TECComponents[num_comp].m_listTG)
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
                if (g.prefix_pbr.Length > 0)
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
        {//??? �������� � �������� ������ ����/�����. MS SQL: 'yyyyMMdd HH:mm:ss', MySql: 'yyyy-MM-dd HH:mm:ss'
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
                        strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];
                    else
                        ;
                    strRes += @" " + @"FROM " +
                        /*strUsedAdminValues*/ m_arNameTableUsedPPBRvsPBR[(int)mode] +
                        @" WHERE " + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" AND " + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        //@" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +
                        @" AND DATEPART(n," + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +
                        //@" AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " IS NULL" +
                        @" ORDER BY DATE_PBR" +
                        @" ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " +
                        //m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR" +
                        //@", " + selectPBR.Split (';')[0] + " AS PBR";
                        @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "]." + "DATE_TIME" + " AS DATE_PBR" +
                        //@", " + "PBR" + " AS PBR";
                        @", " + selectPBR.Split(';')[0];

                    //if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        //strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];
                    strRes += @", " + @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "]." + @"PBR_NUMBER";
                    //else
                    //    ;

                    //������ ������� ��� ��� ���
                    strRes += @", " + "ID_COMPONENT";

                    strRes += @" " + @"FROM " +
                        @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + @"]" + 
                        //@" WHERE " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        //@" AND " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" WHERE " + @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "]." + "DATE_TIME" + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" AND " + @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "]." + "DATE_TIME" + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +

                        @" AND ID_COMPONENT IN (" + selectPBR.Split (';')[1] + ")" +

                        //@" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0";
                        //@" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + "DATE_TIME" + ") = 0";
                        @" AND DATEPART(n," + @"[" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "]." + "DATE_TIME" + ") = 0";
                    /*
                    if (selectPBR.Split(';')[1].Split (',').Length > 1)
                        strRes += @" GROUP BY DATE_PBR";
                    else
                        ;
                    */
                    strRes += @" ORDER BY DATE_PBR" +
                        @" ASC";
                    break;
                default:
                    break;
            }

            if (m_arInterfaceType [(int)CONN_SETT_TYPE.PBR] == DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL) {
                strRes = strRes.Replace(@"DATEPART(n,", @"MINUTE(");
            }
            else
                ;

            return strRes;
        }

        //public string sensorsFactRequest()
        //{
        //    string request = string.Empty;

        //    switch (s_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_ASKUE - (int)CONN_SETT_TYPE.DATA_ASKUE]) {
        //        case INDEX_TYPE_SOURCE_DATA.COMMON:
        //            request = @"SELECT [SENSORS_NAME] as NAME, [ID] FROM [dbo].[ID_PARAM_Piramida2000] WHERE [ID_TEC]=" + m_id;
        //            break;
        //        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
        //            request = @"SELECT DISTINCT SENSORS.NAME, SENSORS.ID " +
        //                    @"FROM DEVICES " +
        //                    @"INNER JOIN SENSORS ON " +
        //                    @"DEVICES.ID = SENSORS.STATIONID " +
        //                    @"INNER JOIN DATA ON " +
        //                    @"DEVICES.CODE = DATA.OBJECT AND " +
        //                    @"SENSORS.CODE = DATA.ITEM " +
        //                    @"WHERE DATA.PARNUMBER = 12 AND " + //����� � '2' �����������
        //                        //@"SENSORS.NAME LIKE '��%P%+'";
        //                    @"SENSORS.NAME LIKE '" + m_strTemplateNameSgnDataFact + @"'";
        //            break;
        //        default:
        //            break;
        //    }

        //    return request;
        //}

        //public string sensorsTMRequest()
        //{
        //    string query = string.Empty;
        //    List <int> ids = new List<int> ();

        //    switch (s_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO_INSTANT - (int)CONN_SETT_TYPE.DATA_ASKUE])
        //    {
        //        case INDEX_TYPE_SOURCE_DATA.COMMON:
        //            //����� �������� ��� ���� ���
        //            query = @"SELECT [name] as NAME, [TEMPLATE_NAME_SGN_DATA_TM] as TEMPLATE_NAME, [ID_IN_REALS_RV] as ID FROM [dbo].[v_ALL_PARAM_TG] WHERE [ID_TG] IN (";
        //            foreach (TECComponent tc in list_TECComponents)
        //            {
        //                if ((tc.m_id > 1000) && (tc.m_id < 10000) && (ids.IndexOf(tc.m_id) < 0))
        //                    query += tc.m_id + @", ";
        //                else
        //                    ;
        //            }
        //            query = query.Substring(0, query.Length - 2);
        //            query += @")";
        //            break;
        //        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
        //            //�������� ��� ������ ��� ����
        //            query = @"SELECT [NAME], [ID] FROM [dbo].[reals_rv] WHERE [NAME] LIKE '" + m_strTemplateNameSgnDataTM + @"'";
        //            break;
        //    }
            
        //    return query;
        //}

        private string minsFactCommonRequest (DateTime dt, string sen) {
            return @"SELECT * FROM [dbo].[ft_get_value_askue](" + m_id + @"," +
                                2 + @"," +
                //usingDate.ToString("yyyy.MM.dd") + @"," +
                                @"'" + dt.ToString("yyyyMMdd HH:00:00") + @"'" + @"," +
                //usingDate.AddDays(1).ToString("yyyy.MM.dd") +
                                @"'" + dt.AddHours(1).ToString("yyyyMMdd HH:00:00") + @"'" +
                                @") WHERE [ID] IN (" +
                                sen +
                                @")" +
                                @" ORDER BY DATA_DATE";
        }
        
        public string minsFactRequest(DateTime usingDate, int hour, string sensors)
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
                    switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = minsFactCommonRequest (usingDate, sensors);
                            break;
                        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
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
                        default:
                            break;
                    }
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = minsFactCommonRequest(usingDate, sensors);
                            break;
                        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                            //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                            request = @"SELECT IZM_TII.IDCHANNEL AS ID, IZM_TII.TIME AS DATA_DATE, IZM_TII.WINTER_SUMMER AS SEASON, IZM_TII.VALUE_UNIT AS VALUE0 " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
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
                            break;
                    }
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            return request;
        }

        public string minTMAverageRequest(DateTime usingDate, int hour, int min, string sensors, int interval)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            if (min == 0) min++; else ;

            DateTime dtReq = usingDate.Date.AddHours(hour).AddMinutes(interval * (min - 1));

            return @"SELECT [ID], AVG([Value]) as [VALUE], COUNT (*) as [CNT]"
                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                        + @" AND [Value] > 1"
                                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    + @" GROUP BY [ID]";
        }

        public string minTMRequest(DateTime usingDate, int h, int m, string sensors, int interval)
        {
            int hour= -1, min = -1;

            if (h == 24)
                hour = 23;
            else
                hour = h;

            if (m == 0) min = 1; else min = m;

            DateTime dtReq = usingDate.Date.AddHours(hour).AddMinutes(interval * (min - 1));
            string request = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.AVERAGE)
                        request =
                            @"SELECT SUM ([VALUE]) as [VALUE], COUNT (*) as [cnt]"
                                + @" FROM ("
                                    + minTMAverageRequest(usingDate, h, m, sensors, interval)
                                + @") t0"
                            ;
                    else
                        if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_APP)
                            request =   
                                //--�������� ����/����� � ��� (�������� �������� � UTC)
                                @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
			                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" +  sensors + @")"
                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes (interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                ;
                        else
                            if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_TSQL)
                                request = @"SELECT [ID], SUM([Value]*[tmdelta])/SUM([tmdelta]) AS [Value]"
	                                    + @" FROM ("
                                            //--�������� ����/����� � ��� (�������� �������� � UTC)
                                            + @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
			                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" +  sensors + @")"
                                                //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString (@"yyyyMMdd HH:mm:00.000") + @"')"
                                                + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes (interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            + @" ) as S0"
                                        + @" GROUP BY S0.[ID]";
                            else
                                ;
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    switch (type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC_TYPE.BIYSK:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"TEC::minTMRequest (hour=" + hour + @", min=" + min + @") - dtReq=" + dtReq.ToString(@"yyyyMMdd HH:mm:00"));

            return request;
        }

        public string minsTMRequest(DateTime usingDate, int hour, string sensors, int interval)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            DateTime dtReq = usingDate.Date.AddHours(hour);
            string request = string.Empty;

            switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.AVERAGE)
                        request =
                        ////������� �1
                        ////--�������� ����/����� � ��� (�������� �������� � UTC)
                        //@"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                        ////--����� ������
                        //+ @", DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [MINUTE]"
                        //+ @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                        //+ @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                        //    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                        //    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                        //        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                        //������� �2
                        @"SELECT [ID] as [ID], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
	                        + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
	                        + @", (DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) / " + interval + @") as [MINUTE]"
                        + @" FROM ("
	                        + @"SELECT [ID] as [ID], [Value] as [VALUE], [tmdelta] as [tmdelta]"
		                        + @", DATEADD (MINUTE, - DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) % " + interval + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [last_changed_at]"
	                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                            + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                + @" AND [Value] > 1"
		                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                     + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                        + @") t0"
                        + @" GROUP BY [ID], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]), DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                        + @" ORDER BY [last_changed_at], [ID]"
                        ;
                    else
                        if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_APP)
                            request = //--�������� ����/����� � ��� (�������� �������� � UTC)
                                      @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                      + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                      + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    ;
                        else
                            if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_TSQL)
                                request = @"SELECT [ID]," +
                                            //--AVG ([value]) as VALUE
                                            @" SUM([Value]*[tmdelta])/SUM([tmdelta]) AS [Value]" +
                                            @", (DATEPART(MINUTE, [last_changed_at])) as [MINUTE]" +
                                            @" FROM (" +
                                                    //--�������� ����/����� � ��� (�������� �������� � UTC)
                                                @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]" +
                                                    @" FROM [dbo].[ALL_PARAM_SOTIASSO]" +
                                                    @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")" +
                                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                                    @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"')" +
                                                        @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')" +
                                            @") as S0" +
                                            @" GROUP BY S0.[ID], DATEPART(MINUTE, S0.[last_changed_at])" +
                                            @" ORDER BY [MINUTE]";
                            else
                                ;
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    switch (type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC_TYPE.BIYSK:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return request;
        }

        private string hoursFactCommonRequest (DateTime dt, string sen) {
            return @"SELECT * FROM [dbo].[ft_get_value_askue](" + m_id + @"," +
                                12 + @"," +
                                @"'" + dt.ToString("yyyyMMdd") + @"'" + @"," +
                                @"'" + dt.AddDays(1).ToString("yyyyMMdd") + @"'" +
                                @") WHERE [ID] IN (" +
                                sen +
                                @")" +
                                @" ORDER BY DATA_DATE";
        }

        public string hoursFactRequest(DateTime usingDate, string sensors)
        {
            string request = string.Empty;

            switch (type())
            {
                case TEC.TEC_TYPE.COMMON:
                    switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = hoursFactCommonRequest(usingDate, sensors);
                            break;
                        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
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
                        default:
                            break;
                    }
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = hoursFactCommonRequest(usingDate, sensors);
                            break;
                        case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                            //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                            request = @"SELECT IZM_TII.IDCHANNEL AS ID, IZM_TII.TIME AS DATA_DATE, IZM_TII.WINTER_SUMMER AS SEASON, IZM_TII.VALUE_UNIT AS VALUE0 " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
                                     @"FROM IZM_TII " +
                                     @"INNER JOIN CHANNELS ON " +
                                     @"IZM_TII.IDCHANNEL = CHANNELS.IDCHANNEL " +
                                     @"INNER JOIN DEVICES ON " +
                                     @"CHANNELS.IDDEVICE = DEVICES.IDDEVICE AND " +
                                     @"IZM_TII.TIME > '" + usingDate.ToString("yyyyMMdd") +
                                     @"' AND " +
                                     @"IZM_TII.TIME <= '" + usingDate.AddDays(1).ToString("yyyyMMdd") +
                                     @"' WHERE IZM_TII.PERIOD = 1800 AND " +
                                     //@"IZM_TII.IDCHANNEL IN(" + sensors +
                                     @"(" + sensors +
                                     @") " +
                                //@"ORDER BY IZM_TII.TIME";
                                     @"ORDER BY IZM_TII.TIME, IZM_TII.WINTER_SUMMER";
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            return request;
        }

        private string hoursTMCommonRequestAverage (DateTime dt1, DateTime dt2, string sensors, int interval) {
            return
                @"SELECT SUM([VALUE]) as [VALUE], COUNT (*) as [CNT], [HOUR]"
                + @" FROM ("
                    + @"SELECT" 
		                + @" [ID] as [ID], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
		                + @", DATEPART (HOUR, [last_changed_at]) as [HOUR]"
                    + @" FROM ("
                        + @"SELECT"
                            + @" [ID] as [ID], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
                            + @", [last_changed_at]"
                            + @", (DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) / " + interval + @") as [MINUTE]"
                            + @" FROM ("
                                + @"SELECT"
                                    + @" [ID] as [ID], [Value] as [VALUE], [tmdelta] as [tmdelta]"
                                    + @", DATEADD (MINUTE, - DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) % " + interval + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                                + @" WHERE [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                    + @" AND [Value] > 1"
					                //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt1.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt2.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                            + @") t0"
                        + @" GROUP BY [ID], [last_changed_at], DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                    + @") t1"
                    + @" GROUP BY "
                        + @" [ID]"
                        + @", DATEPART (HOUR, [last_changed_at])"
                + @") t2"
                + @" GROUP BY [HOUR]"
                ;
        }

        public string hourTMRequest(DateTime usingDate, int lastHour, string sensors, int interval)
        {
            string req = string.Empty;
            DateTime dtReq;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    switch (TEC.s_SourceSOTIASSO)
                    {
                        case SOURCE_SOTIASSO.AVERAGE:
                            //������ �5 �� ���, ����� �� ���
                            dtReq = usingDate.Date;
                            dtReq = dtReq.AddHours(lastHour);
                            req = 
                                //@"SELECT [ID], AVG([Value]) as [VALUE]"
                                //    //--�������� ����/����� � ��� (��������� �� �������� � UTC)
                                //    + @", DATEPART (HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [HOUR]"
                                //    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                                //    + @" WHERE [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                //        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                //        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:00:00.000") + @"')"
                                //            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                //+ @" GROUP BY [ID], DATEPART (HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                                //+ @" ORDER BY [HOUR], [ID]"
                                hoursTMCommonRequestAverage(dtReq, dtReq.AddHours(1).AddMilliseconds(-2), sensors, interval)
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_APP:
                            //������ �4 �� ���, ����� �� ��� - ���������� ���������� "�� �������"
                            dtReq = usingDate.Date;
                            dtReq = dtReq.AddHours(lastHour);
                            req = @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                    + @" WHERE"
                                    + @"[ID_TEC] = " + m_id
                                    + @" AND [ID] IN (" + sensors + @")"
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-3).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds (-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_TSQL:
                            //������ �1 �� ���, ����� �� ��� - ���������� "�� ����"
                            dtReq = usingDate.Date;
                            dtReq = dtReq.AddHours(lastHour);
                            req = @"SELECT [ID], SUM([Value]*[tmdelta])/SUM([tmdelta]) as VALUE, (DATEPART(hour, [last_changed_at])) as [HOUR]"
                                    + @" FROM ("
                                        + @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                            + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                            + @" WHERE"
                                            + @"[ID_TEC] = " + m_id
                                            + @" AND [ID] IN (" + sensors + @")"
                                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"')"
                                                    + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    + @") as S0"
                                    + @" GROUP BY S0.[ID], DATEPART(hour, S0.[last_changed_at])"
                                //+ @" ORDER BY [HOUR]"
                                ;
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    break;
                default:
                    break;
            }

            return req;
        }

        public string hoursTMRequest(DateTime usingDate, string sensors, int interval)
        {//usingDate - ���������� �����
            string request = string.Empty;
            DateTime dtReq;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    switch (TEC.s_SourceSOTIASSO) {
                        case SOURCE_SOTIASSO.AVERAGE:
                            //������ �5 �� ���, ����� �� ���
                            dtReq = usingDate.Date;
                            request = 
                                //@"SELECT [ID], AVG([Value]) as [VALUE]"
                                //    //--�������� ����/����� � ��� (��������� �� �������� � UTC)
                                //    + @", DATEPART (HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [HOUR]"
                                //    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                                //    + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                //        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                //        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd") + @"')"
                                //            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date)).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                //+ @" GROUP BY [ID], DATEPART (HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                                //+ @" ORDER BY [HOUR], [ID]"
                                hoursTMCommonRequestAverage(dtReq, dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date)).AddMilliseconds(-2), sensors, interval)
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_APP:
                            //������ �4 �� ���, ����� �� ��� - ���������� ���������� "�� �������"
                            dtReq = usingDate.Date;
                            request = @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                        + @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")"
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date) - 1).AddMinutes(59).ToString(@"yyyyMMdd HH:mm:59.998") + @"')"
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_TSQL:
                            //������ �3 �� ���, ����� �� ��� - ���������� "�� ����"
                            dtReq = usingDate.Date;
                            request = @"SELECT [ID], SUM([Value]*[tmdelta])/SUM([tmdelta]) as VALUE, (DATEPART(hour, [last_changed_at])) as [HOUR]" +
                                        @" FROM (" +
                                            @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]" +
                                                @" FROM [dbo].[ALL_PARAM_SOTIASSO]" +
                                                @" WHERE  [ID_TEC] = " + m_id + @" AND [ID] IN (" + sensors + @")" +
                                                @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd") + @"')" +
                                                    @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date) - 1).AddMinutes(59).ToString(@"yyyyMMdd HH:mm:59.998") + @"')" +
                                        @") as S0" +
                                        @" GROUP BY S0.[ID], DATEPART(hour, S0.[last_changed_at])" +
                                        @" ORDER BY [HOUR]"
                                ;
                            break;
                        default:
                            break;
                    }
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    break;
                default:
                    break;
            }

            return request;
        }

        public static string getNameTG(string templateNameBD, string nameBD)
        {
            //��������� ��� 1-�� '%'
            int pos = -1;
            string strRes = nameBD.Substring(templateNameBD.IndexOf('%'));

            //����� 1-� �����
            pos = 0;
            while (pos < strRes.Length)
            {
                if ((!(strRes[pos] < '0')) && (!(strRes[pos] > '9')))
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

            strRes = strRes.Substring(pos);

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
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectPBR = "PBR, Pmin, Pmax" + /*�� ������������ m_strNamesField[(int)INDEX_NAME_FIELD.PBR]*/";" + idComponentValueQuery(num_comp);
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
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectPBR = "PBR, Pmin, Pmax"; //�� ������������ m_strNamesField[(int)INDEX_NAME_FIELD.PBR];

                    selectPBR += ";";

                    selectPBR += comp.m_id.ToString ();
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
        {//??? �������� � �������� ������ ����/�����. MS SQL: 'yyyyMMdd HH:mm:ss', MySql: 'yyyy-MM-dd HH:mm:ss'
            string strRes = string.Empty;
            
            switch (mode) {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
                                selectAdmin + //" AS ADMIN_VALUES" +
                        //@", " + selectPBR +
                        //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +
                                @" " + @"FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +

                                @" " + @"WHERE " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                                @" " + @"AND " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +

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

                                //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") +
                        //@"' AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                        //@"' AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +

                                //@" AND " +
                                m_arNameTableAdminValues[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL" +

                                @" " + @"ORDER BY DATE_ADMIN" +
                        //@", DATE_PBR" +
                                @" " + @"ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " +
                        //m_arNameTableAdminValues[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        //selectAdmin.Split (';') [0] +
                        m_arNameTableAdminValues[(int)mode] + "." + "DATE" + " AS DATE_ADMIN" +
                        ", " + selectAdmin.Split(';')[0] +

                        @" " + @"FROM " + m_arNameTableAdminValues[(int)mode] +

                        @" " + @"WHERE" +
                        @" " + @"ID_COMPONENT IN (" + selectAdmin.Split(';')[1] + ")" +

                        @" " + @"AND " +
                        //m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        m_arNameTableAdminValues[(int)mode] + "." + "DATE" + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" " + @"AND " +
                        //m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'";
                        m_arNameTableAdminValues[(int)mode] + "." + "DATE" + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'";
                    /*
                    strRes += @" " + @"UNION " +
                        @"SELECT " + strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +

                        selectAdmin.Split (';') [0] +

                        @" " + @"FROM " + strUsedAdminValues +

                        @" " + @"WHERE" +
                        @" " + @"ID_COMPONENT IN (" + selectAdmin.Split(';')[1] + ")" +

                        @" " + @"AND " +
                        strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL";
                    */
                    /*
                    if (selectAdmin.Split(';')[1].Split(',').Length > 1)
                        strRes += @" GROUP BY DATE_ADMIN";
                    else
                        ;
                    */
                    strRes += @" " + @"ORDER BY DATE_ADMIN" +
                        @" " + @"ASC";
                    break;
                default:
                    break;
            }

            return strRes;
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
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC] +
                                    ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] +
                                    ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT]
                                    + ", " + @"[ID_COMPONENT]"
                                    + ", " + @"[SEASON]"
                                    + ", " + @"[FC]"
                                ;

                    selectAdmin += ";";

                    selectAdmin += comp.m_id.ToString();
                    break;
                default:
                    break;
            }

            strRes = adminValueQuery(selectAdmin, dt, mode);

            return strRes;
        }

        public string currentTMSNRequest()
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    query = @"SELECT * FROM [dbo].[v_LAST_VALUE_TSN] WHERE ID_TEC=" + m_id;
                    break;
                default:
                    break;
            }

            return query;
        }

        public string hoursTMSNPsumRequest(DateTime dtReq)
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    query = @"SELECT AVG ([SUM_P_SN]) as VALUE, DATEPART(hour,[LAST_UPDATE]) as HOUR"
                            + @" FROM [dbo].[P_SUMM_TSN]"
                            + @" WHERE [ID_TEC] = " + m_id
                                + @" AND [LAST_UPDATE] BETWEEN '" + dtReq.Date.ToString(@"yyyyMMdd") + @"'"
                                    + @" AND '" + dtReq.Date.AddHours(HAdmin.CountHoursOfDate(dtReq)).ToString(@"yyyyMMdd HH:00:01") + @"'"
                            + @" GROUP BY DATEPART(hour,[LAST_UPDATE])"
                            + @" ORDER BY [HOUR]";
                    break;
                default:
                    break;
            }

            return query;
        }

        public string currentTMRequest(string sensors)
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    //����� �������� ��� ���� ���
                    query = @"SELECT [ID_IN_SOTIASSO] as id, [last_changed_at], [Current_Value_SOTIASSO] as value " +
                            @"FROM [dbo].[v_ALL_VALUE_SOTIASSO] " +
                            @"WHERE [ID_TEC]=" + m_id + @" " +
                            @"AND [ID_IN_SOTIASSO] IN (" + sensors + @")";
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    //�������� ��� ������ ��� ����
                    query = @"SELECT [dbo].[NAME_TABLE].[id], [dbo].[NAME_TABLE].[last_changed_at], [dbo].[NAME_TABLE].[value] " +
                                    @"FROM [dbo].[NAME_TABLE] " +
                                    @"INNER JOIN " +
                                        @"(SELECT [id], MAX([last_changed_at]) AS last_changed_at " +
                                        @"FROM [dbo].[NAME_TABLE] " +
                                        @"GROUP BY [id]) AS t2 " +
                                    @"ON ([dbo].[NAME_TABLE].[id] = t2.[id] AND [dbo].[NAME_TABLE].[last_changed_at] = t2.last_changed_at AND (" +
                                    sensors +
                                    @"))";
                    query = query.Replace(@"NAME_TABLE", @"states_real_his");
                    break;
                default:
                    break;
            }

            return query;
        }

        public string lastMinutesTMRequest(DateTime dt, string sensors, int cntHours)
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    //dt -= HAdmin.GetUTCOffsetOfMoscowTimeZone();

                    if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.AVERAGE)
                        //������� �3.a (�� ����������� �������)
                        query = @"SELECT [ID], [Value], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                                + @" WHERE [ID_TEC]=" + m_id + @" AND [ID] IN (" + sensors + @") "
                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.AddHours(cntHours).ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                    //-- ������ ������� ������ ����
                                    + @" AND DATEPART(MINUTE, [last_changed_at]) = 59"
                                + @"ORDER BY [ID],[last_changed_at]"

                        ////������� �3.� (�� ����������� �������)
                        //query = @"SELECT SUM([Value]) as [VALUE], DATEPART (HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [HOUR], COUNT (*) as [CNT]"
                        //        + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0]"
                        //        + @" WHERE [ID_TEC]=" + m_id + @" AND [ID] IN (" + sensors + @") "
                        //            //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                        //            + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                        //                + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.AddHours(cntHours).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                        //            //-- ������ ������� ������ ����
                        //            + @" AND DATEPART(MINUTE, [last_changed_at]) = 59"
                        //        + @" GROUP BY DATEPART(HOUR, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                            ;
                    else
                        if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_APP)
                            //������� �6 - ����������� ����������
                            query = //--�������� ����/����� � ��� (�������� �������� � UTC)
                                    @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                        + @" WHERE  [ID_TEC] = " + m_id + " AND [ID] IN (" + sensors + @")"
                                            //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                            + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                                + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.AddHours(cntHours).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            //-- ������ ������� ������ ����
                                            + @" AND DATEPART(MINUTE, [last_changed_at]) IN (58, 59)"
                                        ;
                        else
                            ;
                    break;
                case INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                    //���� ������ � �� �� ���
                    //dtQuery = DateTime.Now.Date.AddMinutes(-1 * (HAdmin.GetOffsetOfCurrentTimeZone()).TotalMinutes); 
                    //���� ������ � �� �� ��������
                    dt = dt.AddMinutes(-1 * (HAdmin.GetUTCOffsetOfMoscowTimeZone()).TotalMinutes);

                    //�������� ��� ������ ��� ���� - ������� �1
                    //query =@"SELECT [dbo].[NAME_TABLE].[id], AVG([dbo].[NAME_TABLE].[value]) as value, DATEPART(hour, [last_changed_at]) as last_changed_at " +
                    //        @"FROM [dbo].[NAME_TABLE] " +
                    //        @"WHERE DATEPART(n, [last_changed_at]) = 59 AND [last_changed_at] between '" + dtQuery.Date.ToString(@"yyyy.MM.dd") + @"' AND '" + dtQuery.AddDays(1).Date.ToString(@"yyyy.MM.dd") + @"' " +
                    //        @"AND (" + sensors + @") " +
                    //        @"GROUP BY [id], , DATEPART(hour, [last_changed_at])";
                    //query = query.Replace("NAME_TABLE", "states_real_his");
                    //�������� ��� ������ ��� ���� - ������� �2
                    query = @"SELECT [dbo].[NAME_TABLE].[id], [dbo].[NAME_TABLE].[value] as value,  [dbo].[NAME_TABLE].[last_changed_at] " +
                            @"FROM [dbo].[NAME_TABLE] " +
                            @"WHERE DATEPART(n, [last_changed_at]) = 0 AND [last_changed_at] between '" + dt.ToString(@"yyyy.MM.dd HH:mm:ss") + @"' AND '" + dt.AddDays(1).ToString(@"yyyy.MM.dd HH:mm:ss") + @"' " +
                                @"AND (" + sensors + @")";
                    query = query.Replace("NAME_TABLE", "states_real_his_0");
                    break;
                default:
                    break;
            }

            if (m_arInterfaceType[(int)CONN_SETT_TYPE.DATA_SOTIASSO] == DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL)
            {
                query = query.Replace(@"DATEPART(n,", @"MINUTE(");
            }
            else
                ;            

            return query;
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
                    }
                    else
                    {
                        selectAdmin = selectAdminValueQuery(list_TECComponents[num_comp]);
                    }
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectAdmin = idComponentValueQuery (num_comp);

                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC]
                                + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER]
                                + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT]
                                + ", " + @"[ID_COMPONENT]"
                                + ", " + @"[SEASON]"
                                + ", " + @"[FC]"
                                + ";"
                                + selectAdmin;
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
            //                 @"WHERE " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") +
            //                 @"' AND " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
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
                    strRes = @"SELECT DATE, ID FROM " + m_arNameTableAdminValues[(int)mode] + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyyMMdd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE, ID, SEASON FROM " + m_arNameTableAdminValues[(int)mode] + " WHERE" +
                            @" ID_COMPONENT = " + comp.m_id +
                          @" AND DATE > '" + dt/*.AddHours(-1 * m_timezone_offset_msc)*/.ToString("yyyyMMdd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty,
                strNameFieldDateTime = m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME];

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    strRes = @"SELECT " + strNameFieldDateTime + @", ID FROM " + m_arNameTableUsedPPBRvsPBR[(int)mode] +
                            @" WHERE " +
                            strNameFieldDateTime + @" > '" + dt.ToString("yyyyMMdd HH:mm:ss") +
                            @"' AND " + strNameFieldDateTime + @"<= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                            @"' ORDER BY " + strNameFieldDateTime + @" ASC";
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " + @"[DATE_TIME]" + @", [ID], [PBR_NUMBER] FROM [" + m_arNameTableUsedPPBRvsPBR[(int)mode] + @"]" +
                            @" WHERE" +
                            @" ID_COMPONENT = " + comp.m_id + "" +
                            @" AND " + @"DATE_TIME" + @" > '" + dt/*.AddHours(-1 * m_timezone_offset_msc)*/.ToString("yyyyMMdd HH:mm:ss") +
                            @"' AND " + @"DATE_TIME" + @" <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                            @"' ORDER BY " + @"DATE_TIME" + @" ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string NameFieldOfAdminRequest(TECComponent comp)
        {
            string strRes = @"" + this.prefix_admin;

            if ((!(comp.prefix_admin == null)) && (comp.prefix_admin.Length > 0))
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

            if ((! (comp.prefix_pbr == null)) && (comp.prefix_pbr.Length > 0))
            {
                strRes += "_" + comp.prefix_pbr;
            }
            else
                ;

            return strRes;
        }
    }
}