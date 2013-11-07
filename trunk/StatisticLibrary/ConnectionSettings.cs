using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;

using MySql.Data.MySqlClient; //Для 'IsConnect'
using System.Data;

//namespace Statistic
namespace StatisticCommon
{
    public enum CONN_SETT_TYPE
    {
        DATA, ADMIN, PBR,
        COUNT_CONN_SETT_TYPE
    };

    public class ConnectionSettings
    {
        public volatile string server;
        public volatile string dbName;
        public volatile string userName;
        public volatile string password;
        public volatile int port;
        public volatile bool ignore;

        override public bool Equals(object obj) {
            if ((ConnectionSettings) obj == this)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator == (ConnectionSettings csLeft, ConnectionSettings csRight)
        {
            bool bRes = false;

            if ((csLeft.server == csRight.server) &&
                (csLeft.dbName == csRight.dbName) &&
                (csLeft.userName == csRight.userName) &&
                (csLeft.password == csRight.password) &&
                (csLeft.port == csRight.port))
                bRes = true;
            else
                ;

            return bRes;
        }

        public static bool operator != (ConnectionSettings csLeft, ConnectionSettings csRight) {
            bool bRes = false;

            if (! (csLeft == csRight))
                bRes = true;
            else
                ;

            return bRes;
        }

        public enum ConnectionSettingsError
        { 
            NoError,
            WrongIp,
            WrongPort,
            WrongDbName,
            IllegalSymbolDbName,
            IllegalSymbolUserName,
            IllegalSymbolPassword,
            NotConnect
        }

        public ConnectionSettings()
        {
            SetDefault();
        }

        public void SetDefault()
        {
            server = dbName = userName = password = "";
            port = 1433;
            //ignore = true;
            ignore = false;
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

            //if (DbInterface.Request(this, "SELECT * FROM TEC_LIST").Rows.Count > 0)
            //    return ConnectionSettingsError.NotConnect;
            //else
            //    ;

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

        public static string GetConnectionStringExcel(string path)
        {
            string var1 = @"Provider=Microsoft.ACE.OLEDB.12.0;" +
                        //@"Persist Security Info=false;" +
                        //@"Extended Properties=Excel 12.0 Xml;HDR=YES;" +
                        @"Extended Properties=Excel 12.0 Xml;" +
                        @"Data Source=" + path + ";",
                    var2 = @"Provider=Microsoft.Jet.OLEDB.4.0;"  +
                    //@"Extended Properties=Excel 8.0;HDR=YES;Mode=Read;ReadOnly=true;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;IMEX=1;Mode=Read;ReadOnly=true;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;IMEX=1;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;" +
                    @"Extended Properties=Excel 8.0;" +
                    //@"Persist Security Info=false;" +
                    @"Data Source=" + path + ";";

            return var1;
        }
    }
}
