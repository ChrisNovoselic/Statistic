using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Odbc;

namespace trans_mc_cmd
{
    /// <summary>
    /// Класс работы с базой techsite на сервере MySQL
    /// </summary>
    class MySQLtechsite
    {
        public OdbcConnection mysqlConn;
        public /*readonly*/ static string sConnectionString;
        public enum Params : int { PBR = 0, Pmin = 1, Pmax = 2, COUNT_PARAMS };

        //public enum TEClist : int { BTEC = ?, TEC2 = 17, TEC3 = сумма104_201_200_14, TEC3_110 = суммма201_200, TEC3_110_2 = 104, TEC3_220 = 14, TEC4 = сумма194и195?, TEC5 = сумма147и146и148, TEC5_110 = 146, TEC5_220 = 147, TEC5_110_2 = 148 };

        /// <summary>
        /// Коллекция почасовых величин генерации. После опроса ModesCentre должны получить 24 почасовых элемента в коллекции. Каждый элемент соответствует одной записи в базе данных techsite.
        /// </summary>
        SortedList<DateTime, OneRecord> HourlyValuesCollection;
        //SortedList<DateTime, OneField> HourlyFieldValues;

        /// <summary>
        /// Конструктор открывает коннект к базе. Закрывает деструктор.
        /// </summary>
        public MySQLtechsite()
        {
            Properties.Settings sett = new Properties.Settings();
            sConnectionString = sett.TechsiteMySQLconnectionString;
            sConnectionString = sConnectionString.Replace("pwd=;", "pwd=" + Connection.CryptoProvider.Decrypt(sett.accessPart, sett.accessKey) + ";");
            mysqlConn = new OdbcConnection(sConnectionString);
            try
            {
                mysqlConn.Open();
            }
            catch (Exception e)
            {
                itssAUX.PrintErrorMessage("Ошибка при подключении к базе MySQL. Драйвер ODBC {MySQL ODBC 3.51 Driver} установлен?" + Environment.NewLine + e.Message);
            }
        }

        public string TestRead()
        {
            OdbcCommand cmd = new OdbcCommand("SELECT page FROM settings", mysqlConn);
            OdbcDataReader rdr = cmd.ExecuteReader();

            rdr.Read();

            return ((string)rdr[0]);
        }

