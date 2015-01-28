using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data;

using HClassLibrary;

namespace StatisticCommon
{
    public class AdminTS_KomDisp : AdminTS
    {
        string m_fullPathPPBRCSVValue; //Для возможности импорта ПБР из *.csv
        private static string s_strMarkSession = @"ГТП(генерация) Сессия(";

        public AdminTS_KomDisp(bool[] arMarkSavePPBRValues)
            : base(arMarkSavePPBRValues)
        {
            delegateImportForeignValuesRequuest = ImpPPBRCSVValuesRequest;
            //delegateExportForeignValuesRequuest = ExpRDGExcelValuesRequest;
            delegateImportForeignValuesResponse = ImpPPBRCSVValuesResponse;
            //delegateExportForeignValuesResponse = ExpRDGExcelValuesResponse;
        }

        //Вызов из панели ком./дисп.
        public int ImpPPBRCSVValues(DateTime date, string fullPath, bool bCheckedPPBRNumber = true)
        {
            int iRes = 0; //Нет ошибки

            if (!(m_tablePPBRValuesResponse == null))
            {
                m_tablePPBRValuesResponse.Clear();
                m_tablePPBRValuesResponse = null;
            }
            else
                ;

            m_fullPathPPBRCSVValue = fullPath;

            //Дата ПБР, номер ПБР из наименования файла
            object[] prop = GetPropertiesOfNameFilePPBRCSVValues();
            //Текущий номер ПБР
            int curPBRNumber = -1;
            if (m_curRDGValues[m_curRDGValues.Length - 1].pbr_number.Length > @"ПБР".Length)
                if (Int32.TryParse(m_curRDGValues[m_curRDGValues.Length - 1].pbr_number.Substring(@"ПБР".Length), out curPBRNumber) == false)
                    curPBRNumber = getPBRNumber();
                else
                    ;
            else
                curPBRNumber = getPBRNumber();

            string strMsg = string.Empty;
            if (!((DateTime)prop[0] == DateTime.Now.Date))
            {
                iRes = -1;
            }
            else
            {
                //Сравнить с текущим номером ПБР
                if ((!((int)prop[1] > curPBRNumber)) && (bCheckedPPBRNumber == true))
                {
                    iRes = -2;
                }
                else
                    ;
            }

            if (iRes == 0)
                lock (m_lockState)
                {
                    ClearStates();

                    ////НЕТ необходимости, т.к. импортируются значения для всех ГТП из списка 'PanelAdmin::m_listTECComponentIndex'
                    //indxTECComponents = indx;

                    ClearValues();

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
                        Logging.Logg().Exception(e, "AdminTS::ImpRDGExcelValues () - semaState.Release(1)");
                    }
                }
            else
                ; //Ошибка

            if (!(iRes == 0))
                m_fullPathPPBRCSVValue = string.Empty;
            else ;

            return iRes;
        }

