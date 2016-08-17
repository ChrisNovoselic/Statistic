using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;

namespace StatisticCommon
{
    public abstract class AdminTS_TG : AdminTS
    {
        /// <summary>
        /// ������ �������� �������� ��� ���������� �������� �������� (����������� �������� ��������)
        /// </summary>
        public List<int> m_listTECComponentIndexDetail;
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

        public AdminTS_TG(bool[] arMarkPPBRValues, TECComponentBase.TYPE type)
            : base(arMarkPPBRValues, type)
        {
            delegateImportForeignValuesRequuest = impRDGExcelValuesRequest;
            delegateExportForeignValuesRequuest = expRDGExcelValuesRequest;
            delegateImportForeignValuesResponse = impRDGExcelValuesResponse;
            //delegateExportForeignValuesResponse = ExpRDGExcelValuesResponse;

            m_listCurRDGValues = new List<RDGStruct[]> ();
            m_listTECComponentIndexDetail = new List<int> ();
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

        public virtual void FillListIndexTECComponent (int id) {
            lock (m_lockSuccessGetData)
            {
                m_listTECComponentIndexDetail.Clear();
                //������� - ���
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == id) && //����������� ���
                        ((comp.IsGTP == true) /*|| //�������� ���
                        ((comp.m_id > 1000) && (comp.m_id < 10000))*/)) //�������� ��
                    {                    
                        m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));
                    }
                    else
                        ;
                }

                //����� - ��
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == id) && //����������� ���
                        (/*((comp.m_id > 100) && (comp.m_id < 500)) ||*/ //�������� ���
                        (comp.IsTG == true))) //�������� ��
                    {                    
                        m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));
                    }
                    else
                        ;
                }

                m_listCurRDGValues.Clear();
            }
        }

        protected virtual void threadGetRDGValuesWithoutDate(object obj)
        {
            int indxEv = -1;

            //lock (m_lockSuccessGetData)
            //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny (m_waitHandleState);
                    if (indxEv == 0)
                        base.GetRDGValues(/*m_typeFields,*/ indx);
                    else
                        break;
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public void BaseGetRDGValue(int indx, DateTime date)
        {
            if(date!=DateTime.MinValue)
                base.GetRDGValues(/*(int)m_typeFields,*/ indx, (DateTime)date);
            else
                base.GetRDGValues(/*(int)m_typeFields,*/ indx);
        }

        public override void GetRDGValues(/*TYPE_FIELDS mode,*/ int id)
        {
            //delegateStartWait ();
            FillListIndexTECComponent(id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithoutDate)).Start ();
            //threadGetRDGValuesWithoutDate (null);
            //delegateStopWait ();
        }

        protected virtual void threadGetRDGValuesWithDate(object date)
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
                        base.GetRDGValues(/*(int)m_typeFields,*/ indx, (DateTime)date);
                    else
                        break;
                }
            //}

            //m_bSavePPBRValues = true;
        }

        public override void GetRDGValues(/*int /*TYPE_FIELDS mode,*/ int id, DateTime date)
        {
            //delegateStartWait ();
            FillListIndexTECComponent (id);

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
                            base.GetRDGValues(/*(int)m_typeFields,*/ indx, (DateTime)date);
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
        /// <summary>
        /// ���������� ������� ���������� ������������� ���������
        /// </summary>
        /// <returns>������� ���������</returns>
        public override bool WasChanged()
        {//??? �� ������, ��� ���� - ����� �� ��������������...
            bool bRes = false;

            for (int i = 0; (i < 24) && (bRes == false); i++)
            {
                if (m_prevRDGValues[i].pbr.Equals (m_curRDGValues[i].pbr) /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/  == false)
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].recomendation != m_curRDGValues[i].recomendation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].deviationPercent != m_curRDGValues[i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                    bRes = true;
                else
                    ;
                if (m_prevRDGValues[i].deviation != m_curRDGValues[i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                    bRes = true;
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

        protected string nameFileRDGExcel (DateTime dt) {
            //return dt.GetDateTimeFormats()[4];
            return dt.ToString (@"yyyy-MM-dd");
        }

        protected override void InitializeSyncState()
        {
            m_evSaveChangesComplete = new ManualResetEvent(false);

            m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.ERROR + 1];
            base.InitializeSyncState();
            for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)(INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
            {
                m_waitHandleState[i] = new ManualResetEvent(false);
            }
        }

        public override void ImpRDGExcelValues(int id, DateTime date)
        {
            //delegateStartWait();

            m_listCurRDGValues.Clear();

            new Thread(new ParameterizedThreadStart(threadImpRDGExcelValues)).Start(date);
            //threadGetRDGExcelValues (date);

            //delegateStopWait();
        }

        protected abstract void /*bool*/ impRDGExcelValuesRequest();

        protected abstract /*override*/ int impRDGExcelValuesResponse();

        protected abstract void /*bool*/ expRDGExcelValuesRequest();
    }
}
