using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace StatisticCommon
{
    public class DbSources
    {
        private static DbSources m_this;
        Dictionary <int, DbTSQLInterface> m_dictDbTSQLInterfaces;
        
        protected class DbSourceListener
        {
            public volatile DbConnection dbConn;
            public volatile int idDbInterface;
            public volatile int iListenerId;

            public DbSourceListener(int id, int indx, DbConnection conn)
            {
                idDbInterface = id;
                iListenerId = indx;
                dbConn = conn;
            }
        }
        protected Dictionary <int, DbSourceListener> m_dictListeners;
        
        private DbSources()
        {
            m_dictDbTSQLInterfaces = new Dictionary <int, DbTSQLInterface> ();
            m_dictListeners = new Dictionary<int,DbSourceListener> ();
        }

        public static DbSources Sources () {
            if (m_this == null)
                m_this = new DbSources ();
            else
                ;

            return m_this;
        }

        /// <summary>
        /// Регистриует клиента соединения, активным или нет, при необходимости принудительно отдельный экземпляр
        /// </summary>
        /// <param name="connSett">параметры соединения</param>
        /// <param name="active">признак активности</param>
        /// <param name="bReq">признак принудительного создания отдельного экземпляра</param>
        /// <returns></returns>
        public int Register(ConnectionSettings connSett, bool active, bool bReq = false)
        {
            int iRes = -1,
                id = -1,
                err = -1;

            if ((m_dictDbTSQLInterfaces.ContainsKey (connSett.id) == true) && (bReq == true))
            {
                id = m_dictDbTSQLInterfaces[connSett.id].ListenerRegister ();
            }
            else 
                ;

            if (iRes < 0)
            {
                string dbNameType = string.Empty;
                DbTSQLInterface.DB_TSQL_INTERFACE_TYPE dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
                switch (connSett.port) {
                    case 1433:
                        dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MSSQL;
                        dbNameType = @"MS SQL";
                        break;
                    case 3306:
                        dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL;
                        dbNameType = @"MySql";
                        break;
                    default:
                        break;
                }
                m_dictDbTSQLInterfaces.Add (connSett.id, new DbTSQLInterface (dbType, @"Интерфейс: " + dbNameType + @"-БД"));
                if (active == true) m_dictDbTSQLInterfaces[connSett.id].Start(); else ;
                m_dictDbTSQLInterfaces[connSett.id].SetConnectionSettings(connSett, active);

                id = m_dictDbTSQLInterfaces[connSett.id].ListenerRegister ();
            }
            else
                ;

            for (iRes = 0; iRes < m_dictListeners.Keys.Count; iRes ++)
            {
                if (m_dictListeners.ContainsKey (id) == false)
                {
                    registerListener(iRes, connSett.id, id, active, out err);
                    break;
                }
                else
                    ;
            }

            if (! (iRes < m_dictListeners.Keys.Count))
                registerListener(iRes, connSett.id, id, active, out err);
            else
                ;

            return iRes;
        }

        private void registerListener(int idReg, int id, int idListener, bool active, out int err)
        {
            err = -1;
            
            if (active == true) {
                m_dictListeners.Add(idReg, new DbSourceListener(id, idListener, null));
                err = 0;
            }
            else {
                m_dictListeners.Add(idReg, new DbSourceListener(id, idListener, DbTSQLInterface.getConnection((ConnectionSettings)m_dictDbTSQLInterfaces[id].m_connectionSettings, out err)));
            }
                
        }

        /// <summary>
        /// Отмена регистрации клиента соединения
        /// </summary>
        /// <param name="id"></param>
        public void UnRegister(int id)
        {
            int err = -1;
            
            if (m_dictListeners.ContainsKey (id) == true)
            {
                if (m_dictListeners[id].dbConn == null) {
                    m_dictDbTSQLInterfaces[m_dictListeners[id].idDbInterface].ListenerUnregister(m_dictListeners[id].iListenerId);
                    if (! (m_dictDbTSQLInterfaces[m_dictListeners[id].idDbInterface].ListenerCount > 0)) {
                        m_dictDbTSQLInterfaces [m_dictListeners [id].idDbInterface].Stop ();

                        m_dictDbTSQLInterfaces.Remove(m_dictListeners[id].idDbInterface);
                    }
                    else
                        ;
                }
                else {
                    DbTSQLInterface.closeConnection(m_dictListeners[id].dbConn, out err);
                }

                m_dictListeners.Remove(id);
            }
            else
                ;
        }

        /// <summary>
        /// Отправляет запрос к источнику БД с идентификатором
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <param name="query">запрос</param>
        public void Request (int id, string query) {
            if (m_dictListeners.ContainsKey (id) == true)
            {
                m_dictDbTSQLInterfaces[m_dictListeners[id].idDbInterface].Request(m_dictListeners[id].iListenerId, query);
            }
            else
                ;
        }

        /// <summary>
        /// Получает рез-т запроса  - таблицу, от источника с идентификатором, с признаком ошибки
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <param name="err">признак4 ошибки</param>
        /// <param name="tableRes">результирующая таблица</param>
        public void Response(int id, out bool err, out DataTable tableRes)
        {
            tableRes = null;
            err = true;

            if (m_dictListeners.ContainsKey (id) == true)
            {
                m_dictDbTSQLInterfaces[m_dictListeners[id].idDbInterface].Response(m_dictListeners[id].iListenerId, out err, out tableRes);
            }
            else
                ;
        }

        public DbConnection GetConnection (int id, out int err) {
            DbConnection res = null;
            err = -1;

            if ((m_dictListeners.ContainsKey (id) == true) && (! (m_dictListeners [id].dbConn == null)))
            {
                res = m_dictListeners[id].dbConn;
                err = 0;
            }
            else
                ;

            return res;
        }
    }
}