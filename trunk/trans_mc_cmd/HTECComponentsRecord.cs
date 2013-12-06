using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Odbc;

namespace trans_mc_cmd
{
    abstract class PPBR_Record {
        /// <summary>
        /// Целевое время, которому соответствуют все показатели. Московское.
        /// </summary>
        public DateTime date_time;
        /// <summary>
        /// Дата-время записи (обновления). Новосибирское.
        /// </summary>
        public DateTime wr_date_time;
        public string PBR_number;
        /// <summary>
        /// Указатель на рабочий экземпляр класса, из которого будут использоваться экземпляры этого класса. Для обращения к методу того класса.
        /// </summary>
        public MySQLtechsite parent;

        public abstract string GenUpdateStatement();
                                                        
        /// <summary>
        /// Читает данные в поля экземпляра класса из базы на указанное время.
        /// </summary>
        public abstract bool ReadFromDatabase(DateTime DT);
    }

    class HTECComponentsRecord : PPBR_Record
    {
        SortedList<int, double?[]> m_srtlist_ppbr;

        public HTECComponentsRecord (List <int> ids) {
            int i = -1;
            m_srtlist_ppbr = new SortedList<int,double?[]> ();

            for (i = 0; i < ids.Count; i ++)
            {
                m_srtlist_ppbr.Add(ids[i], new double?[(int)MySQLtechsite.Params.COUNT_PARAMS]);
            }
        }

        public override string GenUpdateStatement() {
            string strRes = string.Empty;

            return strRes;
        }

