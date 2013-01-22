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

            i = j = k = 0;
            tec.Add (new TEC ());
            tec [i].name = "����";
            
            tec [i].GTP.Add (new GTP (tec [i]));
            tec [i].GTP [j].name = "";
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [ k++].name = "��1";
            tec [i].GTP [j].TG.Add(new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��2";
            tec [i].GTP [j].TG.Add (new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "��3";
            tec [i].GTP [j].TG.Add (new TG(tec[i].GTP[j]));
            tec [i].GTP [j].TG [k ++].name = "��4";
            tec [i].GTP [j].TG.Add(new TG(tec[i].GTP[j]));
            tec [i].GTP [j].TG [k ++].name = "��5";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "���-2";
            tec[i].GTP.Add(new GTP(tec[i]));
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
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "��� 110 ��";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��9";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��10";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��11";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��12";
            k = 0;
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "��� 220 ��";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��13";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��14";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "���-4";
            tec[i].GTP.Add(new GTP(tec[i]));
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

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "���-5";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "��� 220 ��";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "��2";
            k = 0;
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "��� 110 ��";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "��6";
        }
    }
}
