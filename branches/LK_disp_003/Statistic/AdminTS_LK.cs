using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

//using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class AdminTS_LK : AdminTS_TG
    {
        public static int Index_LK = 10;

        public Semaphore m_semaIndxTECComponents;

        public List<RDGStruct[]> m_listPrevRDGValues;

        public AdminTS_LK(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues)
        {
        }

        public override void FillListIndexTECComponent(int id)
        {
            lock (m_lockSuccessGetData)
            {
                m_listTECComponentIndexDetail.Clear();

                //Сначала - ГТП
                foreach (TECComponent comp in allTECComponents)
                {
                    if (comp.tec.m_id > Index_LK)
                    {
                        if ((comp.m_id == GetIdTECComponent(id)) && //Принадлежит ТЭЦ
                            ((comp.IsGTP == true) /*|| //Является ГТП
                        ((comp.m_id > 1000) && (comp.m_id < 10000))*/)) //Является ТГ
                        {
                            m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));

                            foreach (TG tg in comp.m_listTG)
                                foreach (TECComponent comp_tg in allTECComponents)
                                    if(comp_tg.m_id==tg.m_id)
                                        m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp_tg));
                        }
                        else
                            ;
                    }
                }

                m_listTECComponentIndexDetail.Sort();
                m_listCurRDGValues.Clear();
            }
        }

        protected override void threadGetRDGValuesWithoutDate(object obj)
        {
            int indxEv = -1;

            //lock (m_lockSuccessGetData)
            //{
            foreach (int indx in m_listTECComponentIndexDetail)
            {
                indxEv = WaitHandle.WaitAny(m_waitHandleState);

                if (indxEv == 0)
                {
                    m_semaIndxTECComponents.WaitOne();

                    base.GetRDGValues(/*m_typeFields,*/ indx);
                }
                else
                    break;
            }
            //}

            //m_bSavePPBRValues = true;
        }

        protected override void threadGetRDGValuesWithDate(object date)
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
                {
                    m_semaIndxTECComponents.WaitOne();

                    base.GetRDGValues(/*(int)m_typeFields,*/ indx, (DateTime)date);
                    m_listPrevRDGValues = new List<RDGStruct[]>(m_listCurRDGValues);
                }
                else
                    break;
            }
            //}

            //m_bSavePPBRValues = true;
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            for (int a = 0; a < m_listCurRDGValues.Count; a++)
            {
                for (int i = 0; i < 24; i++)
                {
                    if (m_listPrevRDGValues[a][i].pbr.Equals(m_listCurRDGValues[a][i].pbr) /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/  == false)
                        return true;
                    else
                        ;
                    if (m_listPrevRDGValues[a][i].pmin != m_listCurRDGValues[a][i].pmin /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                        return true;
                    else
                        ;
                    if (m_listPrevRDGValues[a][i].deviationPercent != m_listCurRDGValues[a][i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                        return true;
                    else
                        ;
                    if (m_listPrevRDGValues[a][i].deviation != m_listCurRDGValues[a][i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                        return true;
                    else
                        ;
                }

                //for (int i = 0; i < 24; i++)
                //{
                //    if (m_prevRDGValues[i].pbr.Equals(m_curRDGValues[i].pbr) /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/  == false)
                //        return true;
                //    else
                //        ;
                //    if (m_prevRDGValues[i].recomendation != m_curRDGValues[i].recomendation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                //        return true;
                //    else
                //        ;
                //    if (m_prevRDGValues[i].deviationPercent != m_curRDGValues[i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                //        return true;
                //    else
                //        ;
                //    if (m_prevRDGValues[i].deviation != m_curRDGValues[i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                //        return true;
                //    else
                //        ;
                //}
            }

            return bRes;
        }

        public override Errors SaveChanges()
        {
            Errors errRes = Errors.NoError,
                    bErr = Errors.NoError;
            int indxEv = -1;

            m_evSaveChangesComplete.Reset();

            lock (m_lockResSaveChanges)
            {
                m_listResSaveChanges.Clear();
            }

            int prevIndxTECComponent = indxTECComponents;

            foreach (RDGStruct[] curRDGValues in m_listCurRDGValues)
            {
                bErr = Errors.NoError;

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.TG)
                {
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
                    if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                    {
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
                    ;

                lock (m_lockResSaveChanges)
                {
                    m_listResSaveChanges.Add(bErr);

                    if (!(bErr == Errors.NoError) && (errRes == Errors.NoError))
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

        protected void /*bool*/ ExpRDGExcelValuesRequest()
        {
            
        }

        protected override void InitializeSyncState()
        {
            m_semaIndxTECComponents = new Semaphore(1, 1);

            base.InitializeSyncState();
        }

        protected override void initTEC()
        {
            allTECComponents.Clear();

            foreach (StatisticCommon.TEC t in this.m_list_tec)
            {
                //Logging.Logg().Debug("Admin::InitTEC () - формирование компонентов для ТЭЦ:" + t.name);
                if (t.m_id > Index_LK)
                if (t.list_TECComponents.Count > 0)
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        //comboBoxTecComponent.Items.Add(t.name + " - " + g.name);
                        allTECComponents.Add(g);
                    }
                else
                {
                    //comboBoxTecComponent.Items.Add(t.name);
                    allTECComponents.Add(t.list_TECComponents[0]);
                }
            }
        }

        public int GetIdTECOwnerTECComponent(int indx = -1)
        {
            int iRes = -1;

            iRes = allTECComponents[indx].tec.m_id;

            return iRes;
        }
    }
}
