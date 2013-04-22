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

        //Для особенной ТЭЦ (Бийск)
        //public DbDataInterface dataInterface;
        //public DbDataInterface dataInterfaceAdmin;

        public GTP(TEC tec)
        {
            this.tec = tec;
            TG = new List<TG>();
            //dataInterface = new DbDataInterface();
            //dataInterfaceAdmin = new DbDataInterface();
        }
    }
}
