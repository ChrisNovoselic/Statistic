using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mc
{
    public partial class FormMainTransMC : FormMainTransModes
    {
        public FormMainTransMC () : base () {
            this.notifyIconMain.Icon =
            this.Icon = trans_mc.Properties.Resources.statistic5;

            InitializeComponentTransSrc (@"Сервер Модес-Центр");

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);
        }
        
        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettingsCfgDBofAdmins("connsett_mc.ini");

            m_fileINI.Add(@"MCServiceHost", string.Empty);
            m_fileINI.Add(@"ИгнорДатаВремя-ModesCentre", false.ToString());

            InitTECBase.TYPE_DATABASE_CFG typeConfigDB = InitTECBase.TYPE_DATABASE_CFG.UNKNOWN;
            for (InitTECBase.TYPE_DATABASE_CFG t = InitTECBase.TYPE_DATABASE_CFG.CFG_190; t < InitTECBase.TYPE_DATABASE_CFG.UNKNOWN; t++)
            {
                if (t.ToString().Contains(m_fileINI.GetValueOfKey(@"ТипБДКфгНазначение")) == true)
                {
                    typeConfigDB = t;
                    break;
                }
                else
                    ;
            }

            bool bIgnoreTECInUse = false;
            string strTypeField = m_fileINI.GetValueOfKey(@"РДГФорматТаблицаНазначение");
            int idListener = DbMCSources.Sources().Register(m_formConnectionSettingsConfigDB.getConnSett(), false, @"CONFIG_DB");
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i] = new AdminMC(m_fileINI.GetValueOfKey(@"MCServiceHost"), m_report);
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        m_arAdmin[i] = new AdminTS_Modes(m_report, new bool[] { false, true });
                        break;
                    default:
                        break;
                }
                try
                {
                    m_arAdmin[i].InitTEC(idListener, m_modeTECComponent, typeConfigDB, bIgnoreTECInUse);
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMainTransMC::FormMainTransMC ()");
                    //ErrorReport("Ошибка соединения. Переход в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i].m_ignore_date = bool.Parse(m_fileINI.GetValueOfKey(@"ИгнорДатаВремя-ModesCentre"));
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        if (strTypeField.Equals(AdminTS.TYPE_FIELDS.DYNAMIC.ToString()) == true)
                            ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                        else if (strTypeField.Equals(AdminTS.TYPE_FIELDS.STATIC.ToString()) == true)
                            ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                        else
                            ;
                        m_arAdmin[i].m_ignore_date = bool.Parse(m_fileINI.GetValueOfKey(@"ИгнорДатаВремя-techsite"));
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
                    m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                    m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
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

                m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = m_fileINI.GetValueOfKey(@"MCServiceHost");
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
