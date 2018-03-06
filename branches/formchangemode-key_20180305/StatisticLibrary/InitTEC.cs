using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using ASUTP.Core;
using ASUTP;
using ASUTP.Database;
using ASUTP.Forms;

namespace StatisticCommon
{
    public partial class DbTSQLConfigDatabase
    {
        public class ListTEC : List<TEC>
        {
            public ListTEC ()
                : base ()
            {
            }

            private void update ()
            {
            }
        }

        //private static ListTEC _tec;

        //public ListTEC CopyTEC
        //{
        //    get
        //    {
        //        ListTEC tecRes = new ListTEC ();

        //        _tec.ForEach (t => {
        //            tecRes.Add (new TEC(t));
        //        });

        //        return tecRes;
        //    }
        //}

        private ListTEC _listTEC;

        ///// <summary>
        ///// ������ ���� ����������� (���, ���, ��, ��)
        ///// </summary>
        ///// <param name="connSett">��������� ���������� � �� ������������</param>
        ///// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        ///// <param name="arTECLimit">������-�������� ���������� ��������������� ���</param>
        ///// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        //public List<TEC> InitTEC (ConnectionSettings connSett, bool bIgnoreTECInUse, int [] arTECLimit, bool bUseData)
        //{
        //    SetConnectionSettings (connSett);

        //    return InitTEC (bIgnoreTECInUse, arTECLimit, bUseData);
        //}

        /// <summary>
        /// ������ ���� ����������� (���, ���, ��, ��)
        /// </summary>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">������-�������� ���������� ��������������� ���</param>
        /// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        public List<TEC> InitTEC (bool bIgnoreTECInUse, int [] arTECLimit, bool bUseData)
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ����...");

            ListTEC tecRes;

#if MODE_STATIC_CONNECTION_LEAVING
            ModeStaticConnectionLeave = ModeStaticConnectionLeaving.Yes;
#endif
            int err = -1;
            TEC newTECItem = null;
            TECComponent newTECComp;

            tecRes = new ListTEC ();
            //if (Equals(_tec, null) == true)
            //    _tec = new ListTEC ();
            //else {
            //    return CopyTEC;
            //}

            int indx = -1
                //, indx_tec = -1
                ;
            string strLog = string.Empty;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , list_lowPointDev = null
                , all_PARAM_DETAIL = null; // �� �� ������ "������". "�����" ������ ���(��), �������� "������" ������ ��.

