using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;

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
        public bool SuccessGetData {
            get {
                lock (m_lockSuccessGetData)
                {
                    return m_listCurRDGValues.Count == m_listTECComponentIndexDetail.Count ? true : false;
                }
            }
        }

        public bool SuccessSaveChanges {
            get {
                bool bRes = true;

                lock (m_lockResSaveChanges)
                {
                    foreach (Errors err in m_listResSaveChanges)
                    {
                        if (!(err == Errors.NoError))
                            bRes = false;
                        else
                            ;
                    }

                    if (bRes == true)
                        if ((m_listResSaveChanges.Count + 1) == m_listTECComponentIndexDetail.Count)
                            ;
                        else
                            bRes = false;
                    else
                        ;
                }

                return bRes;
            }
        }

        public AdminTS_NSS () {
            m_listCurRDGValues = new List<RDGStruct[]> ();
            m_listTECComponentIndexDetail = new List<int> ();
            m_listResSaveChanges = new List <Errors> ();

            m_lockSuccessGetData = new object();
            m_lockResSaveChanges = new object ();

            m_evSaveChangesComplete = new ManualResetEvent (false);
        }

        protected override bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            bool bRes = base.GetAdminValuesResponse(tableAdminValuesResponse, date);

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

            return bRes;
        }

        public void fillListIndexTECComponent (int id) {
            lock (m_lockSuccessGetData)
            {
                m_listTECComponentIndexDetail.Clear();
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == id) && //Принадлежит ТЭЦ
                        (((comp.m_id > 100) && (comp.m_id < 500)) || //Является ГТП
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

            foreach (int indx in m_listTECComponentIndexDetail)
            {
                indxEv = WaitHandle.WaitAny (m_waitHandleState);
                if (indxEv == 0)
                    base.GetRDGValues(m_typeFields, indx);
                else
                    break;
            }
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
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                indxEv = WaitHandle.WaitAny(m_waitHandleState);
                if (indxEv == 0)
                    base.GetRDGValues((int)m_typeFields, indx, (DateTime)date);
                else
                    break;
            }
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int id, DateTime date)
        {
            //delegateStartWait ();
            fillListIndexTECComponent (id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithDate)).Start (date);
            //threadGetRDGValuesWithDate (date);
            //delegateStopWait ();
        }

        private void threadGetRDGExcelValues (object date) {
            int indxEv = -1;
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                indxEv = WaitHandle.WaitAny(m_waitHandleState);
                if (indxEv == 0)
                    if ((allTECComponents[indx].m_id > 100) && (allTECComponents[indx].m_id < 500))
                        base.GetRDGValues((int)m_typeFields, indx, (DateTime)date);
                    else
                        if ((allTECComponents[indx].m_id > 1000) && (allTECComponents[indx].m_id < 10000))
                            base.GetRDGExcelValues(indx, (DateTime)date);
                        else
                            ;
                else
                    break;
            }
        }

        public override void GetRDGExcelValues(int id, DateTime date)
        {
            //delegateStartWait();

            m_listCurRDGValues.Clear();

            new Thread(new ParameterizedThreadStart(threadGetRDGExcelValues)).Start (date);
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
                    
                    listRes.Add(comp.tec.name);
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

        protected override bool GetRDGExcelValuesResponse()
        {
            bool bRes = base.GetRDGExcelValuesResponse();

            RDGStruct[] curRDGValues = new RDGStruct[m_curRDGValues.Length];

            m_curRDGValues.CopyTo(curRDGValues, 0);

            m_listCurRDGValues.Add(curRDGValues);

            return bRes;
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
                }
            }

            indxTECComponents = prevIndxTECComponent;

            lock (m_lockResSaveChanges)
            {
                foreach (Errors err in m_listResSaveChanges)
                {
                    if (!(err == Errors.NoError))
                        errRes = err;
                    else
                        ;
                }
            }

            //if (indxEv == 0)
            if (errRes == Errors.NoError)
                m_evSaveChangesComplete.Set();
            else
                ;

            return errRes;
        }

        protected override void InitializeSyncState()
        {
            m_waitHandleState = new WaitHandle[2] { new AutoResetEvent(true), new ManualResetEvent(false) };
        }
    }
}
