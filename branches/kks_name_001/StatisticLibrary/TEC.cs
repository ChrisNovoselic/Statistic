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
    /// <summary>
    /// ������������ ����� ���������� � ��
    /// </summary>
    public enum CONN_SETT_TYPE
    {
        CONFIG_DB = 0, LIST_SOURCE,
        DATA_AISKUE_PLUS_SOTIASSO = -1 /*����+��������. - ���������*/, ADMIN = 0, PBR = 1, DATA_AISKUE = 2 /*����. - �����*/, DATA_SOTIASSO = 3, DATA_SOTIASSO_3_MIN = 4, DATA_SOTIASSO_1_MIN = 5 /*������������ - ��������*/, MTERM = 6 /*�����-��������*/,
        COUNT_CONN_SETT_TYPE = 7
    };
    /// <summary>
    /// ��������� ��� �������� ���
    /// </summary>
    interface ITEC
    {
        /// <summary>
        /// ��������� �������� ���������� ���������� � ���������� ������
        /// </summary>
        /// <param name="source">������� �� ������� � ����������� ����������</param>
        /// <param name="type">��� ��������� ������</param>
        /// <returns>������� ���������� ����������</returns>
        int connSettings(System.Data.DataTable source, int type);
        /// <summary>
        /// ���������� ���������� ������� � ������(������������) ��������� ������ ��� ��������� ������� �������� ��
        /// </summary>
        /// <param name="sensors">������-����������� (����������� - �������) ���������������</param>
        /// <returns>������ �������</returns>
        string currentTMRequest(string sensors);
        /// <summary>
        /// ���������� ���������� ������� � ������(������������) ��������� ������ ��� ��������� ������� �������� �� (����������� �����)
        /// </summary>
        /// <param name="sensors">������-����������� (����������� - �������) ���������������</param>
        /// <returns>������ �������</returns>
        string currentTMSNRequest();
        /// <summary>
        /// ������� ��� ������� �������� �������������� ��������� ������ ��� ��������
        /// </summary>
        event HClassLibrary.IntDelegateIntFunc EventGetTECIdLinkSource;
        /// <summary>
        /// ����� ������ �� �� ��������������
        /// </summary>
        /// <param name="id">������������� �� � �������� � ������������ � 'indxVal' � ������� ������� 'id_time_type'</param>
        /// <param name="indxVal">������ �������� ����������</param>
        /// <param name="id_type">������ �������</param>
        /// <returns>������ ��</returns>
        TG FindTGById(object id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_time_type);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� ���������������� ��������
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        string GetAdminDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns>������ �������</returns>
        string GetAdminValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns>������ �������</returns>
        string GetAdminValueQuery(int num_comp, DateTime dt, AdminTS.TYPE_FIELDS mode);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� �������� ���
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        string GetPBRValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        string GetPBRValueQuery(int num_comp, DateTime dt, AdminTS.TYPE_FIELDS mode);
        /// <summary>
        /// ���������� ������-������������ � ����������������
        /// </summary>
        /// <param name="indx">������ ���������� (������� -1 ��� ��� � �����)</param>
        /// <param name="connSettType">��� ���������� � ��</param>
        /// <param name="indxTime">������ ��������� �������</param>
        /// <returns>������-������������ � ����������������</returns>
        string GetSensorsString(int indx, CONN_SETT_TYPE connSettType, TG.ID_TIME indxTime = TG.ID_TIME.UNKNOWN);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� ���� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <returns>������ �������</returns>
        string hoursFactRequest(DateTime usingDate, string sensors);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� ��������
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="interval">������������� ��������� �������, ��������� ��� ���������� ������������������</param>
        /// <returns>������ �������</returns>
        string hoursTMRequest(DateTime usingDate, string sensors, int interval);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� �������� (����������� �����)
        /// </summary>
        /// <param name="dtReq">���� - ������ ���������, ������������� ������</param>
        /// <returns>������ �������</returns>
        string hoursTMSNPsumRequest(DateTime dtReq);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� �������� �������� �� ��������� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="lastHour">��� � ������ ��� ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="interval">������������� ��������� �������, ��������� ��� ���������� ������������������</param>
        /// <returns>������ �������</returns>
        string hourTMRequest(DateTime usingDate, int lastHour, string sensors, int interval);
        /// <summary>
        /// ���������������� ��� ������-������������ � ���������������� ���
        /// </summary>
        void InitSensorsTEC();
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� ����������� �������� �������� �� ������ ��� � ��������� ������
        /// </summary>
        /// <param name="dt">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="cntHours">���������� ����� � ������</param>
        /// <returns>������ �������</returns>
        string lastMinutesTMRequest(DateTime dt, string sensors, int cntHours);
        /// <summary>
        /// ������� ������������� ������ � ���������������� ��
        /// </summary>
        bool m_bSensorsStrings { get; }
        /// <summary>
        /// ���� ��� ���������� � ������-������ MS Excel
        ///  �� ���������� ��� �� ������ �� (���)
        /// </summary>
        string m_path_rdg_excel { get; set; }
        /// <summary>
        /// �������� - �������� (����) ���� ����/������� �� ���� � ������� ������ "������"
        /// </summary>
        int m_timezone_offset_msc { get; set; }
        /// <summary>
        /// ���������� ���������� ������� � ��������� ������ ��� ��������� 3-� ��� �������� � ���� ���
        /// </summary>
        /// <param name="usingDate">���� - ��������� ��� ���������, ������������� ������</param>
        /// <param name="hour">��� � ������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <returns>������ �������</returns>
        string minsFactRequest(DateTime usingDate, int hour, string sensors);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� �������� �������� �� ��������� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="hour">��� �� ������� ��������� �������� ������</param>
        /// <param name="sensors">������-������������ ��� </param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
        string minsTMRequest(DateTime usingDate, int hour, string sensors, int interval);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ����������� �������� �������� �������� �� ��������� ��� � ����� ��������� ����������
        ///  , ����������� ������������ ����
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="hour">��� �� ������� ��������� �������� ������</param>
        /// <param name="min">����� ��������� ����������</param>
        /// <param name="sensors">������-������������ ��� </param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
        string minTMAverageRequest(DateTime usingDate, int hour, int min, string sensors, int interval);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ����������� �������� �������� �������� �� ��������� ��� � ����� ��������� ����������
        ///  , ���������� ������������ � ~ �� �������������� ������
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="h">��� �� ������� ��������� �������� ������</param>
        /// <param name="m">����� ��������� ����������</param>
        /// <param name="sensors">������-������������ ��� ���������������</param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
        string minTMRequest(DateTime usingDate, int h, int m, string sensors, int interval);
        /// <summary>
        /// ���������� ������� ���������� �������� �������������� ��������� ������ � ������� ��������
        /// </summary>
        void OnUpdateIdLinkSourceTM();
        /// <summary>
        /// ���������� ������������ ����� ������ ��� ��������� � �� � ��������� ��� ���������
        ///  ���������������� ��������, ���
        /// </summary>
        /// <param name="admin_datetime">������������ ���� � ������ ����/������� �������� � ������� � ����������������� ����������</param>
        /// <param name="admin_rec">������������ ���� �� ��������� ������������ � ������� � ����������������� ����������</param>
        /// <param name="admin_is_per">������������ ���� �������� �������/�������� ��� ���� ���������� � ������� � ����������������� ����������</param>
        /// <param name="admin_diviat">������������ ���� �� ���������� ���������� � ������� � ����������������� ����������</param>
        /// <param name="pbr_datetime">������������ ���� � ������ ����/������� �������� � ������� � ���</param>
        /// <param name="ppbr_vs_pbr">������������ ���� �� ���������� ������� �������� � ������� � ���</param>
        /// <param name="pbr_number">������������ ���� �� ���������� ������� ��� � ������� � ���</param>
        void SetNamesField(string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat, string pbr_datetime, string ppbr_vs_pbr, string pbr_number);
        /// <summary>
        /// ������� ��� ���
        /// </summary>
        /// <returns>��� ���</returns>
        TEC.TEC_TYPE Type{ get; }
    }
    /// <summary>
    /// ����� �������� ���
    /// </summary>
    public class TEC : StatisticCommon.ITEC
    {
        /// <summary>
        /// ������������ - ������� ����� ���������� ������ (�����-���������������� �������� ������, �������������� ��� ������ ��� - �� ��������������)
        ///  ������ ��� ���� ���, ��������
        /// </summary>
        public enum INDEX_TYPE_SOURCE_DATA { COMMON, /*INDIVIDUAL,*/ COUNT_TYPE_SOURCEDATA };
        /// <summary>
        /// ������ ����� ���������� ������ (������ ��� ���� ���, ��������)
        /// </summary>
        public INDEX_TYPE_SOURCE_DATA[] m_arTypeSourceData = new INDEX_TYPE_SOURCE_DATA[(int)(CONN_SETT_TYPE.DATA_SOTIASSO - CONN_SETT_TYPE.DATA_AISKUE + 1)];
        /// <summary>
        /// ������ ����� ����������� � ���������� ������
        ///  ������� ������������ ��� �������� � ������
        /// </summary>
        public DbInterface.DB_TSQL_INTERFACE_TYPE[] m_arInterfaceType = new DbInterface.DB_TSQL_INTERFACE_TYPE[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
        /// <summary>
        /// ������������ - ������� ����� � �������������� �������
        ///  ��� ��������� ������� ���, ���������������� ��������
        /// </summary>
        public enum INDEX_NAME_FIELD { ADMIN_DATETIME, REC, IS_PER, DIVIAT,
                                        PBR_DATETIME, PBR, PBR_NUMBER, 
                                        COUNT_INDEX_NAME_FIELD};
        /// <summary>
        /// ������������ - ���� ���
        ///  ������� ��� - "���������" � ��������� �� ������ ���� ���, ��������
        /// </summary>
        public enum TEC_TYPE { COMMON, BIYSK };
        /// <summary>
        /// ������������� ��� (�� �� ������������)
        /// </summary>
        public int m_id;
        /// <summary>
        /// ������� ������������
        /// </summary>
        public string name_shr;
        /// <summary>
        /// ������ ������������ ������ �� ���������� ���, ����������������� ����������
        /// </summary>
        public string [] m_arNameTableAdminValues, m_arNameTableUsedPPBRvsPBR;
        /// <summary>
        /// ������ ������������ �����
        /// </summary>
        public List <string> m_strNamesField;
        /// <summary>
        /// �������� - �������� (����) ���� ����/������� �� ���� � ������� ������ "������"
        /// </summary>
        public int m_timezone_offset_msc { get; set; }
        /// <summary>
        /// ���� ��� ���������� � ������-������ MS Excel
        ///  �� ���������� ��� �� ������ �� (���)
        /// </summary>
        public string m_path_rdg_excel { get; set;}
        /// <summary>
        /// ������ ��� ������������ �� (KKS_NAME) � �������� ���� ���, ��������
        /// </summary>
        public string m_strTemplateNameSgnDataTM,
                    m_strTemplateNameSgnDataFact;
        /// <summary>
        /// ������ ����������� ��� ���
        /// </summary>
        public List<TECComponent> list_TECComponents;
        /// <summary>
        /// ������ �� ��� ���
        /// </summary>
        public List<TG> m_listTG;
        /// <summary>
        /// ������-������������ (����������� - �������) � ���������������� �� � ������� ��������
        /// </summary>
        protected volatile string m_SensorsString_SOTIASSO = string.Empty;
        /// <summary>
        /// ������ �����-������������ (����������� - �������) � ���������������� �� � ������� ���� ���
        ///  ������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������
        /// </summary>
        protected volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty };
        /// <summary>
        /// ������������ - ������� ��������� ��������� ���������� "����������" ��������
        /// </summary>
        public enum SOURCE_SOTIASSO { AVERAGE, INSATANT_APP, INSATANT_TSQL };
        /// <summary>
        /// ������� �������� ���������� "����������" ��������
        /// </summary>
        public static SOURCE_SOTIASSO s_SourceSOTIASSO = SOURCE_SOTIASSO.AVERAGE;
        /// <summary>
        /// ������� ��� ���
        /// </summary>
        /// <returns>��� ���</returns>
        public TEC_TYPE Type { get { if (name_shr.IndexOf("�����") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; } }
        /// <summary>
        /// ������ � ����������� ���������� ��� ���������� ������
        /// </summary>
        public ConnectionSettings [] connSetts;        
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
        /// <summary>
        /// ���������� ������-������������ � ����������������
        /// </summary>
        /// <param name="indx">������ ���������� (������� -1 ��� ��� � �����)</param>
        /// <param name="connSettType">��� ���������� � ��</param>
        /// <param name="indxTime">������ ��������� �������</param>
        /// <returns>������-������������ � ����������������</returns>
        public string GetSensorsString (int indx, CONN_SETT_TYPE connSettType, TG.ID_TIME indxTime = TG.ID_TIME.UNKNOWN) {
            string strRes = string.Empty;

            if (indx < 0) { // ��� ���
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
            else { // ��� ��������� ���
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

        private volatile int m_IdSOTIASSOLinkSourceTM;
        /// <summary>
        /// ������� ��� ������� �������� �������������� ��������� ������ ��� ��������
        /// </summary>
        public event IntDelegateIntFunc EventGetTECIdLinkSource;
        /// <summary>
        /// ���������� ������� ���������� �������� �������������� ��������� ������ � ������� ��������
        /// </summary>
        public void OnUpdateIdLinkSourceTM ()
        {
            //���������� �� ������� ������� ������������� ��������� ������ � ������� ��������
            m_IdSOTIASSOLinkSourceTM = EventGetTECIdLinkSource(m_id);
        }
        /// <summary>
        /// ���������� ������� (� �����������)
        /// </summary>
        /// <param name="id">������������ ���</param>
        /// <param name="name_shr">������� ������������</param>
        /// <param name="table_name_admin">������������ ������� � ����������������� ����������</param>
        /// <param name="table_name_pbr">������������ ������� �� ���������� ���</param>
        /// <param name="bUseData">������� �������� �������</param>
        public TEC (int id, string name_shr, string table_name_admin, string table_name_pbr, bool bUseData) {
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

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_strNamesField = new List<string>((int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD);
            for (int i = 0; i < (int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD; i++) m_strNamesField.Add(string.Empty);           
        }
        /// <summary>
        /// ���������� ������������ ����� ������ ��� ��������� � �� � ��������� ��� ���������
        ///  ���������������� ��������, ���
        /// </summary>
        /// <param name="admin_datetime">������������ ���� � ������ ����/������� �������� � ������� � ����������������� ����������</param>
        /// <param name="admin_rec">������������ ���� �� ��������� ������������ � ������� � ����������������� ����������</param>
        /// <param name="admin_is_per">������������ ���� �������� �������/�������� ��� ���� ���������� � ������� � ����������������� ����������</param>
        /// <param name="admin_diviat">������������ ���� �� ���������� ���������� � ������� � ����������������� ����������</param>
        /// <param name="pbr_datetime">������������ ���� � ������ ����/������� �������� � ������� � ���</param>
        /// <param name="ppbr_vs_pbr">������������ ���� �� ���������� ������� �������� � ������� � ���</param>
        /// <param name="pbr_number">������������ ���� �� ���������� ������� ��� � ������� � ���</param>
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
        /// <summary>
        /// �������� ������������� �� � ��� ��������� ������-������������ (����������� - �������) � ���������������� ��
        /// </summary>
        /// <param name="prevSensors">������-������������ (����������� - �������)</param>
        /// <param name="sensor">�������������</param>
        /// <param name="typeTEC">��� ��� (������� ��� + ���������)</param>
        /// <param name="typeSourceData">��� ��������� ������</param>
        /// <returns>������-������������ � ����������� ���������������</returns>
        public static string AddSensor(string prevSensors, object sensor, TEC.TEC_TYPE typeTEC, TEC.INDEX_TYPE_SOURCE_DATA typeSourceData)
        {
            string strRes = prevSensors;
            //������� ������������� ������������ ������� ��� ��������� ���������������
            string strQuote = sensor.GetType().IsPrimitive == true ? string.Empty : @"'";
            //��������� ������� ��� ����������� ���������������
            if (prevSensors.Equals(string.Empty) == false)
                //��� ����������� - ���������� ����� ��������� ��������������� ����������� (�������, �.�. TSQL)
                switch (typeSourceData)
                {
                    case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                        //����� �������� ��� ���� ���
                        strRes += @", "; //@" OR ";
                        break;
                    default:
                        break;
                }
            else
                ; //������ �� ���������
            //�������� �������������
            switch (typeSourceData)
            {
                case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                    //����� �������� ��� ���� ���                    
                    strRes += strQuote + sensor.ToString() + strQuote;
                    break;
                default:
                    break;
            }

            return strRes;
        }
        /// <summary>
        /// ����� ������ �� �� ��������������
        /// </summary>
        /// <param name="id">������������� �� � �������� � ������������ � 'indxVal' � ������� ������� 'id_time_type'</param>
        /// <param name="indxVal">������ �������� ����������</param>
        /// <param name="id_type">������ �������</param>
        /// <returns>������ ��</returns>
        public TG FindTGById(object id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_time_type)
        {
            int i = -1;
            
            for (i = 0; i < list_TECComponents.Count; i++) {
                if ((list_TECComponents [i].m_id > 1000) && (list_TECComponents [i].m_id < 10000)) {
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if (list_TECComponents[i].m_listTG[0].m_arIds_fact[(int)id_time_type] == (int)id)
                                return list_TECComponents[i].m_listTG[0];
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if (list_TECComponents[i].m_listTG[0].m_strKKS_NAME_TM.Equals((string)id) == true)
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
        /// <summary>
        /// ���������������� ��� ������-������������ � ���������������� ���
        /// </summary>
        public void InitSensorsTEC () {
            int i = -1
                , j = -1;
            TEC_TYPE type = Type;

            if (m_listTG == null)
                m_listTG = new List<TG> ();
            else
                m_listTG.Clear ();

            if (m_SensorsStrings_ASKUE == null)
                m_SensorsStrings_ASKUE = new string [(int)TG.ID_TIME.COUNT_ID_TIME];
            else
                m_SensorsStrings_ASKUE [(int)TG.ID_TIME.HOURS] = m_SensorsStrings_ASKUE [(int)TG.ID_TIME.MINUTES] = string.Empty;

            m_SensorsString_SOTIASSO = string.Empty;
            //���� �� ���� ����������� ���
            for (i = 0; i < list_TECComponents.Count; i++) {
                //��������� ��� ����������
                if ((list_TECComponents [i].m_id > 1000) && (list_TECComponents [i].m_id < 10000)) {
                    //������ ��� ��
                    m_listTG.Add(list_TECComponents[i].m_listTG[0]);
                    //����������� ������-������������ � ����������������� ��� ��� � ����� (���� ��� - ���)
                    m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS]
                                                                    , list_TECComponents[i].m_listTG[0].m_arIds_fact[(int)TG.ID_TIME.HOURS]
                                                                    , type
                                                                    , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    //����������� ������-������������ � ����������������� ��� ��� � ����� (���� ��� - ������)
                    m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES]
                                                                    , list_TECComponents[i].m_listTG[0].m_arIds_fact[(int)TG.ID_TIME.MINUTES]
                                                                    , type
                                                                    , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    //����������� ������-������������ � ����������������� ��� ��� � ����� (��������)
                    m_SensorsString_SOTIASSO = AddSensor(m_SensorsString_SOTIASSO
                                                        , list_TECComponents[i].m_listTG[0].m_strKKS_NAME_TM
                                                        , type
                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    //������������ ��������� �������������� ��� ��  (���� ��� - ���)
                    list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS]
                                                                                                , list_TECComponents[i].m_listTG[0].m_arIds_fact[(int)TG.ID_TIME.HOURS]
                                                                                                , type
                                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    //������������ ��������� �������������� ��� ��  (���� ��� - ������)
                    list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES]
                                                                                                , list_TECComponents[i].m_listTG[0].m_arIds_fact[(int)TG.ID_TIME.MINUTES]
                                                                                                , type
                                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    //������������ ��������� �������������� ��� ��  (��������)
                    list_TECComponents[i].m_SensorsString_SOTIASSO = AddSensor(list_TECComponents[i].m_SensorsString_SOTIASSO
                                                                                , list_TECComponents[i].m_listTG[0].m_strKKS_NAME_TM
                                                                                , type
                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                }
                else
                {//��� ��������� (���, �(��)��) �����������
                    //���� �� �� ����������
                    for (j = 0; j < list_TECComponents[i].m_listTG.Count; j++) {
                        //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���� ��� - ���)
                        list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.HOURS]
                                                                                                        , list_TECComponents[i].m_listTG[j].m_arIds_fact[(int)TG.ID_TIME.HOURS]
                                                                                                        , type
                                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                        //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���� ��� - ������)
                        list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES] = AddSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)TG.ID_TIME.MINUTES]
                                                                                                        , list_TECComponents[i].m_listTG[j].m_arIds_fact[(int)TG.ID_TIME.MINUTES]
                                                                                                        , type
                                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                        //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���������)
                        list_TECComponents[i].m_SensorsString_SOTIASSO = AddSensor(list_TECComponents[i].m_SensorsString_SOTIASSO
                                                                                , list_TECComponents[i].m_listTG[j].m_strKKS_NAME_TM
                                                                                , type
                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                    } // - ���� �� �� ����������
                }
            } // - ���� �� ���� ����������� ���
        }
        /// <summary>
        /// ��������� �������� ���������� ���������� � ���������� ������
        /// </summary>
        /// <param name="source">������� �� ������� � ����������� ����������</param>
        /// <param name="type">��� ��������� ������</param>
        /// <returns>������� ���������� ����������</returns>
        public int connSettings (DataTable source, int type)
        {
            int iRes = 0;

            connSetts[type] = new ConnectionSettings(source.Rows[0], -1);

            if ((!((int)type < (int)CONN_SETT_TYPE.DATA_AISKUE)) && (!((int)type > (int)CONN_SETT_TYPE.DATA_SOTIASSO)))
                if (FormMainBase.s_iMainSourceData == connSetts[(int)type].id)
                {
                    m_arTypeSourceData[(int)type - (int)CONN_SETT_TYPE.DATA_AISKUE] = TEC.INDEX_TYPE_SOURCE_DATA.COMMON;
                }
                else
                    ; //??? throw new Exception(@"TEC::connSettings () - ����������� ��� ��������� ������...")
            else
                ;

            m_arInterfaceType[(int)type] = DbTSQLInterface.getTypeDB(connSetts[(int)type].port);

            return iRes;
        }
        /// <summary>
        /// ���������� ������-������������ � ���������������� ��� ��� � ����� ��� �� �����������
        /// </summary>
        /// <param name="num_comp">����� (������) ���������� (��� ��� = -1)</param>
        /// <returns></returns>
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���
        /// </summary>
        /// <param name="selectPBR">������������-������������ ����� (����������� - ����� � �������)</param>
        /// <param name="dt">����/����� - ��������� ��� ���������, ������������� ������</param>
        /// <param name="mode">����� ����� �� �������� (� ����./����� �� ���������� - ������������ ����� 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ � ��������</returns>
        private string pbrValueQuery(string selectPBR, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {//??? �������� � �������� ������ ����/�����. MS SQL: 'yyyyMMdd HH:mm:ss', MySql: 'yyyy-MM-dd HH:mm:ss'
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:                    
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
        /// <summary>
        /// ���������� ���������� ������� � ������(������������) ��������� ������ ��� ��������� 3-� ��� �������� � ���� ���
        /// </summary>
        /// <param name="dt">����/����� - ��������� ��� ���������, ������������� ������</param>
        /// <param name="sen">������-������������ ���������������</param>
        /// <returns>������ �������</returns>
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
        /// <summary>
        /// ���������� ���������� ������� � ��������� ������ ��� ��������� 3-� ��� �������� � ���� ���
        /// </summary>
        /// <param name="usingDate">���� - ��������� ��� ���������, ������������� ������</param>
        /// <param name="hour">��� � ������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <returns>������ �������</returns>
        public string minsFactRequest(DateTime usingDate, int hour, string sensors)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            usingDate = usingDate.Date.AddHours(hour);
            string request = string.Empty;

            switch (Type)
            {
                case TEC.TEC_TYPE.COMMON:
                    switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = minsFactCommonRequest (usingDate, sensors);
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ����������� �������� �������� �������� �� ��������� ��� � ����� ��������� ����������
        ///  , ����������� ������������ ����
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="hour">��� �� ������� ��������� �������� ������</param>
        /// <param name="min">����� ��������� ����������</param>
        /// <param name="sensors">������-������������ ��� ���������������</param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
        public string minTMAverageRequest(DateTime usingDate, int hour, int min, string sensors, int interval)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            if (min == 0) min++; else ;

            DateTime dtReq = usingDate.Date.AddHours(hour).AddMinutes(interval * (min - 1));

            return @"SELECT [KKS_NAME], AVG([Value]) as [VALUE], COUNT (*) as [CNT]"
                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                        + @" AND [Value] > 1"
                                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    + @" GROUP BY [KKS_NAME]";
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ����������� �������� �������� �������� �� ��������� ��� � ����� ��������� ����������
        ///  , ���������� ������������ � ~ �� �������������� ������
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="h">��� �� ������� ��������� �������� ������</param>
        /// <param name="m">����� ��������� ����������</param>
        /// <param name="sensors">������-������������ ��� ���������������</param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
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
                                @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes (interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                ;
                        else
                            if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_TSQL)
                                request = @"SELECT [KKS_NAME], SUM([Value]*[tmdelta])/SUM([tmdelta]) AS [Value]"
	                                    + @" FROM ("
                                            //--�������� ����/����� � ��� (�������� �������� � UTC)
                                            + @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                                //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString (@"yyyyMMdd HH:mm:00.000") + @"')"
                                                + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes (interval).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            + @" ) as S0"
                                        + @" GROUP BY S0.[KKS_NAME]";
                            else
                                ;
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"TEC::minTMRequest (hour=" + hour + @", min=" + min + @") - dtReq=" + dtReq.ToString(@"yyyyMMdd HH:mm:00"));

            return request;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� �������� �������� �� ��������� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="hour">��� �� ������� ��������� �������� ������</param>
        /// <param name="sensors">������-������������ ��� </param>
        /// <param name="interval">������������� ��������� ����������</param>
        /// <returns>������ �������</returns>
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
                        @"SELECT [KKS_NAME] as [KKS_NAME], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
	                        + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
	                        + @", (DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) / " + interval + @") as [MINUTE]"
                        + @" FROM ("
                            + @"SELECT [KKS_NAME] as [KKS_NAME], [Value] as [VALUE], [tmdelta] as [tmdelta]"
		                        + @", DATEADD (MINUTE, - DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) % " + interval + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [last_changed_at]"
	                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                            + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                + @" AND [Value] > 1"
		                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                     + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                        + @") t0"
                        + @" GROUP BY [KKS_NAME], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]), DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                        + @" ORDER BY [last_changed_at], [KKS_NAME]"
                        ;
                    else
                        if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_APP)
                            request = //--�������� ����/����� � ��� (�������� �������� � UTC)
                                      @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                      + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                      + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    ;
                        else
                            if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.INSATANT_TSQL)
                                request = @"SELECT [KKS_NAME],"
                                            //--AVG ([value]) as VALUE
                                            + @" SUM([Value]*[tmdelta])/SUM([tmdelta]) AS [Value]"
                                            + @", (DATEPART(MINUTE, [last_changed_at])) as [MINUTE]"
                                            + @" FROM ("
                                                //--�������� ����/����� � ��� (�������� �������� � UTC)
                                                + @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"')"
                                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            + @") as S0"
                                            + @" GROUP BY S0.[KKS_NAME], DATEPART(MINUTE, S0.[last_changed_at])"
                                            + @" ORDER BY [MINUTE]";
                            else
                                ;
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� ���� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <returns>������ �������</returns>
        public string hoursFactRequest(DateTime usingDate, string sensors)
        {
            string request = string.Empty;

            switch (Type)
            {
                case TEC.TEC_TYPE.COMMON:
                    switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.COMMON:
                            request = hoursFactCommonRequest(usingDate, sensors);
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
		                + @" [KKS_NAME] as [KKS_NAME], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
		                + @", DATEPART (HOUR, [last_changed_at]) as [HOUR]"
                    + @" FROM ("
                        + @"SELECT"
                            + @" [KKS_NAME] as [KKS_NAME], AVG ([VALUE]) as [VALUE], SUM ([tmdelta]) as [tmdelta]"
                            + @", [last_changed_at]"
                            + @", (DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) / " + interval + @") as [MINUTE]"
                            + @" FROM ("
                                + @"SELECT"
                                    + @" [KKS_NAME] as [KKS_NAME], [Value] as [VALUE], [tmdelta] as [tmdelta]"
                                    + @", DATEADD (MINUTE, - DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) % " + interval + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                                + @" WHERE [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                    + @" AND [Value] > 1"
					                //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt1.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt2.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                            + @") t0"
                        + @" GROUP BY [KKS_NAME], [last_changed_at], DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]))"
                    + @") t1"
                    + @" GROUP BY "
                        + @" [KKS_NAME]"
                        + @", DATEPART (HOUR, [last_changed_at])"
                + @") t2"
                + @" GROUP BY [HOUR]"
                ;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� �������� �������� �� ��������� ���
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="lastHour">��� � ������ ��� ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="interval">������������� ��������� �������, ��������� ��� ���������� ������������������</param>
        /// <returns>������ �������</returns>
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
                            req = @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                    + @" WHERE"
                                    + @"[ID_TEC] = " + m_id
                                    + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-3).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds (-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_TSQL:
                            //������ �1 �� ���, ����� �� ��� - ���������� "�� ����"
                            dtReq = usingDate.Date;
                            dtReq = dtReq.AddHours(lastHour);
                            req = @"SELECT [KKS_NAME], SUM([Value]*[tmdelta])/SUM([tmdelta]) as VALUE, (DATEPART(hour, [last_changed_at])) as [HOUR]"
                                    + @" FROM ("
                                        + @"SELECT [ID], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                            + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                            + @" WHERE"
                                            + @"[ID_TEC] = " + m_id
                                            + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"')"
                                                    + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(1).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                    + @") as S0"
                                    + @" GROUP BY S0.[KKS_NAME], DATEPART(hour, S0.[last_changed_at])"
                                //+ @" ORDER BY [HOUR]"
                                ;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return req;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� ��������
        /// </summary>
        /// <param name="usingDate">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="interval">������������� ��������� �������, ��������� ��� ���������� ������������������</param>
        /// <returns>������ �������</returns>
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
                            request = @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                        + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1).ToString(@"yyyyMMdd") + @"')"
                                            + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date) - 1).AddMinutes(59).ToString(@"yyyyMMdd HH:mm:59.998") + @"')"
                                ;
                            break;
                        case SOURCE_SOTIASSO.INSATANT_TSQL:
                            //������ �3 �� ���, ����� �� ��� - ���������� "�� ����"
                            dtReq = usingDate.Date;
                            request = @"SELECT [KKS_NAME], SUM([Value]*[tmdelta])/SUM([tmdelta]) as VALUE, (DATEPART(hour, [last_changed_at])) as [HOUR]"
                                        + @" FROM ("
                                            + @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource +
                                                + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.ToString(@"yyyyMMdd") + @"')"
                                                    + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddHours(HAdmin.CountHoursOfDate(usingDate.Date) - 1).AddMinutes(59).ToString(@"yyyyMMdd HH:mm:59.998") + @"')" +
                                        @") as S0" +
                                        @" GROUP BY S0.[KKS_NAME], DATEPART(hour, S0.[last_changed_at])" +
                                        @" ORDER BY [HOUR]"
                                ;
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        public string GetPBRValueQuery(int num_comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    ;
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        public string GetPBRValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    ;
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectPBR = "PBR, Pmin, Pmax"; //??? �� ������������ m_strNamesField[(int)INDEX_NAME_FIELD.PBR];

                    selectPBR += ";";

                    selectPBR += comp.m_id.ToString ();
                    break;
                default:
                    break;
            }

            strRes = pbrValueQuery(selectPBR, dt, mode);

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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns></returns>
        public string GetAdminValueQuery(TECComponent comp, DateTime dt, AdminTS.TYPE_FIELDS mode)
        {
            string strRes = string.Empty,
                    selectAdmin = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    ;
                    break;
                case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC] +
                                    ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] +
                                    ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT]
                                    + ", " + @"[ID_COMPONENT]"
                                    + ", " + @"[SEASON]"
                                    + ", " + @"[FC]"
                                    + ", " + @"[WR_DATE_TIME]"
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
        /// <summary>
        /// ���������� ���������� ������� � ������(������������) ��������� ������ ��� ��������� ������� �������� �� (����������� �����)
        /// </summary>
        /// <param name="sensors">������-����������� (����������� - �������) ���������������</param>
        /// <returns>������ �������</returns>
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� �������� �������� (����������� �����)
        /// </summary>
        /// <param name="dtReq">���� - ������ ���������, ������������� ������</param>
        /// <returns>������ �������</returns>
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
        /// <summary>
        /// ���������� ���������� ������� � ������(������������) ��������� ������ ��� ��������� ������� �������� ��
        /// </summary>
        /// <param name="sensors">������-����������� (����������� - �������) ���������������</param>
        /// <returns>������ �������</returns>
        public string currentTMRequest(string sensors)
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    //����� �������� ��� ���� ���
                    query = @"SELECT [KKS_NAME] as KKS_NAME, [last_changed_at], [Current_Value_SOTIASSO] as value " +
                            @"FROM [dbo].[v_ALL_VALUE_SOTIASSO_KKS] " +
                            @"WHERE [ID_TEC]=" + m_id + @" " +
                            @"AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                            ;
                    break;
                default:
                    break;
            }

            return query;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ������� ����������� �������� �������� �� ������ ��� � ��������� ������
        /// </summary>
        /// <param name="dt">���� - ������ ���������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="cntHours">���������� ����� � ������</param>
        /// <returns>������ �������</returns>
        public string lastMinutesTMRequest(DateTime dt, string sensors, int cntHours)
        {
            string query = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.COMMON:
                    //dt -= HAdmin.GetUTCOffsetOfMoscowTimeZone();

                    if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.AVERAGE)
                        //������� �3.a (�� ����������� �������)
                        query = @"SELECT [KKS_NAME], [Value], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                                + @" WHERE [ID_TEC]=" + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                    //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                    + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.AddHours(cntHours).ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                    //-- ������ ������� ������ ����
                                    + @" AND DATEPART(MINUTE, [last_changed_at]) = 59"
                                + @"ORDER BY [KKS_NAME],[last_changed_at]"

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
                                    @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO]"
                                        + @" WHERE  [ID_TEC] = " + m_id + " AND [KKS_NAME] IN (" + sensors + @")" //+ @" AND ID_SOURCE=" + m_IdSOTIASSOLinkSource
                                            //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                            + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.ToString(@"yyyyMMdd HH:mm:ss") + @"')"
                                                + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dt.AddHours(cntHours).AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                            //-- ������ ������� ������ ����
                                            + @" AND DATEPART(MINUTE, [last_changed_at]) IN (58, 59)"
                                        ;
                        else
                            ;
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
                    ;
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

            return strRes;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� ���������������� ��������
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        public string GetAdminDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    ;
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
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� �������� ���
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        public string GetPBRDatesQuery(DateTime dt, AdminTS.TYPE_FIELDS mode, TECComponent comp)
        {
            string strRes = string.Empty,
                strNameFieldDateTime = m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME];

            switch (mode)
            {
                case AdminTS.TYPE_FIELDS.STATIC:
                    ;
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
    }
}
