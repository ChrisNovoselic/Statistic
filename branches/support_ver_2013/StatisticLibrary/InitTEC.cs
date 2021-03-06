using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
//using MySql.Data.MySqlClient;

using HClassLibrary;

namespace StatisticCommon
{
    public class InitTECBase
    {
        public class ListTEC : List <TEC>
        {
            public ListTEC () : base ()
            {
            }

            private void update ()
            {
            }
        }

        //protected InitTECBase ()
        //{
        //}

        //public InitTECBase(int iListenerId)
        //{
        //    int err = -1;
        //    m_connConfigDB = DbSources.Sources().GetConnection(iListenerId, out err);
        //}

        public ListTEC tec;
        protected /*static*/ DbConnection m_connConfigDB;

        protected bool IsNameField(DataTable data, string nameField) { return data.Columns.IndexOf(nameField) > -1 ? true : false; }

        public static DataTable getListTEC(ref DbConnection connConfigDB, out int err)
        {
            string req = string.Empty;
            bool bIgnoreTECInUse = true;
            req = "SELECT * FROM TEC_LIST ";

            if (bIgnoreTECInUse == false) req += "WHERE INUSE=1 "; else ;
            if (!(HStatisticUsers.allTEC == 0))
            {
                if (bIgnoreTECInUse == false) req += @"AND "; else ;

                if (bIgnoreTECInUse == true) req += @"WHERE "; else ;
                req += @"ID=" + HStatisticUsers.allTEC.ToString();
            }
            else
                ;

            return DbTSQLInterface.Select(ref connConfigDB, req, null, null, out err);
        }

        protected DataTable getListTECComponent(string prefix, int id_tec, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString() + @" AND ID!=0", null, null, out err);
        }

        protected static DataTable getListTECComponent(ref DbConnection connConfigDB, string prefix, int id_tec, out int err)
        {
            return DbTSQLInterface.Select(ref connConfigDB, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString() + @" AND ID!=0", null, null, out err);
        }

        protected DataTable getListTG(string prefix, int id, out int err)
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

