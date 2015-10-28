using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    /// <summary>
    /// ����� ��� �������� ��������� ���������� ���
    /// </summary>
    public class TECComponentBase
    {
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
        public bool IsGTP { get { return (m_id > 100) && (m_id < 500); } }
        /// <summary>
        /// ���������� ��� (�����) ���������� �� ���������� ��������������
        /// </summary>
        /// <param name="id">������������� ����������</param>
        /// <returns>��� (�����) ����������</returns>
        public static FormChangeMode.MODE_TECCOMPONENT Mode(int id)
        {
            return ((id > 100) && (id < 500)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > 500) && (id < 1000)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                    ((id > 1000) && (id < 10000)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                        FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;
        }
        /// <summary>
        /// ������� �������������� ���������� � ������ ���� ����������
        ///  (�������, ���������)
        /// </summary>
        public bool IsPC { get { return (m_id > 500) && (m_id < 1000); } }
        /// <summary>
        /// ������� �������������� ���������� � ������ ��
        /// </summary>
        public bool IsTG { get { return (m_id > 1000) && (m_id < 10000); } }
    }
    /// <summary>
    /// ����� ��� �������� ���������� ��� - ��
    /// </summary>
    public class TG : TECComponentBase
    {
        /// <summary>
        /// ������������ - �������������� �������� �������
        /// </summary>
        public enum ID_TIME : int { UNKNOWN = -1, MINUTES, HOURS, COUNT_ID_TIME };
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
            m_arIds_fact = new int[(int)ID_TIME.COUNT_ID_TIME];

            m_id_owner_gtp =
            m_id_owner_pc =
                //����������� ��������
                -1;
            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //����������� ���������
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
