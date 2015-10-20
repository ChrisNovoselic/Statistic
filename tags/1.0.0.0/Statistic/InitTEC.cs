using System;
using System.Collections.Generic;
using System.Text;

namespace Statistic
{
    public class InitTEC
    {
        public List<TEC> tec;

        public InitTEC()
        {
            // ������������ � ��, ���������������� ���������� ����������, ������� ����� ������

            int i, j, k; //������� ��� ���, ���, ��
            tec = new List<TEC>();

            i = j = k = 0; //��������� ������� ���, ���, ��

            ////�������� ������� ��� (i = 0, �)
            //tec.Add (new TEC ());
            //tec [i].name = "����";
            ////�������� ��� � ���������� � ���
            //tec [i].GTP.Add (new GTP (tec [i]));
            //tec [i].GTP [j].name = "";
            ////�������� �� � ���������� � ���
            //tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            //tec [i].GTP [j].TG [ k++].name = "��1";
            ////�������� �� � ���������� � ���
            //tec [i].GTP [j].TG.Add(new TG (tec [i].GTP [j]));
            //tec [i].GTP [j].TG [k ++].name = "��2";
            ////�������� �� � ���������� � ���
            //tec [i].GTP [j].TG.Add (new TG(tec [i].GTP [j]));
            //tec [i].GTP [j].TG [k ++].name = "��3";
            ////�������� �� � ���������� � ���
            //tec [i].GTP [j].TG.Add (new TG(tec[i].GTP[j]));
            //tec [i].GTP [j].TG [k ++].name = "��4";
            ////�������� �� � ���������� � ���
            //tec [i].GTP [j].TG.Add(new TG(tec[i].GTP[j]));
            //tec [i].GTP [j].TG [k ++].name = "��5";

            //�������� ������� ��� (i = 0, �)
            tec.Add(new TEC());
            tec[i].name = "����";
            
            //�������� ��� � ���������� � ���
            tec[i].GTP.Add(new GTP(tec[i]));
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
            tec.Add (new TEC ());
            tec[i].name = "���-2";
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
            tec.Add(new TEC());
            tec[i].name = "���-3";
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
            tec.Add(new TEC());
            tec[i].name = "���-4";
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
            tec.Add (new TEC ());
            tec [i].name = "���-5";
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
        }
    }
}
