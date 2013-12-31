using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace StatisticCommon
{
    public class InitTEC
    {
        public List<TEC> tec;

        public static DataTable getListTEC(ConnectionSettings connSett, bool bIgnoreTECInUse)
        {
            int err = 0;

            string req = string.Empty;
            req = "SELECT * FROM TEC_LIST";

            if (!bIgnoreTECInUse) req += " WHERE INUSE=TRUE"; else ;

            return DbTSQLInterface.Select(connSett, req, out err);
        }

        public static DataTable getListTECComponent(ConnectionSettings connSett, string prefix, int id_tec)
        {
            int err = 0;

            return DbTSQLInterface.Select(connSett, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString(), out err);
        }

        public static DataTable getListTG(ConnectionSettings connSett, string prefix, int id)
        {
            int err = 0;

            return DbTSQLInterface.Select(connSett, "SELECT * FROM TG_LIST WHERE ID_" + prefix + " = " + id.ToString(), out err);
        }

        public static DataTable getConnSettingsOfIdSource(ConnectionSettings connSett, int id)
        {
            int err = 0;

            return DbTSQLInterface.Select(connSett, "SELECT * FROM SOURCE WHERE ID = " + id.ToString(), out err);
        }

        private List<int> getMCId(DataTable data, int row)
        {
            int i = -1;
            List<int> listRes = new List<int>();

            if ((!(data.Columns.IndexOf("ID_MC") < 0)) && (!(data.Rows[row]["ID_MC"] is DBNull)))
            {
                string[] ids = data.Rows[row]["ID_MC"].ToString().Split(',');
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

        //������ ���� ����������� (���, ���, ��, ��)
        public InitTEC(ConnectionSettings connSett, bool bIgnoreTECInUse, bool bUseData)
        {
            tec = new List<TEC>();

            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null, list_tg = null;

            //������������� ����������� �������
            list_tec = getListTEC(connSett, bIgnoreTECInUse);

            //Logging.Logg ().LogLock ();
            //Logging.Logg().LogToFile("InitTEC::InitTEC () - m_user.Role = " + m_user.Role, true, false, false);
            //Logging.Logg().LogUnlock();

            for (int i = 0; i < list_tec.Rows.Count; i++)
            {
                //Logging.Logg().LogLock();
                //Logging.Logg().LogToFile("InitTEC::InitTEC () - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"], true, false, false);
                //Logging.Logg().LogUnlock();

                //if ((m_user.allTEC == 0) || (m_user.Role < 100) || (m_user.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"])))
                //{
                    //Logging.Logg().LogLock();
                    //Logging.Logg().LogToFile("InitTEC::InitTEC () - tec.Count = " + tec.Count, true, false, false);
                    //Logging.Logg().LogUnlock();

                    //�������� ������� ���
                    tec.Add(new TEC(Convert.ToInt32(list_tec.Rows[i]["ID"]),
                                    list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                    list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                    list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                    list_tec.Rows[i]["PREFIX_ADMIN"].ToString(),
                                    list_tec.Rows[i]["PREFIX_PBR"].ToString(),
                                    bUseData));

                    //List <string> listNamesField;
                    //listNamesField = new List<string> ();
                    //listNamesField.Add ();

                    int indx_tec = tec.Count - 1;
                    tec[indx_tec].SetNamesField(list_tec.Rows[i]["ADMIN_DATETIME"].ToString(),
                                        list_tec.Rows[i]["ADMIN_REC"].ToString(),
                                        list_tec.Rows[i]["ADMIN_IS_PER"].ToString(),
                                        list_tec.Rows[i]["ADMIN_DIVIAT"].ToString(),
                                        list_tec.Rows[i]["PBR_DATETIME"].ToString(),
                                        list_tec.Rows[i]["PPBRvsPBR"].ToString(),
                                        list_tec.Rows[i]["PBR_NUMBER"].ToString());

                    tec[indx_tec].connSettings(getConnSettingsOfIdSource(connSett, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_DATA"])), (int)CONN_SETT_TYPE.DATA);
                    tec[indx_tec].connSettings(getConnSettingsOfIdSource(connSett, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_ADMIN"])), (int)CONN_SETT_TYPE.ADMIN);
                    tec[indx_tec].connSettings(getConnSettingsOfIdSource(connSett, Convert.ToInt32(list_tec.Rows[i]["ID_SOURCE_PBR"])), (int)CONN_SETT_TYPE.PBR);

                    tec[indx_tec].m_timezone_offset_msc = Convert.ToInt32(list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                    tec[indx_tec].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();

                    //Logging.Logg().LogLock();
                    //Logging.Logg().LogToFile("InitTEC::InitTEC () - tec.Add () = Ok", true, false, false);
                    //Logging.Logg().LogUnlock();

                    int indx = -1;
                    for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                    {
                        list_TECComponents = getListTECComponent(connSett, FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_tec.Rows[i]["ID"]));

                        //Logging.Logg().LogLock();
                        //Logging.Logg().LogToFile("InitTEC::InitTEC () - list_TECComponents.Count = " + list_TECComponents.Rows.Count, true, false, false);
                        //Logging.Logg().LogUnlock();

                        for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                        {
                            //indx = (c - (int)FormChangeMode.MODE_TECCOMPONENT.GTP) * ((int)(FormChangeMode.MODE_TECCOMPONENT.PC - FormChangeMode.MODE_TECCOMPONENT.GTP + 1)) + j;

                            tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], list_TECComponents.Rows[j]["PREFIX_ADMIN"].ToString(), list_TECComponents.Rows[j]["PREFIX_PBR"].ToString()));

                            indx = tec[indx_tec].list_TECComponents.Count - 1;

                            tec[indx_tec].list_TECComponents[indx].name = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                            tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_TECComponents.Rows[j]["ID"]);
                            tec[indx_tec].list_TECComponents[indx].m_listMCId = getMCId(list_TECComponents, j);

                            list_tg = getListTG(connSett, FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]));

                            for (int k = 0; k < list_tg.Rows.Count; k++)
                            {
                                tec[indx_tec].list_TECComponents[indx].TG.Add(new TG(tec[indx_tec].list_TECComponents[indx]));
                                tec[indx_tec].list_TECComponents[indx].TG[k].name = list_tg.Rows[k]["NAME_SHR"].ToString();
                                tec[indx_tec].list_TECComponents[indx].TG[k].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);
                                if (!(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                                    tec[indx_tec].list_TECComponents[indx].TG[k].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                                else
                                    ;
                            }
                        }
                    }

                    //Logging.Logg().LogLock();
                    //Logging.Logg().LogToFile("InitTEC::InitTEC () - list_TECComponents = Ok", true, false, false);
                    //Logging.Logg().LogUnlock();

                    list_tg = getListTG(connSett, FormChangeMode.getPrefixMode((int)FormChangeMode.MODE_TECCOMPONENT.TEC), Convert.ToInt32(list_tec.Rows[i]["ID"]));

                    for (int k = 0; k < list_tg.Rows.Count; k++)
                    {
                        tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], null, null));

                        indx = tec[indx_tec].list_TECComponents.Count - 1;

                        tec[indx_tec].list_TECComponents[indx].name = list_tg.Rows[k]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                        tec[indx_tec].list_TECComponents[indx].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);

                        tec[indx_tec].list_TECComponents[indx].TG.Add(new TG(new TECComponent(tec[indx_tec], null, null)));
                        tec[indx_tec].list_TECComponents[indx].TG[0].name = list_tg.Rows[k]["NAME_SHR"].ToString();
                        tec[indx_tec].list_TECComponents[indx].TG[0].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);
                        if (!(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                            tec[indx_tec].list_TECComponents[indx].TG[0].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                        else
                            ;
                    }

                    //Logging.Logg().LogLock();
                    //Logging.Logg().LogToFile("InitTEC::InitTEC () - list_TG = Ok", true, false, false);
                    //Logging.Logg().LogUnlock();
                //}
                //else
                //    ;
            }

            //Logging.Logg().LogLock();
            //Logging.Logg().LogToFile("InitTEC::InitTEC () - exit...", true, false, false);
            //Logging.Logg().LogUnlock();
        }

        public InitTEC(ConnectionSettings connSett, Int16 indx, bool bIgnoreTECInUse, bool bUseData) //indx = {GTP ��� PC}
        {
            tec = new List<TEC>();

            int err = 0;
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec = null, // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null, list_tg = null;

            //������������� ������� �������
            //int listenerId = -1;
            //bool err = false;
            //DbInterface dbInterface = new DbInterface (DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL, 1);
            //listenerId = dbInterface.ListenerRegister();
            //dbInterface.Start ();

            //dbInterface.SetConnectionSettings(connSett);

            //DbTSQLInterface.Select(listenerId, "SELECT * FROM TEC_LIST");
            //dbInterface.GetResponse(listenerId, out err, out list_tec);

            //dbInterface.Stop();
            //dbInterface.ListenerUnregister(listenerId);

            //������������� ����������� �������
            list_tec = getListTEC(connSett, bIgnoreTECInUse);

            for (int i = 0; i < list_tec.Rows.Count; i++)
            {
                //�������� ������� ���
                tec.Add(new TEC(Convert.ToInt32(list_tec.Rows[i]["ID"]),
                                list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                list_tec.Rows[i]["PREFIX_ADMIN"].ToString(),
                                list_tec.Rows[i]["PREFIX_PBR"].ToString(),
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

                tec[i].connSettings(DbTSQLInterface.Select(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_DATA"].ToString(), out err), (int)CONN_SETT_TYPE.DATA);
                tec[i].connSettings(DbTSQLInterface.Select(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_ADMIN"].ToString(), out err), (int)CONN_SETT_TYPE.ADMIN);
                tec[i].connSettings(DbTSQLInterface.Select(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_PBR"].ToString(), out err), (int)CONN_SETT_TYPE.PBR);

                tec[i].m_timezone_offset_msc = Convert.ToInt32(list_tec.Rows[i]["TIMEZONE_OFFSET_MOSCOW"]);
                tec[i].m_path_rdg_excel = list_tec.Rows[i]["PATH_RDG_EXCEL"].ToString();

                list_TECComponents = getListTECComponent(connSett, FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_tec.Rows[i]["ID"]));
                for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                {
                    tec[i].list_TECComponents.Add(new TECComponent(tec[i], list_TECComponents.Rows[j]["PREFIX_ADMIN"].ToString(), list_TECComponents.Rows[j]["PREFIX_PBR"].ToString()));
                    tec[i].list_TECComponents[j].name = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                    tec[i].list_TECComponents[j].m_id = Convert.ToInt32(list_TECComponents.Rows[j]["ID"]);
                    tec[i].list_TECComponents[j].m_listMCId = getMCId(list_TECComponents, j);

                    list_tg = getListTG(connSett, FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]));

                    for (int k = 0; k < list_tg.Rows.Count; k++)
                    {
                        tec[i].list_TECComponents[j].TG.Add(new TG(tec[i].list_TECComponents[j]));
                        tec[i].list_TECComponents[j].TG[k].name = list_tg.Rows[k]["NAME_SHR"].ToString();
                        tec[i].list_TECComponents[j].TG[k].m_id = Convert.ToInt32(list_tg.Rows[k]["ID"]);
                        if (!(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"] is System.DBNull))
                            tec[i].list_TECComponents[j].TG[k].m_indx_col_rdg_excel = Convert.ToInt32(list_tg.Rows[k]["INDX_COL_RDG_EXCEL"]);
                        else
                            ;
                    }
                }
            }

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
        }
    }
}
