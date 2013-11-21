using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;

namespace StatisticCommon
{
    public class AdminNSS : Admin
    {
        public List<int> m_listTECComponentIndexDetail;
        public List <RDGStruct []> m_listCurRDGValues;

        public AdminNSS () {
            m_listCurRDGValues = new List<RDGStruct[]> ();
            m_listTECComponentIndexDetail = new List<int> ();
        }

        protected override bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            bool bRes = base.GetAdminValuesResponse(tableAdminValuesResponse, date);

            RDGStruct []curRDGValues = new RDGStruct [m_curRDGValues.Length];            

            //curRDGValues = (RDGStruct[])m_curRDGValues.Clone();

            m_curRDGValues.CopyTo(curRDGValues, 0);
            
            for (int i = 0; i < m_curRDGValues.Length; i ++) {
                curRDGValues [i].plan += m_curRDGValues [i].recomendation;
                
                //curRDGValues [i].plan = m_curRDGValues [i].plan;

                //curRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                //curRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                //curRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }

            m_listCurRDGValues.Add (curRDGValues);

            return bRes;
        }

        public void fillListIndexTECComponent (int id) {
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

        private void threadGetRDGValuesWithoutDate(object obj)
        {
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                evStateEnd.WaitOne();
                base.GetRDGValues(m_typeFields, indx);
            }
        }
        
        public override void GetRDGValues(TYPE_FIELDS mode, int id)
        {
            //delegateStartWait ();
            fillListIndexTECComponent(id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithoutDate)).Start ();
            //delegateStopWait ();
        }

        private void threadGetRDGValuesWithDate(object date)
        {
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                evStateEnd.WaitOne();
                base.GetRDGValues(m_typeFields, indx, (DateTime)date);
            }
        }

        public override void GetRDGValues(TYPE_FIELDS mode, int id, DateTime date)
        {
            //delegateStartWait ();
            fillListIndexTECComponent (id);

            new Thread (new ParameterizedThreadStart(threadGetRDGValuesWithDate)).Start (date);
            //delegateStopWait ();
        }

        private void threadGetRDGExcelValues (object date) {
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                evStateEnd.WaitOne();
                if ((allTECComponents[indx].m_id > 100) && (allTECComponents[indx].m_id < 500))
                    base.GetRDGValues(m_typeFields, indx, (DateTime)date);
                else
                    if ((allTECComponents[indx].m_id > 1000) && (allTECComponents[indx].m_id < 10000))
                        base.GetRDGExcelValues(indx, (DateTime)date);
                    else
                        ;
            }
        }

        public override void GetRDGExcelValues(int id, DateTime date)
        {
            //delegateStartWait();

            m_listCurRDGValues.Clear();

            //new Thread(new ParameterizedThreadStart(threadGetRDGExcelValues)).Start (date);
            threadGetRDGExcelValues (date);

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
            Errors bErr = Errors.NoError,
                    bRes = Errors.NoError;

            int prevIndxTECComponent = indxTECComponents;

            foreach (RDGStruct [] curRDGValues in m_listCurRDGValues) {
                if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.TG) {
                    indxTECComponents = m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                    curRDGValues.CopyTo(m_curRDGValues, 0);

                    bErr = base.SaveChanges ();
                    if (!(bErr == Errors.NoError))
                        bRes = bErr;
                    else
                        ;
                }
                else
                    ;
            }

            indxTECComponents = prevIndxTECComponent;

            return bRes;
        }
    }
}
