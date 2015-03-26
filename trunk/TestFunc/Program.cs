using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection

using System.Data.OracleClient;
using System.Data.OleDb;

using HClassLibrary;
using StatisticCommon;

namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            object test = null;

            try {
                //test = new DomainName ();
                //test = new ImportCSV ();

                //test = new HOracleConnection();
                ////test = new HOleDbOracleConnection ();
                //(test as HDbOracle).Open ();
                //(test as HDbOracle).Query();                
                //(test as HDbOracle).Close ();
                //(test as HDbOracle).OutResult ();

                test = new HOleDbOracleConnection();
                int err = -1;
                ConnectionSettings connSett = new ConnectionSettings(@"OraSOTIASSO-ORD", (test as HDbOracle).host, Int32.Parse((test as HDbOracle).port), (test as HDbOracle).dataSource, (test as HDbOracle).uid, (test as HDbOracle).pswd);
                int iListenerId =  DbSources.Sources ().Register (connSett, false, @"СОТИАССО-Бийск-Oracle");
                DbConnection dbConn = DbSources.Sources ().GetConnection (iListenerId, out err);

                if (err == 0)
                {
                    (test as HDbOracle).query = @"SELECT SYSDATE FROM dual";
                    DataTable res = DbTSQLInterface.Select(ref dbConn, (test as HDbOracle).query, null, null, out err);
                }
                else
                    ;
                DbSources.Sources().UnRegister(iListenerId);
            } catch (Exception e) {
                Console.Write(e.Message + Environment.NewLine);
            }

            Console.Write(@"Для завершения работы нажмите любую клавишу..."); Console.ReadKey();
        }

        private class DomainName {
            public DomainName () {
                string strMes = string.Empty;

                strMes = @"Доменное имя пользователя: "; Console.Write(strMes);
                try { strMes = Environment.UserDomainName; Console.Write(strMes + @"\"); }
                catch (Exception e) { Console.WriteLine(e.Message); }

                try { strMes = Environment.UserName; Console.Write(strMes); }
                catch (Exception e) { Console.WriteLine(e.Message); }

                Console.WriteLine();

                strMes = @"IP: "; Console.Write(strMes);
                strMes = string.Empty;
                System.Net.IPAddress[] listIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
                int indxIP = -1;
                for (indxIP = 0; indxIP < listIP.Length; indxIP++)
                {
                    strMes += @"[" + indxIP + @"]=" + listIP[indxIP].ToString() + @", ";
                }
                strMes = strMes.Substring(0, strMes.Length - 2);
                Console.WriteLine(strMes + Environment.NewLine);
            }
        }

        private class ImportCSV {
            public ImportCSV () {
                DataTable m_tablePPBRValuesResponse = null;
                string m_fullPathPPBRCSVValue = @"D:" //Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
                     + @"\" + @"ГТП(генерация) Сессия(13) от 24.12.2014.csv";

                int err = -1;                
                string strPPBRCSVNameFileTemp = Path.GetFileNameWithoutExtension(m_fullPathPPBRCSVValue)
                    , strMes = string.Empty;;

                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace("(", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(")", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(".", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(" ", string.Empty);
                strPPBRCSVNameFileTemp = Path.GetDirectoryName(m_fullPathPPBRCSVValue) + @"\" +
                                        strPPBRCSVNameFileTemp +
                                        Path.GetExtension(m_fullPathPPBRCSVValue);

                ////при аргументе = каталог размещения наборов
                //strPPBRCSVNameFile = m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt;
                //strPPBRCSVNameFileTemp = m_PPBRCSVDirectory + strPPBRCSVNameFileTemp + strCSVExt;

                //File.Copy(strPPBRCSVNameFile, strPPBRCSVNameFileTemp, true);
                File.Copy(m_fullPathPPBRCSVValue, strPPBRCSVNameFileTemp, true);

                StreamReader sr = new StreamReader(strPPBRCSVNameFileTemp);
                string cont = sr.ReadToEnd().Replace(',', '.');
                sr.Close(); sr.Dispose();
                StreamWriter sw = new StreamWriter(strPPBRCSVNameFileTemp);
                sw.Write(cont); sw.Flush(); sw.Close(); sw.Dispose();

                if (!(m_tablePPBRValuesResponse == null)) m_tablePPBRValuesResponse.Clear(); else ;

                if (strPPBRCSVNameFileTemp.Length > 0)
                    m_tablePPBRValuesResponse = DbTSQLInterface.Select(@"CSV_DATASOURCE=" + Path.GetDirectoryName(strPPBRCSVNameFileTemp),
                                                                            @"SELECT * FROM ["
                        //+ @"Sheet1$"
                                                                            + Path.GetFileName(strPPBRCSVNameFileTemp)
                                                                            + @"]"
                        //+ @" WHERE GTP_ID='" +
                        //allTECComponents[indxTECComponents].name_future +
                        //@"'"
                                                                            , out err);
                else
                    ;

                File.Delete(strPPBRCSVNameFileTemp);

                int hour = -1;
                double val = -1F;
                HAdmin.RDGStruct  [] curRDGValues = new HAdmin.RDGStruct [24];
                string name_future = @"GNOVOS02"
                    , pbr_number = string.Empty;
                //Получить значения для сохранения
                DataRow[] rowsTECComponent = m_tablePPBRValuesResponse.Select(@"GTP_ID='" + name_future + @"'");
                //Проверить наличие записей для ГТП
                if (rowsTECComponent.Length > 0)
                {
                    foreach (DataRow r in rowsTECComponent)
                    {
                        hour = int.Parse(r[@"SESSION_INTERVAL"].ToString());

                        if (double.TryParse(r[@"TotalBR"].ToString(), out val) == false)
                            if (hour > 0)
                                curRDGValues[hour].pbr = curRDGValues[hour - 1].pbr;
                            else
                                ;
                        else
                            curRDGValues[hour].pbr = val;

                        curRDGValues[hour].pmin = double.Parse(r[@"PminBR"].ToString());
                        curRDGValues[hour].pmax = double.Parse(r[@"PmaxBR"].ToString());

                        strMes = String.Format(@"Час: {0}\tПБР={1}\tПБРмин={2}\tПБРмакс={3}",
                                            hour.ToString(@"00"), curRDGValues[hour].pbr, curRDGValues[hour].pmin, curRDGValues[hour].pmax);
                        Console.WriteLine (strMes);

                        curRDGValues[hour].pbr_number = pbr_number;
                    }
                }
                else
                    ;
            }
        }
    }
}