        public override bool ReadFromDatabase(DateTime DT)
        {
            bool bRes = true;

            int i = -1;

            string pbrFields = string.Empty;
            for (MySQLtechsite.Params par = 0; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
            {
                pbrFields += par.ToString();
                if ((par + 1) < MySQLtechsite.Params.COUNT_PARAMS)
                    pbrFields += ", ";
                else
                    ;
            }

            OdbcCommand cmd = new OdbcCommand("SELECT " + pbrFields + " FROM PPBRvsPBROfID where date_time = ? AND ID = ?", parent.mysqlConn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("", OdbcType.DateTime).Value = DT;

            foreach (KeyValuePair <int, double?[]> pair in m_srtlist_ppbr)
            {
                //pair.Key;
                //pair.Value;

                if (cmd.Parameters.Count < 2)
                    cmd.Parameters.Add("", OdbcType.Int).Value = pair.Key;
                else
                    cmd.Parameters[1].Value = pair.Key;

                OdbcDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    for (MySQLtechsite.Params par = 0; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                    {
                        pair.Value[i] = Convert.ToDouble (rdr[par.ToString ()]);
                    }
                }
                else
                    ;
            }

            return bRes;
        }
    }

    /// <summary>
    /// Представляет одну запись в таблице PPBRvsPBR базы techsite на MySQL, соответствующую получасовому интервалу.
    /// </summary>
    class OneRecord : PPBR_Record
    {
        public double? TEC2_PBR;
        public double? TEC2_Pmax;
        public double? TEC2_Pmin;

        public double? BTEC_PBR;
        public double? BTEC_Pmax;
        public double? BTEC_Pmin;
        public double? BTEC_TG1_PBR;
        public double? BTEC_TG1_Pmax;
        public double? BTEC_TG1_Pmin;
        public double? BTEC_TG2_PBR;
        public double? BTEC_TG2_Pmax;
        public double? BTEC_TG2_Pmin;
        public double? BTEC_TG4_PBR;
        public double? BTEC_TG4_Pmax;
        public double? BTEC_TG4_Pmin;
        public double? BTEC_TG35_PBR;
        public double? BTEC_TG35_Pmax;
        public double? BTEC_TG35_Pmin;

        public double? TEC4_PBR;
        public double? TEC4_Pmax;
        public double? TEC4_Pmin;
        public double? TEC4_TG3_PBR;
        public double? TEC4_TG3_Pmax;
        public double? TEC4_TG3_Pmin;
        public double? TEC4_TG48_PBR;
        public double? TEC4_TG48_Pmax;
        public double? TEC4_TG48_Pmin;

        public double? TEC3_PBR;
        public double? TEC3_Pmax;
        public double? TEC3_Pmin;
        public double? TEC3_TG1_PBR;
        public double? TEC3_TG1_Pmax;
        public double? TEC3_TG1_Pmin;
        public double? TEC3_TG5_PBR;
        public double? TEC3_TG5_Pmax;
        public double? TEC3_TG5_Pmin;
        public double? TEC3_TG712_PBR;
        public double? TEC3_TG712_Pmax;
        public double? TEC3_TG712_Pmin;
        public double? TEC3_TG1314_PBR;
        public double? TEC3_TG1314_Pmax;
        public double? TEC3_TG1314_Pmin;

        public double? TEC5_PBR;
        public double? TEC5_Pmax;
        public double? TEC5_Pmin;
        public double? TEC5_TG12_PBR;
        public double? TEC5_TG12_Pmax;
        public double? TEC5_TG12_Pmin;
        public double? TEC5_TG36_PBR;
        public double? TEC5_TG36_Pmax;
        public double? TEC5_TG36_Pmin;

        public override bool ReadFromDatabase(DateTime DT)
        {
            date_time = DT;

            OdbcCommand cmd = new OdbcCommand("SELECT * FROM PPBRvsPBRnew where date_time = ?", parent.mysqlConn);
            cmd.Parameters.Add("", OdbcType.DateTime).Value = DT;
            OdbcDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                BTEC_PBR = (double?)rdr["BTEC_PBR"];
                BTEC_Pmax = (double?)rdr["BTEC_Pmax"];
                BTEC_Pmin = (double?)rdr["BTEC_Pmin"];
                BTEC_TG1_PBR = (double?)rdr["BTEC_TG1_PBR"];
                BTEC_TG1_Pmax = (double?)rdr["BTEC_TG1_Pmax"];
                BTEC_TG1_Pmin = (double?)rdr["BTEC_TG1_Pmin"];
                BTEC_TG2_PBR = (double?)rdr["BTEC_TG2_PBR"];
                BTEC_TG2_Pmax = (double?)rdr["BTEC_TG2_Pmax"];
                BTEC_TG2_Pmin = (double?)rdr["BTEC_TG2_Pmin"];
                BTEC_TG35_PBR = (double?)rdr["BTEC_TG35_PBR"];
                BTEC_TG35_Pmax = (double?)rdr["BTEC_TG35_Pmax"];
                BTEC_TG35_Pmin = (double?)rdr["BTEC_TG35_Pmin"];
                BTEC_TG4_PBR = (double?)rdr["BTEC_TG4_PBR"];
                BTEC_TG4_Pmax = (double?)rdr["BTEC_TG4_Pmax"];
                BTEC_TG4_Pmin = (double?)rdr["BTEC_TG4_Pmin"];
                TEC2_PBR = (double?)rdr["TEC2_PBR"];
                TEC2_Pmax = (double?)rdr["TEC2_Pmax"];
                TEC2_Pmin = (double?)rdr["TEC2_Pmin"];
                TEC3_PBR = (double?)rdr["TEC3_PBR"];
                TEC3_Pmax = (double?)rdr["TEC3_Pmax"];
                TEC3_Pmin = (double?)rdr["TEC3_Pmin"];
                TEC3_TG1_PBR = (double?)rdr["TEC3_TG1_PBR"];
                TEC3_TG1_Pmax = (double?)rdr["TEC3_TG1_Pmax"];
                TEC3_TG1_Pmin = (double?)rdr["TEC3_TG1_Pmin"];
                TEC3_TG1314_PBR = (double?)rdr["TEC3_TG1314_PBR"];
                TEC3_TG1314_Pmax = (double?)rdr["TEC3_TG1314_Pmax"];
                TEC3_TG1314_Pmin = (double?)rdr["TEC3_TG1314_Pmin"];
                TEC3_TG5_PBR = (double?)rdr["TEC3_TG5_PBR"];
                TEC3_TG5_Pmax = (double?)rdr["TEC3_TG5_Pmax"];
                TEC3_TG5_Pmin = (double?)rdr["TEC3_TG5_Pmin"];
                TEC3_TG712_PBR = (double?)rdr["TEC3_TG712_PBR"];
                TEC3_TG712_Pmax = (double?)rdr["TEC3_TG712_Pmax"];
                TEC3_TG712_Pmin = (double?)rdr["TEC3_TG712_Pmin"];
                TEC4_PBR = (double?)rdr["TEC4_PBR"];
                TEC4_Pmax = (double?)rdr["TEC4_Pmax"];
                TEC4_Pmin = (double?)rdr["TEC4_Pmin"];
                TEC4_TG3_PBR = (double?)rdr["TEC4_TG3_PBR"];
                TEC4_TG3_Pmax = (double?)rdr["TEC4_TG3_Pmax"];
                TEC4_TG3_Pmin = (double?)rdr["TEC4_TG3_Pmin"];
                TEC4_TG48_PBR = (double?)rdr["TEC4_TG48_PBR"];
                TEC4_TG48_Pmax = (double?)rdr["TEC4_TG48_Pmax"];
                TEC4_TG48_Pmin = (double?)rdr["TEC4_TG48_Pmin"];
                TEC5_PBR = (double?)rdr["TEC5_PBR"];
                TEC5_Pmax = (double?)rdr["TEC5_Pmax"];
                TEC5_Pmin = (double?)rdr["TEC5_Pmin"];
                TEC5_TG12_PBR = (double?)rdr["TEC5_TG12_PBR"];
                TEC5_TG12_Pmax = (double?)rdr["TEC5_TG12_Pmax"];
                TEC5_TG12_Pmin = (double?)rdr["TEC5_TG12_Pmin"];
                TEC5_TG36_PBR = (double?)rdr["TEC5_TG36_PBR"];
                TEC5_TG36_Pmax = (double?)rdr["TEC5_TG36_Pmax"];
                TEC5_TG36_Pmin = (double?)rdr["TEC5_TG36_Pmin"];
            }

            return rdr.HasRows;
        }

        public override string GenUpdateStatement()
        {
            //апдейтить только те, у которых ПБР в будущем (не с текущей датой лучше сравнивать, а с номером в индексе ПБР
            /* При очередном UPDATE надо знать, какой график пришёл с ОДУ - ПБР1, ПБР4, ПБР7, ПБР10, ПБР13, ПБР16, ПБР19 или ПБР22.
             * Каждый последующий обновляет только актуальные часы, ретроспектива же остаётся без изменений, то есть ПБР4 не обновит первые часы ПБР1, а ПБР22 обновит только часы, начиная с 21:30.  */
            string sUpdate = "";
            int? iId;
            //byte PBRHour;

            //PBRHour = byte.Parse(PBR_number.Replace("ПБР", ""));

            //if (PBRHour >= date_time.Hour || (PBRHour == 21 && date_time.Hour == 0))
            if (date_time > TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))       //Московское время
            {
                iId = parent.Insert48HalfHoursIfNeedAndGetId(date_time);
                //sUpdate = string.Format("UPDATE PPBRvsPBR_Test SET wr_date_time = '{0}', PBR_number = '{1}' WHERE id = {2}", wr_date_time.ToString("u").Replace("Z", ""), PBR_number, iId.ToString());
                sUpdate = "UPDATE PPBRvsPBRnew SET wr_date_time = '@wr_date_time', PBR_number = '@PBR_number', BTEC_PBR = @BTEC_PBR, BTEC_Pmax = @BTEC_Pmax, BTEC_Pmin = @BTEC_Pmin, BTEC_TG1_PBR = @BTEC_TG1_PBR, ";
                sUpdate += "BTEC_TG1_Pmax = @BTEC_TG1_Pmax, BTEC_TG1_Pmin = @BTEC_TG1_Pmin, BTEC_TG2_PBR = @BTEC_TG2_PBR, BTEC_TG2_Pmax = @BTEC_TG2_Pmax, BTEC_TG2_Pmin = @BTEC_TG2_Pmin, ";
                sUpdate += "BTEC_TG4_PBR = @BTEC_TG4_PBR, BTEC_TG4_Pmax = @BTEC_TG4_Pmax, BTEC_TG4_Pmin = @BTEC_TG4_Pmin, BTEC_TG35_PBR = @BTEC_TG35_PBR, BTEC_TG35_Pmax = @BTEC_TG35_Pmax, BTEC_TG35_Pmin = @BTEC_TG35_Pmin, ";
                sUpdate += "TEC2_PBR = @TEC2_PBR, TEC2_Pmax = @TEC2_Pmax, TEC2_Pmin = @TEC2_Pmin, ";
                sUpdate += "TEC3_PBR = @TEC3_PBR, TEC3_Pmax = @TEC3_Pmax, TEC3_Pmin = @TEC3_Pmin, TEC3_TG1_PBR = @TEC3_TG1_PBR, TEC3_TG1_Pmax = @TEC3_TG1_Pmax, TEC3_TG1_Pmin = @TEC3_TG1_Pmin, ";
                sUpdate += "TEC3_TG5_PBR = @TEC3_TG5_PBR, TEC3_TG5_Pmax = @TEC3_TG5_Pmax, TEC3_TG5_Pmin = @TEC3_TG5_Pmin, TEC3_TG712_PBR = @TEC3_TG712_PBR, TEC3_TG712_Pmax = @TEC3_TG712_Pmax, ";
                sUpdate += "TEC3_TG712_Pmin = @TEC3_TG712_Pmin, TEC3_TG1314_PBR = @TEC3_TG1314_PBR, TEC3_TG1314_Pmax = @TEC3_TG1314_Pmax, TEC3_TG1314_Pmin = @TEC3_TG1314_Pmin, ";
                sUpdate += "TEC4_PBR = @TEC4_PBR, TEC4_Pmax = @TEC4_Pmax, TEC4_Pmin = @TEC4_Pmin, TEC4_TG3_PBR = @TEC4_TG3_PBR, TEC4_TG3_Pmax = @TEC4_TG3_Pmax, TEC4_TG3_Pmin = @TEC4_TG3_Pmin, ";
                sUpdate += "TEC4_TG48_PBR = @TEC4_TG48_PBR, TEC4_TG48_Pmax = @TEC4_TG48_Pmax, TEC4_TG48_Pmin = @TEC4_TG48_Pmin, ";
                sUpdate += "TEC5_PBR = @TEC5_PBR, TEC5_Pmax = @TEC5_Pmax, TEC5_Pmin = @TEC5_Pmin, TEC5_TG12_PBR = @TEC5_TG12_PBR, TEC5_TG12_Pmax = @TEC5_TG12_Pmax, TEC5_TG12_Pmin = @TEC5_TG12_Pmin, ";
                sUpdate += "TEC5_TG36_PBR = @TEC5_TG36_PBR, TEC5_TG36_Pmax = @TEC5_TG36_Pmax, TEC5_TG36_Pmin = @TEC5_TG36_Pmin ";
                sUpdate += "WHERE id = @id";
                sUpdate = sUpdate.Replace("@wr_date_time", wr_date_time.ToString("u").Replace("Z", ""));
                sUpdate = sUpdate.Replace("@PBR_number", PBR_number);
                sUpdate = sUpdate.Replace("@BTEC_PBR", BTEC_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_Pmax", BTEC_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_Pmin", BTEC_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_PBR", BTEC_TG1_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_Pmax", BTEC_TG1_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_Pmin", BTEC_TG1_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_PBR", BTEC_TG2_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_Pmax", BTEC_TG2_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_Pmin", BTEC_TG2_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_PBR", BTEC_TG4_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_Pmax", BTEC_TG4_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_Pmin", BTEC_TG4_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_PBR", BTEC_TG35_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_Pmax", BTEC_TG35_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_Pmin", BTEC_TG35_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC2_PBR", TEC2_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC2_Pmax", TEC2_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC2_Pmin", TEC2_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_PBR", TEC3_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_Pmax", TEC3_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_Pmin", TEC3_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_PBR", TEC3_TG1_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_Pmax", TEC3_TG1_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_Pmin", TEC3_TG1_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_PBR", TEC3_TG5_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_Pmax", TEC3_TG5_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_Pmin", TEC3_TG5_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_PBR", TEC3_TG712_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_Pmax", TEC3_TG712_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_Pmin", TEC3_TG712_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_PBR", TEC3_TG1314_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_Pmax", TEC3_TG1314_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_Pmin", TEC3_TG1314_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_PBR", TEC4_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_Pmax", TEC4_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_Pmin", TEC4_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_PBR", TEC4_TG3_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_Pmax", TEC4_TG3_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_Pmin", TEC4_TG3_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_PBR", TEC4_TG48_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_Pmax", TEC4_TG48_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_Pmin", TEC4_TG48_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_PBR", TEC5_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_Pmax", TEC5_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_Pmin", TEC5_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_PBR", TEC5_TG12_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_Pmax", TEC5_TG12_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_Pmin", TEC5_TG12_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_PBR", TEC5_TG36_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_Pmax", TEC5_TG36_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_Pmin", TEC5_TG36_Pmin.ToString());
                sUpdate = sUpdate.Replace("@id", iId.ToString());
            }

            //Можно экземпляр объекта OdbcCommand возвращать, и использовать здесь параметризованную команду апдейта. Но строку выводить на экран удобнее.
            return sUpdate;
        }
    }

    /*class OneField
    {
        public DateTime date_time;
        public string sFieldName;
        public double dValue;
    }*/
}
