using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Statistic
{
    public class DbDataInterface
    {
        /* поля для передачи информации между потоками */
        public volatile string[] request;
        public volatile bool dataPresent;
        public volatile bool dataError;
        public volatile DataTable tableData;
        public object lockData;

        public DbDataInterface()
        {
            tableData = new DataTable();
            request = new string[2];
            lockData = new object();
        }
    }
}
