using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data;
using System.Linq;
using ASUTP.Helper;
using ASUTP;
using ASUTP.Core;
using ASUTP.Database;



namespace StatisticCommon
{
    public partial class AdminTS_KomDisp : AdminTS
    {
        /// <summary>
        /// ������ ���� ��� ����� CSV-������ ��� ������� ��� �� *.csv
        /// </summary>
        private string m_fullPathCSVValues;
        /// <summary>
        /// ������ ��� ������������ ����� CSV-������
        /// </summary>
        private static string s_strMarkSession = @"���(���������) ������(";
        /// <summary>
        /// ����������� - ��������(� �����������)
        /// </summary>
        /// <param name="arMarkSavePPBRValues">������ � ���������� ������������� ����� ���� � ����� ���� ��������</param>
        public AdminTS_KomDisp(bool[] arMarkSavePPBRValues)
            : base(arMarkSavePPBRValues, TECComponentBase.TYPE.ELECTRO)
        {
            delegateImportForeignValuesRequest = impCSVValuesRequest;
            delegateImportForeignValuesResponse = impCSVValuesResponse;

            _listCSVValuesFields = new List<string []> () {
                new string [] { @"GTP_ID", @"SESSION_INTERVAL", @"REC", @"IS_PER", @"DIVIAT", @"FC" } // CONN_SETT_TYPE.ADMIN
                , new string [] { @"GTP_ID", @"SESSION_INTERVAL", @"TotalBR", @"PminBR", @"PmaxBR" } // CONN_SETT_TYPE.PBR
            };

            ////������� 'HMath.doubleParse'
            //string strVal = @"456,890";
            //double val = -1F;

            //HMath.doubleParse(strVal, out val);

            //strVal = @"456890"; val = -1F;
            //HMath.doubleParse(strVal, out val);

            //strVal = @"0,007"; val = -1F;
            //HMath.doubleParse(strVal, out val);

            //strVal = @".006"; val = -1F;
            //HMath.doubleParse(strVal, out val);

            //strVal = @"23,000000000"; val = -1F;
            //HMath.doubleParse(strVal, out val);

            //strVal = @"567;890"; val = -1F;
            //HMath.doubleParse(strVal, out val);

            _msExcelIOExportPBRValues = new MSExcelIOExportPBRValues();
            _msExcelIOExportPBRValues.Result += new Action<object, MSExcelIOExportPBRValues.EventResultArgs> (msExcelIOExportPBRValues_onResult);
        }

        public event Action<MSExcelIOExportPBRValues.EventResultArgs> EventExportPBRValues;

        private void msExcelIOExportPBRValues_onResult(object sender, MSExcelIOExportPBRValues.EventResultArgs e)
        {
            string errMes = string.Empty;

            switch (e.Result) {
                case MSExcelIOExportPBRValues.RESULT.OK:
                    break;
                case MSExcelIOExportPBRValues.RESULT.VISIBLE:
                    //??? �� �������� � ��������� ������� �������� (~ HCLASSLIBRARY_MSEXCELIO)
                    if (_msExcelIOExportPBRValues.AllowVisibled == true)
                        _msExcelIOExportPBRValues.Visible = true;
                    else
                        ;
                    break;
                case MSExcelIOExportPBRValues.RESULT.SHEDULE:
                    EventExportPBRValues (e);
                    break;
                case MSExcelIOExportPBRValues.RESULT.ERROR_OPEN:
                    errMes = string.Format ("�� ������� ������� ����� ��� �������� ���-��������");
                    break;
                case MSExcelIOExportPBRValues.RESULT.ERROR_RETRY:
                    errMes = string.Format ("������� �������� ������� ����� ��� �������� ���-��������");
                    break;
                case MSExcelIOExportPBRValues.RESULT.ERROR_TEMPLATE:
                    errMes = string.Format ("�� ������ ������ ��� �������� ���-��������");
                    break;
                case MSExcelIOExportPBRValues.RESULT.ERROR_WAIT:
                    errMes = string.Format("��������� ����� ��������({0} ���) ��� �������� ���-��������", MS_WAIT_EXPORT_PBR_MAX / 1000);
                    break;
                case MSExcelIOExportPBRValues.RESULT.ERROR_APP:
                    errMes = string.Format ("��� ������� �� MS Excel ��� �������� ���-��������");
                    break;
                default:
                    break;
            }

            if (errMes.Equals (string.Empty) == false)
                ErrorReport (errMes);
            else
                ;
        }