        /// <summary>
        /// Пишем в коллекцию с экземплярами класса OneRecord. А потом за раз коллекцию превратим в апдейты таблицы базы и выполним.
        /// Привязка к id станции! Если в API id поменяют - будет неверно работать!
        /// </summary>
        public void WritePlanValue(int iStationId, DateTime DT, string sPBRnum, Params PAR, double dGenValue)
        {
            OneRecord HVCrecord = null;

            if (HourlyValuesCollection == null) HourlyValuesCollection = new SortedList<DateTime, OneRecord>(50);
            //А может здесь из базы читать предыдущие значения в коллекцию? Нет. Данные из базы здесь не нужны.

            /*OneField OFthis = HourlyFieldValues.First(item => item.Key == DT && item.Value.sFieldName == PAR.ToString()).Value;     //Не верно, но тема такая
            if (OFthis == null) OFthis = new OneField();*/

            if (HourlyValuesCollection.ContainsKey(DT))
                HVCrecord = HourlyValuesCollection.First(item => item.Key == DT).Value;
            else
                ;

            if (HVCrecord == null)
            {
                HVCrecord = new OneRecord();
                HVCrecord.date_time = DT;
                HVCrecord.parent = this;
                HourlyValuesCollection.Add(DT, HVCrecord);      //После добавления можно продолжать модифицировать экземпляр класса - в коллекции та же самая ссылка хранится.
            }
            else
                ;

            HVCrecord.wr_date_time = DateTime.Now;
            HVCrecord.PBR_number = sPBRnum;

            int[] arStationIds = new int[] { 8, 13 };

            switch (iStationId)
            {
                case 209:                                                                                           //Барабинская ТЭЦ, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.BTEC_TG1_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.BTEC_TG1_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.BTEC_TG1_Pmin = dGenValue;
                    break;
                case 210:                                                                                           //Барабинская ТЭЦ, ТГ-2 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.BTEC_TG2_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.BTEC_TG2_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.BTEC_TG2_Pmin = dGenValue;
                    break;
                case 211:                                                                                           //Барабинская ТЭЦ, ТГ-4 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.BTEC_TG4_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.BTEC_TG4_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.BTEC_TG4_Pmin = dGenValue;
                    break;
                case 28:                                                                                            //Барабинская ТЭЦ, ТГ-3,5 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.BTEC_TG35_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.BTEC_TG35_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.BTEC_TG35_Pmin = dGenValue;
                    break;
                case 17:                                                                                            //Новосибирская ТЭЦ-2 [РГЕ ТЭЦ, КЭС, АЭС]
                    if (PAR == Params.PBR) HVCrecord.TEC2_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC2_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC2_Pmin = dGenValue;
                    break;
                case 65:                                                                                            //Новосибирская ТЭЦ-3, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC3_TG1_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC3_TG1_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC3_TG1_Pmin = dGenValue;
                    break;
                case 201:                                                                                           //Новосибирская ТЭЦ-3, ТГ-5 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC3_TG5_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC3_TG5_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC3_TG5_Pmin = dGenValue;
                    break;
                case 13:                                                                                            //Новосибирская ТЭЦ-3, ТГ 7-12 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC3_TG712_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC3_TG712_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC3_TG712_Pmin = dGenValue;
                    break;
                case 14:                                                                                            //Новосибирская ТЭЦ-3, ТГ-13,14 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC3_TG1314_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC3_TG1314_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC3_TG1314_Pmin = dGenValue;
                    break;
                case 195:                                                                                           //Новосибирская ТЭЦ-4, ТГ-3 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC4sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC4_TG3_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC4_TG3_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC4_TG3_Pmin = dGenValue;
                    break;
                case 8:                                                                                             //Новосибирская ТЭЦ-4, ТГ 4-8 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC4sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC4_TG48_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC4_TG48_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC4_TG48_Pmin = dGenValue;
                    break;
                case 23:                                                                                            //Новосибирская ТЭЦ-5, ТГ-3,4 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(HVCrecord, PAR, dGenValue);
                    AddTEC5TG36Values(HVCrecord, PAR, dGenValue);
                    break;
                case 24:                                                                                            //Новосибирская ТЭЦ-5, ТГ-5,6 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(HVCrecord, PAR, dGenValue);
                    AddTEC5TG36Values(HVCrecord, PAR, dGenValue);
                    break;
                case 25:                                                                                            //Новосибирская ТЭЦ-5, ТГ-1,2 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(HVCrecord, PAR, dGenValue);
                    if (PAR == Params.PBR) HVCrecord.TEC5_TG12_PBR = dGenValue;
                    if (PAR == Params.Pmax) HVCrecord.TEC5_TG12_Pmax = dGenValue;
                    if (PAR == Params.Pmin) HVCrecord.TEC5_TG12_Pmin = dGenValue;
                    break;
                #region Comment
                /*      Это прежняя, устаревшая в августе 2013 кодировка:
                 157 	-Барабинская ТЭЦ [Электростанция]  P:10
                 209 	--Барабинская ТЭЦ, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 210	--Барабинская ТЭЦ, ТГ-2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 211	--Барабинская ТЭЦ, ТГ-4 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 208	--Барабинская ТЭЦ, ТГ-3,5 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 155	-Новосибирская ТЭЦ-2 [Электростанция]  P:10
                 17 	--Новосибирская ТЭЦ-2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 154	-Новосибирская ТЭЦ-3 [Электростанция]  P:10
                 104	--Новосибирская ТЭЦ-3, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 201	--Новосибирская ТЭЦ-3, ТГ-5 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 200	--Новосибирская ТЭЦ-3, ТГ 7-12 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 14 	--Новосибирская ТЭЦ-3, ТГ-13,14 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 153	-Новосибирская ТЭЦ-4 [Электростанция]  P:10
                 195	--Новосибирская ТЭЦ-4, ТГ-3 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 194	--Новосибирская ТЭЦ-4, ТГ 4-8 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 156	-Новосибирская ТЭЦ-5 [Электростанция]  P:10
                 147	--Новосибирская ТЭЦ-5, ТГ-1,2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 146	--Новосибирская ТЭЦ-5, ТГ-3,4 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 148	--Новосибирская ТЭЦ-5, ТГ-5,6 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                */
                #endregion
            }

        }

