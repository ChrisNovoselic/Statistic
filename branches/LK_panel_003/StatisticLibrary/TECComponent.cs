using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace StatisticCommon
{
    /// <summary>
    /// ����� ��� �������� ��������� ���������� ���
    /// </summary>
    public class TECComponentBase
    {
        /// <summary>
        /// �������������� ��� ����� ���������� ���
        /// </summary>
        public enum ID : int { GTP = 100, PC = 500, TG = 1000, MAX = 10000 }
        /// <summary>
        /// ������� ������������ ����������
        /// </summary>
        public string name_shr;
        /// <summary>
        /// ����������� ��� ������������� � ������� (��� ����������)
        /// </summary>
        public string name_future;
        /// <summary>
        /// ������������� ���������� (�� �� ������������)
        /// </summary>
        public int m_id;
        /// <summary>
        /// ������ ������� � ����� Excel �� ���������� ��� ��� ���������� (� ��������� ����� ������ ��� ����-5)
        /// </summary>
        public int m_indx_col_rdg_excel;
        /// <summary>
        /// ����������� ��� ������ ��������� ��������� ������������
        /// </summary>
        public decimal m_dcKoeffAlarmPcur;

        //����� �� 'class PanelStatisticView : PanelStatistic' - �� 'PanelStatisticView' ������ ��������� ��������???
        public volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������
        public volatile string m_SensorsString_SOTIASSO = string.Empty;        
        /// <summary>
        /// ���������� - �������� (��� ����������)
        /// </summary>
        public TECComponentBase()
        {
            m_dcKoeffAlarmPcur = -1;
        }
        /// <summary>
        /// ������� �������������� ���������� � ������ ���
        /// </summary>
        public bool IsGTP { get { return (m_id > (int)ID.GTP) && (m_id < (int)ID.PC); } }
        /// <summary>
        /// ���������� ��� (�����) ���������� �� ���������� ��������������
        /// </summary>
        /// <param name="id">������������� ����������</param>
        /// <returns>��� (�����) ����������</returns>
        public static FormChangeMode.MODE_TECCOMPONENT Mode(int id)
        {
            return (id < (int)ID.GTP) == true ? FormChangeMode.MODE_TECCOMPONENT.TEC :
                ((id > (int)ID.GTP) && (id < (int)ID.PC)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > (int)ID.PC) && (id < (int)ID.TG)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                    ((id > (int)ID.TG) && (id < (int)ID.MAX)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                        FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;
        }
        /// <summary>
        /// ������� �������������� ���������� � ������ ���� ����������
        ///  (�������, ���������)
        /// </summary>
        public bool IsPC { get { return (m_id > (int)ID.PC) && (m_id < (int)ID.TG); } }
        /// <summary>
        /// ������� �������������� ���������� � ������ ��
        /// </summary>
        public bool IsTG { get { return (m_id > (int)ID.TG) && (m_id < (int)ID.MAX); } }
    }
    /// <summary>
    /// ����� ��� �������� ���������� ��� - ��
    /// </summary>
    public class TG : TECComponentBase
    {
        /// <summary>
        /// ������������ - ������� ��������� ���������� ��� ����������� �������� ��
        /// </summary>
        public enum INDEX_VALUE : int { FACT //����.
                                        , TM //������������
                                        , LABEL_DESC //�������� (������� ������������) ��
                                        , COUNT_INDEX_VALUE }; //���������� ��������
        /// <summary>
        /// ������������ - ��������� ��������� ��
        /// </summary>
        public enum INDEX_TURNOnOff : int { OFF = -1, UNKNOWN, ON };
        /// <summary>
        /// ������ ��������������� �� � ���� ��� (����������� �� 'ID_TIME')
        ///  ��� ��������� ��� (�����) ����������� 3-� � 30-�� ��� ��������������
        ///  ��� ��������� - ���������
        /// </summary>
        public int[] m_arIds_fact;
        /// <summary>
        /// ��������� ������������� � ��������
        /// </summary>
        public string m_strKKS_NAME_TM;
        /// <summary>
        /// �������������� "����������" ��� �� (���, �(��)��)
        /// </summary>
        public int m_id_owner_gtp,
                    m_id_owner_pc;
        /// <summary>
        /// ������� ��������� ��
        /// </summary>
        public INDEX_TURNOnOff m_TurnOnOff; //��������� -1 - ����., 0 - ����������, 1 - ���. (������ ��� AdminAlarm)
        /// <summary>
        /// ����������� - �������� (��� ����������)
        /// </summary>
        public TG()
        {
            m_arIds_fact = new int[(int)HDateTime.INTERVAL.COUNT_ID_TIME];

            m_id_owner_gtp =
            m_id_owner_pc =
                //����������� ��������
                -1;
            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //����������� ���������
        }


        public void InitTG(TG dest, DataRow row_tg, DataTable allParamTG, out int err)
        {
            err = -1;
            
            dest.name_shr = row_tg["NAME_SHR"].ToString();
            dest.m_id = Convert.ToInt32(row_tg["ID"]);
            dest.m_id_owner_gtp = Convert.ToInt32(row_tg["ID_GTP"]);

            //DataRow[] rows_tg = allParamTG.Select(@"ID_TG=" + dest.m_id);
            //dest.m_strKKS_NAME_TM = rows_tg[0][@"KKS_NAME"].ToString();
            //dest.m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_3"].ToString());
            //dest.m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_30"].ToString());
        }

    }
    /// <summary>
    /// ����� ��� �������� ���������� ��� (���, �(��)��)
    /// </summary>
    public class TECComponent : TECComponentBase
    {
        /// <summary>
        /// ������ ��������������� � �����-�����
        /// </summary>
        public List<int> m_listMCentreId;
        /// <summary>
        /// ������ ��������������� � �����-���������
        /// </summary>
        public List<int> m_listMTermId;
        /// <summary>
        /// ������ ��
        /// </summary>
        public List<TG> m_listTG;
        /// <summary>
        /// ������ ��� - "��������" ����������
        /// </summary>
        public TEC tec;
        /// <summary>
        /// ����������� - �������� (��� ����������)
        /// </summary>
        public TECComponent(TEC tec)
        {
            this.tec = tec;

            m_listTG = new List<TG>();
            m_listMCentreId =
            m_listMTermId = null;
        }
    }
}
