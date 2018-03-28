using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;
using ASUTP.Helper;
using ASUTP.Core;

namespace StatisticCommon
{
    public abstract class AdminTS_TG : AdminTS
    {
        /// <summary>
        /// ������ �������� �������� ��� ���������� �������� �������� (����������� �������� ��������)
        /// </summary>
        public List<FormChangeMode.KeyDevice> m_listKeyTECComponentDetail;
        public List<RDGStruct[]> m_listPrevRDGValues
            , m_listCurRDGValues;

        public List <Errors>  m_listResSaveChanges;

        public ManualResetEvent m_evSaveChangesComplete;

        protected object m_lockSuccessGetData
            , m_lockResSaveChanges;

        public bool CompletedGetRDGValues {
            get {
                lock (m_lockSuccessGetData)
                {
                    return (! (m_listCurRDGValues.Count < m_listKeyTECComponentDetail.Count)) ? true : false;
                }
            }
        }

        public bool CompletedSaveChanges {
            get {
                bool bRes = false;
                
                lock (m_lockResSaveChanges)
                {
                    bRes = ! ((m_listResSaveChanges.Count) < m_listKeyTECComponentDetail.Count);
                }

                return bRes;
            }
        }

        public bool SuccessSaveChanges {
            get {
                bool bRes = true;

                lock (m_lockResSaveChanges)
                {
                    if ((m_listResSaveChanges.Count + 1) == m_listKeyTECComponentDetail.Count)
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

        public AdminTS_TG(bool[] arMarkPPBRValues, TECComponentBase.TYPE type)
            : base(arMarkPPBRValues, type)
        {
            delegateImportForeignValuesRequest = impRDGExcelValuesRequest;
            delegateExportForeignValuesRequest = expRDGExcelValuesRequest;
            delegateImportForeignValuesResponse = impRDGExcelValuesResponse;
            //delegateExportForeignValuesResponse = ExpRDGExcelValuesResponse;

            m_listPrevRDGValues = new List<RDGStruct[]>();
            m_listCurRDGValues = new List<RDGStruct[]> ();
            m_listKeyTECComponentDetail = new List<FormChangeMode.KeyDevice> ();
            m_listResSaveChanges = new List <Errors> ();

            m_lockSuccessGetData = new object();
            m_lockResSaveChanges = new object ();
        }

        protected override int GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            int iRes = base.GetAdminValuesResponse(tableAdminValuesResponse, date);

            RDGStruct []curRDGValues = new RDGStruct [m_curRDGValues.Length];

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">��������� ������ ��������� ��� 'id' �������������</param>
        public virtual void FillListKeyTECComponentDetail (int id)
        {
            lock (m_lockSuccessGetData)
            {
                m_listKeyTECComponentDetail.Clear();
                //������� - ���
                foreach (TECComponent comp in allTECComponents)
                    if ((comp.tec.m_id == id) //����������� ���
                        && (comp.IsGTP == true)) //�������� ���
                        m_listKeyTECComponentDetail.Add(new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = comp.Mode });
                    else
                        ;

                //����� - ��
                foreach (TECComponent comp in allTECComponents)
                    if ((comp.tec.m_id == id) && //����������� ���
                        (comp.IsTG == true)) //�������� ��
                        m_listKeyTECComponentDetail.Add(new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = comp.Mode });
                    else
                        ;

                ClearListRDGValues();
            }
        }

        protected virtual void threadGetRDGValuesWithoutDate(object obj)
        {
            INDEX_WAITHANDLE_REASON indxEv = INDEX_WAITHANDLE_REASON.SUCCESS;

            //lock (m_lockSuccessGetData)
            //{
                foreach (FormChangeMode.KeyDevice key in m_listKeyTECComponentDetail)
                {
                    indxEv = WaitAny (Constants.MAX_WATING, true);
                    if (indxEv == INDEX_WAITHANDLE_REASON.SUCCESS)
                        base.GetRDGValues(key);
                    else {
                        ASUTP.Logging.Logg ().Error ($"AdminTS_TG::threadGetRDGValuesWithoutDate () - <{indxEv}>...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

                        break;
                    }
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public void BaseGetRDGValue(FormChangeMode.KeyDevice key, DateTime date)
        {
            if(!(date == DateTime.MinValue))
                base.GetRDGValues(key, (DateTime)date);
            else
                base.GetRDGValues(key);
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key)
        {
            //delegateStartWait ();
            FillListKeyTECComponentDetail(key.Id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithoutDate)).Start ();
            //threadGetRDGValuesWithoutDate (null);
            //delegateStopWait ();
        }

        protected virtual void threadGetRDGValuesWithDate(object date)
        {
            INDEX_WAITHANDLE_REASON indxEv = INDEX_WAITHANDLE_REASON.SUCCESS;

            ResetSyncState ();

            //lock (m_lockSuccessGetData)
            //{
                foreach (FormChangeMode.KeyDevice key in m_listKeyTECComponentDetail) {
                    indxEv = WaitAny(Constants.MAX_WATING, true);
                    if (indxEv == INDEX_WAITHANDLE_REASON.SUCCESS)
                        base.GetRDGValues(key, (DateTime)date);
                    else {
                        ASUTP.Logging.Logg ().Error ($"AdminTS_TG::threadGetRDGValuesWithDate () - <{indxEv}>...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

                        break;
                    }
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            //delegateStartWait ();
            FillListKeyTECComponentDetail (key.Id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithDate)).Start (date);
            //threadGetRDGValuesWithDate (date);
            //delegateStopWait ();
        }

        private void threadImpRDGExcelValues (object date)
        {
            INDEX_WAITHANDLE_REASON indxEv = INDEX_WAITHANDLE_REASON.SUCCESS;

            ResetSyncState ();

            //lock (m_lockSuccessGetData)
            //{
                foreach (FormChangeMode.KeyDevice key in m_listKeyTECComponentDetail) {
                    indxEv = WaitAny(Constants.MAX_WATING, true);
                    if (indxEv == INDEX_WAITHANDLE_REASON.SUCCESS)
                        if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP)
                            base.GetRDGValues(key, (DateTime)date);
                        else
                            if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.TG)
                                base.ImpRDGExcelValues(key, (DateTime)date);
                            else
                                ;
                    else {
                        ASUTP.Logging.Logg ().Error ($"AdminTS_TG::threadImpRDGExcelValues () - <{indxEv}>...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                        break;
                    }
                }
            //}

            //m_bSavePPBRValues = true;
        }

        //public string [] GetListNameTEC()
        //{
        //    int indx = -1;
        //    List<string> listRes = new List<string> ();
        //    List<int> listIdTEC = new List<int>();

        //    foreach (TECComponent comp in allTECComponents)
        //    {
        //        indx = comp.tec.m_id;
        //        if (listIdTEC.IndexOf(indx) < 0) {
        //            listIdTEC.Add (indx);
                    
        //            listRes.Add(comp.tec.name_shr);
        //        }
        //        else
        //            ;
        //    }

        //    return listRes.ToArray ();
        //}
        /// <summary>
        /// ���������� ������� ���������� ������������� ���������
        /// </summary>
        /// <returns>������� ���������</returns>
        public override bool WasChanged()
        {//??? �� ������, ��� ���� - ����� �� ��������������...
            bool bRes = false;

            for (int i = 0; (i < 24) && (bRes == false); i++)
            {
                if ((m_prevRDGValues[i].pbr > 0)
                    && (m_curRDGValues[i].pbr > 0)
                    && (m_prevRDGValues[i].pbr.Equals (m_curRDGValues[i].pbr)== false))
                    bRes = true;
                else
                    ;
                if ((m_prevRDGValues[i].pmin > 0)
                    && (m_curRDGValues[i].pmin > 0)
                    && (m_prevRDGValues[i].pmin.Equals (m_curRDGValues[i].pmin)== false))
                    bRes = true;
                else
                    ;
                if ((m_prevRDGValues[i].pmax > 0)
                    && (m_curRDGValues[i].pmax > 0)
                    && (m_prevRDGValues[i].pmax.Equals (m_curRDGValues[i].pmax)== false))
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].recomendation.Equals (m_curRDGValues[i].recomendation) == false)
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].deviationPercent.Equals (m_curRDGValues[i].deviationPercent) == false)
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].deviation.Equals (m_curRDGValues[i].deviation) == false)
                    bRes = true;
                else
                    ;
            }

            return bRes;
        }

        public override bool IsRDGExcel(FormChangeMode.KeyDevice key_tec)
        {
            bool bRes = false;

            foreach (TECComponent comp in allTECComponents) {
                if (comp.tec.m_id == key_tec.Id) {
                    if (comp.tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString().Length > 0) {
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

        public Errors BaseSaveChanges()
        {
            Errors errRes = Errors.NoError;

            errRes = base.SaveChanges();

            return errRes;
        }

        public override Errors SaveChanges()
        {
            Errors errRes = Errors.NoError,
                    bErr = Errors.NoError;
            INDEX_WAITHANDLE_REASON indxEv = INDEX_WAITHANDLE_REASON.SUCCESS;

            m_evSaveChangesComplete.Reset ();

            lock (m_lockResSaveChanges)
            {
                m_listResSaveChanges.Clear ();
            }

            FormChangeMode.KeyDevice prevKeyTECComponent = CurrentKey;

            foreach (RDGStruct [] curRDGValues in m_listCurRDGValues) {
                bErr = Errors.NoError;

                ResetSyncState ();

                if (m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)].Mode == FormChangeMode.MODE_TECCOMPONENT.TG) {
                    indxEv = WaitAny(Constants.MAX_WATING, true);
                    if (indxEv == INDEX_WAITHANDLE_REASON.SUCCESS)
                    {
                        CurrentKey = m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                        curRDGValues.CopyTo(m_curRDGValues, 0);

                        bErr = base.SaveChanges();
                    }
                    else {
                        ASUTP.Logging.Logg ().Error ($"AdminTS_TG::SaveChanges () - <{indxEv}>...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

                        break;
                    }
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

            CurrentKey = prevKeyTECComponent;

            //if (indxEv == 0)
            //if (errRes == Errors.NoError)
                m_evSaveChangesComplete.Set();
            //else ;

            //??? ������ AdminValues
            saveComplete?.Invoke((m_markSavedValues.IsMarked ((int)INDEX_MARK_PPBRVALUES.PBR_SAVED) == true) ? (int)StatesMachine.SavePPBRValues : (int)StatesMachine.SaveAdminValues);

            return errRes;
        }

        protected string nameFileRDGExcel (DateTime dt) {
            //return dt.GetDateTimeFormats()[4];
            return dt.ToString (@"yyyy-MM-dd");
        }

        protected override void InitializeSyncState(int capacity = 1)
        {
            m_evSaveChangesComplete = new ManualResetEvent(false);

            base.InitializeSyncState((int)INDEX_WAITHANDLE_REASON.ERROR + 1);
            AddSyncState(INDEX_WAITHANDLE_REASON.ERROR, typeof (ManualResetEvent), false);
        }

        public override void ImpRDGExcelValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            //delegateStartWait();

            ClearListRDGValues();

            new Thread(new ParameterizedThreadStart(threadImpRDGExcelValues)).Start(date);
            //threadGetRDGExcelValues (date);

            //delegateStopWait();
        }

        public void ClearListRDGValues()
        {
            m_listPrevRDGValues.Clear();
            m_listCurRDGValues.Clear();
        }

        protected abstract void /*bool*/ impRDGExcelValuesRequest();

        protected abstract /*override*/ int impRDGExcelValuesResponse();

        protected abstract void /*bool*/ expRDGExcelValuesRequest();
    }
}
