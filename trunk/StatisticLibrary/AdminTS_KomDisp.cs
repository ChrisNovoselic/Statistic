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
        public void ImpPPBRCSVValues(DateTime date, string fullPath)
        {
            lock (m_lockState)
            {
                ClearStates();

                ////НЕТ необходимости, т.к. импортируются значения для всех ГТП из списка 'PanelAdmin::m_listTECComponentIndex'
                //indxTECComponents = indx;

                m_fullPathPPBRCSVValue = fullPath;

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
        }

        private void ImpPPBRCSVValuesRequest()
        {
            //Разрешить запись ПБР-значений
            if (m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.ENABLED] == true) m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.MARK] = true; else ;

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

            if ((num_pbr > 0) && (num_pbr > serverTime.Hour))
            {
                //strPPBRCSVNameFileTemp = strPPBRCSVNameFile;
                strPPBRCSVNameFileTemp = m_fullPathPPBRCSVValue;

                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace("(", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(")", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(".", string.Empty);
                strPPBRCSVNameFileTemp = strPPBRCSVNameFileTemp.Replace(" ", string.Empty);

                ////при аргументе = каталог размещения наборов
                //strPPBRCSVNameFile = m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt;
                //strPPBRCSVNameFileTemp = m_PPBRCSVDirectory + strPPBRCSVNameFileTemp + strCSVExt;

                //File.Copy(strPPBRCSVNameFile, strPPBRCSVNameFileTemp, true);
                File.Copy(m_fullPathPPBRCSVValue, strPPBRCSVNameFileTemp, true);

                if (!(m_tablePPBRValuesResponse == null)) m_tablePPBRValuesResponse.Clear(); else ;

                if ((IsCanUseTECComponents() == true) && (strPPBRCSVNameFileTemp.Length > 0))
                    m_tablePPBRValuesResponse = DbTSQLInterface.Select(@"CSV_PATH" + System.IO.Path.GetDirectoryName(strPPBRCSVNameFileTemp),
                                                                            @"SELECT * FROM [" +
                                                                            System.IO.Path.GetFileName(strPPBRCSVNameFileTemp) +
                                                                            @"]"
                                                                            //+ @" WHERE GTP_ID='" +
                                                                            //allTECComponents[indxTECComponents].name_future +
                                                                            //@"'"
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
            bool bRes = m_tablePPBRValuesResponse.Rows.Count > 0 ? true : false;

            //'indxTECComponents' необходимо сохранить ???
            new Thread(new ParameterizedThreadStart(threadPPBRCSVValues)).Start(null); //можно установить значение аргумента = 'm_curDate'

            return bRes;
        }

        private void threadPPBRCSVValues(object date)
        {
            Errors errRes = Errors.NoError;
            
            int indxEv = -1
                , prevIndxTECComponents = indxTECComponents
                , curIndxTECComponents = -1
                , hour = -1;

            RDGStruct [] curRDGValues = new RDGStruct [m_curRDGValues.Length];
            DataRow [] rowsTECComponent;

            //Ожидать окончание обработки события 'PPBRCSVValues'
            m_waitHandleState[0].WaitOne();

            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            foreach (TECComponent comp in allTECComponents)
            {
                if ((comp.m_id > 100) && (comp.m_id < 500))
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0) {
                        //Запомнить текущий индекс компонента
                        curIndxTECComponents = allTECComponents.IndexOf (comp);
                        //Получить значения для сохранения
                        rowsTECComponent = m_tablePPBRValuesResponse.Select(@"ID=" + allTECComponents[curIndxTECComponents].name_future);
                        foreach (DataRow r in rowsTECComponent) {
                            hour = int.Parse (r [@"SESSION_INTERVAL"].ToString ());
                            
                            curRDGValues [hour].pbr = double.Parse (r [@"TotalBR"].ToString ());
                            curRDGValues [hour].pmin = double.Parse(r[@"PminBR"].ToString());
                            curRDGValues [hour].pmax = double.Parse(r[@"PmaxBR"].ToString());
                        }

                        //Копировать полученные значения в "текущий массив"
                        curRDGValues.CopyTo (m_curRDGValues, 0);

                        errRes = SaveChanges();
                    }
                    else
                        //Ошибка ???
                        break;
                }
                else
                    ;
            }

            //Очистить таблицу, полученную из CSV-файла
            m_tablePPBRValuesResponse.Clear ();
            m_tablePPBRValuesResponse = null;

            //Обновить значения на вкладке
            GetRDGValues (m_typeFields, prevIndxTECComponents);

            //m_bSavePPBRValues = true;
        }

        private Errors savePPBRCSVValues (int indx) {
            Errors errRes = Errors.NoError;

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

        public object[] GetPropertiesOfNameFilePPBRCSVValues()
        {
            object [] arObjRes = new object [2]; //0 - DateTime, 1 - int (номер ПБР)

            int indxStartDateTime = m_fullPathPPBRCSVValue.Length - @".csv".Length;
            while (Char.IsWhiteSpace(m_fullPathPPBRCSVValue, indxStartDateTime) == false)
            {
                indxStartDateTime --;
            }

            arObjRes[0] = DateTime.Parse(m_fullPathPPBRCSVValue.Substring(indxStartDateTime + 1, m_fullPathPPBRCSVValue.Length - @".csv".Length - indxStartDateTime - 1));

            int indxStartSession = m_fullPathPPBRCSVValue.IndexOf(s_strMarkSession, 0) + s_strMarkSession.Length
                , indxEndSession = m_fullPathPPBRCSVValue.IndexOf(@")", indxStartSession);
            arObjRes[1] = Int32.Parse(m_fullPathPPBRCSVValue.Substring(indxStartSession, indxEndSession - indxStartSession)) - 2;

            return arObjRes;
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
