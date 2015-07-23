using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;

namespace StatisticCommon
{
    public class AdminTS_NSS : AdminTS
    {
        public List<int> m_listTECComponentIndexDetail;
        public List <RDGStruct []> m_listCurRDGValues;

        public List <Errors>  m_listResSaveChanges;

        public ManualResetEvent m_evSaveChangesComplete;

        private object m_lockSuccessGetData,
                        m_lockResSaveChanges;
        public bool CompletedGetRDGValues {
            get {
                lock (m_lockSuccessGetData)
                {
                    return (! (m_listCurRDGValues.Count < m_listTECComponentIndexDetail.Count)) ? true : false;
                }
            }
        }

        public bool CompletedSaveChanges {
            get {
                bool bRes = false;
                
                lock (m_lockResSaveChanges)
                {
                    bRes = ! ((m_listResSaveChanges.Count) < m_listTECComponentIndexDetail.Count);
                }

                return bRes;
            }
        }

        public bool SuccessSaveChanges {
            get {
                bool bRes = true;

                lock (m_lockResSaveChanges)
                {
                    if ((m_listResSaveChanges.Count + 1) == m_listTECComponentIndexDetail.Count)
                        foreach (Errors err in m_listResSaveChanges)
                        {
                            if (!(err == Errors.NoError)) {
                                bRes = false;

                                break;
                            }
                            else
                                ;
                        }
                    else
                        bRes = false;
                }

                return bRes;
            }
        }

        public AdminTS_NSS(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues)
        {
            delegateImportForeignValuesRequuest = ImpRDGExcelValuesRequest;
            delegateExportForeignValuesRequuest = ExpRDGExcelValuesRequest;
            delegateImportForeignValuesResponse = ImpRDGExcelValuesResponse;
            //delegateExportForeignValuesResponse = ExpRDGExcelValuesResponse;

            m_listCurRDGValues = new List<RDGStruct[]> ();
            m_listTECComponentIndexDetail = new List<int> ();
            m_listResSaveChanges = new List <Errors> ();

            m_lockSuccessGetData = new object();
            m_lockResSaveChanges = new object ();

            m_evSaveChangesComplete = new ManualResetEvent (false);
        }

        protected override int GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            int iRes = base.GetAdminValuesResponse(tableAdminValuesResponse, date);

            RDGStruct []curRDGValues = new RDGStruct [m_curRDGValues.Length];            

            //curRDGValues = (RDGStruct[])m_curRDGValues.Clone();

            m_curRDGValues.CopyTo(curRDGValues, 0);

            for (int i = 0; i < m_curRDGValues.Length; i ++) {
                curRDGValues [i].pbr += m_curRDGValues [i].recomendation;

                //curRDGValues [i].plan = m_curRDGValues [i].plan;

                //curRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                //curRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                //curRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }

            m_listCurRDGValues.Add (curRDGValues);

            return iRes;
        }