        private void ImpPPBRCSVValuesRequest()
        {
            //Противоположные операции при завершении потока 'threadPPBRCSVValues'
            //Разрешить запись ПБР-значений
            if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE); else ;
            //Запретить запись Админ-значений
            if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_MarkSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE); else ;

            int err = -1
                , num_pbr = (int)GetPropertiesOfNameFilePPBRCSVValues()[1];
            string strPPBRCSVNameFileTemp = string.Empty
                ////при аргументе = каталог размещения наборов
                //, strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr)
                //, strCSVExt = @".csv"
                    ;

            delegateStartWait();

            ////Определение самого актуального набора (при аргументе = каталог размещения наборов)
            //while ((num_pbr > 0) && (File.Exists(m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt) == false))
            //{
            //    num_pbr -= 2;
            //    strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr);
            //}

            //if ((num_pbr > 0) && (num_pbr > serverTime.Hour))
            if ((num_pbr > 0) && (! (num_pbr < getPBRNumber())))
            {
                //strPPBRCSVNameFileTemp = strPPBRCSVNameFile;
                strPPBRCSVNameFileTemp = Path.GetFileNameWithoutExtension (m_fullPathPPBRCSVValue);

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
                string cont = sr.ReadToEnd ().Replace (',', '.');
                sr.Close (); sr.Dispose ();
                StreamWriter sw = new StreamWriter(strPPBRCSVNameFileTemp);
                sw.Write(cont); sw.Flush (); sw.Close (); sw.Dispose ();

                if (!(m_tablePPBRValuesResponse == null)) m_tablePPBRValuesResponse.Clear(); else ;

                if ((IsCanUseTECComponents() == true) && (strPPBRCSVNameFileTemp.Length > 0))
                    //m_tablePPBRValuesResponse = DbTSQLInterface.Select(@"CSV_DATASOURCE=" + Path.GetDirectoryName(strPPBRCSVNameFileTemp),
                    //                                                        @"SELECT * FROM ["
                    //                                                        //+ @"Sheet1$"
                    //                                                        + Path.GetFileName (strPPBRCSVNameFileTemp)
                    //                                                        + @"]"
                    //                                                        //+ @" WHERE GTP_ID='" +
                    //                                                        //allTECComponents[indxTECComponents].name_future +
                    //                                                        //@"'"
                    //                                                        , out err);
                    m_tablePPBRValuesResponse = DbTSQLInterface.CSVImport(Path.GetDirectoryName(strPPBRCSVNameFileTemp)
                                                                        + @"\" + Path.GetFileName(strPPBRCSVNameFileTemp)
                                                                        , @"*"
                                                                        , out err);
                else
                    ;

                //Logging.Logg ().LogLock ();
                //Logging.Logg().Send("Admin.cs - GetPPBRCSVValuesRequest () - ...", false, false, false);
                //Logging.Logg().LogUnlock();

                File.Delete(strPPBRCSVNameFileTemp);

                if (! (err == 0)) {
                    m_tablePPBRValuesResponse.Clear();
                    m_tablePPBRValuesResponse = null;
                } else
                    ;
            }
            else
                Logging.Logg().Action(@"Загрузка набора номер которого меньше, чем тек./час");

            delegateStopWait();
        }

        protected bool ImpPPBRCSVValuesResponse()
        {
            bool bRes = (((m_tablePPBRValuesResponse.Columns.Contains (@"GTP_ID") == true)
                            && (m_tablePPBRValuesResponse.Columns.Contains (@"TotalBR") == true)
                            && (m_tablePPBRValuesResponse.Columns.Contains(@"PminBR") == true)
                            && (m_tablePPBRValuesResponse.Columns.Contains(@"PmaxBR") == true))
                        && (m_tablePPBRValuesResponse.Rows.Count > 0)) ? true : false;

            if (bRes == true)
                //'indxTECComponents' необходимо сохранить ??? - сохраняется в потоке !!!
                new Thread(new ParameterizedThreadStart(threadPPBRCSVValues)).Start(null); //можно установить значение аргумента = 'm_curDate'
            else
                Logging.Logg().Error(@"AdminTS_KomDisp::ImpPPBRCSVValuesResponse () - входная таблица не соответствует требованиям...");

            return bRes;
        }

        private void threadPPBRCSVValues(object date)
        {
            Errors errRes = Errors.NoError;

            int indxEv = -1
                , prevIndxTECComponents = indxTECComponents;

            string strPBRNumber = GetPBRNumber((int)GetPropertiesOfNameFilePPBRCSVValues ()[1]);

            //Снять все признаки причин прекращения выполнения обработки событий
            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            foreach (TECComponent comp in allTECComponents)
                if ((comp.m_id > 100) && (comp.m_id < 500))
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                    {
                        errRes = savePPBRCSVValues(allTECComponents.IndexOf(comp), strPBRNumber);
                    }
                    else
                        //Ошибка ???
                        //break;
                        //completeHandleStates();
                        ;
                }
                else
                    ;

            //Очистить таблицу, полученную из CSV-файла
            m_tablePPBRValuesResponse.Clear ();
            m_tablePPBRValuesResponse = null;

            //Противоположные операции в 'ImpPPBRCSVValuesRequest'
            //Запретить запись ПБР-значений
            // , запрет устанавливается автоматически 
            //Разрешить запись Админ-значений
            if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE); else ;

            //Обновить значения на вкладке
            GetRDGValues (m_typeFields, prevIndxTECComponents);
        }

        private Errors savePPBRCSVValues (int indx, string pbr_number) {
            Errors errRes = Errors.NoSet;

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];
            int hour = -1;
            double val = -1F;

            //Получить значения для сохранения
            DataRow [] rowsTECComponent = m_tablePPBRValuesResponse.Select(@"GTP_ID='" + allTECComponents[indx].name_future + @"'");
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

                    curRDGValues[hour].pbr_number = pbr_number;
                }

                //Очистить тек./массив с данными
                ClearValues();

                //Копировать полученные значения в "текущий массив"
                curRDGValues.CopyTo(m_curRDGValues, 0);

                indxTECComponents = indx;

                errRes =
                    SaveChanges()
                    //Errors.NoSet
                    //Errors.NoError
                    ;
            }
            else
                //Пропустить запись ГТП, разрешить переход к следующей
                //Псевдо-закончена обработка всех событий
                completeHandleStates();

            return errRes;
        }

        protected override void InitializeSyncState()
        {
            m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.ERROR + 1];
            base.InitializeSyncState();
            for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)(INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
            {
                m_waitHandleState[i] = new ManualResetEvent(false);
            }
        }

        public static object[] GetPropertiesOfNameFilePPBRCSVValues(string nameFile)
        {
            object[] arObjRes = new object[2]; //0 - DateTime, 1 - int (номер ПБР)

            int indxStartDateTime = nameFile.Length - @".csv".Length;
            while (Char.IsWhiteSpace(nameFile, indxStartDateTime) == false)
            {
                indxStartDateTime--;
            }

            arObjRes[0] = DateTime.Parse(nameFile.Substring(indxStartDateTime + 1, nameFile.Length - @".csv".Length - indxStartDateTime - 1));

            int indxStartSession = nameFile.IndexOf(s_strMarkSession, 0) + s_strMarkSession.Length
                , indxEndSession = nameFile.IndexOf(@")", indxStartSession);
            arObjRes[1] = Int32.Parse(nameFile.Substring(indxStartSession, indxEndSession - indxStartSession)) - 2;

            return arObjRes;
        }

        public object[] GetPropertiesOfNameFilePPBRCSVValues()
        {
            return GetPropertiesOfNameFilePPBRCSVValues(m_fullPathPPBRCSVValue);
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

            return s_strMarkSession +
                num_session +
                @") от " +
                (m_curDate.AddDays(offset_day)).Date.GetDateTimeFormats()[0];
        }
    }
}
