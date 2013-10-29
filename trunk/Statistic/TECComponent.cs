using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TECComponent
    {
        public string name;
        public string prefix_admin, prefix_pbr;
        public int m_id;
        public List<TG> TG;
        public TEC tec;

        public TECComponent(TEC tec, string prefix_admin, string prefix_pbr)
        {
            this.tec = tec;
            TG = new List<TG>();

            this.prefix_admin = prefix_admin;
            this.prefix_pbr = prefix_pbr;
        }
    }
}
