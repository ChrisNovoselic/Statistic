using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ASUTP.Database;
using System.Data;
using System.Data.Common;

namespace StatisticCommon
{
    public class DbTSQLDataSource : DbTSQLInterface
    {
        private static DbTSQLDataSource _this;

        private ConnectionSettings _connSett;

        public static DbTSQLDataSource DataSource ()
        {
            DbTSQLDataSource dbInterfaceRes = null;

            if (Equals (_this, null) == false)
                dbInterfaceRes = _this;
            else
                throw new NullReferenceException ("DbTSQLDataSource::ctor () - не был вызван...");

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
                    throw new NullReferenceException ("DbTSQLDataSource::ConnSett.get () - параметры соединения не были инициализированы...");

                return connSettRes;
            }
        }

        private DbTSQLDataSource (string name)
            : this (new ConnectionSettings(), name)
        {
            throw new Exception (@"Нельзя создать объект для обмена данными с БД конфигурации без параметров соединения с ней...");
        }

        public DbTSQLDataSource (ConnectionSettings connSett, string name)
            : base(getTypeDB(connSett), name, false)
        {
            _iListenerId = -1;
            _counterRegistred = -1;
            SetConnectionSettings (connSett);

            _this = this;
        }

        public void SetConnectionSettings (ConnectionSettings connSett)
        {
            if (IsModeConnectionLeaving == false) {
                _connSett = connSett;
                // при вызове из конструктора повторная инициализация
                _iListenerId = -1;
                _counterRegistred = -1;
            } else
                throw new InvalidOperationException($"DbTSQLDataSource::ListenerId - IsModeConnectionLeaving={IsModeConnectionLeaving}...");
        }

        public bool IsModeConnectionLeaving
        {
            get
            {
                return _iListenerId > 0;
            }
        }

        private int _iListenerId;

        private int _counterRegistred;

        public int ListenerId
        {
            get
            {
                if (IsModeConnectionLeaving == true)
                    return _iListenerId;
                else
                    throw new InvalidOperationException ($"DbTSQLDataSource::ListenerId - IsModeConnectionLeaving={IsModeConnectionLeaving}...");
            }
        }

        public void Register (string name)
        {
            _iListenerId = register (name);

            _counterRegistred += _counterRegistred < 0 ? 2 : 1;
        }

        public void UnRegister ()
        {
            if (unregister (_iListenerId, true) == true)
                _iListenerId = -1;
            else
                ;

            _counterRegistred--;
        }

        protected int register ()
        {
            return register (Name);
        }

        protected int register (string name)
        {
            string mesThrow = string.Empty;

            if ((IsModeConnectionLeaving == false)
                && (Equals (_connSett, null) == false)
                && (_connSett.Validate () == ConnectionSettings.ConnectionSettingsError.NoError))
                return DbSources.Sources ().Register (_connSett, false, name);
            else {
                ASUTP.Logging.Logg ().Warning ($"DbTSQLDataSource::register () - повторная регистрация для {Name}..."
                    , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

                return ListenerId;
            }
        }

        protected bool unregister (int iListenerId, bool bIsLeaving = false)
        {
            bool bRes = false;

            string mesThrow = string.Empty;

            if (bIsLeaving == false) {
                bRes = true;
            } else {
                if (IsModeConnectionLeaving == true)
                    if (_counterRegistred == 1)
                        bRes = true;
                    else if (_counterRegistred == 0)
                        mesThrow = Equals (_connSett, null) == false ? _connSett.Validate ().ToString () : "null";
                    else
                        ;
                else
                    mesThrow = Equals (_connSett, null) == false ? _connSett.Validate ().ToString () : "null";
            }

            if (bRes == true)
                DbSources.Sources ().UnRegister (iListenerId);
            else {
                if (string.IsNullOrEmpty (mesThrow) == false) {
                    mesThrow = $"DbTSQLDataSource::unregister () - IsModeConnectionLeaving={IsModeConnectionLeaving}, ConnectionSettings::Validate={mesThrow}...";

                    throw new InvalidOperationException (mesThrow);
                } else
                    ;
            }

            return bRes;
        }

        public void ExecNonQuery (string query, out int error)
        {
            error = -1;

            int iListenerId = -1;
            DbConnection dbConnection;

            if (Equals (ConnSett, null) == false) {
                if (IsModeConnectionLeaving == false)
                    iListenerId = register (Name);
                else
                    iListenerId = _iListenerId;

                dbConnection = DbSources.Sources ().GetConnection (iListenerId, out error);

                if (error == 0)
                    ExecNonQuery (ref dbConnection, query, null, null, out error);
                else {
                    ;
                }

                if (IsModeConnectionLeaving == false)
                    if (unregister (iListenerId) == false)
                        throw new Exception ("::ExecNonQuery () - Не разорвано временное соединения с БД...");
                    else
                        ;
                else
                    ;
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
                if (IsModeConnectionLeaving == false)
                    iListenerId = register (Name);
                else
                    iListenerId = _iListenerId;

                dbConnection = DbSources.Sources ().GetConnection (iListenerId, out error);

                if (error == 0)
                    tableRes = Select (ref dbConnection, query, null, null, out error);
                else {
                    tableRes = new DataTable ();
                }

                if (IsModeConnectionLeaving == false)
                    if (unregister (iListenerId) == false)
                        throw new Exception ("::ExecNonQuery () - Не разорвано временное соединения с БД...");
                    else
                        ;
                else
                    ;

                return tableRes;
            } else
                throw new Exception ("Не установлены параметры для соединения с БД...");
        }

        public DataTable Select (string query, DbType [] types, object [] paramValues, out int error)
        {
            return Select (query, out error);
        }

        public void ExecNonQuery (string query, DbType [] types, object [] paramValues, out int error)
        {
            ExecNonQuery (query, out error);
        }
    }

    public partial class DbTSQLConfigDatabase : DbTSQLDataSource 
    {
        private static DbTSQLConfigDatabase _this;

        public DbTSQLConfigDatabase (ConnectionSettings connSett)
            : base (connSett, $"{CONN_SETT_TYPE.CONFIG_DB.ToString ()}")
        {
            _this = this;
        }

        public static DbTSQLConfigDatabase DbConfig ()
        {
            return _this;
        }

        public void Register ()
        {
            Register (Name);
        }

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
        public DataTable GetListTEC (bool bIgnoreTECInUse, int [] arIdLimits, out int err)
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

            if (IsModeConnectionLeaving == true) {
                //!!! Перед вызовом д.б. выполнена регистрация
                dbConn = DbSources.Sources ().GetConnection (ListenerId, out err);

                if (err == 0)
                    tableRes = ConnectionSettingsSource.GetConnectionSettings (ref dbConn, id_ext, id_role, out err);
                else
                    tableRes = new DataTable ();
            } else
                throw new InvalidOperationException ($"DbTSQLConfigDatabase::GetDataTableConnSettingsOfIdSource () - объект не переведен в режим удержания соединения...");

            return tableRes;
        }

        /// <summary>
        /// Возвратить таблицу со всеми источниками данных
        /// </summary>
        /// <param name="err">Признак ошибки при выполнении операции</param>
        /// <returns>Таблица с данными - результат запроса</returns>
        public DataTable GetDataTableSource (out int err)
        {
            return Select ("SELECT * FROM SOURCE", out err);
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
