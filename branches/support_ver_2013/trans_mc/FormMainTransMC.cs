﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mc
{
    public partial class FormMainTransMC : FormMainTransModes
    {
        public FormMainTransMC()
            : base((int)ProgramBase.ID_APP.TRANS_MODES_CENTRE_GUI)
        {
            this.notifyIconMain.Icon =
            this.Icon = trans_mc.Properties.Resources.statistic5;
            InitializeComponentTransSrc (@"Сервер Модес-Центр");

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);
        }

        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_mc.ini", false);

            m_sFileINI.AddMainPar(@"MCServiceHost", string.Empty);
            m_sFileINI.AddMainPar(@"ИгнорДатаВремя-ModesCentre", false.ToString());

            TYPE_DATABASE_CFG typeConfigDB = TYPE_DATABASE_CFG.UNKNOWN;
            for (TYPE_DATABASE_CFG t = TYPE_DATABASE_CFG.CFG_190; t < TYPE_DATABASE_CFG.UNKNOWN; t++)
            {
                if (t.ToString().Contains(m_sFileINI.GetMainValueOfKey(@"ТипБДКфгНазначение")) == true)
                {
                    typeConfigDB = t;
                    break;
                }
                else
                    ;
            }

            bool bIgnoreTECInUse = false;
            string strTypeField = m_sFileINI.GetMainValueOfKey(@"РДГФорматТаблицаНазначение");
            int idListener = DbMCSources.Sources().Register(s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            HMark markQueries = new HMark(new int [] {(int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR});

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i] = new AdminMC(m_sFileINI.GetMainValueOfKey(@"MCServiceHost"));
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        m_arAdmin[i] = new AdminTS_Modes(new bool[] { false, true });
                        break;
                    default:
                        break;
                }
                try
                {
                    m_arAdmin[i].InitTEC(idListener, m_modeTECComponent, typeConfigDB, markQueries, bIgnoreTECInUse);
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "FormMainTransMC::FormMainTransMC ()", Logging.INDEX_MESSAGE.NOT_SET);
                    //ErrorReport("Ошибка соединения. Переход в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i].m_ignore_date = bool.Parse(m_sFileINI.GetMainValueOfKey(@"ИгнорДатаВремя-ModesCentre"));
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        if (strTypeField.Equals(AdminTS.TYPE_FIELDS.DYNAMIC.ToString()) == true)
                            ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                        else if (strTypeField.Equals(AdminTS.TYPE_FIELDS.STATIC.ToString()) == true)
                            ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                        else
                            ;
                        m_arAdmin[i].m_ignore_date = bool.Parse(m_sFileINI.GetMainValueOfKey(@"ИгнорДатаВремя-techsite"));
                        break;
                    default:
                        break;
                }

                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор
            }

            DbMCSources.Sources().UnRegister(idListener);

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    //setUIControlConnectionSettings(i); //??? Перенос ДО цикла

                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    //m_arAdmin[i].SetDelegateWait(new DelegateFunc (StartWait), new DelegateFunc(StopWait), delegateEvent);
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

        protected override void setUIControlSourceState()
        {
            if (((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].m_listMCentreId.Count > 0)
            {
                //Properties.Settings sett = new Properties.Settings();
                //tbxSourceServerMC.Text = sett.Modes_Centre_Service_Host_Name;

                m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = m_sFileINI.GetMainValueOfKey(@"MCServiceHost");
            }
            else
                m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = string.Empty;

            enabledButtonSourceExport(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text.Length > 0 ? true : false);
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
            
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged() == true)
            {
                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                setUIControlSourceState();
            }
            else
                ;
        }
    }
}
