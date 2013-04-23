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
        public string field;
        public List<GTP> GTP;

        public TEC_TYPE type() { if (name.IndexOf("Бийск") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;
        public int listenerAdmin;
        public int m_indxDbInterface;

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface dbInterface;
        private int listenerId;

        public TEC (string name, string field) {
            GTP = new List<GTP>();

            this.name = name;
            this.field = field;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            listenerAdmin = -1;
            m_indxDbInterface = -1;

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
                dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL);
                listenerId = dbInterface.ListenerRegister();
                dbInterface.Start();
                dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            used++;
            if (used > GTP.Count)
                used = GTP.Count;
            else
                ;
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
                dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            else
                ;
        }

        public int connSettings (DataTable source, int type) {
            int iRes = 0;

            connSetts[type] = new ConnectionSettings();
            connSetts[type].server = source.Rows[0]["IP"].ToString();
            connSetts[type].port = Int32.Parse(source.Rows[0]["PORT"].ToString());
            connSetts[type].dbName = source.Rows[0]["DB_NAME"].ToString();
            connSetts[type].userName = source.Rows[0]["UID"].ToString();
            connSetts[type].password = source.Rows[0]["PASSWORD"].ToString();
            connSetts[type].ignore = Boolean.Parse(source.Rows[0]["IGNORE"].ToString()); //== "1";

            return iRes;
        }
    }
}
