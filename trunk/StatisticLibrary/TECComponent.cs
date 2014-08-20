using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TECComponent : TECComponentBase
    {
        public string prefix_admin, prefix_pbr;
        public List<int> m_listMCentreId;
        public List<int> m_listMTermId;
        public List<TG> TG;
        public TEC tec;

        public TECComponent(TEC tec, string prefix_admin, string prefix_pbr)
        {
            this.tec = tec;
            TG = new List<TG>();
            m_listMCentreId =
            m_listMTermId = null;

            this.prefix_admin = prefix_admin;
            this.prefix_pbr = prefix_pbr;
        }
    }
}
