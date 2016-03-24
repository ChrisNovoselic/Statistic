﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mt
{
    public partial class FormMainTransMT : FormMainTransModes
    {
        public FormMainTransMT()
            : base((int)ProgramBase.ID_APP.TRANS_MODES_TERMINALE)
        {
            this.notifyIconMain.Icon =
            this.Icon = trans_mt.Properties.Resources.statistic6;

            InitializeComponentTransDB ();

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
        }

        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_mt.ini", true);

            m_sFileINI.AddMainPar(@"ТипБДКфгИсточник", @"200");
            m_sFileINI.AddMainPar(@"ИгнорДатаВремя-ModesTerminale", false.ToString());

            int[] arConfigDB = new int[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            string[] arKeyTypeConfigDB = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { @"ТипБДКфгИсточник", @"ТипБДКфгНазначение" };

            //TYPE_DATABASE_CFG[] arTypeConfigDB = new TYPE_DATABASE_CFG[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { TYPE_DATABASE_CFG.UNKNOWN, TYPE_DATABASE_CFG.UNKNOWN };
            //for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            //{
            //    arConfigDB[i] = Int32.Parse(m_sFileINI.GetMainValueOfKey(arKeyTypeConfigDB[i]));
            //    for (TYPE_DATABASE_CFG t = TYPE_DATABASE_CFG.CFG_190; t < TYPE_DATABASE_CFG.UNKNOWN; t++)
            //    {
            //        if (t.ToString().Contains(arConfigDB[i].ToString()) == true)
            //        {
            //            arTypeConfigDB[i] = t;
            //            break;
            //        }
            //        else
            //            ;
            //    }
            //}

            bool bIgnoreTECInUse = false;
            //string strTypeField = m_sFileINI.GetMainValueOfKey(@"РДГФорматТаблицаНазначение");
            int idListener = -1;

            HMark markQueries = new HMark(new int[] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR, (int)StatisticCommon.CONN_SETT_TYPE.MTERM });
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.MTERM);

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                idListener = DbMCSources.Sources().Register(s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett(i), false, @"CONFIG_DB");

                if (! (idListener < 0))
                {
                    switch (i)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[i] = new AdminMT();
                            break;
                        case (Int16)CONN_SETT_TYPE.DEST:
                            m_arAdmin[i] = new AdminTS_Modes(new bool[] { false, true });
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        m_arAdmin[i].InitTEC(idListener, m_modeTECComponent, /*arTypeConfigDB [i], */markQueries, bIgnoreTECInUse, new int[] { 0, (int)TECComponent.ID.LK });
                        RemoveTEC(m_arAdmin[i]);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "FormMainTransMT::FormMainTransMT ()", Logging.INDEX_MESSAGE.NOT_SET);
                        //ErrorReport("Ошибка соединения. Переход в ожидание.");
                        //setUIControlConnectionSettings(i);
                        break;
                    }
                    switch (i)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[i].m_ignore_date = bool.Parse(m_sFileINI.GetMainValueOfKey(@"ИгнорДатаВремя-ModesTerminale"));
                            break;
                        case (Int16)CONN_SETT_TYPE.DEST:
                            //if (strTypeField.Equals(AdminTS.TYPE_FIELDS.DYNAMIC.ToString()) == true)
                            //    ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                            //else if (strTypeField.Equals(AdminTS.TYPE_FIELDS.STATIC.ToString()) == true)
                            //    ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                            //else
                            //    ;
                            m_arAdmin[i].m_ignore_date = bool.Parse(m_sFileINI.GetMainValueOfKey(@"ИгнорДатаВремя-techsite"));
                            break;
                        default:
                            break;
                    }

                    //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                    DbMCSources.Sources().UnRegister(idListener);
                }
                else
                    ;
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    //setUIControlConnectionSettings(i); //??? Перенос ДО цикла

                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    //m_arAdmin[i].SetDelegateWait(new DelegateFunc(StartWait), new DelegateFunc(StopWait), delegateEvent);
                    m_arAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                    m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
                    m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                    m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                    //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                    //??? Перенос ПОСЛЕ цикла
                    //if (i == (int)(Int16)CONN_SETT_TYPE.DEST)
                    //    (Int16)CONN_SETT_TYPE.DEST
                    m_arAdmin[i].Start();
                    //else
                    //    ;
                }

                //Перенес обратно...
                //((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).StartDbInterface();

                //panelMain.Visible = false;

                base.Start();
            }
            else
                ;
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
        }

        protected override void setUIControlSourceState()
        {
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged() == true)
            {
                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.SOURCE);
            }
            else
                ;
        }

        protected override void timer_Start()
        {
            m_listTECComponentIndex = ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).GetListIndexTECComponent(m_modeTECComponent, true);

            base.timer_Start();
        }
    }
}