        /// <summary>
        /// Т.к. поле TEC3_PBR - это сумма по [ТГ-1] + [ТГ-5] + [ТГ 7-12] + [ТГ-13,14].
        /// Аналогично TEC3_Pmax и TEC3_Pmin.
        /// </summary>
        private void AddTEC3sumValues(OneRecord HVCrecord, Params PAR, double dGenValue)
        {
            if (PAR == Params.PBR) HVCrecord.TEC3_PBR = HVCrecord.TEC3_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmax) HVCrecord.TEC3_Pmax = HVCrecord.TEC3_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmin) HVCrecord.TEC3_Pmin = HVCrecord.TEC3_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddTEC4sumValues(OneRecord HVCrecord, Params PAR, double dGenValue)
        {
            if (PAR == Params.PBR) HVCrecord.TEC4_PBR = HVCrecord.TEC4_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmax) HVCrecord.TEC4_Pmax = HVCrecord.TEC4_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmin) HVCrecord.TEC4_Pmin = HVCrecord.TEC4_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        /// <summary>
        /// Т.к. поле TEC5_PBR - это сумма по [ТГ-1,2] + [ТГ-3,4] + [ТГ-5,6].
        /// Аналогично TEC5_Pmax и TEC5_Pmin.
        /// </summary>
        private void AddTEC5sumValues(OneRecord HVCrecord, Params PAR, double dGenValue)
        {
            if (PAR == Params.PBR) HVCrecord.TEC5_PBR = HVCrecord.TEC5_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmax) HVCrecord.TEC5_Pmax = HVCrecord.TEC5_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmin) HVCrecord.TEC5_Pmin = HVCrecord.TEC5_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddTEC5TG36Values(OneRecord HVCrecord, Params PAR, double dGenValue)
        {
            if (PAR == Params.PBR) HVCrecord.TEC5_TG36_PBR = HVCrecord.TEC5_TG36_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmax) HVCrecord.TEC5_TG36_Pmax = HVCrecord.TEC5_TG36_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmin) HVCrecord.TEC5_TG36_Pmin = HVCrecord.TEC5_TG36_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddBTECsumValues(OneRecord HVCrecord, Params PAR, double dGenValue)
        {
            if (PAR == Params.PBR) HVCrecord.BTEC_PBR = HVCrecord.BTEC_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmax) HVCrecord.BTEC_Pmax = HVCrecord.BTEC_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == Params.Pmin) HVCrecord.BTEC_Pmin = HVCrecord.BTEC_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        /// <summary>
        /// Добавляет в коллекцию HourlyValuesCollection получасовки, расчитанные как среднее арифметическое между показателями соседних часов.
        /// Получасовки только по будущим показателям считаем. По прошлым запрашиваемые через API значения могут расходиться с сохранёнными ранее в базе!
        /// Кроме того, не всегда получасовки окажутся средним арифметическим: после вычисления получасовки до начала следующего часа данные следующего часа могут измениться. В 02:40, например. Данные на 02:30 уже не изменишь, а на 03:00 будут другие.
        /// </summary>
        private void AddCalculatedHalfHourValues()
        {
            DateTime dtMskNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            List<OneRecord> li = HourlyValuesCollection.Values.Where(item => item.date_time > dtMskNow.AddMinutes(30)).ToList();

            //прочесть: http://msmvps.com/blogs/deborahk/archive/2010/10/30/finding-in-a-child-list.aspx

            foreach (OneRecord rec in li.OrderBy(item => item.date_time))     //сортировка по дате                                              //foreach (OneRecord rec in HourlyValuesCollection.Values.Where(item => item.date_time > DateTime.Now.AddMinutes(30)).Select(item => item))
                GenerateHalfHourValues(rec);

        }

        /// <summary>
        /// Создаёт "получасовку" по среднему арифметическому между часовыми показателями.
        /// </summary>
        private void GenerateHalfHourValues(/*OneRecord HVCprev_rec, OneRecord HVClast_rec*/ OneRecord HVCrec)
        {
            //OneRecord HVClast_rec = HourlyValuesCollection.Last().Value;
            OneRecord HVCprev_rec;
            OneRecord HVChalf_hour = new OneRecord();
            DateTime dtMskNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

            if (HVCrec.date_time.AddHours(-1) <= dtMskNow)
            {
                //Показания в прошлом читаем из базы, из API игнорируем (могут отличаться)
                HVCprev_rec = new OneRecord();
                HVCprev_rec.parent = this;
                HVCprev_rec.ReadFromDatabase(HVCrec.date_time.AddHours(-1));
            }
            else
            {
                HVCprev_rec = HourlyValuesCollection.First(item => item.Key == HVCrec.date_time.AddHours(-1)).Value;                            //Вынимаем из коллекции показания предыдущего (по отношению к выбранному) часа
            }

            if (HVCrec.date_time > new DateTime() && HVCprev_rec.date_time > new DateTime())        //Если время в экземплярах классов проставлено - считаем, что данные есть.
            {
                HVChalf_hour.date_time = HVCrec.date_time.AddMinutes(-30);
                HVChalf_hour.parent = this;
                HVChalf_hour.PBR_number = HVCrec.PBR_number;
                HVChalf_hour.wr_date_time = DateTime.Now;

                //Интерполируем средними арифметическими значениями от соседних часов
                HVChalf_hour.BTEC_PBR = (HVCrec.BTEC_PBR.GetValueOrDefault(0) + HVCprev_rec.BTEC_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_Pmax = (HVCrec.BTEC_Pmax.GetValueOrDefault(0) + HVCprev_rec.BTEC_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_Pmin = (HVCrec.BTEC_Pmin.GetValueOrDefault(0) + HVCprev_rec.BTEC_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG1_PBR = (HVCrec.BTEC_TG1_PBR.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG1_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG1_Pmax = (HVCrec.BTEC_TG1_Pmax.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG1_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG1_Pmin = (HVCrec.BTEC_TG1_Pmin.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG1_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG2_PBR = (HVCrec.BTEC_TG2_PBR.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG2_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG2_Pmax = (HVCrec.BTEC_TG2_Pmax.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG2_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG2_Pmin = (HVCrec.BTEC_TG2_Pmin.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG2_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG35_PBR = (HVCrec.BTEC_TG35_PBR.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG35_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG35_Pmax = (HVCrec.BTEC_TG35_Pmax.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG35_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG35_Pmin = (HVCrec.BTEC_TG35_Pmin.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG35_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG4_PBR = (HVCrec.BTEC_TG4_PBR.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG4_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG4_Pmax = (HVCrec.BTEC_TG4_Pmax.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG4_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.BTEC_TG4_Pmin = (HVCrec.BTEC_TG4_Pmin.GetValueOrDefault(0) + HVCprev_rec.BTEC_TG4_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC2_PBR = (HVCrec.TEC2_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC2_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC2_Pmax = (HVCrec.TEC2_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC2_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC2_Pmin = (HVCrec.TEC2_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC2_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_PBR = (HVCrec.TEC3_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC3_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_Pmax = (HVCrec.TEC3_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC3_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_Pmin = (HVCrec.TEC3_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC3_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1_PBR = (HVCrec.TEC3_TG1_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1_Pmax = (HVCrec.TEC3_TG1_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1_Pmin = (HVCrec.TEC3_TG1_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1314_PBR = (HVCrec.TEC3_TG1314_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1314_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1314_Pmax = (HVCrec.TEC3_TG1314_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1314_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG1314_Pmin = (HVCrec.TEC3_TG1314_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG1314_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG5_PBR = (HVCrec.TEC3_TG5_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG5_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG5_Pmax = (HVCrec.TEC3_TG5_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG5_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG5_Pmin = (HVCrec.TEC3_TG5_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG5_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG712_PBR = (HVCrec.TEC3_TG712_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG712_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG712_Pmax = (HVCrec.TEC3_TG712_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG712_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC3_TG712_Pmin = (HVCrec.TEC3_TG712_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC3_TG712_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_PBR = (HVCrec.TEC4_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC4_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_Pmax = (HVCrec.TEC4_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC4_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_Pmin = (HVCrec.TEC4_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC4_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG3_PBR = (HVCrec.TEC4_TG3_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG3_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG3_Pmax = (HVCrec.TEC4_TG3_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG3_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG3_Pmin = (HVCrec.TEC4_TG3_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG3_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG48_PBR = (HVCrec.TEC4_TG48_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG48_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG48_Pmax = (HVCrec.TEC4_TG48_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG48_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC4_TG48_Pmin = (HVCrec.TEC4_TG48_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC4_TG48_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_PBR = (HVCrec.TEC5_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC5_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_Pmax = (HVCrec.TEC5_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC5_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_Pmin = (HVCrec.TEC5_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC5_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG12_PBR = (HVCrec.TEC5_TG12_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG12_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG12_Pmax = (HVCrec.TEC5_TG12_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG12_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG12_Pmin = (HVCrec.TEC5_TG12_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG12_Pmin.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG36_PBR = (HVCrec.TEC5_TG36_PBR.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG36_PBR.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG36_Pmax = (HVCrec.TEC5_TG36_Pmax.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG36_Pmax.GetValueOrDefault(0)) / 2;
                HVChalf_hour.TEC5_TG36_Pmin = (HVCrec.TEC5_TG36_Pmin.GetValueOrDefault(0) + HVCprev_rec.TEC5_TG36_Pmin.GetValueOrDefault(0)) / 2;

                HourlyValuesCollection.Add(HVChalf_hour.date_time, HVChalf_hour);
            }
        }

        /// <summary>
        /// Несмотря на название, сейчас при необходимости вставляет только одну запись, соответствующую указанному часу или получасу.
        /// </summary>
        public int? Insert48HalfHoursIfNeedAndGetId(DateTime DT)
        {
            int? iId;

            OdbcCommand cmd = new OdbcCommand("SELECT id FROM PPBRvsPBRnew where date_time = ?", mysqlConn);
            cmd.Parameters.Add("", OdbcType.DateTime).Value = DT;
            try
            {
                iId = (int?)cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                itssAUX.PrintErrorMessage(e.Message);
                iId = null;
                throw;  //Чтобы остановить дальнейшее выполнение
            }

            if (!iId.HasValue)
            {
                OdbcCommand cmdi = new OdbcCommand("INSERT INTO PPBRvsPBRnew (date_time, wr_date_time, Is_Comdisp) VALUES( ?, ?, 0)", mysqlConn);      //NOW() на этом сервере не совпадает с Новосибирским временем     //OdbcCommand cmdi = new OdbcCommand("INSERT INTO PPBRvsPBR_Test (date_time, wr_date_time, Is_Comdisp) VALUES('" + DateToSQL(DT) + "', '" + DateToSQL(DateTime.Now) + "', 0)", mysqlConn);
                cmdi.Parameters.Add("", OdbcType.DateTime).Value = DT;
                cmdi.Parameters.Add("", OdbcType.DateTime).Value = DateTime.Now;
                if (cmdi.ExecuteNonQuery() != 1)
                    itssAUX.PrintErrorMessage("Ошибка записи в базу MySQL на INSERT!");
                else
                    ;

                iId = (int?)cmd.ExecuteScalar();
            }
            else
                ;

            return iId;
        }

        /// <summary>
        /// Запись данных из коллекции HourlyValuesCollection в базу. Очистка коллекции. Запускается один раз в конце сеанса связи с API Modes-Centre.
        /// </summary>
        public void FlushDataToDatabase()
        {
            string sUpdate;

            if (!(HourlyValuesCollection == null))
            {
                OdbcCommand cmd = new OdbcCommand("", mysqlConn);

                AddCalculatedHalfHourValues();

                foreach (OneRecord rec in HourlyValuesCollection.Values)
                {
                    sUpdate = rec.GenUpdateStatement();
                    if (!(sUpdate == ""))
                    {
                        cmd.CommandText = sUpdate;
                        Console.WriteLine(sUpdate + Environment.NewLine);
                        //Запуск апдейта одной часовой записи
                        if (!(cmd.ExecuteNonQuery() == 1))
                            itssAUX.PrintErrorMessage("Warning: Rows Updated <> 1");
                        else
                            ;
                    }
                    else
                        ;
                }

                HourlyValuesCollection.Clear();
            }
            else
                ; //HourlyValuesCollection пуста
        }

        private string DateToSQL(DateTime DT)
        {
            return DT.ToString("u").Replace("Z", "");
        }

        ~MySQLtechsite()
        {
            /*if(mysqlConn.State != ConnectionState.Closed) 
                mysqlConn.Close();*/
        }
    }

    static class itssAUX
    {
        /// <summary>
        /// Выводит сообщение об ошибке на консоль красным шрифтом и (или) в журнал Windows.
        /// </summary>
        public static void PrintErrorMessage(string sErrMess, bool bConsoleOutput = true, bool bWriteToWinEventLog = true)
        {
            if (bConsoleOutput)
            {
                ConsoleColor cc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(sErrMess);
                Console.ForegroundColor = cc;
            }

            if (bWriteToWinEventLog && Environment.OSVersion.Version.Major < 6)     //На Windows Vista и выше в журнал таким способом записать прав не хватит
            {
                //Для Win7 надо палочкой махнуть, но не кашерно: Try giving the following registry key Read permission for NETWORK SERVICE: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Security
                string sAppName = System.Environment.CommandLine.Substring(System.Environment.CommandLine.LastIndexOf("\\") + 1).Replace("\"", "").Trim();
                System.Diagnostics.EventLog.WriteEntry(sAppName, sErrMess, System.Diagnostics.EventLogEntryType.Error);
            }
        }
    }
}
