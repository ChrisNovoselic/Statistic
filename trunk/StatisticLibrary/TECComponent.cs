using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TECComponentBase
    {
        public string name_shr;
        public string name_future;
        public int m_id;
        public int m_indx_col_rdg_excel;
        public decimal m_dcKoeffAlarmPcur;

        //����� �� 'class PanelStatisticView : PanelStatistic' - �� 'PanelStatisticView' ������ ��������� ��������???
        public volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������
        public volatile string m_SensorsString_SOTIASSO = string.Empty;        

        public TECComponentBase()
        {
            m_dcKoeffAlarmPcur = -1;
        }
    }

    public class TG : TECComponentBase
    {
        public enum ID_TIME : int { UNKNOWN = -1, MINUTES, HOURS, COUNT_ID_TIME };
        public enum INDEX_VALUE : int { FACT, TM, LABEL_DESC, COUNT_INDEX_VALUE };
        public enum INDEX_TURNOnOff : int { OFF, UNKNOWN, ON };

        public double[] power; //��� ���./��������
        public double power_TM; //��� ���./��������
        public double[] power_LastMinutesTM;
        //public bool[] receivedMin;
        //public bool[,] receivedHourHalf;
        //public bool receivedHourHalf1Addon;
        //public bool receivedHourHalf2Addon;
        //public int id;
        public int[] ids_fact; //��� ��������� ��� (�����)
        public int id_tm;
        //public TECComponent m_owner;
        public int m_id_owner_gtp,
                    m_id_owner_pc;
        public INDEX_TURNOnOff m_TurnOnOff; //��������� -1 - ����., 0 - ����������, 1 - ���.

        //public TG(TECComponent comp)
        public TG()
        {
            //this.m_owner = comp;
            power = new double[21];
            power_LastMinutesTM = new double[25];
            //receivedMin = new bool[21];
            //receivedHourHalf = new bool[2, 24];

            ids_fact = new int[(int)ID_TIME.COUNT_ID_TIME];

            m_id_owner_gtp =
            m_id_owner_pc = -1; //����������� ��������
            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //����������� ���������
        }
    }

    public class TECComponent : TECComponentBase
    {
        public string prefix_admin, prefix_pbr;
        public List<int> m_listMCentreId;
        public List<int> m_listMTermId;
        public List<TG> m_listTG;
        public TEC tec;

        public TECComponent(TEC tec, string prefix_admin, string prefix_pbr)
        {
            this.tec = tec;
            m_listTG = new List<TG>();
            m_listMCentreId =
            m_listMTermId = null;

            this.prefix_admin = prefix_admin;
            this.prefix_pbr = prefix_pbr;
        }
    }
}
