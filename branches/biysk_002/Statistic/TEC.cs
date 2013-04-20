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
        public enum TEC_TYPE { COMMON, BIYSK };

        public string name;
        public List<GTP> GTP;

        public TEC_TYPE type() { if (name.IndexOf("Бийск") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings connSett;

        private bool is_connection_error;
        private bool is_data_error;

        //Для особенной ТЭЦ (Бийск)
        //private object lockValue;

        //private Thread dbThread;
        private Semaphore sema;
        //private volatile bool workTread;
        
        private int used;

        private DbInterface dbInterface;
        private int listenerId;

        public volatile DbDataInterface dataInterface;
        public volatile DbDataInterface dataInterfaceAdmin;

        public TEC (string name) {
            GTP = new List<GTP>();

            this.name = name;

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void Request(string request)
        {
            dbInterface.Request(listenerId, request);
        }

        public void Request(string request, int gtp)
        {
            if (gtp < 0)
            {
                lock (dataInterface.lockData)
                {
                    dataInterface.request[1] = request;
                    dataInterface.dataPresent = false;
                    dataInterface.dataError = false;
                }
            }
            else
            {
                lock (GTP[gtp].dataInterface.lockData)
                {
                    GTP[gtp].dataInterface.request[1] = request;
                    GTP[gtp].dataInterface.dataPresent = false;
                    GTP[gtp].dataInterface.dataError = false;
                }
            }
            try
            {
                sema.Release(1);
            }
            catch
            {
            }
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
            else
                ;
        }
    }
}
