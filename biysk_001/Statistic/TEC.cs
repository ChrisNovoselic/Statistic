using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

namespace Statistic
{
    public class TEC
    {
        public string name;
        public List<GTP> GTP;

        public ConnectionSettings connSett;

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface dbInterface;
        private int listenerId;

        public TEC () {
            GTP = new List<GTP>();

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void Request(string request)
        {
            dbInterface.Request(listenerId, request);
        }

        public bool GetResponse(out bool error, out DataTable table)
        {
            return dbInterface.GetResponse(listenerId, out error, out table);
        }

        public void StartDbInterface()
        {
            if (used == 0)
            {
                dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL, 1);
                listenerId = dbInterface.ListenerRegister();
                dbInterface.Start();
                dbInterface.SetConnectionSettings(connSett);
            }
            used++;
            if (used > GTP.Count)
                used = GTP.Count;
        }

        public void StopDbInterface()
        {
            used--;
            if (used == 0)
            {
                dbInterface.Stop();
                dbInterface.ListenerUnregister(listenerId);
            }
            if (used < 0)
                used = 0;
        }

        public void StopDbInterfaceForce()
        {
            if (used > 0)
            {
                dbInterface.Stop();
                dbInterface.ListenerUnregister(listenerId);
            }
        }

        public void RefreshConnectionSettings()
        {
            if (used > 0)
            {
                dbInterface.SetConnectionSettings(connSett);
            }
        }
    }
}
