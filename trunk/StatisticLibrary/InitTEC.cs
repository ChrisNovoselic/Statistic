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

            tecRes = new ListTEC ();
            //if (Equals(_tec, null) == true)
            //    _tec = new ListTEC ();
            //else {
            //    return CopyTEC;
            //}

            string strLog = string.Empty;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , all_PARAM_DETAIL = null; // �� �� ������ "������". "�����" ������ ���(��), �������� "������" ������ ��.

            try
            {
                //�������� ������ ���, ��������� ����������� �������
                list_tec = GetListTEC(bIgnoreTECInUse, arTECLimit, out err);

                if (err == 0)
                {
                    for (int indx_tec = 0; indx_tec < list_tec.Rows.Count; indx_tec++)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                        if ((HStatisticUsers.allTEC == 0)
                            || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[indx_tec]["ID"])
                            /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                            )
                        {
                            //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - tec.Count = " + tec.Count);

                            //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //�������� ������� ���
                            newTECItem = new TEC(list_tec.Rows[indx_tec], bUseData);
                            tecRes.Add(newTECItem);
                            //indx_tec = tec.Count - 1;

                            EventTECListUpdate += newTECItem/*tec[indx_tec]*/.PerformUpdate;

                            initTECConnectionSettings(newTECItem, list_tec.Rows[indx_tec]);
                            // �������� �������� �� �� ����� ���������� ���� �������
                            all_PARAM_DETAIL = getALL_PARAM_TG(0, out err);

                            if (err == 0)
                            {
#region �������� ���������� ��� (���, ��)
                                for (FormChangeMode.MODE_TECCOMPONENT c = FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(c), newTECItem.m_id, out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 ���������) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            initTECComponents (newTECItem, c, list_TECComponents, all_PARAM_DETAIL);
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
                                list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(FormChangeMode.MODE_TECCOMPONENT.VYVOD), newTECItem.m_id, out err);

                                if (err == 0)
                                    initTECComponents (newTECItem, FormChangeMode.MODE_TECCOMPONENT.VYVOD, list_TECComponents, all_PARAM_DETAIL);
                                else
                                    ; // ������ ��������� ������� �������
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

            int err = 01;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , all_PARAM_DETAIL = null;
            TEC newTECItem;

            //������������� ����������� �������
            list_tec = GetListTEC(bIgnoreTECInUse, arTECLimit, out err);
            //??? ��� ������ < 0, mode.Unknown < 0!!!
            if ((!(mode == FormChangeMode.MODE_TECCOMPONENT.VYVOD))
                && (!(mode == FormChangeMode.MODE_TECCOMPONENT.Unknown)))
                all_PARAM_DETAIL = getALL_PARAM_TG (0, out err); // ����� ����� �����
            else
                all_PARAM_DETAIL = getALL_ParamVyvod(-1, out err); // ��� ���� ���

            if (err == 0)
                for (int indx_tec = 0; indx_tec < list_tec.Rows.Count; indx_tec ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �������� ������� ���: " + i);

                    //�������� ������� ���
                    newTECItem =  new TEC(list_tec.Rows[indx_tec], bUseData);
                    tecRes.Add(newTECItem);

                    EventTECListUpdate += newTECItem.PerformUpdate;

                    initTECConnectionSettings(newTECItem, list_tec.Rows[indx_tec]);
                    // �������� ������ �����������, � ������ ���� ����������� �� 'indx'
                    list_TECComponents = GetListTECComponent(FormChangeMode.getPrefixMode(mode), Convert.ToInt32(list_tec.Rows[indx_tec]["ID"]), out err);

                    if (err == 0) {
                    //??? ��� ������ < 0, mode.Unknown < 0!!!
                        initTECComponents (newTECItem, mode, list_TECComponents, all_PARAM_DETAIL);
                    } else
                        ; //������ ???
                }
            else
                ; //������ ��������� ������ ���

            //Logging.Logg().Debug("InitTEC::InitTEC (4 ���������) - �����...");

            return tecRes;
        }

        private void initTECComponents (TEC tec, FormChangeMode.MODE_TECCOMPONENT mode, DataTable list_TECComponents, DataTable all_PARAM_Detail)
        {
            int id_comp = -1
                , indx_comp = -1;
            TECComponent newTECComp;

            for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++) {
                newTECComp = new TECComponent (tec, list_TECComponents.Rows [indx_comp]);
                id_comp = newTECComp.m_id;
                tec.AddTECComponent (newTECComp);
                if (!(mode < 0)) // ������������� "�������" ����������� ���
                    tec.InitTG (id_comp, all_PARAM_Detail.Select ($@"ID_{FormChangeMode.getPrefixMode (mode)}={id_comp}"));
                else
                    tec.InitParamVyvod (newTECComp, all_PARAM_Detail.Select ($"ID_{FormChangeMode.getPrefixMode (mode)}={id_comp}"));
            }
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

            Action<string, Logging.INDEX_MESSAGE, bool> logging;

            foreach (KeyValuePair<CONN_SETT_TYPE, string> pair in TEC.s_dictIdConfigDataSources) {
                logging = Logging.Logg ().Warning;
                err = (rTec [pair.Value] is DBNull) == false ? 0 : -1;

                if (err == 0)
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

                        if (!(err > 0))
                            if (strLog.Equals (string.Empty) == false)
                                logging = Logging.Logg ().Error;
                            else
                                ;
                        else
                            ;
                    }
                    else
                        strLog = string.Format(@"�� ��������������� �������� � ��������������� {0} ��� ���.ID={1}, ���� ��� ���� �� ���������� ������" + @"...", pair.Key, tec.m_id);
                }
                else
                    strLog = string.Format (@"�� ���������� ������������� ��������� ������ {0} ��� ���.ID={1}" + @"...", pair.Key, tec.m_id);

                if (strLog.Equals (string.Empty) == false)
                    logging ($"DbTSQLConfigureDatabase::initTECConnectionSettings () - {strLog}...", Logging.INDEX_MESSAGE.NOT_SET, true);
                else
                    ;
            }
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
