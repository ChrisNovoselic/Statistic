using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
//using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;


using ASUTP.Core;
using ASUTP.Database;
using ASUTP;
using ASUTP.Forms;

namespace StatisticCommon
{
    /// <summary>
    /// ������������ ����� ���������� � ��
    /// </summary>
    public enum CONN_SETT_TYPE
    {
        UNKNOWN = -666
        , CONFIG_DB = 0, LIST_SOURCE
        , DATA_AISKUE_PLUS_SOTIASSO = -1 /*����+��������. - ���������*/, ADMIN = 0, PBR = 1, DATA_AISKUE = 2 /*����. - ������*/, DATA_SOTIASSO = 3
        , DATA_VZLET = 4
            , DATA_SOTIASSO_3_MIN = 5, DATA_SOTIASSO_1_MIN = 6 /*������������ - ��������*/
            , MTERM = 7 /*�����-��������*/,
        COUNT_CONN_SETT_TYPE = 8
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
        event IntDelegateIntFunc EventGetTECIdLinkSource;
        /// <summary>
        /// ����� ������ �� �� ��������������
        /// </summary>
        /// <param name="id">������������� �� � �������� � ������������ � 'indxVal' � ������� ������� 'id_time_type'</param>
        /// <param name="indxVal">������ �������� ����������</param>
        /// <param name="id_type">������ �������</param>
        /// <returns>������ ��</returns>
        TG FindTGById(object id, TG.INDEX_VALUE indxVal, HDateTime.INTERVAL id_time_type);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� ���������������� ��������
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        string GetAdminDatesQuery(DateTime dt, /*AdminTS.TYPE_FIELDS mode, */TECComponent comp);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns>������ �������</returns>
        string GetAdminValueQuery(TECComponent comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns>������ �������</returns>
        string GetAdminValueQuery(int num_comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ��� ��������� �������� ���
        ///  (����� ����/������� ��� ���� ��������)
        /// </summary>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <returns>������ �������</returns>
        string GetPBRDatesQuery(DateTime dt, /*AdminTS.TYPE_FIELDS mode, */TECComponent comp);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        string GetPBRValueQuery(TECComponent comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/);
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        string GetPBRValueQuery(int num_comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/);
        /// <summary>
        /// ���������� ������-������������ � ����������������
        /// </summary>
        /// <param name="indx">������ ���������� (������� -1 ��� ��� � �����)</param>
        /// <param name="connSettType">��� ���������� � ��</param>
        /// <param name="indxTime">������ ��������� �������</param>
        /// <returns>������-������������ � ����������������</returns>
        string GetSensorsString(int indx, CONN_SETT_TYPE connSettType, HDateTime.INTERVAL indxTime = HDateTime.INTERVAL.UNKNOWN);
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
        ///// ���������� ���������� ������� ��� ��������� ������� �������� �������� (����������� �����)
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
    public partial class TEC //: StatisticCommon.ITEC
    {
        /// <summary>
        /// ������������ - ������� ����� ���������� ������ (�����-���������������� �������� ������, �������������� ��� ������ ��� - �� ��������������)
        ///  ������ ��� ���� ���, ��������
        /// </summary>
        public enum INDEX_TYPE_SOURCE_DATA { EQU_MAIN, /*INDIVIDUAL,*/ COUNT_TYPE_SOURCEDATA };
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
        public enum TEC_TYPE { UNKNOWN = -1, COMMON, BIYSK };
        /// <summary>
        /// ������������� ��� (�� �� ������������)
        /// </summary>
        public int m_id;
        /// <summary>
        /// ������� ������������
        /// </summary>
        public string name_shr;
        /// <summary>
        /// ������������-������������� � �����-�����
        /// </summary>
        public string name_MC;
        /// <summary>
        /// ������ ������������ ������ �� ���������� ���, ����������������� ����������
        /// </summary>
        public string m_strNameTableAdminValues, m_strNameTableUsedPPBRvsPBR;
        //public string [] m_arNameTableAdminValues, m_arNameTableUsedPPBRvsPBR;
        /// <summary>
        /// ������ ������������ �����
        /// </summary>
        public List <string> m_strNamesField;
        ///// <summary>
        ///// �������� - �������� (����) ���� ����/������� �� ���� � ������� ������ "������"
        ///// </summary>
        //public int m_timezone_offset_msc { get; set; }
        ///// <summary>
        ///// ���� ��� ���������� � ������-������ MS Excel
        /////  �� ���������� ��� �� ������ �� (���)
        ///// </summary>
        //public string m_path_rdg_excel { get; set;}
        ///// <summary>
        ///// ������ ��� ������������ �� (KKS_NAME) � �������� ���� ���, ��������
        ///// </summary>
        //public string m_strTemplateNameSgnDataTM
        //    , m_strTemplateNameSgnDataFact;
        /// <summary>
        /// ������ ����������� ��� ���
        /// </summary>
        public List<TECComponent> list_TECComponents;
        ///// <summary>
        ///// ������ ������� ��� ���
        ///// </summary>
        //public List<TECComponent> m_list_Vyvod;

        private List<Vyvod.ParamVyvod> _listParamVyvod;
        /// <summary>
        /// ������ �� ��� ���
        /// </summary>
        private List<TG> _listTG;

        public List<TECComponentBase> GetListLowPointDev(TECComponentBase.TYPE type) {
            List<TECComponentBase> listRes = new List<TECComponentBase>();

            switch (type) {
                case TECComponentBase.TYPE.TEPLO:
                    if (!(_listParamVyvod == null))
                        _listParamVyvod.ForEach(pv => { listRes.Add(pv); });
                    else
                        ;
                    break;
                case TECComponentBase.TYPE.ELECTRO:
                    if (!(_listTG == null))
                        _listTG.ForEach(tg => { listRes.Add(tg); });
                    else
                        ;
                    break;
                default:
                    break;
            }

            return listRes;
        }
        /// <summary>
        /// ������-������������ (����������� - �������) � ���������������� �� � ������� ��������
        /// </summary>
        protected volatile string m_SensorsString_SOTIASSO = string.Empty;
        /// <summary>
        /// ������ �����-������������ (����������� - �������) � ���������������� �� � ������� ���� ���
        ///  ������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������
        /// </summary>
        protected volatile string [] m_SensorsStrings_ASKUE;

        protected volatile string m_SensorsString_VZLET = string.Empty;

        //private string m_prefixVzletData;
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

        private static Dictionary<CONN_SETT_TYPE, string> _dictIdConfigDataSources = new Dictionary<CONN_SETT_TYPE, string>() {
            {CONN_SETT_TYPE.DATA_AISKUE, @"ID_SOURCE_DATA"}
            , {CONN_SETT_TYPE.DATA_SOTIASSO, @"ID_SOURCE_DATA_TM"}
            , {CONN_SETT_TYPE.ADMIN, @"ID_SOURCE_ADMIN"}
            , {CONN_SETT_TYPE.PBR, @"ID_SOURCE_PBR"}
            , {CONN_SETT_TYPE.MTERM, @"ID_SOURCE_MTERM"}
            , {CONN_SETT_TYPE.DATA_VZLET, @"ID_SOURCE_DATAVZLET"}
        };
        /// <summary>
        /// ������� � ������ - ����: ������������� ���� ���������� ������, �������� - ������������ ���� ������� [TEC_LIST] � �� ������������
        /// </summary>
        public static Dictionary<CONN_SETT_TYPE, string> s_dictIdConfigDataSources { get { return _dictIdConfigDataSources; } }

        /// <summary>
        /// ������� ������������� ������ � ���������������� ��
        /// </summary>
        public bool m_bSensorsStrings {
            get {
                bool bRes = false;
                if ((string.IsNullOrEmpty (m_SensorsString_SOTIASSO) == false)
                    && (Equals(m_SensorsStrings_ASKUE, null) == false)) {
                    bRes = (string.IsNullOrEmpty (m_SensorsString_VZLET) == false)
                        || Type == TEC_TYPE.BIYSK
                        || m_id > (int)TECComponentBase.ID.LK;
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
        public string GetSensorsString (int indx, CONN_SETT_TYPE connSettType, HDateTime.INTERVAL indxTime = HDateTime.INTERVAL.UNKNOWN) {
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
                        Logging.Logg().Error($"TEC::GetSensorsString (CONN_SETT_TYPE={connSettType.ToString ()}; HDateTime.INTERVAL={indxTime.ToString()})"
                            , Logging.INDEX_MESSAGE.NOT_SET);
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
                                        + @"; HDateTime.INTERVAL=" + indxTime.ToString() + @")", Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                }
            }

            return strRes;
        }

        private volatile int m_IdSOTIASSOLinkSourceTM;
        /// <summary>
        /// ������� ��� ������� �������� �������������� ��������� ������ ��� ��������
        /// </summary>
        public event EventHandler EventUpdate;
        /// <summary>
        /// ���������� ������� ���������� ���������� ������� - ���������� ����������
        /// </summary>
        public void PerformUpdate (int iListenerId)
        {
            //���������� �� ������� ������� ������������� ��������� ������ � ������� ��������
            EventUpdate?.Invoke(this, new DbTSQLConfigDatabase.TECListUpdateEventArgs () { m_iListenerId = iListenerId });
        }

        public enum ADDING_PARAM_KEY : short { PREFIX_MODES_TERMINAL
            , PREFIX_VZLETDATA
            , PATH_RDG_EXCEL
            , COLUMN_TSN_EXCEL
            //, TEMPLATE_NAME_SGN_DATA_TM
            //, TEMPLATE_NAME_SGN_DATA_FACT
        }

        public TEC (TEC src)
            : this (src.m_id, src.name_shr, src.name_MC, src.m_strNameTableAdminValues, src.m_strNameTableUsedPPBRvsPBR, false)
        {
            setNamesField (@"DATE",
                @"REC",
                @"IS_PER",
                "DIVIAT",
                "DATE_TIME",
                "PBR",
                "PBR_NUMBER");

            m_dictAddingParam = new Dictionary<ADDING_PARAM_KEY, PARAM_ADDING> ();
            foreach (KeyValuePair<ADDING_PARAM_KEY, PARAM_ADDING> pair in src.m_dictAddingParam)
                m_dictAddingParam.Add (pair.Key, new PARAM_ADDING (pair.Value));
        }

        public TEC(DataRow rTec, bool bUseData)
            : this(Convert.ToInt32(rTec["ID"])
                , rTec["NAME_SHR"].ToString().Trim() //"NAME_SHR"
                , rTec["NAME_MC"].ToString().Trim() //"NAME_MC"
                , @"AdminValuesOfID"
                , @"PPBRvsPBROfID"
                , bUseData)
        {
            setNamesField(@"DATE",
                @"REC",
                @"IS_PER",
                "DIVIAT",
                "DATE_TIME",
                "PBR",
                "PBR_NUMBER");

            m_dictAddingParam = new Dictionary<ADDING_PARAM_KEY, PARAM_ADDING>();
            foreach (ADDING_PARAM_KEY key in Enum.GetValues(typeof(ADDING_PARAM_KEY)))
                m_dictAddingParam.Add(key, new PARAM_ADDING() {
                    m_type = typeof(string)
                    , m_value = !(rTec[key.ToString()] is DBNull)
                        ? rTec [key.ToString ()].ToString ().Trim ()
                            : string.Empty
                });

            //string strTest = GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString();
        }
        /// <summary>
        /// ���������� ������� (� �����������)
        /// </summary>
        /// <param name="id">������������ ���</param>
        /// <param name="name_shr">������� ������������</param>
        /// <param name="name_MC">������������-������������� � �����-�����</param>
        /// <param name="table_name_admin">������������ ������� � ����������������� ����������</param>
        /// <param name="table_name_pbr">������������ ������� �� ���������� ���</param>
        /// <param name="bUseData">������� �������� �������</param>
        private TEC (int id, string name_shr, string name_MC , string table_name_admin, string table_name_pbr, bool bUseData) {
            int iNameMC = -1;

            list_TECComponents = new List<TECComponent>();
            //m_list_Vyvod = new List<TECComponent>();

            this.m_id = id;
            this.name_shr = name_shr;
            this.name_MC = name_MC;
            if ((this.m_id < (int)TECComponent.ID.LK)
                && (int.TryParse(this.name_MC, out iNameMC) == false))
                Logging.Logg().Warning(string.Format(@"�������� �������������� ������������� [{0}] � �����-����� �� �����������...", this.name_shr), Logging.INDEX_MESSAGE.NOT_SET);
            else
                ;

            this.m_strNameTableAdminValues = table_name_admin;
            this.m_strNameTableUsedPPBRvsPBR = table_name_pbr;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_strNamesField = new List<string>((int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD);
            for (int i = 0; i < (int)INDEX_NAME_FIELD.COUNT_INDEX_NAME_FIELD; i++) m_strNamesField.Add(string.Empty);

            EventUpdate += new EventHandler(StatisticCommon.DbTSQLConfigDatabase.DbConfig().OnTECUpdate);
        }

        public void AddTECComponent(DataRow r)
        {
            list_TECComponents.Add(new TECComponent(this, r));
        }

        //public void AddVyvod (DataRow []rows_param)
        //{
        //    m_list_Vyvod.Add(new Vyvod(this, rows_param));
        //}
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
        private void setNamesField(string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat
            , string pbr_datetime, string ppbr_vs_pbr, string pbr_number)
        {
            //INDEX_NAME_FIELD.ADMIN_DATETIME
            m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] = admin_datetime;
            m_strNamesField[(int)INDEX_NAME_FIELD.REC] = admin_rec; //INDEX_NAME_FIELD.REC
            m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] = admin_is_per; //INDEX_NAME_FIELD.IS_PER
            m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT] = admin_diviat; //INDEX_NAME_FIELD.DIVIAT

            m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] = pbr_datetime; //INDEX_NAME_FIELD.PBR_DATETIME
            m_strNamesField[(int)INDEX_NAME_FIELD.PBR] = ppbr_vs_pbr; //INDEX_NAME_FIELD.PBR

            m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER] = pbr_number; //INDEX_NAME_FIELD.PBR_NUMBER
        }

