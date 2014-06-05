using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TG : TECComponentBase
    {
        public enum ID_TIME : int { MINUTES, HOURS, COUNT_ID_TIME };
        public enum INDEX_VALUE : int { FACT, TM, COUNT_INDEX_VALUE };

        public double[] power;
        public double power_TM;
        public double[] power_LastMinutesTM;
        public bool[] receivedMin;
        public bool[] receivedHourHalf1;
        public bool[] receivedHourHalf2;
        public bool receivedHourHalf1Addon;
        public bool receivedHourHalf2Addon;
        //public int id;
        public int [] ids_fact; //Для особенной ТЭЦ (Бийск)
        public int id_tm;
        public TECComponent m_owner;

        public TG(TECComponent comp)
        {
            this.m_owner = comp;
            power = new double[21];
            receivedMin = new bool[21];
            receivedHourHalf1 = new bool[24];
            receivedHourHalf2 = new bool[24];

            ids_fact = new int[(int) ID_TIME.COUNT_ID_TIME];
        }
    }
}
