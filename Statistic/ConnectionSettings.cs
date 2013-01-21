using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;

namespace Statistic
{
    public class ConnectionSettings
    {
        public volatile string server;
        public volatile string dbName;
        public volatile string userName;
        public volatile string password;
        public volatile int port;
        public volatile bool ignore;

        public enum ConnectionSettingsError
        { 
            NoError,
            WrongIp,
            WrongPort,
            WrongDbName,
            IllegalSymbolDbName,
            IllegalSymbolUserName,
            IllegalSymbolPassword,
        }

        public ConnectionSettings()
        {
            SetDefault();
        }

        public void SetDefault()
        {
            server = dbName = userName = password = "";
            port = 1433;
            ignore = true;
        }

        public ConnectionSettingsError Validate()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(server, out ip))
            {
                //MessageBox.Show("Неправильный ip-адрес.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.WrongIp;
            }

            if (port > 65535)
            {
                //MessageBox.Show("Порт должен лежать в пределах [0:65535].", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.WrongPort;
            }

            if (dbName == "")
            {
                //MessageBox.Show("Не задано имя базы данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.WrongDbName;
            }

            if (dbName.IndexOf('\'') >= 0 ||
                dbName.IndexOf('\"') >= 0 ||
                dbName.IndexOf('\\') >= 0 ||
                dbName.IndexOf('/') >= 0 ||
                dbName.IndexOf('?') >= 0 ||
                dbName.IndexOf('<') >= 0 ||
                dbName.IndexOf('>') >= 0 ||
                dbName.IndexOf('*') >= 0 ||
                dbName.IndexOf('|') >= 0 ||
                dbName.IndexOf(':') >= 0 ||
                dbName.IndexOf(';') >= 0)
            {
                //MessageBox.Show("Недопустимый символ в имени базы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.IllegalSymbolDbName;
            }

            if (userName.IndexOf('\'') >= 0 ||
                userName.IndexOf('\"') >= 0 ||
                userName.IndexOf('\\') >= 0 ||
                userName.IndexOf('/') >= 0 ||
                userName.IndexOf('?') >= 0 ||
                userName.IndexOf('<') >= 0 ||
                userName.IndexOf('>') >= 0 ||
                userName.IndexOf('*') >= 0 ||
                userName.IndexOf('|') >= 0 ||
                userName.IndexOf(':') >= 0 ||
                userName.IndexOf(';') >= 0)
            {
                //MessageBox.Show("Недопустимый символ в имени пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.IllegalSymbolUserName;
            }

            if (password.IndexOf('\'') >= 0 ||
                password.IndexOf('\"') >= 0 ||
                password.IndexOf('\\') >= 0 ||
                password.IndexOf('/') >= 0 ||
                password.IndexOf('?') >= 0 ||
                password.IndexOf('<') >= 0 ||
                password.IndexOf('>') >= 0 ||
                password.IndexOf('*') >= 0 ||
                password.IndexOf('|') >= 0 ||
                password.IndexOf(':') >= 0 ||
                password.IndexOf(';') >= 0)
            {
                //MessageBox.Show("Недопустимый символ в пароле пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.IllegalSymbolPassword;
            }

            return ConnectionSettingsError.NoError;
        }

        public string GetConnectionStringMSSQL()
        {
            return @"Data Source=" + server +
                   @"," + port.ToString() +
                   @";Network Library=DBMSSOCN;Initial Catalog=" + dbName +
                   @";User Id=" + userName +
                   @";Password=" + password + @";";
        }

        public string GetConnectionStringMySQL()
        {
            return @"Server=" + server +
                   @";Port=" + port.ToString() +
                   @";Database=" + dbName +
                   @";User Id=" + userName +
                   @";Password=" + password + @";";
        }
    }
}