        /// <summary>
        /// ����� �� ������ ���./����.
        /// </summary>
        /// <param name="date">����, ��������� ������������ ��� ������� �������� �� ����� CSV-������</param>
        /// <param name="fullPath">������ ���� � ����� CSV-������</param>
        /// <returns>�������(�����/������) ���������� ������</returns>
        public int ImpCSVValues(DateTime date, string fullPath)
        {
            int iRes = 0; //��� ������

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

                    ////��� �������������, �.�. ������������� �������� ��� ���� ��� �� ������ 'PanelAdmin::m_listTECComponentIndex'
                    //indxTECComponents = indx;

                    ClearValues();

                    using_date = false;
                    //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                    m_prevDate = date.Date;
                    m_curDate = m_prevDate;

                    AddState((int)StatesMachine.CSVValues);

                    Run(@"AdminTS::ImpRDGExcelValues ()");
                }
            else
                ; //������

            if (!(iRes == 0))
                m_fullPathCSVValues = string.Empty;
            else ;

            return iRes;
        }

        private void impCSVValues(out int err)
        {
            err = -1;

            object[] objRow = null;
            int iCol = -1
                , hour = -1;
            List<TECComponent> listGTP;
            List<DataRow> rowsTECComponent;
            DataRow rowNew;

            if (!(m_fullPathCSVValues == string.Empty)) {
                if (File.Exists(m_fullPathCSVValues) == true) {
                    string strCSVNameFileTemp = Path.GetFileNameWithoutExtension(m_fullPathCSVValues);

                    if ((IsCanUseTECComponents == true)
                        && (strCSVNameFileTemp.Length > 0)) {
                        m_tableValuesResponse = DbTSQLInterface.CSVImport(m_fullPathCSVValues
                                                                            , @"*"
                                                                            , out err);
                        //??? GemBox.SpeadSheet ������������ 150 ����� �� ����
                        //ExcelFile excel = new ExcelFile();
                        //excel.LoadCsv(m_fullPathCSVValues, ';');
                        //ExcelWorksheet w = excel.Worksheets[0];
                        //m_tableValuesResponse = new DataTable();
                        //foreach (ExcelRow r in w.Rows)
                        //{
                        //    if (r.Index > 0) {
                        //        objRow = new object[m_tableValuesResponse.Columns.Count];

                        //        iCol = 0;
                        //        foreach (ExcelCell c in r.Cells) {
                        //            if ((!(c.Value == null))
                        //                && (string.IsNullOrEmpty(c.Value.ToString()) == false))
                        //                objRow[iCol] = c.Value.ToString().Trim();
                        //            else
                        //                ;

                        //            iCol++;
                        //        }

                        //        m_tableValuesResponse.Rows.Add(objRow);

                        //        //for (int i = 1; i < 24; i++) {
                        //        //    m_tableValuesResponse.Rows.Add
                        //        //        (new object[] {
                        //        //            r.Cells[0].Value
                        //        //            , r.Cells[1].Value
                        //        //            , i
                        //        //            , r.Cells[3].Value
                        //        //            , r.Cells[4].Value
                        //        //            , r.Cells[5].Value
                        //        //            , r.Cells[6].Value });
                        //        //}
                        //    } else {
                        //        foreach (ExcelCell c in r.Cells)
                        //            if ((!(c.Value == null))
                        //                && (string.IsNullOrEmpty(c.Value.ToString()) == false))
                        //                m_tableValuesResponse.Columns.Add(c.Value.ToString().Trim());
                        //            else
                        //                ;
                        //    }
                        //}

                        // �������� ��� ���
                        listGTP = allTECComponents.FindAll(comp => { return comp.IsGTP == true; });
                        // ��������� ������� ���� ������� �������� ��� ������� �� ���
                        // ��� ������������� �������� ������������� �������� (��� ����� ���������� ������)
                        rowsTECComponent = new List<DataRow>(new DataRow[1]);
                        foreach (TECComponent gtp in listGTP) {
                            rowsTECComponent = new List<DataRow>(m_tableValuesResponse.Select(@"GTP_ID='" + gtp.name_future + @"'"));

                            if ((rowsTECComponent.Count > 0)
                                && (rowsTECComponent.Count < 24)) {
                                hour = rowsTECComponent.Count;

                                while (hour < 24) {
                                    rowNew = m_tableValuesResponse.Rows.Add(rowsTECComponent[rowsTECComponent.Count - 1].ItemArray);

                                    if (m_tableValuesResponse.Columns.Contains(@"SESSION_INTERVAL") == true)
                                        rowNew[@"SESSION_INTERVAL"] = ++hour - 1;
                                    else
                                        ;
                                }
                            } else
                                ;
                        };

                        //err = 0;
                    }
                    else
                        ;
                } else
                    err = -2; //���� �� ���������� (����� ����������, �.�. ������ � ������� ����������� ����)
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

        private void impCSVValuesRequest()
        {
            int err = -1
                //, num_pbr = (int)GetPropertiesOfNameFilePPBRCSVValues()[1]
                ;

            delegateStartWait();

            ////����������� ������ ����������� ������ (��� ��������� = ������� ���������� �������)
            //while ((num_pbr > 0) && (File.Exists(m_PPBRCSVDirectory + strPPBRCSVNameFile + strCSVExt) == false))
            //{
            //    num_pbr -= 2;
            //    strPPBRCSVNameFile = getNameFileSessionPPBRCSVValues(num_pbr);
            //}

            ////if ((num_pbr > 0) && (num_pbr > serverTime.Hour))
            //if ((num_pbr > 0) && (! (num_pbr < GetPBRNumber())))
                impCSVValues(out err);
            //else
            //    Logging.Logg().Action(@"�������� ������ ����� �������� ������, ��� ���./���");

            delegateStopWait();
        }

        private static List<string []> _listCSVValuesFields;

        private int impCSVValuesResponse()
        {
            int iRes = -1;

            //??? ����������� 3-�(2-�� ������) ����, �.�. 1-�� 2 ���� ��������� ("GTP_ID", "SESSION_INTERVAL")
            int indxFieldtypeValues = 2;

            //���������� ��� ����������� ��������
            // �� ������� � ����������� ������� ���� � �������� <indxFieldtypeValues>
            CONN_SETT_TYPE typeValues = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
            for (typeValues = CONN_SETT_TYPE.ADMIN; typeValues < (CONN_SETT_TYPE.PBR + 1); typeValues ++)
                if (m_tableValuesResponse.Columns.Contains(_listCSVValuesFields [(int)typeValues][indxFieldtypeValues]) == true)
                    break;
                else
                    ;
            // � ~ �� ���� �������� (ADMIN | PBR) ��������� ������� ���� ����������� ��������
            if (typeValues < (CONN_SETT_TYPE.PBR + 1))
                iRes = CheckNameFieldsOfTable(m_tableValuesResponse, _listCSVValuesFields [(int)typeValues]) == true ? 0 : -1;
            else;

            if (iRes == 0)
                //'indxTECComponents' ���������� ��������� ??? - ����������� � ������ !!!
                new Thread(new ParameterizedThreadStart(threadCSVValues)).Start(typeValues);
            else
                Logging.Logg().Error(@"AdminTS_KomDisp::ImpCSVValuesResponse () - ������� ������� �� ������������� �����������...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        private void threadCSVValues(object type)
        {
            Errors errRes = Errors.NoError;

            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //new System.Globalization.CultureInfo(@"en-US")

            FormChangeMode.KeyDevice key;
            //���������� ��� ����������� ��������
            CONN_SETT_TYPE typeValues = (CONN_SETT_TYPE)type;
            object arg;

            INDEX_WAITHANDLE_REASON indxEv = INDEX_WAITHANDLE_REASON.SUCCESS;
            FormChangeMode.KeyDevice prevKeyTECComponents = CurrentKey;
            string strPBRNumber = string.Empty; // ...������ ��� ���

            if (typeValues == CONN_SETT_TYPE.PBR)
            {//������ ��� ���
                //��������������� �������� ��� ���������� ������ 'threadPPBRCSVValues'
                //��������� ������ ���-��������
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.PBR_SAVED); else ;
                //��������� ������ �����-��������
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_markSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_SAVED); else ;

                strPBRNumber = getNamePBRNumber((int)GetPropertiesOfNameFilePPBRCSVValues()[1] - 1);
            }
            else
                ;

            //����� ��� �������� ������ ����������� ���������� ��������� �������
            ResetSyncState ();

            foreach (TECComponent comp in allTECComponents)
                if (comp.IsGTP == true) //�������� ���
                {
                    arg = null;
                    key = new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = comp.Mode };
                    indxEv = WaitAny(Constants.MAX_WATING, true); // System.Threading.Timeout.Infinite
                    switch ((INDEX_WAITHANDLE_REASON)indxEv) {
                        case INDEX_WAITHANDLE_REASON.SUCCESS:
                            switch (typeValues) {
                                case CONN_SETT_TYPE.ADMIN:
                                    arg = typeValues;
                                    break;
                                case CONN_SETT_TYPE.PBR:
                                    arg = strPBRNumber;
                                    break;
                                default:
                                    break;
                            }

                            if (Equals (arg, null) == false) {
                                errRes = saveCSVValues (key, arg);

                                //if (! (errRes == Errors.NoError))
                                //    ; //������ ???
                                //else
                                //    ;
                            } else
                                ;
                            break;
                        case INDEX_WAITHANDLE_REASON.ERROR:
                            Logging.Logg ().Error ($"AdminTS_KomDisp::threadCSVValues () - ������ ��� <{key.ToString ()}>..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                        //case INDEX_WAITHANDLE_REASON.BREAK: � ���� ������� �� ������������ (��. InitSyncState)
                        //    break;
                        case INDEX_WAITHANDLE_REASON.TIMEOUT:
                            Logging.Logg ().Warning ($"AdminTS_KomDisp::threadCSVValues () - ����� �������� <{Constants.MAX_WATING} ����> ������� ��� <{key.ToString ()}>..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                        default:
                            //������ ???
                            //break;
                            //completeHandleStates();

                            Logging.Logg ().Warning ($"AdminTS_KomDisp::threadCSVValues () - ����������� ��������� �������� ������������� ��� �������� ��� <{key.ToString ()}>..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                    }
                }
                else
                    ;

            //�������� �������, ���������� �� CSV-�����
            m_tableValuesResponse.Clear ();
            m_tableValuesResponse = null;

            if (typeValues == CONN_SETT_TYPE.PBR)
            {//������ ��� ���
                //��������������� �������� � 'ImpPPBRCSVValuesRequest'
                //��������� ������ ���-��������
                // , ������ ��������������� ������������� 
                //��������� ������ �����-��������
                if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED) == true) m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_SAVED); else ;
            }
            else
                ;

            //�������� �������� �� �������
            GetRDGValues (prevKeyTECComponents, false);
        }

        private Errors saveCSVValues (FormChangeMode.KeyDevice key, object pbr_number) {
            Errors errRes = Errors.NoSet;

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];
            int hour = -1;
            double val = -1F;
            string name_future = string.Empty;
            List<DataRow> rowsTECComponent = null;

            CONN_SETT_TYPE typeValues = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
            if (pbr_number is string)
            // ���� ������, �� ������������� ��������� ����� ���
                typeValues = CONN_SETT_TYPE.PBR;
            else if (pbr_number is CONN_SETT_TYPE)
            //!!! ������ CONN_SETT_TYPE.ADMIN
                typeValues = (CONN_SETT_TYPE)pbr_number;
            else
                ;
            // ��������� ��� �� ��������� ��� ����������� ��������
            //  � ����� �� ������� ��������������� ���� �������� ��������� (������������ �� � ������� ����������� ����)
            if ((!(typeValues == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
                && (CheckNameFieldsOfTable (m_tableValuesResponse, _listCSVValuesFields[(int)typeValues]) == true)) {
                //�������� �������� ��� ���������
                name_future = allTECComponents.Find(comp => comp.m_id == key.Id).name_future;
                rowsTECComponent = new List<DataRow>(m_tableValuesResponse.Select(@"GTP_ID='" + name_future + @"'"));
                //������� �2 - ��������
                //foreach (DataRow r in m_tableValuesResponse.Rows)
                //    if (name_future.Equals(r["GTP_ID"]) == true)
                //        rowsTECComponent.Add(r);
                //    else
                //        ;
                //��������� ������� ������� ��� ���
                if (rowsTECComponent.Count > 0) {
                //!!! ������ ���� 24 ������ (�������������� ����������� ��� ��������)
                    if (rowsTECComponent.Count < 24) {
                        Logging.Logg ().Error (string.Format (@"AdminTS_KomDisp::saveCSVValues () - ��� ���(��={0}) ���������� �������={1} ..."
                            , name_future, rowsTECComponent.Count)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    } else
                        ;

                    foreach (DataRow r in rowsTECComponent) {
                        hour = int.Parse(r[@"SESSION_INTERVAL"].ToString());

                        try {
                            switch (typeValues) {
                                case CONN_SETT_TYPE.PBR:
                                    HMath.doubleParse(r[@"TotalBR"].ToString(), out curRDGValues[hour].pbr);
                                    HMath.doubleParse(r[@"PminBR"].ToString(), out curRDGValues[hour].pmin);
                                    HMath.doubleParse(r[@"PmaxBR"].ToString(), out curRDGValues[hour].pmax);

                                    curRDGValues[hour].pbr_number = pbr_number as string;

                                    ////�������
                                    //Console.WriteLine(@"GTP_ID=" + allTECComponents[indx].name_future + @"(" + hour + @") TotalBR=" + curRDGValues[hour].pbr + @"; PBRNumber=" + curRDGValues[hour].pbr_number);
                                    break;
                                case CONN_SETT_TYPE.ADMIN:
                                    HMath.doubleParse(r[@"REC"].ToString(), out curRDGValues[hour].recomendation);
                                    curRDGValues[hour].deviationPercent = Int16.Parse(r[@"IS_PER"].ToString()) == 1;
                                    HMath.doubleParse(r[@"DIVIAT"].ToString(), out curRDGValues[hour].deviation);
                                    curRDGValues[hour].fc = Int16.Parse(r[@"FC"].ToString()) == 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception e) {
                            Logging.Logg().Exception(e
                                , $@"AdminTS_KomDisp::saveCSVValues () - GTP_ID={FindTECComponent(key).name_shr}, ��� ({hour})..."
                                , Logging.INDEX_MESSAGE.NOT_SET);

                            errRes = Errors.ParseError;
                        }

                        if (errRes == Errors.ParseError)
                            break;
                        else {
                                
                        }
                            ;
                    }

                    if (errRes == Errors.NoSet)
                    {
                        //�������� ���./������ � �������
                        ClearValues();

                        //���������� ���������� �������� � "������� ������"
                        curRDGValues.CopyTo(m_curRDGValues, 0);

                        CurrentKey = key;

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
                    //���������� ������ ���, ��������� ������� � ���������
                    //������-��������� ��������� ���� �������
                    completeHandleStates(INDEX_WAITHANDLE_REASON.SUCCESS);
                else
                    ;
            }
            else
                ;

            return errRes;
        }

        protected override void InitializeSyncState(int capacity = 1)
        {
            base.InitializeSyncState((int)INDEX_WAITHANDLE_REASON.ERROR + 1);

            AddSyncState (INDEX_WAITHANDLE_REASON.ERROR, typeof (ManualResetEvent), false);
        }

        public static object[] GetPropertiesOfNameFilePPBRCSVValues(string nameFile)
        {
            object[] arObjRes = new object[2]; //0 - DateTime, 1 - int (����� ���)

            int indxStartDateTime = nameFile.Length - @".csv".Length;
            while (Char.IsWhiteSpace(nameFile, indxStartDateTime) == false)
            {
                indxStartDateTime--;
            }

            //arObjRes[0] = DateTime.Parse(nameFile.Substring(indxStartDateTime + 1, nameFile.Length - @".csv".Length - indxStartDateTime - 1), new System.Globalization.CultureInfo (@"ru-Ru"));
            arObjRes[0] = DateTime.Parse(nameFile.Substring(indxStartDateTime + 1, nameFile.Length - @".csv".Length - indxStartDateTime - 1));

            int indxStartSession = nameFile.IndexOf(s_strMarkSession, 0) + s_strMarkSession.Length
                , indxEndSession = nameFile.IndexOf(@")", indxStartSession);
            arObjRes[1] = Int32.Parse(nameFile.Substring(indxStartSession, indxEndSession - indxStartSession)) - 1;

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
                @") �� " +
                (m_curDate.AddDays(offset_day)).Date.GetDateTimeFormats()[0];
        }

        #region ������� �������� � MS Excel-���� ��� ��������� � ��������� �� �����. ���������
        /// <summary>
        /// �������� ������� - ������������ ��� �������� ���������� �������� ��������
        /// </summary>
        public static int MS_WAIT_EXPORT_PBR_MAX = 16666
            , MS_WAIT_EXPORT_PBR_ABORT = 666
            , SEC_SHEDULE_START_EXPORT_PBR = 46 * 60
            , SEC_SHEDULE_PERIOD_EXPORT_PBR = 60 * 60;

        public static string Folder_CSV = string.Empty;

        public enum MODE_EXPORT_PBRVALUES
        {
            MANUAL
            , AUTO
                , COUNT
        }

        public void SetModeExportPBRValues(MODE_EXPORT_PBRVALUES mode)
        {
            _msExcelIOExportPBRValues.Mode = mode;
        }

        public void SetAllowMSExcelVisibledExportPBRValues(bool bVisibled)
        {
            _msExcelIOExportPBRValues.AllowVisibled = bVisibled;
        }

        public void SetSheduleExportPBRValues(TimeSpan tsShedule)
        {
            SEC_SHEDULE_START_EXPORT_PBR = (int)tsShedule.TotalSeconds;
        }

        public void SetPeriodExportPBRValues(TimeSpan tsPeriod)
        {
            SEC_SHEDULE_PERIOD_EXPORT_PBR = (int)tsPeriod.TotalSeconds;
        }

        public override void Stop ()
        {
            if (_msExcelIOExportPBRValues.Busy == true)
                _msExcelIOExportPBRValues.Abort ();
            else
                ;

            base.Stop ();
        }

        /// <summary>
        /// ������ ��� ����������/���������� ����� MS Excel ����������
        ///  , ����������� ������� �������� ��� ���� ��� ��������������� (�� ����� �� ��������� �� ��) 
        /// </summary>
        private MSExcelIOExportPBRValues _msExcelIOExportPBRValues;
        
        /// <summary>
        /// ���� ��� �������� ������ � ������ 'AUTO'
        /// </summary>
        public DateTime DateDoExportPBRValues
        {
            get
            {
                DateTime datetimeRes
                    , datetimeMsc;

                if (_msExcelIOExportPBRValues.Mode == MODE_EXPORT_PBRVALUES.AUTO) {
                    datetimeMsc = HDateTime.ToMoscowTimeZone();
                    datetimeRes = datetimeMsc.Hour < 23 ? datetimeMsc.Date : datetimeMsc.Date.AddDays(1);
                } else
                // ������� �������� ���������� � ��������� (��������� � ��������� �� �������)
                    datetimeRes = DateTime.MinValue;

                return datetimeRes;
            }
        }

        /// <summary>
        /// ����������� ������ ��������������� ��� ��� ������������ ������� �� ��������� ������
        /// </summary>
        /// <returns>���� 0-�� ������������ �� ������</returns>
        public override FormChangeMode.KeyDevice PrepareActionRDGValues()
        {
            List<FormChangeMode.KeyDevice> listKey = GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT.GTP, true);

            if (_listTECComponentKey == null)
                _listTECComponentKey = new List<FormChangeMode.KeyDevice>();
            else
                ;

            try {
                // ��������� �� ������� ����������
                if (listKey.Count - listKey.Distinct ().Count () == 0) {
                    _listTECComponentKey.Clear ();
                    listKey.ForEach ((key) => {
                        if (_listTECComponentKey.Contains (key) == false)
                            _listTECComponentKey.Add (key);
                        else
                            Logging.Logg ().Error (string.Format ("AdminTS_KomDisp::PrepareExportRDGValues () - ���������� �������������� ������� {0}...", key.ToString ()), Logging.INDEX_MESSAGE.NOT_SET);
                    });

                    if (_msExcelIOExportPBRValues.Busy == true)
                        _msExcelIOExportPBRValues.Abort ();
                    else
                        ;
                } else
                    Logging.Logg ().Error (string.Format ("AdminTS_KomDisp::PrepareExportRDGValues () - � ���������� ������ <{0}> ���� ���������...", string.Join (",", listKey.Select (key => key.ToString ()).ToArray ()))
                        , Logging.INDEX_MESSAGE.NOT_SET);

                Logging.Logg ().Action ($"AdminTS_KomDisp::PrepareExportRDGValues () - ����������� ������ ��� ��������: <{string.Join (", ", _listTECComponentKey.ConvertAll<string> (key => key.Id.ToString ()).ToArray ())}>..."
                    , Logging.INDEX_MESSAGE.D_006);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, string.Format("AdminTS_KomDisp::PrepareExportRDGValues () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return base.PrepareActionRDGValues();
        }

        #region ��������� ���� �������� ���-��������
        /// <summary>
        /// ��� �������� ������ ��� ������������� � ��������� ������
        /// </summary>
        /// <param name="nextIndex">��������� ������</param>
        /// <param name="date">����, �� ������� ��������� ��������/��������� ��������</param>
        /// <param name="currentIndex">������� ������ �� ������ ��������-����������� (�.�. == listTECComponentIndex[0])</param>
        /// <param name="listTECComponentIndex">������ �������� ���������� � ���������</param>
        public delegate void DelegateUnitTestExportPBRValuesRequest (FormChangeMode.KeyDevice nextKey, DateTime date, FormChangeMode.KeyDevice currentKey, IEnumerable<FormChangeMode.KeyDevice> listTECComponentKey);

        private DelegateUnitTestExportPBRValuesRequest _eventUnitTestExportPBRValuesRequest;

        public event DelegateUnitTestExportPBRValuesRequest EventUnitTestExportPBRValuesRequest
        {
            add
            {
                if (Equals (_eventUnitTestExportPBRValuesRequest, null) == true)
                    _eventUnitTestExportPBRValuesRequest += value;
                else
                    ;
            }

            remove
            {
                if (Equals (_eventUnitTestExportPBRValuesRequest, null) == false) {
                    _eventUnitTestExportPBRValuesRequest -= value;
                    _eventUnitTestExportPBRValuesRequest = null;
                } else
                    ;
            }
        }
        #endregion

        /// <summary>
        ///  �������� �������� ��� ��������
        /// </summary>
        /// <param name="compValues">�������� (�����. + ���) ��� ������ �� ����������� ���</param>
        /// <param name="date">����, �� ������� �������� ��������</param>
        /// <returns>��������� ������ ��� ������� �������� �� ��</returns>
        public FormChangeMode.KeyDevice AddValueToExportRDGValues(RDGStruct[]compValues, DateTime date)
        {
            FormChangeMode.KeyDevice keyRes = new FormChangeMode.KeyDevice();

            if ((date - DateTime.MinValue.Date).Days > 0) {
                if ((_listTECComponentKey.Count > 0)
                    && (!(CurrentKey.Id < 0))
                    && (!(_listTECComponentKey [0].Id < 0))) {
                    if (CurrentKey.Id - _listTECComponentKey[0].Id == 0) {
                        //??? ������ �� � ���� ������
                        //Logging.Logg().Action(string.Format("AdminTS_KomDisp::AddValueToExportRDGValues () - �������� �������� ��� [ID={0}, Index={1}, �� ����={2}, ���-��={3}] ����������..."
                        //        , _listTECComponentKey[0].Id, _listTECComponentKey[0], date, compValues.Length)
                        //    , Logging.INDEX_MESSAGE.D_006);

                        actionReport (string.Format ("�������� �������� ��� [ID={0}, Index={1}, �� ����={2}, ���-��={3}] ����������..."
                            , _listTECComponentKey[0].Id, _listTECComponentKey[0], date, compValues.Length));

                        if ((_msExcelIOExportPBRValues.AddTECComponent(allTECComponents.Find(comp => comp.m_id == CurrentKey.Id)) == 0)
                            && (_msExcelIOExportPBRValues.SetDate(date) == true)) {
                            TECComponentComplete(-1, true); // ���������� ���� �� ������ (��. ���������� ���������)

                            //Console.WriteLine(@"AdminTS_KomDisp::AddValueToExportRDGValues () - ��������� ��������=[{0}], ���������� ���������={1}", indxTECComponents, _lisTECComponentIndex.Count);

                            // �������� �������� �� ���������� �����: [DateTime, Index]
                            _msExcelIOExportPBRValues.AddPBRValues(CurrentKey.Id, compValues);

                            keyRes = FirstTECComponentKey;
                            // ��������� �������� ����� �������� ��������
                            if (keyRes == FormChangeMode.KeyDevice.Empty) {
                            // ��� �������� �� ���� ����������� ��������/���������
                                Logging.Logg().Action(string.Format("AdminTS_KomDisp::AddValueToExportRDGValues () - �������� ��� �������� ��� ���� �����������...")
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                                actionReport (string.Format ("�������� ��� �������� ��� ���� �����������..."));

                                _msExcelIOExportPBRValues.Run();
                                // ���������� �������� ������� ����������
                                keyRes.Id = 0;
                            } else
                            // ��������� ������ ���������� ��� ��������
                                ;
                        } else {
                            Logging.Logg().Error(string.Format($"AdminTS_KomDisp::AddValueToExportRDGValues () - ��������� � ������ [{CurrentKey.ToString ()}] �� ����� ���� �������� (����. �������� �������� �� ���������)...")
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            // ��������� ���������
                            _listTECComponentKey.Clear ();

                            _msExcelIOExportPBRValues.Abort ();
                            // ���������� ������� ���������� ����������
                            keyRes.Id = -1;

                            errorReport ("������� ���-��������");
                        }
                    } else
                    // ������� ������ � 0-�� ������� ������� �������� ������ ���������
                         Logging.Logg().Error(string.Format($"AdminTS_KomDisp::AddValueToExportRDGValues () - ������� ���� <{CurrentKey.ToString ()}> � 0-�� <{_listTECComponentKey [0].ToString()}> ������� ������� �������� �� ���������...")
                            , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                //??? ������, �.�. �������� ������ � �������� ��������, � ������ ���������� �� ��������
                    Logging.Logg().Error(string.Format("AdminTS_KomDisp::AddValueToExportRDGValues () - �������� �������� ��� ������������ ���������� ����={0}..."
                        , CurrentKey.ToString())
                            , Logging.INDEX_MESSAGE.NOT_SET);
            } else
            // ���� ��� ���������� �������� ����������
                ;

            _eventUnitTestExportPBRValuesRequest?.Invoke (keyRes, date, CurrentKey, _listTECComponentKey);

            return keyRes;
        }
        #endregion
    }
}