        protected DataTable getConnSettingsOfIdSource(int id_ext, int id_role, out int err)
        {
            return ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, id_ext, id_role, out err);
        }

        protected List<int> getMCentreId(DataTable data, int row)
        {
            return getModesId(data, row, @"ID_MC");
        }

        protected List<int> getMTermId(DataTable data, int row)
        {
            return getModesId(data, row, @"ID_MT");
        }

        protected List<int> getModesId(DataTable data, int row, string col)
        {
            int i = -1;
            List<int> listRes = new List<int>();

            if ((!(data.Columns.IndexOf(col) < 0)) && (!(data.Rows[row][col] is DBNull)))
            {
                string[] ids = data.Rows[row][col].ToString().Split(',');
                for (i = 0; i < ids.Length; i++)
                    if (ids[i].Length > 0)
                        listRes.Add(Convert.ToInt32(ids[i]));
                    else
                        listRes.Add(-1);
            }
            else
                ;

            return listRes;
        }

        public DataTable getConnSettingsOfIdSource(int id)
        {
            int err = 0;

            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM SOURCE WHERE ID = " + id.ToString(), null, null, out err);
        }
    }

    public class InitTEC : InitTECBase
    {
        private DataTable getALL_PARAM_TG(int ver, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (" + ver + @")", null, null, out err);
        }
        
        private bool IsNameField(DataRow data, string nameField) { return data.Table.Columns.IndexOf(nameField) > -1 ? true : false; }

        private void InitTG (List <TECComponent> dest, int indx, string prefix, DataTable allParamTG) {
            int j = -1, k = -1;
            DataRow [] rows_tg = allParamTG.Select (@"ID_" + prefix + @"=" + dest [indx].m_id);
            
            for (j = 0; j < rows_tg.Length; j++)
            {
                for (k = 0; k < dest.Count; k++)
                {
                    if (((dest[k].IsTG == true)) && (Int32.Parse (rows_tg[j][@"ID_TG"].ToString ()) == dest[k].m_id))
                        break;
                    else
                        ;
                }

                if (k < dest.Count) {
                    dest[indx].m_listTG.Add(dest[k].m_listTG[0]);
                    if (dest[indx].IsGTP == true)
                        dest[k].m_listTG[0].m_id_owner_gtp = dest[indx].m_id;
                    else
                        if (dest[indx].IsPC == true)
                            dest[k].m_listTG[0].m_id_owner_pc = dest[indx].m_id;
                        else
                            ;
                }
                else
                    ;
            }
        }

        private void initTG (TG dest, DataRow row_tg, DataTable allParamTG) {
            dest.name_shr = row_tg["NAME_SHR"].ToString();
            if (IsNameField(row_tg, "NAME_FUTURE") == true) dest.name_future = row_tg["NAME_FUTURE"].ToString(); else ;
            dest.m_id = Convert.ToInt32(row_tg["ID"]);
            if (!(row_tg["INDX_COL_RDG_EXCEL"] is System.DBNull))
                dest.m_indx_col_rdg_excel = Convert.ToInt32(row_tg["INDX_COL_RDG_EXCEL"]);
            else
                ;

            DataRow[] rows_tg = allParamTG.Select(@"ID_TG=" + dest.m_id);
            dest.m_strKKS_NAME_TM = rows_tg[0][@"KKS_NAME"].ToString();
            dest.m_arIds_fact[(int)TG.ID_TIME.MINUTES] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_3"].ToString());
            dest.m_arIds_fact[(int)TG.ID_TIME.HOURS] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_30"].ToString());
        }

        /// <summary>
        /// ������ ���� ����������� (���, ���, ��, ��)
        /// </summary>
        /// <param name="idListener"></param>
        /// <param name="bIgnoreTECInUse"></param>
        /// <param name="bUseData"></param>
        public InitTEC(int idListener, bool bUseData)
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ����...");

            int err = -1;

            tec = new ListTEC ();
            //m_user = new Users(idListener);
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ��������� ������� MySqlConnection...");

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null,
                    list_tg = null
                    , all_PARAM_TG = null;

            all_PARAM_TG = getALL_PARAM_TG (0, out err);

            //������������� ����������� �������
            list_tec = getListTEC(ref m_connConfigDB, out err);

            if (err == 0) {
                for (int i = 0; i < list_tec.Rows.Count; i++)
                {
                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                    if ((HStatisticUsers.allTEC == 0) || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"]))
                        /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Count = " + tec.Count);

                        //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //�������� ������� ���
                            tec.Add(new TEC(Convert.ToInt32 (list_tec.Rows[i]["ID"]),
                                            list_tec.Rows[i]["NAME_SHR"].ToString() //"NAME_SHR"
                                    ));

                            int indx_tec = tec.Count - 1;
                            EventTECListUpdate += tec[indx_tec].PerformUpdate;

                            tec[indx_tec].SetNamesField(list_tec.Rows[i]["ADMIN_DATETIME"].ToString(),
                                                list_tec.Rows[i]["ADMIN_REC"].ToString(),
                                                list_tec.Rows[i]["ADMIN_IS_PER"].ToString(),
                                                list_tec.Rows[i]["ADMIN_DIVIAT"].ToString(),
                                                list_tec.Rows[i]["PBR_DATETIME"].ToString(),
                                                list_tec.Rows[i]["PPBRvsPBR"].ToString(),
                                                list_tec.Rows[i]["PBR_NUMBER"].ToString());

                            int indx = -1;
                            tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32 (list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_AISKUE);
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO); else ;
                            //if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN); else ;
                            //if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN); else ;
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;
                            if ((err == 0) && ((list_tec.Rows[i]["ID_SOURCE_MTERM"] is DBNull) == false)) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_MTERM"]), -1, out err), (int)CONN_SETT_TYPE.MTERM); else ;

                            if (err == 0) {
                                tec[indx_tec].m_timezone_offset_msc = Convert.ToInt32(list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                                tec[indx_tec].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();
                                tec[indx_tec].m_strTemplateNameSgnDataTM = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_TM"].ToString();
                                tec[indx_tec].m_strTemplateNameSgnDataFact = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_FACT"].ToString();

                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Add () = Ok");

                                list_tg = getListTG(FormChangeMode.getPrefixMode((int)FormChangeMode.MODE_TECCOMPONENT.TEC), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                if (err == 0)
                                    for (int k = 0; k < list_tg.Rows.Count; k++)
                                    {
                                        tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec]));

                                        indx = tec[indx_tec].list_TECComponents.Count - 1;

                                        tec[indx_tec].list_TECComponents[indx].name_shr = list_tg.Rows[k]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                                        if (IsNameField(list_tg, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].name_future = list_tg.Rows[k]["NAME_FUTURE"].ToString(); else ;
                                        tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);

                                        tec[indx_tec].list_TECComponents[indx].m_listTG.Add(new TG());
                                        initTG (tec[indx_tec].list_TECComponents[indx].m_listTG[0], list_tg.Rows[k], all_PARAM_TG);
                                    }
                                else
                                    ; //������ ��������� ������ ��

                                for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; ! (c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                            {
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                                tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec]));

                                                indx = tec[indx_tec].list_TECComponents.Count - 1;
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - indx = " + indx);

                                                tec[indx_tec].list_TECComponents[indx].name_shr = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                                                if (IsNameField(list_TECComponents, "NAME_FUTURE") == true) tec[indx_tec].list_TECComponents[indx].name_future = list_TECComponents.Rows[j]["NAME_FUTURE"].ToString(); else ;
                                                tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_TECComponents.Rows[j]["ID"]);
                                                tec[indx_tec].list_TECComponents[indx].m_listMCentreId = getMCentreId(list_TECComponents, j);
                                                tec[indx_tec].list_TECComponents[indx].m_listMTermId = getMTermId(list_TECComponents, j);
                                                if ((!(list_TECComponents.Columns.IndexOf("INDX_COL_RDG_EXCEL") < 0)) && (!(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                                                    tec[indx_tec].list_TECComponents[indx].m_indx_col_rdg_excel = Convert.ToInt32(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"]);
                                                else
                                                    ;
                                                if ((list_TECComponents.Columns.IndexOf("KoeffAlarmPcur") > 0) && (!(list_TECComponents.Rows[j]["KoeffAlarmPcur"] is System.DBNull)))
                                                    tec[indx_tec].list_TECComponents[indx].m_dcKoeffAlarmPcur = Convert.ToInt32(list_TECComponents.Rows[j]["KoeffAlarmPcur"]);
                                                else
                                                    ;

                                                //list_tg = getListTG(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]), out err);

                                                if (err == 0)
                                                    InitTG (tec[indx_tec].list_TECComponents, indx, FormChangeMode.getPrefixMode(c), all_PARAM_TG);
                                                else
                                                    ; //������ ��������� ������ ��
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Logging.Logg().Exception(e, "InitTEC::InitTEC (3 ���������) - ...for (int j = 0; j < list_TECComponents.Rows.Count; j++)...", Logging.INDEX_MESSAGE.NOT_SET);
                                        }
                                    else
                                        ; //������ ��� ��������� ������ �����������
                                }
                            }
                            else
                                ; //������ ��������� ���������� ���������� � ��
                        //} else ;

                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TG = Ok");
                    }
                    else
                        ;
                }
            }
            else
                ; //������ ��������� ������ ���

            //DbTSQLInterface.CloseConnection(m_connConfigDB, out err);

            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - �����...");
        }

        public InitTEC(int idListener, Int16 indx, bool bUseData) //indx = {GTP ��� PC}
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ����...");

            tec = new ListTEC ();

            int err = 0;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null,
                    list_tg = null
                    , allParamTG = null;

            //������������� ������� �������
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

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ��������� ������� MySqlConnection...");
            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            //������������� ����������� �������
            list_tec = getListTEC(ref m_connConfigDB, out err);

            allParamTG = getALL_PARAM_TG (0, out err);

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �������� ������� ���: " + i);

                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {

                        //�������� ������� ���
                        tec.Add(new TEC(Convert.ToInt32 (list_tec.Rows[i]["ID"]),
                                        list_tec.Rows[i]["NAME_SHR"].ToString() //"NAME_SHR"
                            ));

                        EventTECListUpdate += tec[i].PerformUpdate;

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

                        tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_AISKUE);
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO); else ;
                        //if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(m_typeDB_CFG, ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN); else ;
                        //if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(m_typeDB_CFG, ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN); else ;
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;
                        if ((err == 0) && ((list_tec.Rows[i]["ID_SOURCE_MTERM"] is DBNull) == false)) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_MTERM"]), -1, out err), (int)CONN_SETT_TYPE.MTERM); else ;

                        tec[i].m_timezone_offset_msc = Convert.ToInt32 (list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                        tec[i].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();
                        tec[i].m_strTemplateNameSgnDataTM = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_TM"].ToString();
                        tec[i].m_strTemplateNameSgnDataFact = list_tec.Rows[i]["TEMPLATE_NAME_SGN_DATA_FACT"].ToString();

                        if (err == 0) list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32 (list_tec.Rows[i]["ID"]), out err); else ;
                        if (err == 0)
                            for (int j = 0; j < list_TECComponents.Rows.Count; j ++) {
                                tec[i].list_TECComponents.Add(new TECComponent(tec[i]));
                                tec[i].list_TECComponents[j].name_shr = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                                if (IsNameField(list_TECComponents, "NAME_FUTURE") == true) tec[i].list_TECComponents[j].name_future = list_TECComponents.Rows[j]["NAME_FUTURE"].ToString(); else ;
                                tec[i].list_TECComponents[j].m_id = Convert.ToInt32 (list_TECComponents.Rows[j]["ID"]);
                                tec[i].list_TECComponents[j].m_listMCentreId = getMCentreId(list_TECComponents, j);
                                tec[i].list_TECComponents[j].m_listMTermId = getMTermId(list_TECComponents, j);
                                if ((!(list_TECComponents.Columns.IndexOf("INDX_COL_RDG_EXCEL") < 0)) && (!(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                                    tec[i].list_TECComponents[j].m_indx_col_rdg_excel = Convert.ToInt32(list_TECComponents.Rows[j]["INDX_COL_RDG_EXCEL"]);
                                else
                                    ;
                                if ((!(list_TECComponents.Columns.IndexOf("KoeffAlarmPcur") < 0)) && (!(list_TECComponents.Rows[j]["KoeffAlarmPcur"] is System.DBNull)))
                                    tec[i].list_TECComponents[j].m_dcKoeffAlarmPcur = Convert.ToInt32(list_TECComponents.Rows[j]["KoeffAlarmPcur"]);
                                else
                                    ;

                                //list_tg = getListTG(FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]), out err);

                                if (err == 0)
                                    InitTG (tec[i].list_TECComponents, j, FormChangeMode.getPrefixMode(indx), allParamTG);
                                else
                                    ; //������ ��������� ������ ��
                            }
                        else
                            ; //������ ???
                    //} else ;
                }
            else
                ; //������ ��������� ������ ���

            //DbTSQLInterface.CloseConnection (m_connConfigDB, out err);

            //ConnectionSettings connSett = new ConnectionSettings();
            //connSett.server = "127.0.0.1";
            //connSett.port = 3306;
            //connSett.dbName = "techsite";
            //connSett.userName = "techsite";
            //connSett.password = "12345";
            //connSett.ignore = false;

            /*
            int i, j, k; //������� ��� ���, ���, ��
            tec = new List<TEC>();

            i = j = k = 0; //��������� ������� ���, ���, ��

            //�������� ������� ��� (i = 0, �)
            tec.Add(new TEC("����"));
            
            //�������� ��� � ���������� � ���
            tec[i].list_TECComponents.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1";
            tec[i].TECComponent[j].name = "��� ��1"; //GNOVOS36
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��1";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG2";
            tec[i].TECComponent[j].name = "��� ��2"; //GNOVOS37
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG35"; //GNOVOS38
            tec[i].TECComponent[j].name = "��� ��3,5";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��3";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��5";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG4";
            tec[i].TECComponent[j].name = "��� ��4"; //GNOVOS08
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��4";

            j = k = 0; //��������� ������� ���, ��
            i ++; //�������������� ������ ���
            tec.Add(new TEC("���-2"));
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "";
            tec[i].TECComponent[j].name = "";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��3";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��4";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��5";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��6";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��7";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��8";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��9";

            j = k = 0;
            i++;
            tec.Add(new TEC("���-3"));
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1";
            tec[i].TECComponent[j].name = "��� ��1"; //GNOVOS33
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��1";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG5";
            tec[i].TECComponent[j].name = "��� ��5"; //GNOVOS34
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��5";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG712"; //GNOVOS03
            tec[i].TECComponent[j].name = "��� ��7-12";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��7";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��8";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��9";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��10";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��11";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��12";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG1314"; //GNOVOS04
            tec[i].TECComponent[j].name = "��� ��13,14";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��13";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��14";

            j = k = 0;
            i++;
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("���-4"));
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG3";
            tec[i].TECComponent[j].name = "��� ��3"; //GNOVOS35
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��3";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG48";
            tec[i].TECComponent[j].name = "��� ��4-8"; //GNOVOS07
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��4";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��5";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��6";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��7";
            //�������� �� � ���������� � ���
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��8";

            j = k = 0; //��������� ������� ���, ��
            i ++; //�������������� ������ ���
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("���-5"));
            //�������� ��� � ���������� � ���
            tec [i].TECComponent.Add (new TECComponent (tec [i]));
            tec[i].TECComponent[j].field = "TG12";
            tec[i].TECComponent[j].name = "��� ��1,2"; //GNOVOS06
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "��1";
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j ++].TG [k ++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec [i].TECComponent.Add (new TECComponent (tec [i]));
            tec[i].TECComponent[j].field = "TG36";
            tec[i].TECComponent[j].name = "��� ��3-6"; //GNOVOS07
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add (new TG (tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "��3";
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "��4";
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "��5";
            //�������� �� � ���������� � ���
            tec [i].TECComponent [j].TG.Add(new TG(tec [i].TECComponent [j]));
            tec [i].TECComponent [j].TG [k ++].name = "��6";

            j = k = 0; //��������� ������� ���, ��
            i++; //�������������� ������ ���
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("�����-���"));
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG12";
            tec[i].TECComponent[j].name = "��� ��1,2";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��1";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j++].TG[k++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].TECComponent.Add(new TECComponent(tec[i]));
            tec[i].TECComponent[j].field = "TG38";
            tec[i].TECComponent[j].name = "��� ��3-8";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��3";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��4";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��5";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��6";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��7";
            tec[i].TECComponent[j].TG.Add(new TG(tec[i].TECComponent[j]));
            tec[i].TECComponent[j].TG[k++].name = "��8";
            */

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �����...");
        }

        /// <summary>
        /// ������� ��� ������������� ���������� ���������� ��� ��� ���� ��������� ������� (List <TEC>)
        ///  ��� �������� ����������� ������� ��� 'TEC.EventUpdate'
        /// </summary>
        public static event DelegateIntFunc EventTECListUpdate;

        public class TECListUpdateEventArgs : EventArgs
        {
            public int m_iListenerId;
        }

        public static void PerformTECListUpdate (int iListenerId)
        {
            if (! (EventTECListUpdate == null))
                EventTECListUpdate (iListenerId);
            else
                ;
        }

        public static void OnTECUpdate (object obj, EventArgs ev)
        {
            TEC tec = obj as TEC;
            int iListenerId = (ev as TECListUpdateEventArgs).m_iListenerId
                , err = -1;
            DataTable tableRes;
            DataRow []selRows;

            DbConnection connConfigDB = DbSources.Sources ().GetConnection (iListenerId, out err);

            tableRes = getListTEC(ref connConfigDB, out err);
            //??? ���������� ���������� ��� (��������: m_IdSOTIASSOLinkSourceTM)
            //tec.Update (tableRes);

            tableRes = getListTECComponent (ref connConfigDB, @"GTP", tec.m_id, out err);
            // ���������� ���������� ���
            if (tableRes.Columns.IndexOf("KoeffAlarmPcur") > 0)            
                // ����� ���
                foreach (TECComponent tc in tec.list_TECComponents)
                    if (tc.IsGTP == true)
                    {
                        selRows = tableRes.Select (@"ID=" + tc.m_id);
                        // ��������� ������� ��������
                        if ((selRows.Length == 1)
                            && (!(selRows[0]["KoeffAlarmPcur"] is System.DBNull)))
                            // �������� �������� ������������
                            tc.m_dcKoeffAlarmPcur = Convert.ToInt32(selRows[0]["KoeffAlarmPcur"]);
                        else
                            ;
                    }
                    else
                        ;
            else
                ;
        }
    }
}
