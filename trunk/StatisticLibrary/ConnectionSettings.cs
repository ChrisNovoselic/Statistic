using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;

using MySql.Data.MySqlClient; //��� 'IsConnect'
using System.Data;

//namespace Statistic
namespace StatisticCommon
{
    public enum CONN_SETT_TYPE
    {
        CONFIG_DB = 0, LIST_SOURCE,
        ADMIN = 0, PBR = 1, DATA_ASKUE = 2 /*����. - �����*/, DATA_SOTIASSO = 3 /*������������ - ��������*/, MTERM = 4 /*�����-��������*/,
        COUNT_CONN_SETT_TYPE = 5
    };

    public class ConnectionSettings
    {
        public static int UN_ENUMERABLE_ID = -666666;

        public volatile int id;
        public volatile string name;

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

        public static bool operator ==(ConnectionSettings csLeft, ConnectionSettings csRight)
        {
            bool bRes = false;

            if ((object.ReferenceEquals(csRight, null) == false) &&
                object.ReferenceEquals(csLeft, null) == false)
                if ((csLeft.server == csRight.server) &&
                    (csLeft.dbName == csRight.dbName) &&
                    (csLeft.userName == csRight.userName) &&
                    (csLeft.password == csRight.password) &&
                    (csLeft.port == csRight.port))
                    bRes = true;
                else
                    ;
            else
                if (object.ReferenceEquals(csLeft, null) == true)
                    bRes = true;
                else
                    ;

            return bRes;
        }

        public static bool operator !=(ConnectionSettings csLeft, ConnectionSettings csRight)
        {
            bool bRes = false;

            if (!(csLeft == csRight))
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

        /// <summary>
        /// ����������� ������ ��� ���������� ���������� � �� �����������
        /// </summary>
        /// <param name="r"></param>
        public ConnectionSettings(DataRow r)
        {
            id = UN_ENUMERABLE_ID + 1; //Int32.Parse (r[@"ID"].ToString ());
            name = r[@"NAME_SHR"].ToString ();
            server = r[@"IP"].ToString ();
            dbName = r[@"DB_NAME"].ToString();
            userName = r[@"UID"].ToString();
            password = r[@"PASSWORD"].ToString();
            port = Int32.Parse (r[@"PORT"].ToString ());

            ignore = Int32.Parse(r[@"IGNORE"].ToString ()) == 1;
        }

        public void SetDefault()
        {
            id = -1;
            name =
            server = dbName = userName = password = string.Empty;
            port = 1433;
            //ignore = true;
            ignore = false;
        }

        public ConnectionSettingsError Validate()
        {
            try {
                IPAddress ip;
                ip = IPAddress.Parse(server);
                if (IPAddress.TryParse(server, out ip) == false)
                {
                    //MessageBox.Show("������������ ip-�����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return ConnectionSettingsError.WrongIp;
                }
            }
            catch (Exception e) {
                Logging.Logg().Exception(e, @"ConnectionSettings::Validate() - ...");
            }

            if (port > 65535)
            {
                //MessageBox.Show("���� ������ ������ � �������� [0:65535].", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.WrongPort;
            }

            if (dbName == "")
            {
                //MessageBox.Show("�� ������ ��� ���� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                //MessageBox.Show("������������ ������ � ����� ����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                //MessageBox.Show("������������ ������ � ����� ������������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                //MessageBox.Show("������������ ������ � ������ ������������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ConnectionSettingsError.IllegalSymbolPassword;
            }

            //if (DbTSQLInterface.Select(this, "SELECT * FROM TEC_LIST").Rows.Count > 0)
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
                        @"Extended Properties=Excel 12.0 Xml" + @";" +
                        @"Data Source=" + path + ";",
                    
                    var2 = @"Provider=Microsoft.Jet.OLEDB.4.0;"  +
                    //@"Extended Properties=Excel 8.0;HDR=YES;Mode=Read;ReadOnly=true;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;IMEX=1;Mode=Read;ReadOnly=true;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;IMEX=1;" +
                    //@"Extended Properties=Excel 8.0;HDR=YES;" +
                    @"Extended Properties=""Excel 8.0;ReadOnly=false;"";" +
                    //@"Persist Security Info=false;" +
                    @"Data Source=" + path + @";";
                    //@"Data Source=" + path + ";" + @"Jet OLEDB:Database Password=" + @"nss;";

            return var1;
        }

        public static string GetConnectionStringCSV(string path)
        {
            string var1 = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + @"';Extended Properties='text;HDR=Yes;FMT=Delimited'",
                    var2 = @"";

            return var1;
        }

        public static string GetConnectionStringDBF(string path)
        {
            string var1 = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                         path +
                        ";Extended Properties=dBase III;";
            ;

            return var1;
        }
    }
}
