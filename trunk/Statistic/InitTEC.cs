using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using MySql.Data.MySqlClient;

namespace Statistic
{
    public class InitTEC
    {
        public List<TEC> tec;

        public static DataTable getListTEC(ConnectionSettings connSett, bool bAll = false)
        {
            string req = string.Empty;
            req = "SELECT * FROM TEC_LIST";

            if (!bAll) req += " WHERE INUSE=TRUE"; else ;

            return DbInterface.Request(connSett, req);
        }

        public static DataTable getListTECComponent(ConnectionSettings connSett, string prefix, int id_tec)
        {
            return DbInterface.Request(connSett, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString());
        }

        public static DataTable getListTG(ConnectionSettings connSett, string prefix, int id_tec)
        {
            return DbInterface.Request(connSett, "SELECT * FROM TG_LIST WHERE ID_" + prefix + " = " + id_tec.ToString());
        }
        
        public InitTEC(ConnectionSettings connSett, Int16 indx)
        {
            tec = new List<TEC> ();

            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null, // = DbInterface.Request(connSett, "SELECT * FROM TEC_LIST"),
                    list_TECComponents = null, list_tg = null;

            //������������� ������� �������
            //int listenerId = -1;
            //bool err = false;
            //DbInterface dbInterface = new DbInterface (DbInterface.DbInterfaceType.MySQL, 1);
            //listenerId = dbInterface.ListenerRegister();
            //dbInterface.Start ();

            //dbInterface.SetConnectionSettings(connSett);

            //dbInterface.Request(listenerId, "SELECT * FROM TEC_LIST");
            //dbInterface.GetResponse(listenerId, out err, out list_tec);

            //dbInterface.Stop();
            //dbInterface.ListenerUnregister(listenerId);

            //������������� ����������� �������
            list_tec = getListTEC(connSett);

            for (int i = 0; i < list_tec.Rows.Count; i ++) {
                //�������� ������� ���
                tec.Add(new TEC(list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                list_tec.Rows[i]["PREFIX_ADMIN"].ToString(),
                                list_tec.Rows[i]["PREFIX_PBR"].ToString()));

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

                tec[i].connSettings (DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_DATA"].ToString()), (int) CONN_SETT_TYPE.DATA);
                tec[i].connSettings(DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_ADMIN"].ToString()), (int) CONN_SETT_TYPE.ADMIN);
                tec[i].connSettings(DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_PBR"].ToString()), (int) CONN_SETT_TYPE.PBR);

                list_TECComponents = getListTECComponent(connSett, ChangeMode.getPrefixMode(indx), Convert.ToInt32 (list_tec.Rows[i]["ID"]));
                for (int j = 0; j < list_TECComponents.Rows.Count; j ++) {
                    tec[i].list_TECComponents.Add(new TECComponent(tec[i], list_TECComponents.Rows [j]["PREFIX_ADMIN"].ToString (), list_TECComponents.Rows [j]["PREFIX_PBR"].ToString ()));
                    tec[i].list_TECComponents[j].name = list_TECComponents.Rows[j]["NAME_SHR"].ToString(); //list_TECComponents.Rows[j]["NAME_GNOVOS"]
                    tec[i].list_TECComponents[j].m_id = Convert.ToInt32 (list_TECComponents.Rows[j]["ID"]);

                    list_tg = getListTG(connSett, ChangeMode.getPrefixMode(indx), Convert.ToInt32(list_TECComponents.Rows[j]["ID"]));

                    for (int k = 0; k < list_tg.Rows.Count; k++)
                    {
                        tec[i].list_TECComponents[j].TG.Add(new TG(tec[i].list_TECComponents[j]));
                        tec[i].list_TECComponents[j].TG[k].name = list_tg.Rows[k]["NAME_SHR"].ToString();
                        tec[i].list_TECComponents[j].TG[k].m_id = Convert.ToInt32 (list_tg.Rows[k]["ID"]);
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
