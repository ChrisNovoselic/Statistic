using System;
using System.Collections.Generic;
using System.Text;

namespace Statistic
{
    public class TG
    {
        public string name;
        public double[] power;
        public bool[] receivedMin;
        public bool[] receivedHourHalf1;
        public bool[] receivedHourHalf2;
        public bool receivedHourHalf1Addon;
        public bool receivedHourHalf2Addon;
        public int id;
        public GTP gtp;

        public TG(GTP gtp)
        {
            this.gtp = gtp;
            power = new double[21];
            receivedMin = new bool[21];
            receivedHourHalf1 = new bool[24];
            receivedHourHalf2 = new bool[24];
        }
    }
}
