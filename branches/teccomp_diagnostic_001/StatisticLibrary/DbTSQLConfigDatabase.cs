using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ASUTP.Database;
using System.Data;
using System.Data.Common;

namespace StatisticCommon
{
    public partial class DbTSQLConfigDatabase : DbTSQLInterface
    {
        protected static string s_Name = $"{CONN_SETT_TYPE.CONFIG_DB.ToString ()}";

        private static DbTSQLConfigDatabase _this;

        private ConnectionSettings _connSett;

        public static DbTSQLConfigDatabase DbConfig()
        {
            DbTSQLConfigDatabase dbInterfaceRes = null;

            if (Equals (_this, null) == false)
                dbInterfaceRes = _this;
            else
                throw new NullReferenceException("DbTSQLConfigDatabase::ctor () - не был вызван...");

            return dbInterfaceRes;
        }

        private ConnectionSettings ConnSett
        {
            get
            {
                ConnectionSettings connSettRes;

                if (Equals (_connSett, null) == false)
                    connSettRes = _connSett;
                else
                    throw new NullReferenceException ("DbTSQLConfigDatabase::ConnSett.get () - параметры соединения не были инициализированы...");

                return connSettRes;
            }
        }

        //protected /*static*/ DbConnection m_connConfigDB;

        private DbTSQLConfigDatabase ()
            : this (new ConnectionSettings())
        {
            throw new Exception (@"Нельзя создать объект для обмена данными с БД конфигурации без параметров соединения с ней...");
        }

        public DbTSQLConfigDatabase (ConnectionSettings connSett)
            : base(getTypeDB(connSett), s_Name)
        {
            SetConnectionSettings (connSett);

            _this = this;
        }

        public void SetConnectionSettings (ConnectionSettings connSett)
        {
            _connSett = connSett;
        }

        private int register ()
        {
            return DbSources.Sources ().Register (_connSett, false, s_Name);
        }

        private void unregister (int iListenerId)
        {
            DbSources.Sources ().UnRegister (iListenerId);
        }

        public void ExecNonQuery (string query, out int error)
        {
            error = -1;

            int iListenerId = -1;
            DbConnection dbConnection;

            if (Equals (ConnSett, null) == false) {
                iListenerId = register ();

                dbConnection = DbSources.Sources ().GetConnection (iListenerId, out error);

                if (error == 0)
                    ExecNonQuery (ref dbConnection, query, null, null, out error);
                else {
                    ;
                }

                unregister (iListenerId);
            } else
                throw new Exception ("Не установлены параметры для соединения с БД...");
        }

        public DataTable Select (string query, out int error)
        {
            error = -1;
            DataTable tableRes;

            int iListenerId = -1;
            DbConnection dbConnection;

            if (Equals (ConnSett, null) == false) {
                iListenerId = register ();

                dbConnection = DbSources.Sources ().GetConnection(iListenerId, out error);

                if (error == 0)
                    tableRes = Select (ref dbConnection, query, null, null, out error);
                else {
                    tableRes = new DataTable ();
                }

                unregister (iListenerId);

                return tableRes;
            } else
                throw new Exception ("Не установлены параметры для соединения с БД...");
        }

        //public static DataTable Select (string query, DbType[]types, object[]paramValues, out int error)
        //{
        //    return Select (_connSett, query, types, paramValues, out error);
        //}

        /// <summary>
        /// Возвратить строку запроса для получения списка ТЭЦ
        /// </summary>
        /// <param name="bIgnoreTECInUse">Признак игнорирования поля [InUse] в таблице [TEC_LIST]</param>
        /// <param name="arIdLimits">Диапазон идентификаторов ТЭЦ</param>
        /// <returns>Строка запроса</returns>
        public static string getQueryListTEC (bool bIgnoreTECInUse, int [] arIdLimits)
        {
            string strRes = "SELECT * FROM TEC_LIST ";

            if (bIgnoreTECInUse == false)
                strRes += "WHERE INUSE=1 ";
            else
                ;

            if (bIgnoreTECInUse == true)
                // условие еще не добавлено - добавляем
                strRes += @"WHERE ";
            else
                if (bIgnoreTECInUse == false)
                // условие уже добавлено
                strRes += @"AND ";
            else
                ;

            if (!(ASUTP.Helper.HUsers.allTEC == 0)) {
                strRes += $"ID={ASUTP.Helper.HUsers.allTEC.ToString ()}";
            } else
                //??? ограничение (временное) для ЛК
                strRes += $"NOT ID<{arIdLimits [0]} AND NOT ID>{arIdLimits [1]}";

            return strRes;
        }

