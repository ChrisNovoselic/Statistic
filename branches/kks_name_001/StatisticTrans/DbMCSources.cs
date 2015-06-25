﻿using System;
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
    /// <summary>
    /// Класс для описания объекта управления установленными соединенями
    /// </summary>
    public class DbMCSources : DbSources
    {
        /// <summary>
        /// Уникальный идентификатор Модес-Центра
        /// </summary>
        private const int MC_ID = -666;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        protected DbMCSources () : base () {
        }
        /// <summary>
        /// Функция доступа к объекту управления установленными соединенями
        /// </summary>
        /// <returns>Объект управления установленными соединенями</returns>
        public static new DbMCSources Sources()
        {
            if (m_this == null)
                m_this = new DbMCSources();
            else
                ;

            return (DbMCSources) m_this;
        }
        /// <summary>
        /// Регистриует клиента соединения, активным или нет, при необходимости принудительно отдельный экземпляр
        /// </summary>
        /// <param name="connSett">параметры соединения</param>
        /// <param name="active">признак активности</param>
        /// <param name="bReq">признак принудительного создания отдельного экземпляра</param>
        /// <returns></returns>
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