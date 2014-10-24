using System;
using System.Windows.Forms;
using System.IO;

namespace StatisticCommon
{
    public class AdminTS_KomDisp : AdminTS
    {
        string m_PPBRCSVDirectory; //Для возможности импорта ПБР из *.csv

        public AdminTS_KomDisp(bool[] arMarkSavePPBRValues)
            : base(arMarkSavePPBRValues)
        {
            delegateImportForeignValuesRequuest = ImpPPBRCSVValuesRequest;
            //delegateExportForeignValuesRequuest = ExpRDGExcelValuesRequest;
            delegateImportForeignValuesResponse = ImpPPBRCSVValuesResponse;
            //delegateExportForeignValuesResponse = ExpRDGExcelValuesResponse;
        }

        public void ImpPPBRCSVValues(int indx, DateTime date, string dir)
        {
            //Разрешить запись ПБР-значений
            if (m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.ENABLED] == true) m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.MARK] = true; else ;
            
            lock (m_lockState)
            {
                ClearStates ();
                
                indxTECComponents = indx;
                m_PPBRCSVDirectory = dir;

                m_tablePPBRValuesResponse.Clear();
                m_tablePPBRValuesResponse = null;

                for (int i = 0; i < 24; i++)
                {
                    m_curRDGValues[i].pbr =
                    m_curRDGValues[i].pmin = m_curRDGValues[i].pmax = 0;
                }

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                states.Add((int)StatesMachine.PPBRCSVValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "catch - GetPPBRCSVValues () - semaState.Release(1)");
                }
            }
        }

        private void ImpPPBRCSVValuesRequest()
        {
            int err = 0,
                num_pbr = 23;

            //if ((m_curDate.Date == DateTime.Now.Date) && (!(allTECComponents[indxTECComponents].name_future == string.Empty)))
            if (m_curDate.Date == DateTime.Now.Date)
            {
                string strCSVExt = @".csv"
                    , strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr)
                    , strPPBRCSVNameFileTemp = string.Empty;

                delegateStartWait();

                while ((num_pbr > 0) && (File.Exists(m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt) == false))
                {
                    num_pbr -= 2;
                    strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr);
                }

                if ((num_pbr > 0) && (num_pbr > serverTime.Hour))
                {
                    strPPBRCSVNameFileTemp = strPPBRCSVNameFile;

                    strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace("(", string.Empty);
                    strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(")", string.Empty);
                    strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(".", string.Empty);
                    strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(" ", string.Empty);

                    strPPBRCSVNameFile = m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt;
                    strPPBRCSVNameFileTemp = m_PPBRCSVDirectory + strPPBRCSVNameFileTemp + strCSVExt;

                    File.Copy(strPPBRCSVNameFile, strPPBRCSVNameFileTemp, true);

                    if (!(m_tablePPBRValuesResponse == null)) m_tablePPBRValuesResponse.Clear(); else ;

                    if ((IsCanUseTECComponents() == true) && (strPPBRCSVNameFileTemp.Length > 0))
                        m_tablePPBRValuesResponse = DbTSQLInterface.Select(@"CSV_PATH" + System.IO.Path.GetDirectoryName(strPPBRCSVNameFileTemp),
                                                                                @"SELECT * FROM [" +
                                                                                System.IO.Path.GetFileName(strPPBRCSVNameFileTemp) +
                                                                                @"]"
                                                                                + @" WHERE GTP_ID='" +
                                                                                allTECComponents[indxTECComponents].name_future +
                                                                                @"'"
                                                                                , out err);
                    else
                        ;

                    //Logging.Logg ().LogLock ();
                    //Logging.Logg().Send("Admin.cs - GetPPBRCSVValuesRequest () - ...", false, false, false);
                    //Logging.Logg().LogUnlock();

                    File.Delete(strPPBRCSVNameFileTemp);
                }
                else
                    ;

                delegateStopWait();
            }
            else
                ;
        }

        protected bool ImpPPBRCSVValuesResponse()
        {
            bool bRes = m_tablePPBRValuesResponse.Rows.Count > 0 ? true : false,
                bValParse = false;
            int i = -1,
                h = -1;
            double val = 0.0;

            for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
            {
                h = Int32.Parse(m_tablePPBRValuesResponse.Rows[i]["SESSION_INTERVAL"].ToString()) + 1;

                bValParse = Double.TryParse(m_tablePPBRValuesResponse.Rows[i]["TotalBR"].ToString(), out val);
                if (bValParse == true) m_curRDGValues[h - 1].pbr = val; else ;
                bValParse = Double.TryParse(m_tablePPBRValuesResponse.Rows[i]["PminBR"].ToString(), out val);
                if (bValParse == true) m_curRDGValues[h - 1].pmin = val; else ;
                bValParse = Double.TryParse(m_tablePPBRValuesResponse.Rows[i]["PmaxBR"].ToString(), out val);
                if (bValParse == true) m_curRDGValues[h - 1].pmax = val; else ;
            }

            return bRes;
        }

        private string getNameFileSessionPPBRCSVValues(int num_pbr)
        {
            int offset_day = -1,
                num_session = -1;

            if (num_pbr == 23)
            {
                num_session = 1;
                offset_day = 1;
            }
            else
            {
                num_session = num_pbr + 2;
                offset_day = 0;
            }

            return @"ГТП(генерация) Сессия(" +
                num_session +
                @") от " +
                (m_curDate.AddDays(offset_day)).Date.GetDateTimeFormats()[0];
        }
    }
}
