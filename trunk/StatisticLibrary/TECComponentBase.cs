using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    public class TECComponentBase
    {
        public string name;
        public int m_id,
                    m_MCId;

        public TECComponentBase()
        {
            m_MCId = -1;
        }
    }
}
