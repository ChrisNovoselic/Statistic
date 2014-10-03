using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mt
{
    public partial class FormMainTransMT : FormMainTransModes
    {        
        public FormMainTransMT () : base () {
            this.notifyIconMain.Icon =
            this.Icon = trans_mt.Properties.Resources.statistic6;

            InitializeComponentTransDB ();

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
        }

        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettings("connsett_mt.ini", false);

            m_fileINI.Add(@"ТипБДКфгИсточник", @"200");
            m_fileINI.Add(@"ИгнорДатаВремя-ModesTerminale", false.ToString ());

            int[] arConfigDB = new int[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            string[] arKeyTypeConfigDB = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { @"ТипБДКфгИсточник", @"ТипБДКфгНазначение" };

            InitTECBase.TYPE_DATABASE_CFG[] arTypeConfigDB = new InitTECBase.TYPE_DATABASE_CFG[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { InitTECBase.TYPE_DATABASE_CFG.UNKNOWN, InitTECBase.TYPE_DATABASE_CFG.UNKNOWN };
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                arConfigDB[i] = Int32.Parse(m_fileINI.GetValueOfKey(arKeyTypeConfigDB[i]));
                for (InitTECBase.TYPE_DATABASE_CFG t = InitTECBase.TYPE_DATABASE_CFG.CFG_190; t < InitTECBase.TYPE_DATABASE_CFG.UNKNOWN; t++)
                {
                    if (t.ToString().Contains(arConfigDB[i].ToString()) == true)
                    {
                        arTypeConfigDB[i] = t;
                        break;
                    }
                    else
                        ;
                }
            }

            bool bIgnoreTECInUse = false;
            string strTypeField = m_fileINI.GetValueOfKey(@"РДГФорматТаблицаНазначение");
            int idListener = -1;

            HMark markQueries = new HMark();
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.MTERM);

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                idListener = DbMCSources.Sources().Register(m_formConnectionSettingsConfigDB.getConnSett(i), false, @"CONFIG_DB");

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
                        m_arAdmin[i].InitTEC(idListener, m_modeTECComponent, arTypeConfigDB [i], markQueries, bIgnoreTECInUse);
                        RemoveTEC(m_arAdmin[i]);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "FormMainTransMT::FormMainTransMT ()");
                        //ErrorReport("Ошибка соединения. Переход в ожидание.");
                        //setUIControlConnectionSettings(i);
                        break;
                    }
                    switch (i)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[i].m_ignore_date = bool.Parse(m_fileINI.GetValueOfKey(@"ИгнорДатаВремя-ModesTerminale"));
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
                    m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

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
    }
}
