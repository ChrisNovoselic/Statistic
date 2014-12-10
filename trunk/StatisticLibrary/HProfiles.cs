using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;

namespace StatisticCommon
{
    class HStatisticProfiles : HProfiles
    {
        public HStatisticProfiles(int iListenerId, int id_role, int id_user)
            : base(iListenerId, id_role, id_user)
        {
        }
    }
}