        private struct PARAM_ADDING
        {
            public Type m_type;

            public object m_value;

            public PARAM_ADDING (Type type, object value)
            {
                m_type = type;

                m_value = value;
            }

            public PARAM_ADDING (PARAM_ADDING values)
                : this (values.m_type, values.m_value)
            {
            }
        }

        private Dictionary<ADDING_PARAM_KEY, PARAM_ADDING> m_dictAddingParam;

        public object GetAddingParameter(ADDING_PARAM_KEY key)
        {
            return (m_dictAddingParam.ContainsKey(key) == true) ? m_dictAddingParam[key].m_value : string.Empty;
        }

        public void InitTG(int indx, DataRow[] rows_tg)
        {
            int j = -1 // ������ ����� ������� �������� ���������
                , k = -1; // ������ ���������� ��

            for (j = 0; j < rows_tg.Length; j++)
            {
                // ����� ��
                for (k = 0; k < list_TECComponents.Count; k++)
                    if (((list_TECComponents[k].IsTG == true))
                        && (Int32.Parse(rows_tg[j][@"ID_TG"].ToString()) == list_TECComponents[k].m_id))
                        break;
                    else
                        ;
                // ��������� ������ �� ��
                if (k < list_TECComponents.Count)
                {// �� ������
                    list_TECComponents[indx].m_listLowPointDev.Add(list_TECComponents[k].m_listLowPointDev[0]);
                    if (list_TECComponents[indx].IsGTP == true)
                        (list_TECComponents[k].m_listLowPointDev[0] as TG).m_id_owner_gtp = list_TECComponents[indx].m_id;
                    else
                        if (list_TECComponents[indx].IsPC == true)
                            (list_TECComponents[k].m_listLowPointDev[0] as TG).m_id_owner_pc = list_TECComponents[indx].m_id;
                        else
                            ;
                }
                else
                    ; // �� �� ������
            }
        }
        /// <summary>
        /// ������������� ���� ���������� ��� ���� �������
        /// </summary>
        /// <param name="indx">������ ����������-������, ������� ���������������� �����������</param>
        /// <param name="rows_param">������ ����� �� ���������� ������� ���������</param>
        public void InitParamVyvod(int indx, DataRow[] rows_param)
        {
            TECComponent pv = null; // ��������� - �������� ������
            int j = -1;
            bool bNewParamVyvod = true;

            if (indx < 0)
                indx = list_TECComponents.Count - 1;
            else
                ;

            for (j = 0; j < rows_param.Length; j++)
            {
                pv = list_TECComponents.Find(comp => { return comp.m_id == Convert.ToInt32(rows_param[j][@"ID"]); });
                bNewParamVyvod = pv == null;
                // ��������� ������ �� ��������������
                if (bNewParamVyvod == true)
                    pv = new TECComponent(this, rows_param[j]);
                else
                    ; // ������ ��� �������� ��� ��������

                list_TECComponents[indx].m_listLowPointDev.Add(pv.m_listLowPointDev[0]);
                if ((bNewParamVyvod == true)
                    && (list_TECComponents[indx].IsVyvod == true))
                    (pv.m_listLowPointDev[0] as Vyvod.ParamVyvod).m_owner_vyvod = list_TECComponents[indx].m_id;
                else
                    ;
            }
        }
        /// <summary>
        /// �������� ������������� �� � ��� ��������� ������-������������ (����������� - �������) � ���������������� ��
        /// </summary>
        /// <param name="prevSensors">������-������������ (����������� - �������)</param>
        /// <param name="sensor">�������������</param>
        ///// <param name="typeTEC">��� ��� (������� ��� + ���������)</param>
        /// <param name="typeSourceData">��� ��������� ������</param>
        /// <returns>������-������������ � ����������� ���������������</returns>
        private static string addSensor(string prevSensors, object sensor/*, TEC.TEC_TYPE typeTEC*/, TEC.INDEX_TYPE_SOURCE_DATA typeSourceData)
        {
            string strRes = prevSensors;
            //������� ������������� ������������ ������� ��� ��������� ���������������
            string strQuote =
                //sensor.GetType().IsPrimitive == true ? string.Empty : @"'"
                // ChrjapinAN 26.12.2017 ������� "OBJECT/ITEM"
                sensor.GetType ().Equals(typeof(string)) == true ? @"'" : string.Empty
                ;
            //��������� ������� ��� ����������� ���������������
            if (prevSensors.Equals(string.Empty) == false)
                //��� ����������� - ���������� ����� ��������� ��������������� ����������� (�������, �.�. TSQL)
                switch (typeSourceData)
                {
                    case TEC.INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                        //����� �������� ��� ���� ���
                        // ChrjapinAN 26.12.2017 ������� "OBJECT/ITEM"
                        strRes += sensor.GetType ().Equals (typeof (string)) == true ? @", " : @" OR ";
                        break;
                    default:
                        break;
                }
            else
                ; //������ �� ���������
            //�������� �������������
            switch (typeSourceData)
            {
                case TEC.INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
        public TG FindTGById(object id, TG.INDEX_VALUE indxVal, HDateTime.INTERVAL id_time_type)
        {
            TG tgRes = null;
            int i = -1;
            
            for (i = 0; i < list_TECComponents.Count; i++) {
                if (list_TECComponents [i].IsTG == true) {
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if ((list_TECComponents[i].m_listLowPointDev[0] as TG).m_arIds_fact[(int)id_time_type] == (TG.AISKUE_KEY)id)
                                tgRes = list_TECComponents[i].m_listLowPointDev[0] as TG;
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if ((list_TECComponents[i].m_listLowPointDev[0] as TG).m_strKKS_NAME_TM.Equals((string)id) == true)
                                tgRes = list_TECComponents[i].m_listLowPointDev[0] as TG;
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

            return tgRes;
        }
        /// <summary>
        /// ���������������� ��� ������-������������ � ���������������� ���
        /// </summary>
        public void InitSensorsTEC () {
            int i = -1
                , j = -1;
            TEC_TYPE type = Type;

            if (_listParamVyvod == null)
                _listParamVyvod = new List<Vyvod.ParamVyvod>();
            else
                _listParamVyvod.Clear();

            if (_listTG == null)
                _listTG = new List<TG> ();
            else
                _listTG.Clear ();

            if (m_SensorsStrings_ASKUE == null)
            // �� ����������� '(int)HDateTime.INTERVAL.COUNT_ID_TIME'
                m_SensorsStrings_ASKUE = new string [] { string.Empty, string.Empty };
            else {
            // �������� - �������
                m_SensorsStrings_ASKUE [(int)HDateTime.INTERVAL.HOURS] =
                m_SensorsStrings_ASKUE [(int)HDateTime.INTERVAL.MINUTES] =
                    string.Empty;
            }
            // �������� - ��������
            m_SensorsString_SOTIASSO = string.Empty;
            // �������� - �����
            m_SensorsString_VZLET = string.Empty;
            //���� �� ���� ����������� ���
            for (i = 0; i < list_TECComponents.Count; i++)
                // � ~ �� ���� ������������
                switch (list_TECComponents[i].Type) {
                    case TECComponentBase.TYPE.TEPLO:
                        //if (list_TECComponents[i].IsParamVyvod == true)
                        //{
                        //}
                        //else
                        //{
                            list_TECComponents[i].m_SensorsString_VZLET = string.Empty;

                            //foreach (Vyvod.ParamVyvod pv in v.m_listParam)
                            foreach (Vyvod.ParamVyvod pv in list_TECComponents[i].m_listLowPointDev)
                            {
                                m_SensorsString_VZLET = addSensor(m_SensorsString_VZLET
                                    , pv.m_SensorsString_VZLET //.name_future
                                    , INDEX_TYPE_SOURCE_DATA.EQU_MAIN);

                                list_TECComponents[i].m_SensorsString_VZLET = addSensor(list_TECComponents[i].m_SensorsString_VZLET
                                    , pv.m_SensorsString_VZLET //.name_future
                                    , INDEX_TYPE_SOURCE_DATA.EQU_MAIN);
                            }
                        //}
                        break;
                    case TECComponentBase.TYPE.ELECTRO:
                        //��������� ��� ����������
                        if (list_TECComponents[i].IsTG == true)
                        {
                            //������ ��� ��
                            _listTG.Add(list_TECComponents[i].m_listLowPointDev[0] as TG);
                            //����������� ������-������������ � ����������������� ��� ��� � ����� (���� ��� - ���)
                            m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS] = addSensor(m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS]
                                                                            , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.HOURS]
                                /*, type*/
                                                                            , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            //����������� ������-������������ � ����������������� ��� ��� � ����� (���� ��� - ������)
                            m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES] = addSensor(m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES]
                                                                            , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES]
                                /*, type*/
                                                                            , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            //����������� ������-������������ � ����������������� ��� ��� � ����� (��������)
                            m_SensorsString_SOTIASSO = addSensor(m_SensorsString_SOTIASSO
                                                                , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_strKKS_NAME_TM
                                /*, type*/
                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            //������������ ��������� �������������� ��� ��  (���� ��� - ���)
                            list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS] = addSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS]
                                                                                                        , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.HOURS]
                                /*, type*/
                                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            //������������ ��������� �������������� ��� ��  (���� ��� - ������)
                            list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES] = addSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES]
                                                                                                        , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES]
                                /*, type*/
                                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            //������������ ��������� �������������� ��� ��  (��������)
                            list_TECComponents[i].m_SensorsString_SOTIASSO = addSensor(list_TECComponents[i].m_SensorsString_SOTIASSO
                                                                                        , (list_TECComponents[i].m_listLowPointDev[0] as TG).m_strKKS_NAME_TM
                                /*, type*/
                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                        }
                        else
                        {//��� ��������� (���, �(��)��) �����������
                            //���� �� �� ����������
                            for (j = 0; j < list_TECComponents[i].m_listLowPointDev.Count; j++)
                            {
                                //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���� ��� - ���)
                                list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS] = addSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.HOURS]
                                                                                                                , (list_TECComponents[i].m_listLowPointDev[j] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.HOURS]
                                    /*, type*/
                                                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                                //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���� ��� - ������)
                                list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES] = addSensor(list_TECComponents[i].m_SensorsStrings_ASKUE[(int)HDateTime.INTERVAL.MINUTES]
                                                                                                                , (list_TECComponents[i].m_listLowPointDev[j] as TG).m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES]
                                    /*, type*/
                                                                                                                , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                                //����������� ������-������������ � ����������������� ��� ���������� ��� � ����� (���������)
                                list_TECComponents[i].m_SensorsString_SOTIASSO = addSensor(list_TECComponents[i].m_SensorsString_SOTIASSO
                                                                                        , (list_TECComponents[i].m_listLowPointDev[j] as TG).m_strKKS_NAME_TM
                                    /*, type*/
                                                                                        , m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE]);
                            } // - ���� �� �� ����������
                        }
                        break;
                    default:
                        break;
                }
            // - ���� �� ���� ����������� ���
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
            string strLog = string.Empty;

            if (source.Rows.Count > 0)
            {
                connSetts[type] = new ConnectionSettings(source.Rows[0], -1);

                if (source.Rows.Count == 1)
                {
                    if ((!(type < (int)CONN_SETT_TYPE.DATA_AISKUE)) && (!((int)type > (int)CONN_SETT_TYPE.DATA_SOTIASSO)))
                        if (FormMainBase.s_iMainSourceData == connSetts[(int)type].id)
                            // 
                            m_arTypeSourceData[type - (int)CONN_SETT_TYPE.DATA_AISKUE] = TEC.INDEX_TYPE_SOURCE_DATA.EQU_MAIN;
                        else                            
                            iRes = 1; //??? throw new Exception(@"TEC::connSettings () - ����������� ��� ��������� ������...")
                    else
                        ;

                    m_arInterfaceType[(int)type] = DbTSQLInterface.getTypeDB(connSetts[(int)type].port);
                }
                else
                {
                    iRes = -1;
                    connSetts[type] = null;
                }
            }
            else
            {// ������ ����� ������ ���, �.�. 'Count' �� �.�. < 0
                iRes = -2;
            }

            if (iRes < 0)
                m_arInterfaceType[(int)type] = DbInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
            else
                ;

            return iRes;
        }
        /// <summary>
        /// ���������� ������-������������ � ���������������� ��� ��� � ����� ��� �� �����������
        /// </summary>
        /// <param name="num_comp">����� (������) ���������� (��� ��� = -1)</param>
        /// <returns>������-������������ � ����������������</returns>
        private string idComponentValueQuery (int num_comp, TECComponentBase.TYPE type) {
            string strRes = string.Empty;

            if (num_comp < 0) {
                switch (type) {
                    case TECComponentBase.TYPE.TEPLO:
                        //m_list_Vyvod.ForEach(v => { strRes += @", " + (v as Vyvod).m_listParam[0].m_id.ToString(); });
                        list_TECComponents.ForEach(v => {
                            if ((v.IsParamVyvod == true)
                                && ((v.m_listLowPointDev[0] as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV))
                                strRes += @", " + v.m_listLowPointDev[0].m_id.ToString();
                            else ;
                        });
                        break;
                    case TECComponentBase.TYPE.ELECTRO:
                        list_TECComponents.ForEach(g => { if ((g.IsGTP == true) || (g.IsGTP_LK == true)) strRes += @", " + (g.m_id).ToString(); else ; });
                        break;
                    default:
                        break;
                }
                // �������� ������ ������� � ��������
                strRes = strRes.Substring(2);
            }
            else {
                switch (type) {
                    case TECComponentBase.TYPE.TEPLO:
                        //??? ���� ��-������������ ������ �� ��������� - ������ ��� ��� � �����
                        break;
                    case TECComponentBase.TYPE.ELECTRO:
                        if ((list_TECComponents[num_comp].IsGTP == true)
                            || (list_TECComponents[num_comp].IsPC == true)
                            )
                            strRes += (list_TECComponents[num_comp].m_id).ToString();
                        else {
                            list_TECComponents[num_comp].m_listLowPointDev.ForEach(tc => { strRes += @", " + (tc.m_id).ToString(); });
                            // �������� ������ ������� � ��������
                            strRes = strRes.Substring(2);
                        }
                        break;
                    default:
                        break;
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
        private string pbrValueQuery(string selectPBR, DateTime dt)
        {//??? �������� � �������� ������ ����/�����. MS SQL: 'yyyyMMdd HH:mm:ss', MySql: 'yyyy-MM-dd HH:mm:ss'
            string strRes = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " +
                        //m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR" +
                        //@", " + selectPBR.Split (';')[0] + " AS PBR";
                        @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + "]." + "DATE_TIME" + " AS DATE_PBR" +
                        //@", " + "PBR" + " AS PBR";
                        @", " + selectPBR.Split(';')[0];

                    //if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        //strRes += @", " + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];
                    strRes += @", " + @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + "]." + @"PBR_NUMBER";
                    //else
                    //    ;

                    //������ ������� ��� ��� ���
                    strRes += @", " + "ID_COMPONENT";

                    strRes += @" " + @"FROM " +
                        @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + @"]" + 
                        //@" WHERE " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        //@" AND " + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" WHERE " + @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + "]." + "DATE_TIME" + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" AND " + @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + "]." + "DATE_TIME" + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +

                        @" AND ID_COMPONENT IN (" + selectPBR.Split (';')[1] + ")" +

                        //@" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0";
                        //@" AND MINUTE(" + m_arNameTableUsedPPBRvsPBR[(int)mode] + "." + "DATE_TIME" + ") = 0";
                        @" AND DATEPART(n," + @"[" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + "]." + "DATE_TIME" + ") = 0";
                    /*
                    if (selectPBR.Split(';')[1].Split (',').Length > 1)
                        strRes += @" GROUP BY DATE_PBR";
                    else
                        ;
                    */
                    strRes += @" ORDER BY DATE_PBR" +
                        @" ASC";
            //        break;
            //    default:
            //        break;
            //}

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
        private string minsFactCommonRequest (DateTime dt, string sen, TecView.ID_AISKUE_PARNUMBER idParNumber) {
            // ��� 30-�� ��� ����. �������� 1 ��� �����, ����� �������������� �������� ��� ��������
            // ��� ���� 30-�� ��� ����. �� ������� ��� �� ������ ����
            int offsetHoursParNumber = idParNumber == TecView.ID_AISKUE_PARNUMBER.FACT_03 ? 0 : 0;

            return $"SELECT * FROM [dbo].[ft_get_value_askue]({m_id},{(int)idParNumber}," +
                $"'{dt.AddHours(-offsetHoursParNumber).ToString("yyyyMMdd HH:00:00")}'" + @"," +
                $"'{dt.AddHours(1 - offsetHoursParNumber).ToString("yyyyMMdd HH:00:00")}'" +
                //$") WHERE IN ({sen})" +
                // ChrjapinAN 26.12.2017 ������� �� "OBJECT/ITEM"
                $") WHERE {sen}" +
                @" ORDER BY DATA_DATE";
        }
        /// <summary>
        /// ���������� ���������� ������� � ��������� ������ ��� ��������� 3-� ��� �������� � ���� ���
        /// </summary>
        /// <param name="usingDate">���� - ��������� ��� ���������, ������������� ������</param>
        /// <param name="hour">��� � ������, ������������� ������</param>
        /// <param name="sensors">������-������������ ���������������</param>
        /// <param name="idParNumber">������������� ���� �������� (3-�, 30-�� ���)</param>
        /// <returns>������ �������</returns>
        public string minsFactRequest(DateTime usingDate, int hour, string sensors, TecView.ID_AISKUE_PARNUMBER idParNumber)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            usingDate = usingDate./*Date.*/AddHours(hour);
            string request = string.Empty;

            switch (Type)
            {
                case TEC.TEC_TYPE.COMMON:
                    switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                            request = minsFactCommonRequest (usingDate, sensors, idParNumber);
                            break;
                        default:
                            break;
                    }
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    switch (m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                            request = minsFactCommonRequest(usingDate, sensors, idParNumber);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            //Debug.WriteLine(@"TEC::minsFactRequest () = " + request);

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
                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
        
        public string minTMDetailRequest(DateTime usingDate, int h, int m, string sensors, int interval)
        {
            int hour = -1, min = -1;

            if (h == 24)
                hour = 23;
            else
                hour = h;

            if (m == 0) min = 1; else min = m;

            DateTime dtReq = usingDate.Date.AddHours(hour).AddMinutes(interval * (min - 1));
            string request = string.Empty;

            switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_AISKUE])
            {
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                    request = @"SELECT [KKS_NAME], [Value], [tmdelta], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                    + @" FROM [dbo].[ALL_PARAM_SOTIASSO_KKS]"
                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
                                        //--�������� ����/����� � UTC (��������� �� �������� � UTC)
                                        + @" AND [last_changed_at] BETWEEN DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMinutes(-1 * interval).ToString(@"yyyyMMdd HH:mm:00.000") + @"')"
                                        + @" AND DATEADD (HH, DATEDIFF (HH, GETDATE (), GETUTCDATE()), '" + dtReq.AddMilliseconds(-2).ToString(@"yyyyMMdd HH:mm:ss.fff") + @"')"
                                        ;
                    break;
                default:
                    break;
            }

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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
                        @"SELECT [KKS_NAME] as [KKS_NAME], AVG ([VALUE]) AS [VALUE], SUM ([VALUE] / (60 / " + interval + @")) as [VALUE0], SUM ([tmdelta]) as [tmdelta]"
                            + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
	                        + @", (DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) / " + interval + @") as [MINUTE]"
                        + @" FROM ("
                            + @"SELECT [KKS_NAME] as [KKS_NAME], [Value] as [VALUE], [tmdelta] as [tmdelta]"
		                        + @", DATEADD (MINUTE, - DATEPART (MINUTE, DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) % " + interval + @", DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at])) as [last_changed_at]"
	                        + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                            + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                                      + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                                                    + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
            return $"SELECT * FROM [dbo].[ft_get_value_askue]({ m_id},{12},'{dt.ToString("yyyyMMdd HH:mm:00")}','{dt.AddDays(1).ToString("yyyyMMdd HH:mm:00")}')"
                //+ $" WHERE [ID] IN ({sen})"
                // ChrjapinAN 26.12.2017 ������� �� "OBJECT/ITEM"
                + $" WHERE {sen}"
                + @" ORDER BY DATA_DATE";
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
                        case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                            request = hoursFactCommonRequest(usingDate, sensors);
                            break;
                        default:
                            break;
                    }
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    switch (m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_AISKUE - (int)CONN_SETT_TYPE.DATA_AISKUE])
                    {
                        case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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

            //Debug.WriteLine(@"TEC::hoursFactRequest () = " + request);

            return request;
        }

        private string queryIdSOTIASSOLinkSource
        {
            get { return
                @"("
                //+ @"SELECT [ID_LINK_SOURCE_DATA_TM] FROM [techsite_cfg-2.X.X].[dbo].[TEC_LIST]"+ @" WHERE [ID]=" 
                + @"SELECT [ID] FROM [v_CURR_ID_LINK_SOURCE_DATA_TM]" + @" WHERE [ID_TEC]=" 
                    + m_id + @")"
                ;
            }
        }

        private string hoursTMCommonRequestAverage (DateTime dt1, DateTime dt2, string sensors, int interval) {
            return
                @"SELECT SUM([VALUE]) as [VALUE], SUM([VALUE0]) as [VALUE0], COUNT (*) as [CNT], [HOUR]"
                + @" FROM ("
                    + @"SELECT" 
		                + @" [KKS_NAME] as [KKS_NAME], AVG ([VALUE]) AS [VALUE], SUM ([VALUE] / (60 / " + interval + @")) as [VALUE0], SUM ([tmdelta]) as [tmdelta]"
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
                                + @" WHERE [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
                                    + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
                                        + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                                                + @" WHERE  [ID_TEC] = " + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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

            //Console.WriteLine(string.Format(@"TEC::hoursTMRequest (usingDate={1}, sensors={2}, interval={3}) - variant SOTIASSO={4} {0}REQUEST={5}"
            //    , Environment.NewLine, usingDate, sensors, interval, TEC.s_SourceSOTIASSO.ToString()
            //    , request));

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
        public string GetPBRValueQuery(int num_comp, DateTime dt, TECComponentBase.TYPE type)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectPBR = "PBR, Pmin, Pmax" + /*�� ������������ m_strNamesField[(int)INDEX_NAME_FIELD.PBR]*/";" + idComponentValueQuery(num_comp, type);
            //        break;
            //    default:
            //        break;
            //}

            strRes = pbrValueQuery(selectPBR, dt/*, mode*/);

            return strRes;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� �������� ���
        /// </summary>
        /// <param name="num_comp">����� ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>
        /// <returns>������ �������</returns>
        public string GetPBRValueQuery(TECComponent comp, DateTime dt)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectPBR = "PBR, Pmin, Pmax"; //??? �� ������������ m_strNamesField[(int)INDEX_NAME_FIELD.PBR];

                    selectPBR += ";";

                    selectPBR += comp.m_id.ToString ();
            //        break;
            //    default:
            //        break;
            //}

            strRes = pbrValueQuery(selectPBR, dt);

            return strRes;
        }

        private string adminValueQuery(string selectAdmin, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/)
        {//??? �������� � �������� ������ ����/�����. MS SQL: 'yyyyMMdd HH:mm:ss', MySql: 'yyyy-MM-dd HH:mm:ss'
            string strRes = string.Empty;
            
            //switch (mode) {
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        strRes = @"SELECT " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
            //            //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
            //                    selectAdmin + //" AS ADMIN_VALUES" +
            //            //@", " + selectPBR +
            //            //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +
            //                    @" " + @"FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +

            //                    @" " + @"WHERE " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
            //                    @" " + @"AND " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'" +

            //                    @" " + @"UNION " +
            //                    @"SELECT " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +

            //                    //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
            //                    selectAdmin +
            //            //@", " + selectPBR +
            //            //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +

            //                    @" " + @"FROM " + m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +

            //                    //" RIGHT JOIN " + strUsedPPBRvsPBR +
            //            //" ON " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " = " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " " +

            //                    @" " + @"WHERE " +

            //                    //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") +
            //            //@"' AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
            //            //@"' AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +

            //                    //@" AND " +
            //                    m_arNameTableAdminValues[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL" +

            //                    @" " + @"ORDER BY DATE_ADMIN" +
            //            //@", DATE_PBR" +
            //                    @" " + @"ASC";
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " +
                        //m_arNameTableAdminValues[(int)mode] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        //selectAdmin.Split (';') [0] +
                        m_strNameTableAdminValues/*[(int)mode]*/ + "." + "DATE" + " AS DATE_ADMIN" +
                        ", " + selectAdmin.Split(';')[0] +

                        @" " + @"FROM " + m_strNameTableAdminValues/*[(int)mode]*/ +

                        @" " + @"WHERE" +
                        @" " + @"ID_COMPONENT IN (" + selectAdmin.Split(';')[1] + ")" +

                        @" " + @"AND " +
                        //m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        m_strNameTableAdminValues/*[(int)mode]*/ + "." + "DATE" + " >= '" + dt.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                        @" " + @"AND " +
                        //m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'";
                        m_strNameTableAdminValues/*[(int)mode]*/ + "." + "DATE" + " <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") + @"'";
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
            //        break;
            //    default:
            //        break;
            //}

            return strRes;
        }
        /// <summary>
        /// ���������� ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="comp">������ ���������� ��� ��� �������� ������������� ������</param>
        /// <param name="dt">����/����� - ������ ���������, ������������� ������</param>
        /// <param name="mode">����� ����� � ������� (� ����./����� �� ��������� - ������������ 'AdminTS.TYPE_FIELDS.DYNAMIC')</param>        
        /// <returns></returns>
        public string GetAdminValueQuery(TECComponent comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string strRes = string.Empty,
                    selectAdmin = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
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
            //        break;
            //    default:
            //        break;
            //}

            strRes = adminValueQuery(selectAdmin, dt/*, mode*/);

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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                    query = @"SELECT AVG ([SUM_P_SN]) as VALUE, DATEPART(hour,[LAST_UPDATE]) as HOUR"
                            + @" FROM [dbo].[P_SUMM_TSN_KKS]"
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                    //����� �������� ��� ���� ���
                    query = @"SELECT [KKS_NAME] as KKS_NAME, [last_changed_at], [Current_Value_SOTIASSO] as value, [ID_SOURCE] " +
                            @"FROM [dbo].[v_ALL_VALUE_SOTIASSO_KKS] " +
                            @"WHERE [ID_TEC]=" + m_id + @" " +
                            @"AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                case INDEX_TYPE_SOURCE_DATA.EQU_MAIN:
                    //dt -= HDateTime.GetUTCOffsetOfMoscowTimeZone();

                    if (TEC.s_SourceSOTIASSO == SOURCE_SOTIASSO.AVERAGE)
                        //������� �3.a (�� ����������� �������)
                        query = @"SELECT [KKS_NAME], [Value], DATEADD (HH, DATEDIFF (HH, GETUTCDATE (), GETDATE()), [last_changed_at]) as [last_changed_at]"
                                + @" FROM [dbo].[ALL_PARAM_SOTIASSO_0_KKS]"
                                + @" WHERE [ID_TEC]=" + m_id + @" AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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
                                        + @" WHERE  [ID_TEC] = " + m_id + " AND [KKS_NAME] IN (" + sensors + @")" + @" AND ID_SOURCE=" + queryIdSOTIASSOLinkSource //m_IdSOTIASSOLinkSourceTM
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

        public string GetAdminValueQuery(int num_comp, DateTime dt, TECComponentBase.TYPE type)
        {
            string strRes = string.Empty,
                selectAdmin = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    selectAdmin = idComponentValueQuery (num_comp, type);

                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC]
                                + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER]
                                + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT]
                                + ", " + @"[ID_COMPONENT]"
                                + ", " + @"[SEASON]"
                                + ", " + @"[FC]"
                                + ";"
                                + selectAdmin;
            //        break;
            //    default:
            //        break;
            //}

            strRes = adminValueQuery(selectAdmin, dt/*, mode*/);

            return strRes;
        }

        public enum TYPE_DBVZLET : short { UNKNOWN = -1, GRAFA = 0, KKS_NAME }
        public static TYPE_DBVZLET TypeDbVzlet { get { return TYPE_DBVZLET.KKS_NAME; } }

        private string getVzletSensorsParamVyvod ()
        {
            string strRes = string.Empty;

            Vyvod.ParamVyvod pv;

            // ����������� ������� � �����������
            foreach (TECComponent tc in list_TECComponents)
                if (tc.IsParamVyvod == true)
                {
                    pv = tc.m_listLowPointDev[0] as Vyvod.ParamVyvod;

                    if (((pv.m_id_param == Vyvod.ID_PARAM.G_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T_PV)
                            || (pv.m_id_param == Vyvod.ID_PARAM.G2_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T2_PV))
                        && (pv.m_SensorsString_VZLET.Equals(string.Empty) == false))
                        strRes += @"(" + pv.m_id + @",'" + pv.m_SensorsString_VZLET + @"'),";
                    else
                        ;
                }
                else
                    ;
            // ������ ������ �������
            strRes = strRes.Substring(0, strRes.Length - 1);

            return strRes;
        }

        public string GetHoursVzletTDirectQuery(DateTime dt)
        {
            string strRes = string.Empty;

            // �������� ����� ������� ������� ��������� ������ � ������� ������� �������� � �����������
            TimeSpan  tsOffset = HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
            DateTime dtReq = dt.Date.Add(tsOffset); //�������� �������� ��� - ���, �.�. � �� ����� ������� ���

            Vyvod.ParamVyvod pv;
            string strParamVyvod = string.Empty
                , strSummaGpr = string.Empty
                , NL = string.Empty //Environment.NewLine
                ;

            switch (TypeDbVzlet)
            {
                case TYPE_DBVZLET.KKS_NAME:
                    strRes = @"DECLARE @getdate AS DATETIME2;" + NL;
                    strRes += @"SELECT @getdate = CAST('" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"' AS DATETIME2(7));" + NL;

                    strRes += @"DECLARE @SETTINGS_TABLE AS TABLE ([ID_POINT_ASKUTE] [int] NOT NULL, [KKS_NAME] [nvarchar](256) NOT NULL);" + NL;

                    //���� ������� ��������� ����������� ID_POINT_ASKUTE � KKS-�����
                    strRes += @"INSERT INTO @SETTINGS_TABLE ([ID_POINT_ASKUTE],[KKS_NAME])"
                        + @" SELECT [ID_POINT_ASKUTE],[KKS_NAME] FROM (VALUES " //+ NL
                        ;
                    // ����������� ������� � �����������
                    strParamVyvod = getVzletSensorsParamVyvod();
                    strRes += strParamVyvod;
                    strRes += @") AS [SETTINGS]([ID_POINT_ASKUTE],[KKS_NAME]);" + NL;

                    strRes += @"SELECT [GROUP_DATA].[ID_TEC], [GROUP_DATA].[KKS_NAME], [SET].[ID_POINT_ASKUTE], [GROUP_DATA].[VALUE], DATEADD(HOUR, -" + tsOffset.Hours + @", [GROUP_DATA].[DATETIME]) AS [DATETIME]"
                        + @" FROM ("
                            + @"SELECT [ID_TEC], [KKS_NAME], AVG([VALUE]) AS [VALUE],"
                                + @" DATEADD(minute, (DATEDIFF(minute, @getdate, [DATETIME]) / 60) * 60, @getdate) AS [DATETIME]"
                            + @" FROM ("
                                + @"SELECT [ARCH].[ID_TEC], [ARCH].[KKS_NAME], [ARCH].[VALUE], [ARCH].[DATETIME]"
                                    + @" FROM [VZLET_CURRENT_ARCHIVES_MIN_" + GetAddingParameter(ADDING_PARAM_KEY.PREFIX_VZLETDATA).ToString() + @"] AS [ARCH] WITH(INDEX(KKS_DATETIME), READUNCOMMITTED)"
                                        + @" INNER JOIN @SETTINGS_TABLE AS [SET] ON ([ARCH].[KKS_NAME] = [SET].[KKS_NAME])"
                                    + @" WHERE [ARCH].[DATETIME] BETWEEN @getdate AND DATEADD(ms, -3, DATEADD(dd,1,@getdate))"
                                + @") AS [DATA]"
                        + @" GROUP BY [ID_TEC], [KKS_NAME], DATEADD(minute, (DATEDIFF(minute, @getdate, [DATETIME]) / 60) * 60, @getdate)"
                            + @") AS [GROUP_DATA] INNER JOIN @SETTINGS_TABLE AS [SET] ON ([GROUP_DATA].[KKS_NAME] = [SET].[KKS_NAME])"
                        + @" ORDER BY [GROUP_DATA].[DATETIME];" + NL;
                    //strRes += @"GO";
                    break;
                case TYPE_DBVZLET.GRAFA:
                default:
                    // ����������� ������� � �����������
                    foreach (TECComponent tc in list_TECComponents)
                        if (tc.IsParamVyvod == true)
                        {
                            pv = tc.m_listLowPointDev[0] as Vyvod.ParamVyvod;

                            if (((pv.m_id_param == Vyvod.ID_PARAM.G_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T_PV)
                                    || (pv.m_id_param == Vyvod.ID_PARAM.G2_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T2_PV))
                                && (pv.m_SensorsString_VZLET.Equals(string.Empty) == false))
                            {
                                strParamVyvod += ", AVG ([" + pv.m_SensorsString_VZLET + @"]) as [" + pv.m_Symbol + @"pv_" + pv.m_id + @"]";

                                if ((pv.m_id_param == Vyvod.ID_PARAM.G_PV)
                                    || (pv.m_id_param == Vyvod.ID_PARAM.G2_PV))
                                    strSummaGpr += @"[" + pv.m_SensorsString_VZLET + @"]+";
                                else
                                    ;
                            }
                            else
                                ;
                        }
                        else
                            ;

                    // ������� ������ "+"
                    strSummaGpr = strSummaGpr.Substring(0, strSummaGpr.Length - 1);

                    strRes += @"SELECT DATEPART(HH, [����]) - " + tsOffset.Hours + @" as [iHOUR]" // ������� ����������� �������� ��� - ���
                        // ������� � �����������
                        + strParamVyvod
                        // ��������� �������� ��������
                        + ", AVG (" + strSummaGpr + @")"
                    + " FROM [teplo1]"
                    + " WHERE [����] > '" + dtReq.ToString(@"yyyyMMdd HH:00:00") + @"'"
                        + " AND [����] < '" + dtReq.AddDays(1).ToString(@"yyyyMMdd HH:00:00") + @"'"
                    + " GROUP BY DATEPART(DD, [����]), DATEPART(HH, [����])"
                    + " ORDER BY DATEPART(DD, [����]), DATEPART(HH, [����])";
                    break;
            }

            return strRes;
        }

        public string GetCurrentVzletTDirectQuery()
        {
            string strRes = string.Empty;

            Vyvod.ParamVyvod pv;
            string strParamVyvod = string.Empty
                , strSummaGpr = string.Empty
                , NL = string.Empty //Environment.NewLine
                ;

            switch (TypeDbVzlet)
            {
                case TYPE_DBVZLET.KKS_NAME:
                    strRes = @"DECLARE @SETTINGS_TABLE AS TABLE ([ID_POINT_ASKUTE] [int] NOT NULL, [KKS_NAME] [nvarchar](256) NOT NULL);" + NL;
//----����������� �������� ������� �� ������� ���� ������� ��������������� ������������ ������� ������������ ��������� ����� �������
//--DECLARE @OFFSET_TIME AS INT;
//--SELECT @OFFSET_TIME = (SELECT CAST([VALUE] AS INT) FROM [VZLETDATAARCHIVES].[dbo].[VZLETDATAARCHIVES_SETTINGS] WHERE [ID] = 'TIMEZONE_OFFSET_NSK') 
//--                    - (DATEPART(tz, SYSDATETIMEOFFSET())/60);

                    //���� ������� ��������� ����������� ID_POINT_ASKUTE � KKS-�����
                    strRes += @"INSERT INTO @SETTINGS_TABLE ([ID_POINT_ASKUTE],[KKS_NAME])"
                        + @" SELECT [ID_POINT_ASKUTE],[KKS_NAME] FROM (VALUES " //+ NL
                        ;
                    // ����������� ������� � �����������                    
                    strParamVyvod = getVzletSensorsParamVyvod ();
                    strRes += strParamVyvod;
                    strRes += @") AS [SETTINGS]([ID_POINT_ASKUTE],[KKS_NAME]);" + NL;

                    strRes += @"SELECT [SET].[ID_POINT_ASKUTE], [V].[ID_TEC], [V].[KKS_NAME], [V].[VALUE]"
                            + @", V.[DATETIME]" //DATEADD(hh, @OFFSET_TIME, [V].[DATETIME]) AS [DATETIME] 
                            //+ @", [V].[LIVE_PARAM]"
                        + @" FROM [v_CURRENT_VALUES] AS [V]"
                        + @" INNER JOIN @SETTINGS_TABLE AS [SET] ON ([V].[KKS_NAME] = [SET].[KKS_NAME]) ORDER BY [SET].[ID_POINT_ASKUTE] ASC;" + NL;
                    //strRes += @"GO";
                    break;
                case TYPE_DBVZLET.GRAFA:
                default:
                    strRes = @"SELECT TOP 1 [����]";
                    // ����������� ������� � �����������
                    foreach (TECComponent tc in list_TECComponents)
                        if (tc.IsParamVyvod == true)
                        {
                            pv = tc.m_listLowPointDev[0] as Vyvod.ParamVyvod;

                            if (((pv.m_id_param == Vyvod.ID_PARAM.G_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T_PV)
                                || (pv.m_id_param == Vyvod.ID_PARAM.G2_PV) || (pv.m_id_param == Vyvod.ID_PARAM.T2_PV))
                                && (pv.m_SensorsString_VZLET.Equals(string.Empty) == false))
                            {
                                strParamVyvod += ", [" + pv.m_SensorsString_VZLET + @"] as [" + pv.m_Symbol + @"_" + pv.m_id + @"]";

                                if ((pv.m_id_param == Vyvod.ID_PARAM.G_PV)
                                    || (pv.m_id_param == Vyvod.ID_PARAM.G2_PV))
                                    strSummaGpr += @"[" + pv.m_SensorsString_VZLET + @"]+";
                                else
                                    ;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    // ������� ������ "+"
                    strSummaGpr = strSummaGpr.Substring(0, strSummaGpr.Length - 1);
                    strRes += strParamVyvod + @"," + strSummaGpr;

                    strRes += @" FROM [teplo1]"
                        + @" ORDER BY [����] DESC";
                    break;
            }

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
        public string GetAdminDatesQuery(TECComponent comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string strRes = string.Empty;

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT DATE, ID, SEASON FROM " + m_strNameTableAdminValues/*[(int)mode]*/ + " WHERE" +
                            @" ID_COMPONENT = " + comp.m_id +
                          @" AND DATE > '" + dt/*.AddHours(-1 * m_timezone_offset_msc)*/.ToString("yyyyMMdd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
            //        break;
            //    default:
            //        break;
            //}

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
        public string GetPBRDatesQuery(TECComponent comp, DateTime dt/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string strRes = string.Empty,
                strNameFieldDateTime = m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME];

            //switch (mode)
            //{
            //    case AdminTS.TYPE_FIELDS.STATIC:
            //        ;
            //        break;
            //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                    strRes = @"SELECT " + @"[DATE_TIME]" + @", [ID], [PBR_NUMBER] FROM [" + m_strNameTableUsedPPBRvsPBR/*[(int)mode]*/ + @"]" +
                            @" WHERE" +
                            @" ID_COMPONENT = " + comp.m_id + "" +
                            @" AND " + @"DATE_TIME" + @" > '" + dt/*.AddHours(-1 * m_timezone_offset_msc)*/.ToString("yyyyMMdd HH:mm:ss") +
                            @"' AND " + @"DATE_TIME" + @" <= '" + dt.AddDays(1).ToString("yyyyMMdd HH:mm:ss") +
                            @"' ORDER BY " + @"DATE_TIME" + @" ASC";
            //        break;
            //    default:
            //        break;
            //}

            return strRes;
        }
    }
}
