using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;

using StatisticCommon;

using Modes;
using ModesApiExternal;

namespace trans_mc
{
    public class DbMCInterface : DbInterface
    {
        public DbMCInterface(string name)
            : base(name)
        {
        }

        public override void SetConnectionSettings(object mcHostName)
        {
            lock (lockConnectionSettings) // изменение настроек подключения и выставление флага для переподключения - атомарная операция
            {
                m_connectionSettings = mcHostName;

                needReconnect = true;
            }

            SetConnectionSettings();
        }


        protected override bool Connect()
        {
            if (!(((string)m_connectionSettings).Length > 0))
                return false;
            else
                ;

            bool result = false, bRes = false;

            try
            {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            }
            catch (Exception e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Исключение обращения к переменной", true, true, false);
                Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }

            bRes = ModesApiFactory.IsInitilized;

            if (bRes == false)
                return bRes;
            else
                ;

            lock (lockConnectionSettings)
            {
                if (needReconnect) // если перед приходом в данную точку повторно были изменены настройки, то подключения со старыми настройками не делаем
                    return false;
                else
                    ;
            }

            try
            {
                ModesApiFactory.Initialize((string)m_connectionSettings);

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Соединение с Modes-Centre (" + (string)m_connectionSettings + ")", true, true, false);
                Logging.Logg().LogUnlock();
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "Ошибка соединения с Modes-Centre (" + (string)m_connectionSettings + ")");
            }

            return result;
        }

        protected override bool Disconnect()
        {
            bool result = false, bRes = false;

            return result;
        }

        protected override bool GetData(DataTable table, object query)
        {
            bool result = false;

            return result;
        }
    }
}
