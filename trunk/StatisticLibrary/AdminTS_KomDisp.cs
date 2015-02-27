using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data;

using HClassLibrary;

namespace StatisticCommon
{
    public class AdminTS_KomDisp : AdminTS
    {
        string m_fullPathCSVValues; //Для возможности импорта ПБР из *.csv
        private static string s_strMarkSession = @"ГТП(генерация) Сессия(";

        public AdminTS_KomDisp(bool[] arMarkSavePPBRValues)
            : base(arMarkSavePPBRValues)
        {
            delegateImportForeignValuesRequuest = ImpCSVValuesRequest;
            delegateImportForeignValuesResponse = ImpCSVValuesResponse;
        }

        //Вызов из панели ком./дисп.
        public int ImpCSVValues(DateTime date, string fullPath)
        {
            int iRes = 0; //Нет ошибки

            if (!(m_tableValuesResponse == null))
            {
                m_tableValuesResponse.Clear();
                m_tableValuesResponse = null;
            }
            else
                ;

            m_fullPathCSVValues = fullPath;

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

                    states.Add((int)StatesMachine.CSVValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "AdminTS::ImpRDGExcelValues () - semaState.Release(1)");
                    }
                }
            else
                ; //Ошибка

            if (!(iRes == 0))
                m_fullPathCSVValues = string.Empty;
            else ;

