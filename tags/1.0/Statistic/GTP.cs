using System;
using System.Collections.Generic;
using System.Text;

namespace Statistic
{
    public class GTP
    {
        public string name;
        public string field;
        public List<TG> TG;
        public TEC tec;

        public GTP(TEC tec)
        {
            this.tec = tec;
            TG = new List<TG>();
        }
    }
}
