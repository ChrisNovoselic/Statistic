using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace StatisticCommon
{
    public class InitTEC
    {
        public List<TEC> tec;
        private Users m_user;
        private static DbConnection m_connConfigDB;

        public static DataTable getListTEC(ref DbConnection connConfigDB, bool bIgnoreTECInUse, out int err)
        {
            string req = string.Empty;
            req = "SELECT * FROM TEC_LIST";

            if (bIgnoreTECInUse == false) req += " WHERE INUSE=1"; else ;

            return DbTSQLInterface.Select(ref connConfigDB, req, null, null, out err);
        }

        private DataTable getListTECComponent(string prefix, int id_tec, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString(), null, null, out err);
        }

        private DataTable getListTG(string prefix, int id, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM TG_LIST WHERE ID_" + prefix + " = " + id.ToString(), null, null, out err);
        }

        public static DataTable getConnSettingsOfIdSource(int idListener, int id_ext, int id_role, out int err)
        {
            DbConnection conn = DbSources.Sources().GetConnection(idListener, out err);
            return getConnSettingsOfIdSource(ref conn, id_ext, id_role, out err);
        }

        public static DataTable getConnSettingsOfIdSource(ref DbConnection conn, int id_ext, int id_role, out int err)
        {
            return ConnectionSettingsSource.GetConnectionSettings(ref conn, id_ext, id_role, out err);
        }

        private DataTable getConnSettingsOfIdSource(int id_ext, int id_role, out int err)
        {
            return ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, id_ext, id_role, out err);
        }

        private List <int> getMCId (DataTable data, int row)
        {
            int i = -1;
            List <int> listRes = new List<int> ();

            if ((!(data.Columns.IndexOf("ID_MC") < 0)) && (!(data.Rows[row]["ID_MC"] is DBNull)))
            {
                string [] ids = data.Rows[row]["ID_MC"].ToString ().Split (',');
                for (i = 0; i < ids.Length; i ++)
                    if (ids[i].Length > 0)
                        listRes.Add(Convert.ToInt32(ids[i]));
                    else
                        listRes.Add(-1);
            }
            else
                ;

            return listRes;
        }

        private bool IsNameField(DataTable data, string nameField) { return data.Columns.IndexOf(nameField) > -1 ? true : false; }

        //Список ВСЕХ компонентов (ТЭЦ, ГТП, ЩУ, ТГ)
        public InitTEC(int idListener, bool bIgnoreTECInUse, bool bUseData)
        {
            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - вход...");
            
            int err = -1;

            string prefix_admin = string.Empty,
                    prefix_pbr = string.Empty;

            tec = new List<TEC>();
            m_user = new Users(idListener);
            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - получение объекта MySqlConnection...");

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы
            DataTable list_tec = null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null, list_tg = null;

            //Использование статической функции
            list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, out err);

            if (err == 0) {
                for (int i = 0; i < list_tec.Rows.Count; i++)
                {
                    //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                    if ((m_user.allTEC == 0) || (m_user.Role < 100) || (m_user.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"])))
                    {
                        //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - tec.Count = " + tec.Count);

                        prefix_admin = string.Empty; prefix_pbr = string.Empty;
                        if ((list_tec.Columns.IndexOf("PREFIX_ADMIN") < 0) && (list_tec.Columns.IndexOf("PREFIX_PBR") < 0))
                        {
                        }
                        else
                        {
                            prefix_admin = list_tec.Rows[i]["PREFIX_ADMIN"].ToString();
                            prefix_pbr = list_tec.Rows[i]["PREFIX_PBR"].ToString();
                        }
                        
                        //Создание объекта ТЭЦ
                        tec.Add(new TEC(Convert.ToInt32 (list_tec.Rows[i]["ID"]),
                                        list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                        list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                        list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                        prefix_admin,
                                        prefix_pbr,
                                        bUseData));

                        int indx_tec = tec.Count - 1;
                        tec[indx_tec].SetNamesField(list_tec.Rows[i]["ADMIN_DATETIME"].ToString(),
                                            list_tec.Rows[i]["ADMIN_REC"].ToString(),
                                            list_tec.Rows[i]["ADMIN_IS_PER"].ToString(),
                                            list_tec.Rows[i]["ADMIN_DIVIAT"].ToString(),
                                            list_tec.Rows[i]["PBR_DATETIME"].ToString(),
                                            list_tec.Rows[i]["PPBRvsPBR"].ToString(),
                                            list_tec.Rows[i]["PBR_NUMBER"].ToString());

                        int indx = -1;
                        tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32 (list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_FACT);
                        if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_TM); else ;
                        if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                        if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;

                        if (err == 0) {
                            tec[indx_tec].m_timezone_offset_msc = Convert.ToInt32(list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                            tec[indx_tec].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();
                            tec[i].m_strTemplateNameSgnDataTM = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_TM"].ToString();
                            tec[i].m_strTemplateNameSgnDataFact = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_FACT"].ToString();

                            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - tec.Add () = Ok");

                            for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; ! (c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                            {
                                list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                if (err == 0)
                                    try
                                    {
                                        for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                        {
                                            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                            prefix_admin = string.Empty; prefix_pbr = string.Empty;
                                            if ((list_TECComponents.Columns.IndexOf("PREFIX_ADMIN") < 0) && (list_TECComponents.Columns.IndexOf("PREFIX_PBR") < 0)) {
                                            }
                                            else {
                                                prefix_admin = list_TECComponents.Rows[j]["PREFIX_ADMIN"].ToString();
                                                prefix_pbr = list_TECComponents.Rows[j]["PREFIX_PBR"].ToString();
                                            }

                                            tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], prefix_admin, prefix_pbr));

                                            indx = tec[indx_tec].list_TECComponents.Count - 1;
                                            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - indx = " + indx);

                                            tec[indx_tec].list_TECComponents[indx].name_shr = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                                            if (IsNameField(list_TECComponents, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].name_future = list_TECComponents.Rows[j]["NAME_FUTURE"].ToString(); else ;
                                            tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_TECComponents.Rows[j]["ID"]);
                                            tec[indx_tec].list_TECComponents[indx].m_listMCId = getMCId(list_TECComponents, j);
                                            if ((!(list_TECComponents.Columns.IndexOf("INDX_COL_RDG_EXCEL") < 0)) && (!(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                                                tec[indx_tec].list_TECComponents[j].m_indx_col_rdg_excel = Convert.ToInt32(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"]);
                                            else
                                                ;

                                            list_tg = getListTG(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]), out err);

                                            if (err == 0)
                                                for (int k = 0; k < list_tg.Rows.Count; k++)
                                                {
                                                    tec[indx_tec].list_TECComponents[indx].TG.Add(new TG(tec[indx_tec].list_TECComponents[indx]));
                                                    tec[indx_tec].list_TECComponents[indx].TG[k].name_shr = list_tg.Rows[k]["NAME_SHR"].ToString();
                                                    if (IsNameField(list_tg, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].TG[k].name_future = list_tg.Rows[k]["NAME_FUTURE"].ToString(); else ;
                                                    tec[indx_tec].list_TECComponents[indx].TG[k].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);
                                                    if (!(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                                                        tec[indx_tec].list_TECComponents[indx].TG[k].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                                                    else
                                                        ;
                                                }
                                            else
                                                ; //Ошибка получения списка ТГ
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Logging.Logg().LogExceptionToFile(e, "InitTEC::InitTEC (3 параметра) - ...for (int j = 0; j < list_TECComponents.Rows.Count; j++)...");
                                    }
                                else
                                    ; //Ошибка при получении списка компонентов
                            }
                        }
                        else
                            ; //Ошибка получения параметров соединений с БД

                        //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - list_TECComponents = Ok");

                        list_tg = getListTG(FormChangeMode.getPrefixMode((int)FormChangeMode.MODE_TECCOMPONENT.TEC), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                        if (err == 0)
                            for (int k = 0; k < list_tg.Rows.Count; k++)
                            {
                                tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], null, null));

                                indx = tec[indx_tec].list_TECComponents.Count - 1;

                                tec[indx_tec].list_TECComponents[indx].name_shr = list_tg.Rows[k]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                                if (IsNameField(list_tg, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].name_future = list_tg.Rows[k]["NAME_FUTURE"].ToString(); else ;
                                tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);

                                tec[indx_tec].list_TECComponents[indx].TG.Add(new TG(new TECComponent(tec[indx_tec], null, null)));
                                tec[indx_tec].list_TECComponents[indx].TG[0].name_shr = list_tg.Rows[k]["NAME_SHR"].ToString();
                                if (IsNameField(list_tg, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].TG[0].name_future = list_tg.Rows[k]["NAME_FUTURE"].ToString(); else ;
                                tec[indx_tec].list_TECComponents[indx].TG[0].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);
                                if (!(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                                    tec[indx_tec].list_TECComponents[indx].TG[0].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                                else
                                    ;
                            }
                        else
                            ; //Ошибка получения списка ТГ

                        //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - list_TG = Ok");
                    }
                    else
                        ;
                }
            }
            else
                ; //Ошибка получения списка ТЭЦ

            //DbTSQLInterface.CloseConnection(m_connConfigDB, out err);

            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (3 параметра) - вЫход...");
        }

        public InitTEC(int idListener, Int16 indx, bool bIgnoreTECInUse, bool bUseData) //indx = {GTP или PC}
        {
            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (4 параметра) - вход...");

            tec = new List<TEC> ();

            string prefix_admin = string.Empty,
                    prefix_pbr = string.Empty;

            int err = 0;
            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы
            DataTable list_tec= null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null, list_tg = null;

            //Использование методов объекта
            //int listenerId = -1;
            //bool err = false;
            //DbInterface dbInterface = new DbInterface (DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL, 1);
            //listenerId = dbInterface.ListenerRegister();
            //dbInterface.Start ();

            //dbInterface.SetConnectionSettings(connSett);

            //DbTSQLInterface.Select(listenerId, "SELECT * FROM TEC_LIST");
            //dbInterface.Response(listenerId, out err, out list_tec);

            //dbInterface.Stop();
            //dbInterface.ListenerUnregister(listenerId);

            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (4 параметра) - получение объекта MySqlConnection...");
            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            //Использование статической функции
            list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, out err);

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (4 параметра) - Создание объекта ТЭЦ: " + i);

                    prefix_admin = string.Empty; prefix_pbr = string.Empty;
                    if ((list_tec.Columns.IndexOf("PREFIX_ADMIN") < 0) && (list_tec.Columns.IndexOf("PREFIX_PBR") < 0))
                    {
                    }
                    else
                    {
                        prefix_admin = list_tec.Rows[i]["PREFIX_ADMIN"].ToString();
                        prefix_pbr = list_tec.Rows[i]["PREFIX_PBR"].ToString();
                    }
                    
                    //Создание объекта ТЭЦ
                    tec.Add(new TEC(Convert.ToInt32 (list_tec.Rows[i]["ID"]),
                                    list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                    list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                    list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                    prefix_admin,
                                    prefix_pbr,
                                    bUseData));

                    //List <string> listNamesField;
                    //listNamesField = new List<string> ();
                    //listNamesField.Add ();
                    tec[i].SetNamesField(list_tec.Rows[i]["ADMIN_DATETIME"].ToString(),
                                        list_tec.Rows[i]["ADMIN_REC"].ToString(),
                                        list_tec.Rows[i]["ADMIN_IS_PER"].ToString(),
                                        list_tec.Rows[i]["ADMIN_DIVIAT"].ToString(),
                                        list_tec.Rows[i]["PBR_DATETIME"].ToString(),
                                        list_tec.Rows[i]["PPBRvsPBR"].ToString(),
                                        list_tec.Rows[i]["PBR_NUMBER"].ToString());

                    tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_FACT);
                    if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_TM); else ;
                    if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                    if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;

                    tec[i].m_timezone_offset_msc = Convert.ToInt32 (list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                    tec[i].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();
                    tec[i].m_strTemplateNameSgnDataTM = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_TM"].ToString();
                    tec[i].m_strTemplateNameSgnDataFact = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_FACT"].ToString();

                    if (err == 0) list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32 (list_tec.Rows[i]["ID"]), out err); else ;
                    if (err == 0)
                        for (int j = 0; j < list_TECComponents.Rows.Count; j ++) {
                            prefix_admin = string.Empty; prefix_pbr = string.Empty;
                            if ((list_TECComponents.Columns.IndexOf("PREFIX_ADMIN") < 0) && (list_TECComponents.Columns.IndexOf("PREFIX_PBR") < 0))
                            {
                            }
                            else
                            {
                                prefix_admin = list_TECComponents.Rows[j]["PREFIX_ADMIN"].ToString();
                                prefix_pbr = list_TECComponents.Rows[j]["PREFIX_PBR"].ToString();
                            }
                            tec[i].list_TECComponents.Add(new TECComponent(tec[i], prefix_admin, prefix_pbr));
                            tec[i].list_TECComponents[j].name_shr = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                            if (IsNameField(list_TECComponents, "NAME_FUTURE") == true) tec[i].list_TECComponents[j].name_future = list_TECComponents.Rows[j]["NAME_FUTURE"].ToString(); else ;
                            tec[i].list_TECComponents[j].m_id = Convert.ToInt32 (list_TECComponents.Rows[j]["ID"]);
                            tec[i].list_TECComponents[j].m_listMCId = getMCId(list_TECComponents, j);
                            if ((!(list_TECComponents.Columns.IndexOf("INDX_COL_RDG_EXCEL") < 0)) && (!(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                                tec[i].list_TECComponents[j].m_indx_col_rdg_excel = Convert.ToInt32(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"]);
                            else
                                ;

                            list_tg = getListTG(FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]), out err);

                            if (err == 0)
                                for (int k = 0; k < list_tg.Rows.Count; k++)
                                {
                                    tec[i].list_TECComponents[j].TG.Add(new TG(tec[i].list_TECComponents[j]));
                                    tec[i].list_TECComponents[j].TG[k].name_shr = list_tg.Rows[k]["NAME_SHR"].ToString();
                                    if (IsNameField(list_tg, "NAME_FUTURE") == true) tec[i].list_TECComponents[j].TG[k].name_future = list_tg.Rows[k]["NAME_FUTURE"].ToString(); else ;
                                    tec[i].list_TECComponents[j].TG[k].m_id = Convert.ToInt32 (list_tg.Rows[k]["ID"]);
                                    if (! (list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                                        tec[i].list_TECComponents[j].TG[k].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                                    else
                                        ;
                                }
                            else
                                ; //Ошибка получения списка ТГ
                        }
                    else
                        ; //Ошибка ???
                }
            else
                ; //Ошибка получения списка ТЭЦ

            //DbTSQLInterface.CloseConnection (m_connConfigDB, out err);

            //ConnectionSettings connSett = new ConnectionSettings();
            //connSett.server = "127.0.0.1";
            //connSett.port = 3306;
            //connSett.dbName = "techsite";
            //connSett.userName = "techsite";
            //connSett.password = "12345";
            //connSett.ignore = false;

            /*
            int i, j, k; //Индексы для ТЭЦ, ГТП, ТГ
            tec = new List<TEC>();

            i = j = k = 0; //Обнуление индекса ТЭЦ, ГТП, ТГ

            //Создание объекта ТЭЦ (i = 0, Б)
            tec.Add(new TEC("БТЭЦ"));
            
            //Создание ГТП и добавление к ТЭЦ
            tec[i].list_TECComponents.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1";
            tec[i].TECComponent[j].name = "ГТП ТГ1"; //GNOVOS36
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ1";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG2";
            tec[i].TECComponent[j].name = "ГТП ТГ2"; //GNOVOS37
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG35"; //GNOVOS38
            tec[i].TECComponent[j].name = "ГТП ТГ3,5";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ3";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ5";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG4";
            tec[i].TECComponent[j].name = "ГТП ТГ4"; //GNOVOS08
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ4";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            tec.Add(new TEC("ТЭЦ-2"));
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "";
            tec[i].TECComponent[j].name = "";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ3";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ4";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ5";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ6";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ7";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ8";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ9";

            j = k = 0;
            i++;
            tec.Add(new TEC("ТЭЦ-3"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1";
            tec[i].TECComponent[j].name = "ГТП ТГ1"; //GNOVOS33
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ1";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG5";
            tec[i].TECComponent[j].name = "ГТП ТГ5"; //GNOVOS34
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ5";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG712"; //GNOVOS03
            tec[i].TECComponent[j].name = "ГТП ТГ7-12";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ7";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ8";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ9";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ10";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ11";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ12";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1314"; //GNOVOS04
            tec[i].TECComponent[j].name = "ГТП ТГ13,14";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ13";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ14";

            j = k = 0;
            i++;
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("ТЭЦ-4"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG3";
            tec[i].TECComponent[j].name = "ГТП ТГ3"; //GNOVOS35
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ3";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG48";
            tec[i].TECComponent[j].name = "ГТП ТГ4-8"; //GNOVOS07
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ4";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ5";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ6";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ7";
            //Создание ТГ и добавление к ГТП
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ8";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("ТЭЦ-5"));
            //Создание ГТП и добавление к ТЭЦ
            tec [i].TECComponent.Add (new TECComponent (tec [i]));
            tec[i].TECComponent[j].field = "TG12";
            tec[i].TECComponent[j].name = "ГТП ТГ1,2"; //GNOVOS06
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "ТГ1";
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j ++].TG [k ++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec [i].TECComponent.Add (new TECComponent (tec [i]));
            tec[i].TECComponent[j].field = "TG36";
            tec[i].TECComponent[j].name = "ГТП ТГ3-6"; //GNOVOS07
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "ТГ3";
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "ТГ4";
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "ТГ5";
            //Создание ТГ и добавление к ГТП
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "ТГ6";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i++; //Инкрементируем индекс ТЭЦ
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("Бийск-ТЭЦ"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG12";
            tec[i].TECComponent[j].name = "ГТП ТГ1,2";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ1";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG38";
            tec[i].TECComponent[j].name = "ГТП ТГ3-8";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ3";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ4";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ5";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ6";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ7";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "ТГ8";
            */

            //Logging.Logg().LogDebugToFile("InitTEC::InitTEC (4 параметра) - вЫход...");
        }
    }
}
