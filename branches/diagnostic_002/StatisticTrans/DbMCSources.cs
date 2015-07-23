using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using HClassLibrary;
using StatisticTrans;

namespace StatisticCommon
{
    public class DbMCSources : DbSources
    {
        private const int MC_ID = -666;
        
        protected DbMCSources () : base () {
        }

        public static new DbMCSources Sources()
        {
            if (m_this == null)
                m_this = new DbMCSources();
            else
                ;

            return (DbMCSources) m_this;
        }

        public override int Register(object connSett, bool active, string desc, bool bReq = false)
        {
            int id = -1,
                err = -1;

            if (connSett is ConnectionSettings == true)
                return base.Register (connSett, active, desc, bReq);
            else
                if (connSett is string == true) {
                    if (m_dictDbInterfaces.ContainsKey (MC_ID) == true) id = m_dictDbInterfaces[MC_ID].ListenerRegister(); else ;
                }
                else
                    ;

            if (id < 0)
            {
                string dbNameType = string.Empty;
                DbTSQLInterface.DB_TSQL_INTERFACE_TYPE dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.ModesCentre;
                dbNameType = dbType.ToString();

                m_dictDbInterfaces.Add(MC_ID, new DbMCInterface ((string)connSett));
                
                if (active == true) m_dictDbInterfaces[MC_ID].Start(); else ;
                m_dictDbInterfaces[MC_ID].SetConnectionSettings(connSett, active);

                id = m_dictDbInterfaces[MC_ID].ListenerRegister();
            }
            else
                ;

            return registerListener(MC_ID, id, active, out err);
        }
    }
}