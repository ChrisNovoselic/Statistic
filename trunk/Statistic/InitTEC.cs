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

        public InitTEC(ConnectionSettings connSett)
        {
            tec = new List<TEC> ();

            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������
            DataTable list_tec= null, // = DbInterface.Request(connSett, "SELECT * FROM TEC_LIST"),
                    list_gtp = null, list_tg = null;

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
            list_tec= DbInterface.Request(connSett, "SELECT * FROM TEC_LIST");

            for (int i = 0; i < list_tec.Rows.Count; i ++) {
                //�������� ������� ���
                tec.Add(new TEC(list_tec.Rows[i]["NAME_SHR"].ToString(), //"NAME_SHR"
                                list_tec.Rows[i]["TABLE_NAME_ADMIN"].ToString(),
                                list_tec.Rows[i]["TABLE_NAME_PBR"].ToString(),
                                list_tec.Rows[i]["PREFIX_ADMIN"].ToString(),
                                list_tec.Rows[i]["PREFIX_PBR"].ToString()));

                tec[i].connSettings (DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_DATA"].ToString()), (int) CONN_SETT_TYPE.DATA);
                tec[i].connSettings(DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_ADMIN"].ToString()), (int) CONN_SETT_TYPE.ADMIN);
                tec[i].connSettings(DbInterface.Request(connSett, "SELECT * FROM SOURCE WHERE ID = " + list_tec.Rows[i]["ID_SOURCE_PBR"].ToString()), (int) CONN_SETT_TYPE.PBR);

                list_gtp = DbInterface.Request(connSett, "SELECT * FROM GTP_LIST WHERE ID_TEC = " + list_tec.Rows[i]["ID"].ToString ());
                for (int j = 0; j < list_gtp.Rows.Count; j ++) {
                    tec[i].list_GTP.Add(new GTP(tec[i]));
                    tec[i].list_GTP[j].prefix = list_gtp.Rows [j]["PREFIX"].ToString ();
                    tec[i].list_GTP[j].name = list_gtp.Rows[j]["NAME"].ToString(); //list_gtp.Rows[j]["NAME_GNOVOS"]
                    
                    list_tg = DbInterface.Request(connSett, "SELECT * FROM TG_LIST WHERE ID_GTP = " + list_gtp.Rows[j]["ID"].ToString());

                    for (int k = 0; k < list_tg.Rows.Count; k++)
                    {
                        tec[i].list_GTP[j].TG.Add(new TG(tec[i].list_GTP[j]));
                        tec[i].list_GTP[j].TG[k].name = list_tg.Rows [k]["NAME"].ToString ();
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
            tec[i].list_GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1";
            tec[i].GTP[j].name = "��� ��1"; //GNOVOS36
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��1";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG2";
            tec[i].GTP[j].name = "��� ��2"; //GNOVOS37
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG35"; //GNOVOS38
            tec[i].GTP[j].name = "��� ��3,5";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��3";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��5";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG4";
            tec[i].GTP[j].name = "��� ��4"; //GNOVOS08
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��4";

            j = k = 0; //��������� ������� ���, ��
            i ++; //�������������� ������ ���
            tec.Add(new TEC("���-2"));
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "";
            tec[i].GTP[j].name = "";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��9";

            j = k = 0;
            i++;
            tec.Add(new TEC("���-3"));
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1";
            tec[i].GTP[j].name = "��� ��1"; //GNOVOS33
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��1";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG5";
            tec[i].GTP[j].name = "��� ��5"; //GNOVOS34
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��5";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG712"; //GNOVOS03
            tec[i].GTP[j].name = "��� ��7-12";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��7";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��8";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��9";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��10";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��11";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��12";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1314"; //GNOVOS04
            tec[i].GTP[j].name = "��� ��13,14";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��13";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��14";

            j = k = 0;
            i++;
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("���-4"));
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG3";
            tec[i].GTP[j].name = "��� ��3"; //GNOVOS35
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��3";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG48";
            tec[i].GTP[j].name = "��� ��4-8"; //GNOVOS07
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��4";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��5";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��6";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��7";
            //�������� �� � ���������� � ���
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��8";

            j = k = 0; //��������� ������� ���, ��
            i ++; //�������������� ������ ���
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("���-5"));
            //�������� ��� � ���������� � ���
            tec [i].GTP.Add (new GTP (tec [i]));
            tec[i].GTP[j].field = "TG12";
            tec[i].GTP[j].name = "��� ��1,2"; //GNOVOS06
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��1";
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j ++].TG [k ++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec [i].GTP.Add (new GTP (tec [i]));
            tec[i].GTP[j].field = "TG36";
            tec[i].GTP[j].name = "��� ��3-6"; //GNOVOS07
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��3";
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��4";
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��5";
            //�������� �� � ���������� � ���
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��6";

            j = k = 0; //��������� ������� ���, ��
            i++; //�������������� ������ ���
            //�������� ��� � ���������� � ������ ���
            tec.Add(new TEC("�����-���"));
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG12";
            tec[i].GTP[j].name = "��� ��1,2";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��2";
            k = 0; //��������� ������� ��
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG38";
            tec[i].GTP[j].name = "��� ��3-8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��8";
            */
        }
    }
}