        /// <summary>
        /// Возвратить таблицу [TEC_LIST] из БД конфигурации
        /// </summary>
        /// <param name="connConfigDB">Ссылка на объект с установленным соединением с БД</param>
        /// <param name="bIgnoreTECInUse">Признак игнорирования поля [InUse] в таблице [TEC_LIST]</param>
        /// <param name="arIdLimits">Диапазон идентификаторов ТЭЦ</param>
        /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
        /// <returns>Таблица - с данными</returns>
        public DataTable getListTEC (bool bIgnoreTECInUse, int [] arIdLimits, out int err)
        {
            string req = getQueryListTEC (bIgnoreTECInUse, arIdLimits);

            return Select (req, out err);
        }

        public DataTable GetListTECComponent (string prefix, int id_tec, out int err)
        {
            return Select ("SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString () + @" AND ID!=0", out err);
        }

        public DataTable GetListTECComponent (ref DbConnection connConfigDB, string prefix, int id_tec, out int err)
        {
            return Select ("SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString () + @" AND ID!=0", out err);
        }

        protected DataTable getListTG (int id, out int err)
        {
            return Select ("SELECT * FROM TG_LIST WHERE ID_TEC" + " = " + id.ToString (), out err);
        }

        public DataTable GetDataTableConnSettingsOfIdSource (int id_ext, int id_role, out int err)
        {
            DataTable tableRes;

            DbConnection dbConn;
            int iListenerId;

            iListenerId = register();
            dbConn = DbSources.Sources ().GetConnection (iListenerId, out err);

            if (err == 0)
                tableRes = ConnectionSettingsSource.GetConnectionSettings (ref dbConn, id_ext, id_role, out err);
            else
                tableRes = new DataTable ();

            unregister (iListenerId);

            return tableRes;
        }

        public DataTable GetDataTableConnSettingsOfIdSource (int id, out int err)
        {
            return Select ("SELECT * FROM SOURCE WHERE ID = " + id.ToString (), out err);
        }

        // комментарий для тестирования подключения к redmine #1001
        // комментарий для тестирования подключения к redmine #1002
        // комментарий для тестирования подключения к redmine #1003
        /// <summary>
        /// Возвратить результат запроса "Параметры всех ТГ"
        /// </summary>
        /// <param name="ver">Версия набора параметров (0 - самая новая, 1 - предыдущая, и т.д.)</param>
        /// <param name="err">Признак наличия ошибки при выполнении</param>
        /// <returns>Объект таблицы с результатами запроса</returns>
        protected DataTable getALL_PARAM_TG (int ver, out int err)
        {
            return Select (@"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (" + ver + @")", out err);
        }

        /// <summary>
        /// Возвратить результат запроса "Все параметры всех выводов"
        /// </summary>
        /// <param name="err">Признак наличия ошибки при выполнении метода</param>
        /// <returns>Объект-таблица с результатами запроса</returns>
        protected DataTable getALL_ParamVyvod (int id_tec, out int err)
        {
            string strQuery = @"SELECT tl.NAME_SHR as TEC_NAME_SHR"
                        + @", pnt.[ID_TEC], pnt.ID, pnt.[KKS_NAME], pnt.[VZLET_GRAFA]"
                        + @", vl.ID as [ID_VYVOD], vl.NAME_SHR as VYVOD_NAME_SHR, vl.KOM_UCHET"
                        + @", par.[ID] as [ID_PARAM], par.[NAME_SHR], par.[SYMBOL], par.[TYPE_AGREGATE]"
                    + @" FROM [dbo].[ID_POINT_ASKUTE] pnt"
                    + @" LEFT JOIN [dbo].[TEC_LIST] tl ON tl.ID = pnt.ID_TEC"
                    + @" LEFT JOIN [dbo].[ID_PARAM_ASKUTE] par ON par.ID = pnt.ID_PARAM"
                    + @" LEFT JOIN [dbo].[VYVOD_LIST] vl ON vl.ID = pnt.ID_VYVOD";

            if (!(id_tec < 0))
                strQuery += @" WHERE pnt.[ID_TEC] = " + id_tec;
            else
                ;

            return Select (strQuery
                , out err);
        }

        public DataTable GetDataTableParametersBiyskTG (int ver, out int error)
        {
            return Select (GetParametersBiyskTGQuery(ver), out error);
        }

        public static string GetParametersBiyskTGQuery (int ver)
        {
            return string.Format(@"SELECT * FROM [dbo].[ID_TG_ASKUE_BiTEC]
                WHERE [LAST_UPDATE] = (SELECT * FROM [dbo].[ft_Date-Versions_ID_TG_ASKUE_Bitec] ({0}))", ver.ToString ());
        }

        public static string GetWhereParameterBiyskTG (int num, string prefix, string postfix)
        {
            return string.Format(@"[SENSORS_NAME] LIKE '{1}{0}{2}'", num.ToString (), prefix, postfix);
        }

        public void UpadteDiagnosticSource (int id_gtp)
        {
        }

        public DataTable GetDataTableSetupParameters (out int err)
        {
            err = -1;

            DataTable tableRes = null;
            string query = string.Empty;

            tableRes = Select (
                // $"SELECT * FROM [dbo].[setup] WHERE [KEY]='{key}'"
                @"SELECT * FROM setup"
                , out err);

            return tableRes;
        }

        /// <summary>
        /// Идентификаторы типа значения в таблице конфигурации (соответсвует таблице [techsite_cfg-2.X.X].[dbo].[units])
        /// </summary>
        private enum UNITS {
            UNKNOWN = -1, BOOL = 8, STRING, INTEGER, FLOAT = 12
        }

        public string GetSetupParameterQuery (string key, object val, QUERY_TYPE type)
        {
            int err = -1;
            string strRes = string.Empty;
            UNITS id_unit = UNITS.UNKNOWN;

            id_unit = val.GetType ().Equals (typeof (bool)) == true ? UNITS.BOOL
                : val.GetType ().Equals (typeof (string)) == true ? UNITS.STRING
                    : val.GetType ().Equals (typeof (int)) == true ? UNITS.INTEGER
                        : new List<Type> () { typeof (double), typeof (float), typeof (decimal) }.Contains (val.GetType ()) == true ? UNITS.FLOAT
                            : UNITS.UNKNOWN;

            if (type == QUERY_TYPE.UPDATE)
                //query = @"UPDATE [dbo].[setup] SET [VALUE] = '" + val + @"' WHERE [KEY]='" + key + @"'";
                strRes = string.Format (@"UPDATE setup SET [VALUE]='{0}', [LAST_UPDATE]=GETDATE() WHERE [KEY]='{1}'", val, key);
            else if (type == QUERY_TYPE.INSERT)
                if (!(id_unit == UNITS.UNKNOWN))
                    strRes = string.Format (@"INSERT INTO [setup] ([ID], [VALUE],[KEY],[LAST_UPDATE],[ID_UNIT]) VALUES (({0}),'{1}','{2}',GETDATE(),{3})"
                        , "SELECT MAX([ID]) + 1 FROM [setup]"
                        , val
                        , key
                        , (int)id_unit
                    );
                else
                    ASUTP.Logging.Logg ().Error ($"FormParametersDB.getWriteStringRequest(KEY={key}, VALUE={val.ToString ()}) - не удалось определить тип значения ..."
                        , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
            else
                ;

            return strRes;
        }

        public string ReadSetupParameter (string key, string valuueDefault, out int err)
        {
            err = -1;
            string strRes = valuueDefault;
            
            DataTable table = null;
            string query = string.Empty;

            //query = @"SELECT * FROM [dbo].[setup] WHERE [KEY]='" + key + @"'";
            query = string.Format (@"SELECT * FROM setup WHERE [KEY]='{0}'", key);

            table = Select (query, out err);

            if (err == (int)DbTSQLInterface.Error.NO_ERROR)
                if (!(table == null))
                    if (table.Rows.Count == 1)
                        strRes = table.Rows [0] [@"Value"].ToString ().Trim ();
                    else
                        err = (int)DbTSQLInterface.Error.TABLE_ROWS_0;
                else
                    err = (int)DbTSQLInterface.Error.TABLE_NULL;
            else
                ;

            return strRes;
        }

        public static string GetParameterBiyskTGInsertQuery (int id_tec
            , string sensor_name
            , int id_tg
            , int id_tg_3
            , int id_tg_30
            , int id_vpiramida_object
            , int id_vpiramida_item)
        {
            string strRes = string.Empty;

            strRes = @"INSERT INTO [dbo].[ID_TG_ASKUE_BiTEC] ([ID_TEC],[SENSORS_NAME],[LAST_UPDATE],[ID_TG],[ID_3],[ID_30],[VPIRAMIDA_OBJECT],[VPIRAMIDA_ITEM]) VALUES ";

            strRes += $"({id_tec},";
            strRes += sensor_name;
            strRes += @"GETDATE(),";
            strRes += $"{id_tg},";
            strRes += $"{id_tg_3},";
            strRes += $"{id_tg_30},";
            // ChrjapiAN, 27.12.2017 возможность обработки OBJECT/ITEM
            strRes += $"{id_vpiramida_object},";
            strRes += $"{id_vpiramida_item}";
            strRes += @");";

            return strRes;
        }
    }
}