        public void fillListIndexTECComponent (int id) {
            lock (m_lockSuccessGetData)
            {
                m_listTECComponentIndexDetail.Clear();
                //Сначала - ГТП
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == id) && //Принадлежит ТЭЦ
                        (((comp.m_id > 100) && (comp.m_id < 500)) /*|| //Является ГТП
                        ((comp.m_id > 1000) && (comp.m_id < 10000))*/)) //Является ТГ
                    {                    
                        m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));
                    }
                    else
                        ;
                }

                //Потом - ТГ
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == id) && //Принадлежит ТЭЦ
                        (/*((comp.m_id > 100) && (comp.m_id < 500)) ||*/ //Является ГТП
                        ((comp.m_id > 1000) && (comp.m_id < 10000)))) //Является ТГ
                    {                    
                        m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));
                    }
                    else
                        ;
                }

                m_listCurRDGValues.Clear();
            }
        }

        private void threadGetRDGValuesWithoutDate(object obj)
        {
            int indxEv = -1;

            //lock (m_lockSuccessGetData)
            //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny (m_waitHandleState);
                    if (indxEv == 0)
                        base.GetRDGValues(m_typeFields, indx);
                    else
                        break;
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public override void GetRDGValues(TYPE_FIELDS mode, int id)
        {
            //delegateStartWait ();
            fillListIndexTECComponent(id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithoutDate)).Start ();
            //threadGetRDGValuesWithoutDate (null);
            //delegateStopWait ();
        }

        private void threadGetRDGValuesWithDate(object date)
        {
            int indxEv = -1;

            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            //lock (m_lockSuccessGetData)
            //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                        base.GetRDGValues((int)m_typeFields, indx, (DateTime)date);
                    else
                        break;
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int id, DateTime date)
        {
            //delegateStartWait ();
            fillListIndexTECComponent (id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithDate)).Start (date);
            //threadGetRDGValuesWithDate (date);
            //delegateStopWait ();
        }

        private void threadImpRDGExcelValues (object date) {
            int indxEv = -1;

            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            //lock (m_lockSuccessGetData)
            //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                        if (modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                            base.GetRDGValues((int)m_typeFields, indx, (DateTime)date);
                        else
                            if (modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                                base.ImpRDGExcelValues(indx, (DateTime)date);
                            else
                                ;
                    else
                        break;
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public override void ImpRDGExcelValues(int id, DateTime date)
        {
            //delegateStartWait();

            m_listCurRDGValues.Clear();

            new Thread(new ParameterizedThreadStart(threadImpRDGExcelValues)).Start (date);
            //threadGetRDGExcelValues (date);

            //delegateStopWait();
        }

        public string [] GetListNameTEC()
        {
            int indx = -1;
            List<string> listRes = new List<string> ();
            List<int> listIdTEC = new List<int>();

            foreach (TECComponent comp in allTECComponents)
            {
                indx = comp.tec.m_id;
                if (listIdTEC.IndexOf(indx) < 0) {
                    listIdTEC.Add (indx);
                    
                    listRes.Add(comp.tec.name_shr);
                }
                else
                    ;
            }

            return listRes.ToArray ();
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            for (int i = 0; i < 24; i++)
            {
                if (m_prevRDGValues[i].pbr.Equals (m_curRDGValues[i].pbr) /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/  == false)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].recomendation != m_curRDGValues[i].recomendation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviationPercent != m_curRDGValues[i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviation != m_curRDGValues[i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                    return true;
                else
                    ;
            }

            return bRes;
        }

        public override bool IsRDGExcel(int id_tec)
        {
            bool bRes = false;

            foreach (TECComponent comp in allTECComponents) {
                if (comp.tec.m_id == id_tec) {
                    if (comp.tec.m_path_rdg_excel.Length > 0) {
                        bRes = true;

                        break;
                    }
                    else
                        ;
                }
                else
                    ;
            }

            return bRes;
        }

        protected virtual /*override*/ int ImpRDGExcelValuesResponse()
        {
            //bool bRes = base.ImpRDGExcelValuesResponse();
            int iRes = IsCanUseTECComponents() == true ? 0 : -1;
            int rowOffsetData = 0;

            if (iRes == 0)
            {
                int i = -1,
                    iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc,
                    rowRDGExcelStart = 1 + iTimeZoneOffset,
                    hour = -1;

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) iRes = 0; else ;

                if (iRes == 0)
                {
                    for (i = rowRDGExcelStart; i < m_tableRDGExcelValuesResponse.Rows.Count - rowOffsetData; i++)
                    {
                        hour = i - iTimeZoneOffset;
                        setRDGExcelValuesItem(out m_curRDGValues[hour - 1], i);
                    }

                    /*for (i = hour; i < 24 + 1; i++)
                    {
                        hour = i;

                        m_curRDGValues.plan[hour - 1] = 0;
                        m_curRDGValues.recommendations[hour - 1] = 0;
                        m_curRDGValues.deviationPercent[hour - 1] = false;
                        m_curRDGValues.diviation[hour - 1] = 0;
                    }*/
                }
                else
                    ;
            }
            else
                ;

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];

            m_curRDGValues.CopyTo(curRDGValues, 0);

            m_listCurRDGValues.Add(curRDGValues);

            return iRes;
        }

        public override Errors SaveChanges()
        {
            Errors errRes = Errors.NoError,
                    bErr = Errors.NoError;
            int indxEv = -1;

            m_evSaveChangesComplete.Reset ();

            lock (m_lockResSaveChanges)
            {
                m_listResSaveChanges.Clear ();
            }

            int prevIndxTECComponent = indxTECComponents;

            foreach (RDGStruct [] curRDGValues in m_listCurRDGValues) {
                bErr = Errors.NoError;

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.TG) {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                    {                        
                        indxTECComponents = m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                        curRDGValues.CopyTo(m_curRDGValues, 0);

                        bErr = base.SaveChanges();
                    }
                    else
                        break;
                }
                else
                    ;

                lock (m_lockResSaveChanges)
                {
                    m_listResSaveChanges.Add(bErr);

                    if (! (bErr == Errors.NoError) && (errRes == Errors.NoError))
                        errRes = bErr;
                    else
                        ;
                }
            }

            indxTECComponents = prevIndxTECComponent;

            //if (indxEv == 0)
            //if (errRes == Errors.NoError)
                m_evSaveChangesComplete.Set();
            //else ;

            if (!(saveComplete == null)) saveComplete(); else ;

            return errRes;
        }

        private string nameFileRDGExcel (DateTime dt) {
            //return dt.GetDateTimeFormats()[4];
            return dt.ToString (@"yyyy-MM-dd");
        }

        protected void /*bool*/ ExpRDGExcelValuesRequest()
        {
            //bool bRes = true;
            Boolean bMidnightValues = false;
            int err = 0,
                i = -1,
                rowOffsetData = 2,
                rowOffsetNextDay = 0,
                iMidnightValues = 0,
                iTimeZoneOffset = allTECComponents[m_listTECComponentIndexDetail[0/*любой из индексов, т.к. они принадлежат одной ТЭЦ*/]].tec.m_timezone_offset_msc;
            string path_rdg_excel = allTECComponents[m_listTECComponentIndexDetail [0/*любой из индексов, т.к. они принадлежат одной ТЭЦ*/]].tec.m_path_rdg_excel,
                strUpdate = string.Empty;
            TECComponentBase comp;

            if ((IsCanUseTECComponents() == true) && (path_rdg_excel.Length > 0))
            {
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook excelAppWorkbook;
                Excel.Worksheet excelAppWorksheet;
                Excel.Range excelAppCellA1, excelAppWorkcell;

                excelApp.Visible = false;

                Boolean bPrevExcelAppDisplayAlerts = excelApp.DisplayAlerts;
                excelApp.DisplayAlerts = false;

                string nameExcelBook = nameFileRDGExcel (m_curDate.Date) + ".xls",
                        pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                i = 0;
                while (File.Exists(pathExcelBook) == false)
                {
                    i--;

                    nameExcelBook = nameFileRDGExcel(m_curDate.Date.AddDays(i)) + ".xls";
                    pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;
                }

                excelApp.Workbooks.Open(pathExcelBook,
                                        Type.Missing,
                                        false,
                                        Type.Missing,
                                        Type.Missing,
                                        "nss",
                                        true);

                if (i < 0)
                {//Файл искали - необходимо созлать нпа сегодняшний день
                    nameExcelBook = nameFileRDGExcel (m_curDate.Date) + ".xls";
                    pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                    excelApp.Workbooks [1].SaveAs (pathExcelBook, Type.Missing, Type.Missing, "nss", false, false);
                }
                else
                    ;

                excelAppWorkbook = excelApp.Workbooks [1];
                excelAppWorksheet = (Excel.Worksheet)excelAppWorkbook.Worksheets[1];
                excelAppCellA1 = excelAppWorksheet.get_Range("A1", Type.Missing);

                for (i = 0; i < 24; i++)
                {
                    strUpdate = string.Empty;

                    if ((i + iTimeZoneOffset) < 24)
                    {
                    }
                    else
                    {
                        if (rowOffsetNextDay == 0)
                        {
                            //Проверить существование книги
                            nameExcelBook = nameFileRDGExcel (m_curDate.Date.AddDays(1)) + ".xls";
                            pathExcelBook = path_rdg_excel + "\\" + nameExcelBook;

                            if (File.Exists(pathExcelBook) == false)
                                excelApp.Workbooks[1].SaveAs(pathExcelBook, Type.Missing, Type.Missing, "nss", false, false);
                            else
                            {
                                excelApp.Windows[1].Close(true, excelApp.Workbooks[1].FullName, false);

                                excelApp.Workbooks.Open(pathExcelBook,
                                                        Type.Missing,
                                                        false,
                                                        Type.Missing,
                                                        Type.Missing,
                                                        "nss",
                                                        true);

                                excelAppWorkbook = excelApp.Workbooks[1];
                                excelAppWorksheet = (Excel.Worksheet)excelAppWorkbook.Worksheets[1];
                                excelAppCellA1 = excelAppWorksheet.get_Range("A1", Type.Missing);
                            }

                             rowOffsetNextDay = 24;
                        }
                        else
                            ;
                    }

                    foreach (RDGStruct [] curRDGValues in m_listCurRDGValues) {
                        indxTECComponents = m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)];
                        comp = null;

                        //strUpdate += @"'A";
                        //strUpdate += @"A";
                        if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.GTP) {
                            //strUpdate += allTECComponents[indxTECComponents].m_indx_col_rdg_excel;
                            comp = allTECComponents[indxTECComponents];
                        } else
                            if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.TG) {
                                //strUpdate += allTECComponents[indxTECComponents].TG [0].m_indx_col_rdg_excel;
                                comp = allTECComponents[indxTECComponents].m_listTG[0];
                            } else ;

                        //strUpdate += @"'='" + (curRDGValues[i].pbr - curRDGValues[i].recomendation) + @"', ";
                        //strUpdate += @"='" + (curRDGValues[i].pbr - curRDGValues[i].recomendation) + @"', ";

                        if (!(comp == null))
                        {
                            excelAppWorkcell = excelAppCellA1.get_Offset(rowOffsetData + i + iTimeZoneOffset - rowOffsetNextDay, comp.m_indx_col_rdg_excel - 1);

                            try { excelAppWorkcell.Value = curRDGValues[i].pbr - curRDGValues[i].recomendation; }
                            catch (Exception e) {
                                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "excelAppWorkcell.set_Value () - ...");

                                i = 24;
                                break;
                            }

                            if (!(rowOffsetNextDay == 0) && (bMidnightValues == false))
                            {//Выполняется ОДИН раз (bMidnightValues - флаг) для всех компонентов
                                excelAppWorkcell = excelAppCellA1.get_Offset(rowOffsetData -1, comp.m_indx_col_rdg_excel - 1);

                                try { excelAppWorkcell.Value = curRDGValues[24 - iTimeZoneOffset].pbr - curRDGValues[24 - iTimeZoneOffset].recomendation; }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, "excelAppWorkcell.set_Value () - ...");

                                    i = 24;
                                    break;
                                }
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }

                    if (!(rowOffsetNextDay == 0) && (bMidnightValues == false))
                    {//Выполняется ОДИН раз (bMidnightValues - флаг) для всех компонентов
                        bMidnightValues = true;
                    }
                    else
                        ;

                    //strUpdate = strUpdate.Substring (0, strUpdate.Length - 2);

                    //strUpdate = @"UPDATE [Лист1$] SET " + strUpdate + @" WHERE A" + (i + 2) + "='" + ((i.ToString ().Length < 2) ? @"0" + i.ToString () : i.ToString ()) + @"'";
                    //strUpdate = @"UPDATE [Лист1$] SET " + strUpdate + @" WHERE 1='" + ((i.ToString().Length < 2) ? @"0" + i.ToString() : i.ToString()) + @"'";
                }

                excelApp.Windows[1].Close(true, pathExcelBook, false);

                excelApp.DisplayAlerts = bPrevExcelAppDisplayAlerts;
                excelApp.Quit();                

                //base.ExpRDGExcelValuesRequest();
            }
            else
                ;

            //return bRes;
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

        private void /*bool*/ ImpRDGExcelValuesRequest()
        {
            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            //bool bRes = true;
            int err = 0,
                i = -1, j = -1,
                rowOffsetData = 2,
                iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc;
            string path_rdg_excel = allTECComponents[indxTECComponents].tec.m_path_rdg_excel,
                strSelect = @"SELECT * FROM [Лист1$]";
            object[] dataRowAddIn = null;

            DataTable tableRDGExcelValuesNextDay;
            if (!(m_tableRDGExcelValuesResponse == null)) m_tableRDGExcelValuesResponse.Clear(); else ;

            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - path_rdg_excel=" + path_rdg_excel + @", nameFileRDGExcel=" + nameFileRDGExcel(m_curDate.Date), Logging.INDEX_MESSAGE.NOT_SET);

            delegateStartWait();
            if ((IsCanUseTECComponents() == true) && (path_rdg_excel.Length > 0))
            {
                try { m_tableRDGExcelValuesResponse = DbTSQLInterface.Select(path_rdg_excel + "\\" + nameFileRDGExcel (m_curDate.Date) + ".xls", strSelect, out err); }
                catch (Exception e) {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"AdminTS_NSS::ImpRDGExcelValuesRequest () - DbTSQLInterface.Select (" + strSelect + @") - ...");
                }

                if (! (m_tableRDGExcelValuesResponse ==  null)) {
                    Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - m_tableRDGExcelValuesResponse.Rows.Count=" + m_tableRDGExcelValuesResponse.Rows.Count, Logging.INDEX_MESSAGE.NOT_SET);

                    if (m_tableRDGExcelValuesResponse.Rows.Count > 0)
                    {
                        while (m_tableRDGExcelValuesResponse.Rows[m_tableRDGExcelValuesResponse.Rows.Count - 1][1] is DBNull)
                            m_tableRDGExcelValuesResponse.Rows.RemoveAt(m_tableRDGExcelValuesResponse.Rows.Count - 1);

                        //if (File.Exists(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).GetDateTimeFormats()[4] + ".xls") == true)
                        if (File.Exists(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).ToString(@"yyyyMMdd") + ".xls") == true)
                        {
                            tableRDGExcelValuesNextDay = DbTSQLInterface.Select(path_rdg_excel + "\\" + m_curDate.Date.AddDays(1).ToString(@"yyyyMMdd") + ".xls", strSelect, out err);
                            if (tableRDGExcelValuesNextDay.Rows.Count > 0)
                            {
                                while (tableRDGExcelValuesNextDay.Rows[tableRDGExcelValuesNextDay.Rows.Count - 1][1] is DBNull)
                                    tableRDGExcelValuesNextDay.Rows.RemoveAt(tableRDGExcelValuesNextDay.Rows.Count - 1);

                                for (i = 0; i < iTimeZoneOffset; i++)
                                {
                                    dataRowAddIn = new object[m_tableRDGExcelValuesResponse.Columns.Count];

                                    for (j = 0; j < m_tableRDGExcelValuesResponse.Columns.Count; j++)
                                    {
                                        dataRowAddIn.SetValue(tableRDGExcelValuesNextDay.Rows[i + rowOffsetData - 1][j], j); //"-1" т.к. заголовок для OleDb не существует
                                    }

                                    m_tableRDGExcelValuesResponse.Rows.Add(dataRowAddIn); //Т.к.
                                }
                            }
                            else
                                ;

                            tableRDGExcelValuesNextDay.Clear();
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - m_tableRDGExcelValuesResponse=null", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                ;

            //Logging.Logg ().LogLock ();
            //Logging.Logg().Send("Admin.cs - GetRDGExcelValuesRequest () - (path_rdg_excel = " + path_rdg_excel + ")", false, false, false);
            //Logging.Logg().LogUnlock();

            delegateStopWait();

            Logging.Logg().Debug(@"AdminTS_NSS::ImpRDGExcelValuesRequest () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            //return bRes;
        }
    }
}
