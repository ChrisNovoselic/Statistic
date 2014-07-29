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
        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettingsConfigDB("connsett_mt.ini");

            int[] arConfigDB = new int[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            string[] arKeyTypeConfigDB = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { @"ТипБДКфгИсточник", @"ТипБДКфгНазначение" };
            //FileINI fileINI = new FileINI(@"setup.ini");
            //string sec = "Main (" + ProgramBase.AppName + ")";

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

            string[] arStrTypeField = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            arStrTypeField[(int)CONN_SETT_TYPE.SOURCE] = m_fileINI.GetValueOfKey(@"РДГФорматТаблицаИсточник");
            arStrTypeField[(int)CONN_SETT_TYPE.DEST] = m_fileINI.GetValueOfKey(@"РДГФорматТаблицаНазначение");

            bool bIgnoreDateTime = false;
            if (Boolean.TryParse(m_fileINI.GetValueOfKey(@"ИгнорДатаВремя-techsite"), out bIgnoreDateTime) == false)
                bIgnoreDateTime = false;
            else
                ;

            int idListener = -1;
            //Инициализация объектов получения данных
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arAdmin[i] = new AdminTS_KomDisp(m_report, new bool[] { false, true });
                idListener = DbSources.Sources().Register(m_formConnectionSettingsConfigDB.getConnSett(i), false, @"CONFIG_DB");
                try
                {
                    //((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true, false);
                    ((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(idListener, m_modeTECComponent, arTypeConfigDB[i], true);
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMainTransGTP::FormMainTransGTP ()");
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }

                //((AdminTS)m_arAdmin[i]).connSettConfigDB = m_formConnectionSettings.getConnSett(i);

                for (AdminTS.TYPE_FIELDS tf = AdminTS.TYPE_FIELDS.STATIC; i < (int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS; tf++)
                    if (arStrTypeField[i].Equals(tf.ToString()) == true)
                    {
                        ((AdminTS)m_arAdmin[i]).m_typeFields = tf;
                        break;
                    }
                    else
                        ;

                m_arAdmin[i].m_ignore_date = bIgnoreDateTime;
                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].Start();

                DbSources.Sources().UnRegister(idListener);
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                start ();
            }
            else
                ;
        }
    }
}