            return iRes;
        }

        private void impCSVValues(out int err)
        {
            err = -1;

            if (!(m_fullPathCSVValues == string.Empty))
            {
                if (File.Exists(m_fullPathCSVValues) == true)
                {
                    //strCSVNameFileTemp = strPPBRCSVNameFile;
                    string strCSVNameFileTemp = Path.GetFileNameWithoutExtension(m_fullPathCSVValues);

                    strCSVNameFileTemp = strCSVNameFileTemp.Replace("(", string.Empty);
                    strCSVNameFileTemp = strCSVNameFileTemp.Replace(")", string.Empty);
                    strCSVNameFileTemp = strCSVNameFileTemp.Replace(".", string.Empty);
                    strCSVNameFileTemp = strCSVNameFileTemp.Replace(" ", string.Empty);
                    strCSVNameFileTemp = Path.GetDirectoryName(m_fullPathCSVValues) + @"\" +
                                            strCSVNameFileTemp + @"_копия" +
                                            Path.GetExtension(m_fullPathCSVValues);

                    ////при аргументе = каталог размещения наборов
                    //strPPBRCSVNameFile = m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt;
                    //strCSVNameFileTemp = m_PPBRCSVDirectory + strCSVNameFileTemp + strCSVExt;

                    //File.Copy(strPPBRCSVNameFile, strCSVNameFileTemp, true);
                    File.Copy(m_fullPathCSVValues, strCSVNameFileTemp, true);

                    ////Для en-US
                    //StreamReader sr = new StreamReader(strCSVNameFileTemp, System.Text.Encoding.Default);
                    //string cont = sr.ReadToEnd().Replace(',', '.');
                    //sr.Close(); sr.Dispose();
                    //StreamWriter sw = new StreamWriter(strCSVNameFileTemp);
                    //sw.Write(cont); sw.Flush(); sw.Close(); sw.Dispose();

                    if (!(m_tableValuesResponse == null)) m_tableValuesResponse.Clear(); else ;

                    if ((IsCanUseTECComponents() == true) && (strCSVNameFileTemp.Length > 0))
                        //m_tableValuesResponse = DbTSQLInterface.Select(@"CSV_DATASOURCE=" + Path.GetDirectoryName(strCSVNameFileTemp),
                        //                                                        @"SELECT * FROM ["
                        //                                                        //+ @"Sheet1$"
                        //                                                        + Path.GetFileName (strCSVNameFileTemp)
                        //                                                        + @"]"
                        //                                                        //+ @" WHERE GTP_ID='" +
                        //                                                        //allTECComponents[indxTECComponents].name_future +
                        //                                                        //@"'"
                        //                                                        , out err);
                        m_tableValuesResponse = DbTSQLInterface.CSVImport(Path.GetDirectoryName(strCSVNameFileTemp)
                                                                            + @"\" + Path.GetFileName(strCSVNameFileTemp)
                                                                            , @"*"
                                                                            , out err);
                    else
                        ;

                    //Logging.Logg ().LogLock ();
                    //Logging.Logg().Send("Admin.cs - GetPPBRCSVValuesRequest () - ...", false, false, false);
                    //Logging.Logg().LogUnlock();

                    File.Delete(strCSVNameFileTemp);
                }
                else
                    err = -2; //Файл не существует (очень НЕвероятно, т.к. выбран с помощью диалогового окна)
            }
            else
            {
            }

            if (!(err == 0))
            {
                m_tableValuesResponse.Clear();
                m_tableValuesResponse = null;
            }
            else
                ;
        }

        private void ImpCSVValuesRequest()
        {
            int err = -1
                //, num_pbr = (int)GetPropertiesOfNameFilePPBRCSVValues()[1]
                ;

            delegateStartWait();

            ////Определение самого актуального набора (при аргументе = каталог размещения наборов)
            //while ((num_pbr > 0) && (File.Exists(m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt) == false))
            //{
            //    num_pbr -= 2;
            //    strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr);
            //}

            ////if ((num_pbr > 0) && (num_pbr > serverTime.Hour))
            //if ((num_pbr > 0) && (! (num_pbr < GetPBRNumber())))
                impCSVValues(out err);
            //else
            //    Logging.Logg().Action(@"Загрузка набора номер которого меньше, чем тек./час");

            delegateStopWait();
        }

        private int ImpCSVValuesResponse()
        {
            int iRes = -1;

            int indxFieldtypeValues = 2;
            List <string []> listFields = new List <string[]> ();
            listFields.Add ( new string [] { @"GTP_ID", @"SESSION_INTERVAL", @"REC", @"IS_PER", @"DIVIAT", @"FC" } );
            listFields.Add(new string[] { @"GTP_ID", @"SESSION_INTERVAL", @"TotalBR", @"PminBR", @"PmaxBR" });

            //Определить тип загружаемых значений
            // по наличию в загруженной таблице поля с индексом [1]
            CONN_SETT_TYPE typeValues = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
            for (typeValues = CONN_SETT_TYPE.ADMIN; typeValues < (CONN_SETT_TYPE.PBR + 1); typeValues ++)
                if (m_tableValuesResponse.Columns.Contains(listFields[(int)typeValues][indxFieldtypeValues]) == true)
                    break;
                else
                    ;

            if (typeValues < (CONN_SETT_TYPE.PBR + 1))
                iRes = CheckNameFieldsOfTable(m_tableValuesResponse, listFields[(int)typeValues]) == true ? 0 : -1;
            else;

            if (iRes == 0)
                //'indxTECComponents' необходимо сохранить ??? - сохраняется в потоке !!!
                new Thread(new ParameterizedThreadStart(threadCSVValues)).Start(typeValues);
            else
                Logging.Logg().Error(@"AdminTS_KomDisp::ImpCSVValuesResponse () - входная таблица не соответствует требованиям...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        private void threadCSVValues(object type)
        {
            Errors errRes = Errors.NoError;

            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //new System.Globalization.CultureInfo(@"en-US")

            //Определить тип загружаемых значений
            CONN_SETT_TYPE typeValues = (CONN_SETT_TYPE)type;

            int indxEv = -1
                , prevIndxTECComponents = indxTECComponents;
            string strPBRNumber = string.Empty; // ...только для ПБР

            if (typeValues == CONN_SETT_TYPE.PBR)
            {//Только для ПБР
                //Противоположные операции при завершении потока 'threadPPBRCSVValues'
                //Разрешить запись ПБР-значений
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE); else ;
                //Запретить запись Админ-значений
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_markSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE); else ;

                strPBRNumber = getNamePBRNumber((int)GetPropertiesOfNameFilePPBRCSVValues()[1]);
            }
            else
                ;

            //Снять все признаки причин прекращения выполнения обработки событий
            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            foreach (TECComponent comp in allTECComponents)
                if ((comp.m_id > 100) && (comp.m_id < 500))
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                    {
                        switch (typeValues) {
                            case CONN_SETT_TYPE.ADMIN:
                                errRes = saveCSVValues(allTECComponents.IndexOf(comp), typeValues);
                                break;
                            case CONN_SETT_TYPE.PBR:
                                errRes = saveCSVValues(allTECComponents.IndexOf(comp), strPBRNumber);
                                break;
                            default:
                                break;
                        }

                        //if (! (errRes == Errors.NoError))
                        //    ; //Ошибка ???
                        //else
                        //    ;
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
            m_tableValuesResponse.Clear ();
            m_tableValuesResponse = null;

            if (typeValues == CONN_SETT_TYPE.PBR)
            {//Только для ПБР
                //Противоположные операции в 'ImpPPBRCSVValuesRequest'
                //Запретить запись ПБР-значений
                // , запрет устанавливается автоматически 
                //Разрешить запись Админ-значений
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE); else ;
            }
            else
                ;

            //Обновить значения на вкладке
            GetRDGValues (m_typeFields, prevIndxTECComponents);
        }

        private Errors saveCSVValues (int indx, object pbr_number) {
            Errors errRes = Errors.NoSet;

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];
            int hour = -1;
            double val = -1F;

            CONN_SETT_TYPE typeValues = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
            if (pbr_number is string)
                typeValues = CONN_SETT_TYPE.PBR;
            else
                if (pbr_number is CONN_SETT_TYPE)
                    typeValues = (CONN_SETT_TYPE)pbr_number; //ADMIN
                else
                    ;

            if ((typeValues == CONN_SETT_TYPE.PBR)
                || (typeValues == CONN_SETT_TYPE.ADMIN))
            {
                //Получить значения для сохранения
                DataRow [] rowsTECComponent = m_tableValuesResponse.Select(@"GTP_ID='" + allTECComponents[indx].name_future + @"'");
                //Проверить наличие записей для ГТП
                if (rowsTECComponent.Length > 0)
                {
                    foreach (DataRow r in rowsTECComponent)
                    {
                        hour = int.Parse(r[@"SESSION_INTERVAL"].ToString());

                        try
                        {
                            switch (typeValues)
                            {
                                case CONN_SETT_TYPE.PBR:                                    
                                    curRDGValues[hour].pbr = double.Parse(r[@"TotalBR"].ToString());
                                    curRDGValues[hour].pmin = double.Parse(r[@"PminBR"].ToString());
                                    curRDGValues[hour].pmax = double.Parse(r[@"PmaxBR"].ToString());

                                    curRDGValues[hour].pbr_number = pbr_number as string;

                                    ////Отладка
                                    //Console.WriteLine(@"GTP_ID=" + allTECComponents[indx].name_future + @"(" + hour + @") TotalBR=" + curRDGValues[hour].pbr + @"; PBRNumber=" + curRDGValues[hour].pbr_number);
                                    break;
                                case CONN_SETT_TYPE.ADMIN:
                                    curRDGValues[hour].recomendation = double.Parse(r[@"REC"].ToString());
                                    curRDGValues[hour].deviationPercent = Int16.Parse(r[@"IS_PER"].ToString()) == 1;
                                    curRDGValues[hour].deviation = double.Parse(r[@"DIVIAT"].ToString());
                                    curRDGValues[hour].fc = Int16.Parse(r[@"FC"].ToString()) == 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception e) {
                            Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET
                                , @"AdminTS_KomDisp::saveCSVValues () - GTP_ID=" + allTECComponents[indx].name_future + @"(" + hour + @")");

                            errRes = Errors.ParseError;
                        }

                        if (errRes == Errors.ParseError)
                            break;
                        else
                            ;
                    }

                    if (errRes == Errors.NoSet)
                    {
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
                        ; //errRes = Errors.ParseError;
                }
                else
                    errRes = Errors.ParseError;

                if (errRes == Errors.ParseError)
                    //Пропустить запись ГТП, разрешить переход к следующей
                    //Псевдо-закончена обработка всех событий
                    completeHandleStates();
                else
                    ;
            }
            else
                ;

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

            //arObjRes[0] = DateTime.Parse(nameFile.Substring(indxStartDateTime + 1, nameFile.Length - @".csv".Length - indxStartDateTime - 1), new System.Globalization.CultureInfo (@"ru-Ru"));
            arObjRes[0] = DateTime.Parse(nameFile.Substring(indxStartDateTime + 1, nameFile.Length - @".csv".Length - indxStartDateTime - 1));

            int indxStartSession = nameFile.IndexOf(s_strMarkSession, 0) + s_strMarkSession.Length
                , indxEndSession = nameFile.IndexOf(@")", indxStartSession);
            arObjRes[1] = Int32.Parse(nameFile.Substring(indxStartSession, indxEndSession - indxStartSession)) - 2;

            return arObjRes;
        }

        public object[] GetPropertiesOfNameFilePPBRCSVValues()
        {
            return GetPropertiesOfNameFilePPBRCSVValues(m_fullPathCSVValues);
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
