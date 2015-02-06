using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using StatisticCommon;

namespace update_pbr
{
    class Program
    {
        static void Main(string[] args)
        {
            int err = -1
                , i = -1
                , idListenerCfgDB = -1
                , idListenerDB = -1;
            string query = string.Empty
                , strMsg = string.Empty
                ;
            ConnectionSettings connSett = new ConnectionSettings ();
            System.Data.Common.DbConnection dbConn = null;
            FIleConnSett fileConnSett = new FIleConnSett(@"connsett_ne22.ini", FIleConnSett.MODE.FILE);
            FileINI fileINI = null;
            //fileINI = new FileINI(@"setup.ini", new string[] { @"ID_TECNotUse" }, new string[] { @"6" });
            fileINI = new FileINI(@"setup.ini");

            fileINI.Add(@"ID_TECNotUse", string.Empty);

            ProgramBase.Start();

            ////Запись 1-ый запуск
            //connSett.id = 102;
            //connSett.name = @"";

            //connSett.server = @"10.100.104.22";
            //connSett.port = 3306;
            //connSett.dbName = @"techsite_cfg";
            //connSett.userName = @"techsite";
            //connSett.ignore = false;
            ////Password
            //connSett.password = @"12345";

            //fileConnSett.SaveSettingsFile(-1, new List<ConnectionSettings>() { connSett }, out err);

            //Последующие запуски
            List<ConnectionSettings> listConnSetts;
            string strErr = string.Empty;
            fileConnSett.ReadSettingsFile(-1, out listConnSetts, out err, out strErr);

            //listConnSetts[0].id = 102;

            idListenerCfgDB = DbSources.Sources().Register(listConnSetts[0], false, @"Конф. БД");
            List<TEC> listTEC = new InitTEC_190 (idListenerCfgDB, true, false).tec;
            DbSources.Sources().UnRegister(idListenerCfgDB);

            string [] arPrefix = new string [] {@"PBR", @"Pmin", @"Pmax"};
            query = string.Empty;
            string strIds_TECNotUse = fileINI.GetValueOfKey(@"ID_TECNotUse");
            int[] arIds_TECNotUse = new int[strIds_TECNotUse.Split(',').Length];
            for (i = 0; i < arIds_TECNotUse.Length; i++)
            {
                if (Int32.TryParse(strIds_TECNotUse.Split(',')[i], out arIds_TECNotUse[i]) == false)
                    arIds_TECNotUse [i] = -1;
                else
                    ;
            }

            foreach (TEC t in listTEC)
            {
                strMsg = t.name_shr + @" = ";

                //if (!(t.m_id == Int32.Parse()))
                if (arIds_TECNotUse.Contains<int>(t.m_id) == false)
                {
                    //switch (t.m_id)
                    switch (t.type())
                    {
                        //case 1:
                        //case 2:
                        //case 3:
                        //case 4:
                        //case 5:
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        //case 6:
                        case TEC.TEC_TYPE.BIYSK:
                            idListenerCfgDB = DbSources.Sources().Register(listConnSetts[0], false, @"Конф. БД");
                            t.connSettings(ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_190, idListenerCfgDB, 103, -1, out err), (int)CONN_SETT_TYPE.PBR);
                            DbSources.Sources().UnRegister(idListenerCfgDB);
                            break;
                        default:
                            break;
                    }

                    idListenerDB = DbSources.Sources().Register(t.connSetts[(int)CONN_SETT_TYPE.PBR], false, @"ТЭЦ=" + t.name_shr + @", DESC=" + i.ToString());
                    dbConn = DbSources.Sources().GetConnection(idListenerDB, out err);

                    switch (t.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            query = @"UPDATE `techsite`.`PPBRvsPBRnew2014` SET ";

                            for (i = 0; i < arPrefix.Length; i++)
                            {
                                //query += @"'" + t.prefix_pbr + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + @"'=";
                                query += @"`" + t.prefix_pbr + @"_" + arPrefix[i] + @"`=";

                                foreach (TECComponent tc in t.list_TECComponents)
                                {
                                    if ((tc.m_id > 100) && (tc.m_id < 500))
                                    {
                                        query += @"`" + t.NameFieldOfPBRRequest(tc) + @"_" + arPrefix[i];

                                        query += @"`+";
                                    }
                                    else
                                        ;
                                }

                                //Устранить лишний '+'
                                query = query.Substring(0, query.Length - 1);

                                query += @", ";
                            }

                            //Устранить лишние символы SPACE + ','
                            query = query.Substring(0, query.Length - 2);

                            query += @" WHERE `" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @"`>'" + DateTime.Now.Date.ToString(@"yyyyMMdd") + @"'";

                            Console.WriteLine(query);
                            Logging.Logg().Debug(query, Logging.INDEX_MESSAGE.NOT_SET);

                            DbTSQLInterface.ExecNonQuery(ref dbConn, query, null, null, out err);
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            string nameField = string.Empty
                                , tec_prefix_pbr = @"BiTEC";
                            query = @"UPDATE `techsite`.`BiPPBRvsPBR` SET ";

                            for (i = 0; i < arPrefix.Length; i++)
                            {
                                //query += @"'" + t.prefix_pbr + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + @"'=";
                                query += @"`" + tec_prefix_pbr + @"_" + arPrefix[i] + @"`=";

                                foreach (TECComponent tc in t.list_TECComponents)
                                {
                                    if ((tc.m_id > 100) && (tc.m_id < 500))
                                    {
                                        switch (tc.m_id)
                                        {
                                            case 117:
                                                nameField = @"TG1";
                                                break;
                                            case 118:
                                                nameField = @"TG28";
                                                break;
                                            default:
                                                nameField = string.Empty;
                                                break;
                                        }

                                        if (nameField.Equals(string.Empty) == false)
                                        {
                                            query += @"`" + tec_prefix_pbr + @"_" + nameField + @"_" + arPrefix[i];

                                            query += @"`+";
                                        }
                                        else { }
                                    }
                                    else
                                        ;
                                }

                                //Устранить лишний '+'
                                query = query.Substring(0, query.Length - 1);

                                query += @", ";
                            }

                            //Устранить лишние символы SPACE + ','
                            query = query.Substring(0, query.Length - 2);

                            query += @" WHERE `" + @"DATE_TIME" + @"`>'" + DateTime.Now.Date.ToString(@"yyyyMMdd") + @"'";

                            Console.WriteLine(query);
                            Logging.Logg().Debug(query, Logging.INDEX_MESSAGE.NOT_SET);

                            DbTSQLInterface.ExecNonQuery(ref dbConn, query, null, null, out err);
                            break;
                        default:
                            break;
                    }

                    if (!(err == 0))
                        strMsg += @"Ошибка...";
                    else
                        strMsg += @"Успех!!!";

                    DbSources.Sources().UnRegister(idListenerDB);
                }
                else
                    strMsg += @"Пропуск..."; //ТЭЦ не обрабатываем

                Console.WriteLine(strMsg + Environment.NewLine);
                Logging.Logg().Debug(strMsg, Logging.INDEX_MESSAGE.NOT_SET);
            }

            Console.WriteLine(@"Выход...");

            ProgramBase.Exit();
        }
    }
}
