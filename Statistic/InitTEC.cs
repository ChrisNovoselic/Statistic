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
            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы

            int i, j, k; //Индексы для ТЭЦ, ГТП, ТГ
            tec = new List<TEC>();

            i = j = k = 0; //Обнуление индекса ТЭЦ, ГТП, ТГ

            ////Создание объекта ТЭЦ (i = 0, Б)
            //tec.Add (new TEC ());
            //tec [i].name = "БТЭЦ";
            ////Создание ГТП и добавление к ТЭЦ
            //tec [i].GTP.Add (new GTP (tec [i]));
            //tec [i].GTP [j].name = "";
            ////Создание ТГ и добавление к ГТП
            //tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            //tec [i].GTP [j].TG [ k++].name = "ТГ1";
            ////Создание ТГ и добавление к ГТП
            //tec [i].GTP [j].TG.Add(new TG (tec [i].GTP [j]));
            //tec [i].GTP [j].TG [k ++].name = "ТГ2";
            ////Создание ТГ и добавление к ГТП
            //tec [i].GTP [j].TG.Add (new TG(tec [i].GTP [j]));
            //tec [i].GTP [j].TG [k ++].name = "ТГ3";
            ////Создание ТГ и добавление к ГТП
            //tec [i].GTP [j].TG.Add (new TG(tec[i].GTP[j]));
            //tec [i].GTP [j].TG [k ++].name = "ТГ4";
            ////Создание ТГ и добавление к ГТП
            //tec [i].GTP [j].TG.Add(new TG(tec[i].GTP[j]));
            //tec [i].GTP [j].TG [k ++].name = "ТГ5";

            //Создание объекта ТЭЦ (i = 0, Б)
            tec.Add(new TEC());
            tec[i].name = "БТЭЦ";
            
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП ТГ№1";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ1";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП ТГ№2";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП ТГ№3";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ3";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ5";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП ТГ№4";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ4";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            tec.Add (new TEC ());
            tec[i].name = "ТЭЦ-2";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ9";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "ТЭЦ-3";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП 110 кВ";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ9";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ10";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ11";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ12";
            k = 0;
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ГТП 220 кВ";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ13";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ14";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "ТЭЦ-4";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ8";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add (new TEC ());
            tec [i].name = "ТЭЦ-5";
            //Создание ГТП и добавление к ТЭЦ
            tec [i].GTP.Add (new GTP (tec [i]));
            tec [i].GTP [j].name = "ГТП 220 кВ";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ1";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j ++].TG [k ++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec [i].GTP.Add (new GTP (tec [i]));
            tec [i].GTP [j].name = "ГТП 110 кВ";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ3";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ4";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ5";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add(new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ6";
        }
    }
}
