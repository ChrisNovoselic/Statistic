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

        #region �������������� ������������
        //protected InitTECBase ()
        //{
        //}

        //public InitTECBase(int iListenerId)
        //{
        //    int err = -1;
        //    m_connConfigDB = DbSources.Sources().GetConnection(iListenerId, out err);
        //}
        #endregion

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

        protected DataTable getListTG(int id, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM TG_LIST WHERE ID_TEC" + " = " + id.ToString(), null, null, out err);
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
        // ����������� ��� ������������ ����������� � redmine #1001
        // ����������� ��� ������������ ����������� � redmine #1002
        // ����������� ��� ������������ ����������� � redmine #1003
        /// <summary>
        /// ���������� ��������� ������� "��������� ���� ��"
        /// </summary>
        /// <param name="ver">������ ������ ���������� (0 - ����� �����, 1 - ����������, � �.�.)</param>
        /// <param name="err">������� ������� ������ ��� ����������</param>
        /// <returns>������ ������� � ������������ �������</returns>
        private DataTable getALL_PARAM_TG(int ver, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (" + ver + @")", null, null, out err);
        }
        /// <summary>
        /// ���������� ��������� ������� "��� ��������� ���� �������"
        /// </summary>        
        /// <param name="err">������� ������� ������ ��� ���������� ������</param>
        /// <returns>������-������� � ������������ �������</returns>
        private DataTable getALL_ParamVyvod(int id_tec, out int err)
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

            return DbTSQLInterface.Select(ref m_connConfigDB
                , strQuery
                , null, null, out err);
        }

        /// <summary>
        /// ������ ���� ����������� (���, ���, ��, ��)
        /// </summary>
        /// <param name="idListener">������������� �������������� ���������� � �� ������������</param>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">������-�������� ���������� ��������������� ���</param>
        /// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        public InitTEC_200(int idListener, bool bIgnoreTECInUse, int [] arTECLimit, bool bUseData)
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ����...");

            int err = -1;
            TEC newTECItem = null;

            tec = new ListTEC ();
            //m_user = new Users(idListener);
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ��������� ������� MySqlConnection...");

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            int indx = -1
                //, indx_tec = -1
                ;
            string strLog = string.Empty;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , list_lowPointDev = null
                , all_PARAM_DETAIL = null; // �� �� ������ "������". "�����" ������ ���(��), �������� "������" ������ ��.

            if (err == 0) {
                //�������� ������ ���, ��������� ����������� �������
                list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

                if (err == 0)
                {
                    for (int i = 0; i < list_tec.Rows.Count; i++)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                        if ((HStatisticUsers.allTEC == 0) || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"]))
                            /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                        {
                            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Count = " + tec.Count);

                            //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //�������� ������� ���
                            newTECItem = new TEC(list_tec.Rows[i], bUseData);
                            tec.Add(newTECItem);
                            //indx_tec = tec.Count - 1;

                            EventTECListUpdate += newTECItem/*tec[indx_tec]*/.PerformUpdate;

                            indx = -1;
                            initTECConnectionSettings(i, list_tec.Rows[i]);
                            // �������� �������� �� �� ����� ���������� ���� �������
                            all_PARAM_DETAIL = getALL_PARAM_TG (0, out err);

                            if (err == 0) {
                                #region �������� �� ��� ���
                                list_lowPointDev = getListTG(newTECItem.m_id, out err);

                                if (err == 0)
                                    for (int k = 0; k < list_lowPointDev.Rows.Count; k++)
                                    {
                                        newTECItem/*tec[indx_tec]*/.list_TECComponents.Add(new TECComponent(newTECItem/*tec[indx_tec]*/, list_lowPointDev.Rows[k]));
                                        indx = newTECItem/*tec[indx_tec]*/.list_TECComponents.Count - 1;

                                        newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_listLowPointDev.Add(new TG(list_lowPointDev.Rows[k], all_PARAM_DETAIL.Select(@"ID_TG=" + newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_id)[0]));
                                    }
                                else
                                    ; //������ ��������� ������ ��
                                #endregion

                                #region �������� ���������� ��� (���, ��)
                                for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(c), newTECItem.m_id, out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                            {
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                                newTECItem/*tec[indx_tec]*/.list_TECComponents.Add(new TECComponent(newTECItem/*tec[indx_tec]*/, list_TECComponents.Rows[j]));
                                                indx = newTECItem/*tec[indx_tec]*/.list_TECComponents.Count - 1;

                                                newTECItem/*tec[indx_tec]*/.InitTG(indx
                                                    , all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_id));
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Logging.Logg().Exception(e, "InitTEC::InitTEC (3 ���������) - ...for (int j = 0; j < list_TECComponents.Rows.Count; j++)...", Logging.INDEX_MESSAGE.NOT_SET);
                                        }
                                    else
                                        ; //������ ��� ��������� ������ �����������
                                }
                                #endregion
                            } else
                                ; // ������ ��� ��������� ���������� ��

                            // �������� �������� ���������� ������� �� ����� ���������� ���� �������
                            all_PARAM_DETAIL = getALL_ParamVyvod(-1, out err);

                            if (err == 0)
                            {// �����! ������������� ����������� ���������� ������� ������
                                #region �������� ��������� ������� ��� ���
                                list_lowPointDev = getALL_ParamVyvod(newTECItem.m_id, out err);

                                if (err == 0)
                                    for (int k = 0; k < list_lowPointDev.Rows.Count; k++)
                                    {
                                        newTECItem/*tec[indx_tec]*/.list_TECComponents.Add(new TECComponent(newTECItem/*tec[indx_tec]*/, list_lowPointDev.Rows[k]));
                                        indx = newTECItem/*tec[indx_tec]*/.list_TECComponents.Count - 1;

                                        newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_listLowPointDev.Add(new Vyvod.ParamVyvod(all_PARAM_DETAIL.Select(@"ID=" + newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_id)[0]));
                                    }
                                else
                                    ; //������ ��������� ������ ���������� �������
                                #endregion

                                #region �������� ���������� ��� (������)                            
                                list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(-1), newTECItem.m_id, out err);

                                if (err == 0)
                                    foreach (DataRow r in list_TECComponents.Rows)
                                    {
                                        newTECItem/*tec[indx_tec]*/.list_TECComponents.Add(new Vyvod(newTECItem/*tec[indx_tec]*/, r));
                                        indx = newTECItem/*tec[indx_tec]*/.list_TECComponents.Count - 1;

                                        //???
                                        newTECItem/*tec[indx_tec]*/.InitParamVyvod(-1, all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(-1) + @"=" + newTECItem/*tec[indx_tec]*/.list_TECComponents[indx].m_id));
                                    }
                                else
                                    ; // ������ ��������� ������� �������
                                #endregion
                            }
                            else
                                ; // ������ ��������� ���������� �������

                            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TG = Ok");
                        }
                        else
                            ;
                    } // for i
                }
                else
                    ; //������ ��������� ������ ���
            }
            else
                ; //������ ��������� ���� ���������� ��� ���� ��

            //DbTSQLInterface.CloseConnection(m_connConfigDB, out err);

            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - �����...");
        }
        /// <summary>
        /// ������ ����������� (���, ���, ��, ��) � ~ �� ������� ����������
        /// </summary>
        /// <param name="idListener">������������� �������������� ���������� � �� ������������</param>
        /// <param name="indx">������ ���������� - �������� �� ������������ 'FormChangeMode.MODE_TECCOMPONENT' ('0' ��� > '0' �������� TEC(GTP, PC, TG), '-1' �������� VYVOD)</param>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">������-�������� ���������� ��������������� ���</param>
        /// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        public InitTEC_200(int idListener, Int16 indx, bool bIgnoreTECInUse, int []arTECLimit, bool bUseData) //indx = {GTP ��� PC}
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ����...");

            tec = new ListTEC ();

            int err = 0
                , id_comp = -1
                , indx_comp = -1;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , all_PARAM_DETAIL = null;

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ��������� ������� MySqlConnection...");
            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            //������������� ����������� �������
            list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

            if (!(indx < 0))
                all_PARAM_DETAIL = getALL_PARAM_TG (0, out err); // ����� ����� �����
            else
                all_PARAM_DETAIL = getALL_ParamVyvod(-1, out err); // ��� ���� ���

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �������� ������� ���: " + i);

                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {

                        //�������� ������� ���
                        tec.Add(new TEC(list_tec.Rows[i], bUseData));

                        EventTECListUpdate += tec[i].PerformUpdate;

                        initTECConnectionSettings(i, list_tec.Rows[i]);
                        // �������� ������ �����������, � ������ ���� ����������� �� 'indx'
                        list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                        if (err == 0)
                            if (!(indx < 0))
                            {// ������������� "�������" ����������� ���                            
                                for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                {
                                    id_comp = Convert.ToInt32 (list_TECComponents.Rows[indx_comp][@"ID"]);
                                    tec[i].AddTECComponent(list_TECComponents.Rows[indx_comp]);
                                    tec[i].InitTG(indx_comp, all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(indx) + @"=" + id_comp));
                                }                            
                            }
                            else
                            {// ������������� "��������� �����������" - �������
                                for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                {
                                    id_comp = Convert.ToInt32(list_TECComponents.Rows[indx_comp][@"ID"]);
                                    tec[i].AddTECComponent(list_TECComponents.Rows[indx_comp]);
                                    tec[i].InitParamVyvod(-1, all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(indx) + @"=" + id_comp));
                                }
                            }
                        else
                            ; //������ ???
                }
            else
                ; //������ ��������� ������ ���

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �����...");
        }
        /// <summary>
        /// ������������� ���������� ��� ���������� � �� ���� ���������� ������, ������������ ��� ����� �����������
        /// </summary>
        /// <param name="indx_tec">������ ��� � ������ �������� �������</param>
        /// <param name="rTec">������ ������� [TEC_LIST], ���������� ����������� �������� ����������</param>
        private void initTECConnectionSettings(int indx_tec, DataRow rTec)
        {
            int err = -1
                , idConnSett = -1;
            string strLog = string.Empty;
            DataTable tableConnSett = null;

            foreach (KeyValuePair<CONN_SETT_TYPE, string> pair in TEC.s_dictIdConfigDataSources)
                if ((rTec[pair.Value] is DBNull) == false)
                {
                    idConnSett = Convert.ToInt32(rTec[pair.Value]);
                    tableConnSett = ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, idConnSett, -1, out err);

                    if (err == 0)
                    {
                        err = tec[indx_tec].connSettings(tableConnSett, (int)pair.Key);
                                        
                        switch (err)
                        {
                            case 1:
                                strLog = string.Format(@"������������ {0} ��������� ������ ��� ���� � �������� {1} �� ��������� � �������"
                                    , idConnSett
                                    , pair.Key);
                                break;
                            case -1:
                                strLog = string.Format(@"������ �����, ��� ���� �������� � ��������������� {0} ��� ���� � �������� {1}"
                                    , idConnSett
                                    , pair.Key);
                                break;
                            case -2:
                                strLog = string.Format(@"�� ������ �� ���� �������� ��� ���� � �������� {0}"
                                    , pair.Key);
                                break;
                            default:
                                break;
                        }

                        if (err > 0)
                            Logging.Logg().Warning(@"InitTEC_200::InitTEC_200 () - " + strLog + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            if (strLog.Equals(string.Empty) == false)
                                Logging.Logg().Error(@"InitTEC_200::InitTEC_200 () - " + strLog + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                            else
                                ;
                    }
                    else
                        Logging.Logg().Warning(string.Format(@"InitTEC_200::InitTEC_200 () - " + @"�� ��������������� �������� � ��������������� {0} ��� ���.ID={1}, ���� ��� ���� �� ���������� ������" + @"...", pair.Key, tec[indx_tec].m_id)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    Logging.Logg().Warning(string.Format(@"InitTEC_200::InitTEC_200 () - " + @"�� ���������� ������������� ��������� ������ {0} ��� ���.ID={1}" + @"...", pair.Key, tec[indx_tec].m_id)
                        , Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// ������� ��� ������������� ���������� ���������� ��� ��� ���� ��������� ������� (������ ���)
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

        #region �������������� ������ ��� ���������� ������ ������ ���������
        //public InitTEC_200(List<TEC> listTec)
        //{
        //    tec = new ListTEC ();
        //    tec.AddRange(listTec);

        //    tec.ForEach(getVyvod);
        //}

        ////public InitTEC_200(ListTEC listTec) : this (listTec.)
        ////{
        ////}

        //private void getVyvod (TEC t)
        //{
        //}
        #endregion
    }
}
