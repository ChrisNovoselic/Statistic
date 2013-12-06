using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TECComponentBase
    {
        public string name;
        public int m_id;
        public List <int> m_listMCId;

        public TECComponentBase()
        {
            m_listMCId = null;
        }
    }
}