            try
            {
                //�������� ������ ���, ��������� ����������� �������
                list_tec = GetListTEC(bIgnoreTECInUse, arTECLimit, out err);

                if (err == 0)
                {
                    for (int i = 0; i < list_tec.Rows.Count; i++)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                        if ((HStatisticUsers.allTEC == 0)
                            || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"])
                            /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                            )
                        {
                            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Count = " + tec.Count);

                            //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //�������� ������� ���
                            newTECItem = new TEC(list_tec.Rows[i], bUseData);
                            tecRes.Add(newTECItem);
                            //indx_tec = tec.Count - 1;

                            EventTECListUpdate += newTECItem/*tec[indx_tec]*/.PerformUpdate;

                            indx = -1;
                            initTECConnectionSettings(newTECItem, list_tec.Rows[i]);
                            // �������� �������� �� �� ����� ���������� ���� �������
                            all_PARAM_DETAIL = getALL_PARAM_TG(0, out err);

                            if (err == 0)
                            {
#region �������� �� ��� ���
                                try
                                {
                                    newTECItem.InitTG (all_PARAM_DETAIL.Select (@"ID_TEC=" + newTECItem.m_id), out err);
                                } catch (Exception e) {
                                    Logging.Logg().Exception(e, @"InitTEC_200.ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                }
#endregion

#region �������� ���������� ��� (���, ��)
                                for (FormChangeMode.MODE_TECCOMPONENT c = FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(c), newTECItem.m_id, out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                            {
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                                newTECComp = new TECComponent (newTECItem, list_TECComponents.Rows [j]);

                                                newTECItem.AddTECComponent(newTECComp);
                                                indx = newTECItem.ListTECComponents.Count - 1;

                                                newTECItem.InitTG(newTECComp.m_id
                                                    , all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + newTECComp.m_id));
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
                            }
                            else
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
                                        newTECItem.AddTECComponent (new TECComponent(newTECItem/*tec[indx_tec]*/, list_lowPointDev.Rows[k]));
                                        indx = newTECItem.ListTECComponents.Count - 1;

                                        newTECItem.ListTECComponents[indx].AddLowPointDev(new Vyvod.ParamVyvod(all_PARAM_DETAIL.Select($"ID={newTECItem.ListTECComponents[indx].m_id}") [0]));
                                    }
                                else
                                    ; //������ ��������� ������ ���������� �������
#endregion

#region �������� ���������� ��� (������)
                                list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(FormChangeMode.MODE_TECCOMPONENT.VYVOD), newTECItem.m_id, out err);

                                if (err == 0)
                                    foreach (DataRow r in list_TECComponents.Rows)
                                    {
                                        newTECItem.AddTECComponent (new TECComponent(newTECItem, r));
                                        indx = newTECItem.ListTECComponents.Count - 1;

                                        //???
                                        newTECItem.InitParamVyvod(-1, all_PARAM_DETAIL.Select($"ID_{FormChangeMode.getPrefixMode(FormChangeMode.MODE_TECCOMPONENT.VYVOD)}={newTECItem.ListTECComponents[indx].m_id}"));
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
            catch (Exception e) {
                Logging.Logg().Exception(e, "������ ��������� ���������� ��� ���� ��", Logging.INDEX_MESSAGE.NOT_SET);
            }
#if MODE_STATIC_CONNECTION_LEAVING
            ModeStaticConnectionLeave = ModeStaticConnectionLeaving.No;
#endif
            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - �����...");

            return tecRes;
        }

        /// <summary>
        /// ������ ����������� (���, ���, ��, ��) � ~ �� ������� ����������
        /// </summary>
        /// <param name="idListener">������������� �������������� ���������� � �� ������������</param>
        /// <param name="mode">������ ���������� - �������� �� ������������ 'FormChangeMode.MODE_TECCOMPONENT' ('0' ��� > '0' �������� TEC(GTP, PC, TG), '-1' �������� VYVOD)</param>
        /// <param name="bIgnoreTECInUse">������� ������������� ���� [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">������-�������� ���������� ��������������� ���</param>
        /// <param name="bUseData">������� ����������� ��������� � ������ ����������� ����������� ������</param>
        public List<TEC> InitTEC(FormChangeMode.MODE_TECCOMPONENT mode, bool bIgnoreTECInUse, int []arTECLimit, bool bUseData) //indx = {GTP ��� PC}
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - ����...");

            ListTEC tecRes = new ListTEC ();

            int err = 0
                , id_comp = -1
                , indx_comp = -1;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , all_PARAM_DETAIL = null;

            //������������� ����������� �������
            list_tec = GetListTEC(bIgnoreTECInUse, arTECLimit, out err);
            //??? ��� ������ < 0, mode.Unknown < 0!!!
            if ((!(mode == FormChangeMode.MODE_TECCOMPONENT.VYVOD))
                && (!(mode == FormChangeMode.MODE_TECCOMPONENT.Unknown)))
                all_PARAM_DETAIL = getALL_PARAM_TG (0, out err); // ����� ����� �����
            else
                all_PARAM_DETAIL = getALL_ParamVyvod(-1, out err); // ��� ���� ���

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �������� ������� ���: " + i);

                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {

                        //�������� ������� ���
                        tecRes.Add(new TEC(list_tec.Rows[i], bUseData));

                        EventTECListUpdate += tecRes[i].PerformUpdate;

                        initTECConnectionSettings(tecRes [i], list_tec.Rows[i]);
                        // �������� ������ �����������, � ������ ���� ����������� �� 'indx'
                        list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(mode), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                        if (err == 0) {
                        //??? ��� ������ < 0, mode.Unknown < 0!!!
                            if (!(mode < 0))
                            {// ������������� "�������" ����������� ���
                                for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                {
                                    id_comp = Convert.ToInt32 (list_TECComponents.Rows[indx_comp][@"ID"]);

                                    #region �������� �� ��� ���
                                    //try
                                    //{
                                    //    initTG (tecRes [i], all_PARAM_DETAIL.Select ($@"ID_TEC={tecRes [i].m_id} AND ID_{FormChangeMode.getPrefixMode (mode)}={id_comp}"), out err);
                                    //} catch (Exception e) {
                                    //    Logging.Logg().Exception(e, @"InitTEC_200.ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                    //}
                                    #endregion

                                    tecRes[i].AddTECComponent(list_TECComponents.Rows[indx_comp]);
                                    tecRes[i].InitTG(id_comp, all_PARAM_DETAIL.Select($@"ID_{FormChangeMode.getPrefixMode(mode)}={id_comp}"));
                                }
                            }
                            else
                            {// ������������� "��������� �����������" - �������
                                for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                {
                                    id_comp = Convert.ToInt32(list_TECComponents.Rows[indx_comp][@"ID"]);
                                    tecRes[i].AddTECComponent(list_TECComponents.Rows[indx_comp]);
                                    tecRes[i].InitParamVyvod(-1, all_PARAM_DETAIL.Select($"ID_{FormChangeMode.getPrefixMode(mode)}={id_comp}"));
                                }
                            }
                        } else
                            ; //������ ???
                }
            else
                ; //������ ��������� ������ ���

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �����...");

            return tecRes;
        }
        /// <summary>
        /// ������������� ���������� ��� ���������� � �� ���� ���������� ������, ������������ ��� ����� �����������
        /// </summary>
        /// <param name="indx_tec">������ ��� � ������ �������� �������</param>
        /// <param name="rTec">������ ������� [TEC_LIST], ���������� ����������� �������� ����������</param>
        private void initTECConnectionSettings(TEC tec, DataRow rTec)
        {
            int err = -1
                , idConnSett = -1;
            string strLog = string.Empty;
            DataTable tableConnSett = null;

            foreach (KeyValuePair<CONN_SETT_TYPE, string> pair in TEC.s_dictIdConfigDataSources)
                if ((rTec[pair.Value] is DBNull) == false)
                {
                    idConnSett = Convert.ToInt32(rTec[pair.Value]);
                    tableConnSett = DbTSQLConfigDatabase.DbConfig().GetDataTableConnSettingsOfIdSource (idConnSett, -1, out err);

                    if (err == 0)
                    {
                        err = tec.connSettings(tableConnSett, (int)pair.Key);

                        switch (err)
                        {
                            case 1:
                                strLog = string.Format(@"������������ <{0}> ��������� ������ ��� ���� � �������� <{1}> �� ��������� � �������"
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
                            Logging.Logg().Warning(@"DbTSQLConfigureDatabase::initTECConnectionSettings () - " + strLog + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            if (strLog.Equals(string.Empty) == false)
                                Logging.Logg().Error(@"DbTSQLConfigureDatabase::initTECConnectionSettings () - " + strLog + @"...", Logging.INDEX_MESSAGE.NOT_SET);
                            else
                                ;
                    }
                    else
                        Logging.Logg().Warning(string.Format(@"DbTSQLConfigureDatabase::initTECConnectionSettings () - " + @"�� ��������������� �������� � ��������������� {0} ��� ���.ID={1}, ���� ��� ���� �� ���������� ������" + @"...", pair.Key, tec.m_id)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    Logging.Logg().Warning(string.Format(@"DbTSQLConfigureDatabase::initTECConnectionSettings () - " + @"�� ���������� ������������� ��������� ������ {0} ��� ���.ID={1}" + @"...", pair.Key, tec.m_id)
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

        public void OnTECUpdate (object obj, EventArgs ev)
        {
            TEC tec = obj as TEC;
            int iListenerId = (ev as TECListUpdateEventArgs).m_iListenerId
                , err = -1;
            string strMesError = string.Empty;
            DataTable tableRes;
            DataRow []selRows;
            DbConnection connConfigDB;

            try {
                connConfigDB = DbSources.Sources().GetConnection(iListenerId, out err);

                if (err == 0) {
                    //tableRes = getListTEC(ref connConfigDB, true, new int[] { 0, (int)TECComponent.ID.GTP }, out err);
                    ////??? ���������� ���������� ��� (��������: m_IdSOTIASSOLinkSourceTM)
                    //tec.Update (tableRes);

                    tableRes = GetListTECComponent(ref connConfigDB, @"GTP", tec.m_id, out err);
                    // ���������� ���������� ���
                    if (err == 0) {
                        err = tableRes.Columns.IndexOf ("KoeffAlarmPcur") > 0 ? 0 : -2; // ����� ������������ �������� "-1"

                        if (err == 0)
                            // ����� ���
                            foreach (TECComponent tc in tec.ListTECComponents)
                                if (tc.IsGTP == true) {
                                    selRows = tableRes.Select (@"ID=" + tc.m_id);
                                    // ��������� ������� ��������
                                    if ((selRows.Length == 1)
                                        && (!(selRows [0] ["KoeffAlarmPcur"] is System.DBNull)))
                                        // �������� �������� ������������
                                        tc.m_dcKoeffAlarmPcur = Convert.ToInt32 (selRows [0] ["KoeffAlarmPcur"]);
                                    else
                                        ;
                                } else
                                    ;
                        else
                            strMesError = "�������. ����. �� �������� ���� [KoeffAlarmPcur]";
                    } else
                        //strMesError = "�� ������� �������� ������� - ������ ����������� ���"
                        throw new InvalidOperationException (string.Format (@"DbTSQLConfigureDatabase::OnTECUpdate (ID={0}, NAME={1}) - {2} ..."
                            , (obj as TEC).m_id, (obj as TEC).name_shr, @"�� ������� �������� ������� - ������ ����������� ���"))
                            ;
                } else
                    strMesError = "�� ������� �������� ������ ���������� � �� ������������";
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"DbTSQLConfigureDatabase::OnTECUpdate (ID={0}, NAME={1}) - ...", (obj as TEC).m_id, (obj as TEC).name_shr), Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (err < 0)
                Logging.Logg ().Error (string.Format (@"DbTSQLConfigureDatabase::OnTECUpdate (ID={0}, NAME={1}) - {2} ..."
                    , (obj as TEC).m_id, (obj as TEC).name_shr, strMesError), Logging.INDEX_MESSAGE.NOT_SET);
            else
                ;
        }
    }
}
