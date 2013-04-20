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
            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы

            bool err = false, bRes = false;
            DataTable dataTableConnectSettings = new DataTable();

            //ConnectionSettings connSett = new ConnectionSettings();
            //connSett.server = "127.0.0.1";
            //connSett.port = 3306;
            //connSett.dbName = "techsite";
            //connSett.userName = "techsite";
            //connSett.password = "12345";
            //connSett.ignore = false;

            MySqlConnection connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            MySqlCommand commandMySQL = new MySqlCommand();
            commandMySQL.Connection = connectionMySQL;
            commandMySQL.CommandType = CommandType.Text;

            MySqlDataAdapter adapterMySQL = new MySqlDataAdapter();
            adapterMySQL.SelectCommand = commandMySQL;

            commandMySQL.CommandText = "SELECT * FROM connect";

            dataTableConnectSettings.Reset();
            dataTableConnectSettings.Locale = System.Globalization.CultureInfo.InvariantCulture;

            connectionMySQL.Open();

            if (connectionMySQL.State != ConnectionState.Open)
            {
                try
                {
                    adapterMySQL.Fill(dataTableConnectSettings);
                    bRes = true;
                }
                catch (MySqlException e)
                {
                }
            }
            else
                ; //

            connectionMySQL.Close();

            /*
            int i, j, k; //Индексы для ТЭЦ, ГТП, ТГ
            tec = new List<TEC>();

            i = j = k = 0; //Обнуление индекса ТЭЦ, ГТП, ТГ

            //Создание объекта ТЭЦ (i = 0, Б)
            tec.Add(new TEC("БТЭЦ"));
            
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1";
            tec[i].GTP[j].name = "ГТП ТГ1"; //GNOVOS36
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ1";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG2";
            tec[i].GTP[j].name = "ГТП ТГ2"; //GNOVOS37
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG35"; //GNOVOS38
            tec[i].GTP[j].name = "ГТП ТГ3,5";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ3";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ5";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG4";
            tec[i].GTP[j].name = "ГТП ТГ4"; //GNOVOS08
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ4";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            tec.Add(new TEC("ТЭЦ-2"));
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "";
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
            tec.Add(new TEC("ТЭЦ-3"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1";
            tec[i].GTP[j].name = "ГТП ТГ1"; //GNOVOS33
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ1";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG5";
            tec[i].GTP[j].name = "ГТП ТГ5"; //GNOVOS34
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ5";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG712"; //GNOVOS03
            tec[i].GTP[j].name = "ГТП ТГ7-12";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ7";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ8";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ9";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ10";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ11";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ12";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG1314"; //GNOVOS04
            tec[i].GTP[j].name = "ГТП ТГ13,14";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ13";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ14";

            j = k = 0;
            i++;
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("ТЭЦ-4"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG3";
            tec[i].GTP[j].name = "ГТП ТГ3"; //GNOVOS35
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ3";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG48";
            tec[i].GTP[j].name = "ГТП ТГ4-8"; //GNOVOS07
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ4";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ5";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ6";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ7";
            //Создание ТГ и добавление к ГТП
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ8";

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i ++; //Инкрементируем индекс ТЭЦ
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("ТЭЦ-5"));
            //Создание ГТП и добавление к ТЭЦ
            tec [i].GTP.Add (new GTP (tec [i]));
            tec[i].GTP[j].field = "TG12";
            tec[i].GTP[j].name = "ГТП ТГ1,2"; //GNOVOS06
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "ТГ1";
            //Создание ТГ и добавление к ГТП
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j ++].TG [k ++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec [i].GTP.Add (new GTP (tec [i]));
            tec[i].GTP[j].field = "TG36";
            tec[i].GTP[j].name = "ГТП ТГ3-6"; //GNOVOS07
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

            j = k = 0; //Обнуление индекса ГТП, ТГ
            i++; //Инкрементируем индекс ТЭЦ
            //Создание ТЭЦ и добавление к списку ТЭЦ
            tec.Add(new TEC("Бийск-ТЭЦ"));
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG12";
            tec[i].GTP[j].name = "ГТП ТГ1,2";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "ТГ1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "ТГ2";
            k = 0; //Обнуление индекса ТГ
            //Создание ГТП и добавление к ТЭЦ
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].field = "TG38";
            tec[i].GTP[j].name = "ГТП ТГ3-8";
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
            */
        }
    }
}
