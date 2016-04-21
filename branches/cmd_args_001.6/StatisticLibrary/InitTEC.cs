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
        //protected TYPE_DATABASE_CFG m_typeDB_CFG { get { return this is InitTEC_200 ? TYPE_DATABASE_CFG.CFG_200 : this is InitTEC_190 ? TYPE_DATABASE_CFG.CFG_190 : TYPE_DATABASE_CFG.UNKNOWN; } }
        
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

        /// <summary>
        /// ���������� ������ ������� ��� ��������� ������ ���
        /// </summary>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [InUse] � ������� [TEC_LIST]</param>
        /// <param name="arIdLimits">�������� ��������������� ���</param>
        /// <returns>������ �������</returns>
        public static string getQueryListTEC(bool bIgnoreTECInUse, int[] arIdLimits)
        {
            string strRes = "SELECT * FROM TEC_LIST ";

            if (bIgnoreTECInUse == false)
                strRes += "WHERE INUSE=1 ";
            else
                ;

            if (bIgnoreTECInUse == true)
            // ������� ��� �� ��������� - ���������
                strRes += @"WHERE ";
            else
                if (bIgnoreTECInUse == false)
                // ������� ��� ���������
                    strRes += @"AND ";
                else
                    ;

            if (!(HStatisticUsers.allTEC == 0))
            {
                strRes += @"ID=" + HStatisticUsers.allTEC.ToString();
            }
            else
                //??? ����������� (���������) ��� ��
                strRes += @"NOT ID<" + arIdLimits[0] + @" AND NOT ID>" + arIdLimits[1];

            return strRes;
        }
        /// <summary>
        /// ���������� ������� [TEC_LIST] �� �� ������������
        /// </summary>
        /// <param name="connConfigDB">������ �� ������ � ������������� ����������� � ��</param>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [InUse] � ������� [TEC_LIST]</param>
        /// <param name="arIdLimits">�������� ��������������� ���</param>
        /// <param name="err">������������� ������ ��� ���������� �������</param>
        /// <returns>������� - � �������</returns>
        public static DataTable getListTEC(ref DbConnection connConfigDB, bool bIgnoreTECInUse, int []arIdLimits, out int err)
        {
            string req = getQueryListTEC(bIgnoreTECInUse, arIdLimits);

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

        public DataTable getConnSettingsOfIdSource(int id)
        {
            int err = 0;

            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM SOURCE WHERE ID = " + id.ToString(), null, null, out err);
        }
    }

    public class InitTEC_200 : InitTECBase
    {
        private DataTable getALL_PARAM_TG(int ver, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (" + ver + @")", null, null, out err);
        }

        /// <summary>
        /// ������ ���� ����������� (���, ���, ��, ��)
        /// </summary>
        /// <param name="idListener">������������� �������������� ���������� � �� ������������</param>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        /// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        public InitTEC_200(int idListener, bool bIgnoreTECInUse, int [] arTECLimit, bool bUseData)
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

            if (err == 0) {
                //�������� ������ ���, ��������� ����������� �������
                list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

                if (err == 0)
                    for (int i = 0; i < list_tec.Rows.Count; i++)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                        if ((HStatisticUsers.allTEC == 0) || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"]))
                            /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                        {
                            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Count = " + tec.Count);

                            //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //�������� ������� ���
                            tec.Add(new TEC(list_tec.Rows[i], bUseData));

                            int indx_tec = tec.Count - 1;
                            EventTECListUpdate += tec[indx_tec].PerformUpdate;

                            int indx = -1;
                            tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_AISKUE);
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO); else ;
                            //if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN); else ;
                            //if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN); else ;
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                            if (err == 0) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;
                            if ((err == 0) && ((list_tec.Rows[i]["ID_SOURCE_MTERM"] is DBNull) == false)) tec[indx_tec].connSettings(getConnSettingsOfIdSource(Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_MTERM"]), -1, out err), (int)CONN_SETT_TYPE.MTERM); else ;

                            if (err == 0)
                            {
                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Add () = Ok");

                                list_tg = getListTG(FormChangeMode.getPrefixMode((int)FormChangeMode.MODE_TECCOMPONENT.TEC), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                if (err == 0)
                                    for (int k = 0; k < list_tg.Rows.Count; k++)
                                    {
                                        tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], list_tg.Rows[k]));

                                        indx = tec[indx_tec].list_TECComponents.Count - 1;

                                        tec[indx_tec].list_TECComponents[indx].m_listTG.Add(new TG(list_tg.Rows[k], all_PARAM_TG.Select(@"ID_TG=" + tec[indx_tec].list_TECComponents[indx].m_id)[0]));
                                    }
                                else
                                    ; //������ ��������� ������ ��

                                for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                            {
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                                tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], list_TECComponents.Rows[j]));

                                                indx = tec[indx_tec].list_TECComponents.Count - 1;

                                                if (err == 0)
                                                    tec[indx_tec].InitTG(indx, all_PARAM_TG.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + tec[indx_tec].list_TECComponents[indx].m_id));
                                                //InitTG(tec[indx_tec].list_TECComponents, indx, all_PARAM_TG.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + tec[indx_tec].list_TECComponents[indx].m_id));
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
                else
                    ; //������ ��������� ������ ���
            }
            else
                ; //������ ��������� ���� ���������� ��� ���� ��

            //DbTSQLInterface.CloseConnection(m_connConfigDB, out err);

            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - �����...");
        }

        public InitTEC_200(int idListener, Int16 indx, bool bIgnoreTECInUse, int []arTECLimit, bool bUseData) //indx = {GTP ��� PC}
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ����...");

            tec = new ListTEC ();

            int err = 0;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null,
                    list_tg = null
                    , all_PARAM_TG = null;

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ��������� ������� MySqlConnection...");
            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            //������������� ����������� �������
            list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

            all_PARAM_TG = getALL_PARAM_TG (0, out err);

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �������� ������� ���: " + i);

                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {

                        //�������� ������� ���
                        tec.Add(new TEC(list_tec.Rows[i], bUseData));

                        EventTECListUpdate += tec[i].PerformUpdate;

                        tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA"]), -1, out err), (int)CONN_SETT_TYPE.DATA_AISKUE);
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO); else ;
                        //if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(m_typeDB_CFG, ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN); else ;
                        //if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(m_typeDB_CFG, ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA_TM"]), -1, out err), (int)CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN); else ;
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"]), -1, out err), (int)CONN_SETT_TYPE.ADMIN); else ;
                        if (err == 0) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"]), -1, out err), (int)CONN_SETT_TYPE.PBR); else ;
                        if ((err == 0) && ((list_tec.Rows[i]["ID_SOURCE_MTERM"] is DBNull) == false)) tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_MTERM"]), -1, out err), (int)CONN_SETT_TYPE.MTERM); else ;

                        if (err == 0) list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32 (list_tec.Rows[i]["ID"]), out err); else ;

                        if (err == 0)
                            for (int j = 0; j < list_TECComponents.Rows.Count; j ++) {
                                tec[i].list_TECComponents.Add(new TECComponent(tec[i], list_TECComponents.Rows[j]));

                                if (err == 0)
                                    tec[i].InitTG (j, all_PARAM_TG.Select (@"ID_" + FormChangeMode.getPrefixMode(indx) + @"=" + tec[i].list_TECComponents[indx]));
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

            tableRes = getListTEC(ref connConfigDB, true, new int[] { 0, (int)TECComponent.ID.GTP }, out err);
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
