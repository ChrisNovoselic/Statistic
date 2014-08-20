using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;

namespace trans_gtp
{
    public partial class FormMainTransGTP : FormMainTrans
    {
        public FormMainTransGTP()
            : base(new string[] { @"ИгнорДатаВремя-techsite", @"ТипБДКфгНазначение", @"РДГФорматТаблицаНазначение" },
                    new string[] { @"False", @"200", @"DYNAMIC" })
        {
            InitializeComponentTransDB();

            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.SuspendLayout();
            // 
            // m_dgwAdminTable
            // 
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            //Созжание массива для объектов получения данных
            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        protected override void start () {
            base.Start ();
        }
        
        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettingsCfgDB("connsett_gtp.ini");

            m_fileINI.Add (@"ТипБДКфгИсточник", @"190");
            m_fileINI.Add (@"РДГФорматТаблицаИсточник", @"STATIC");

            int[] arConfigDB = new int[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            string[] arKeyTypeConfigDB = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { @"ТипБДКфгИсточник", @"ТипБДКфгНазначение" };

            InitTECBase.TYPE_DATABASE_CFG[] arTypeConfigDB = new InitTECBase.TYPE_DATABASE_CFG[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { InitTECBase.TYPE_DATABASE_CFG.UNKNOWN, InitTECBase.TYPE_DATABASE_CFG.UNKNOWN };
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                arConfigDB[i] = Int32.Parse (m_fileINI.GetValueOfKey (arKeyTypeConfigDB[i]));
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
            arStrTypeField[(int)CONN_SETT_TYPE.SOURCE] = m_fileINI.GetValueOfKey (@"РДГФорматТаблицаИсточник");
            arStrTypeField[(int)CONN_SETT_TYPE.DEST] = m_fileINI.GetValueOfKey(@"РДГФорматТаблицаНазначение");

            bool bIgnoreDateTime = false;
            if (Boolean.TryParse(m_fileINI.GetValueOfKey (@"ИгнорДатаВремя-techsite"), out bIgnoreDateTime) == false)
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

        protected override void getDataGridViewAdmin(int indxDB)
        {
            //int indxDB = m_IndexDB;

            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = value;
                            //((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmin = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmin;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmax = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmax;

                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr_number = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr_number;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE:
                            {
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation = value;

                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        protected override void updateDataGridViewAdmin(DateTime date)
        {
            int indxDB = m_IndexDB;

            for (int i = 0; i < 24; i++)
            {
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation.ToString("F2");
            }

            //m_arAdmin[indxDB].CopyCurToPrevRDGValues ();

            //this.m_dgwAdminTable.Invalidate();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged() == true)
            {
                ClearTables();

                short indexDB = m_IndexDB;

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        ((AdminTS)m_arAdmin[indexDB]).GetRDGValues((int)((AdminTS)m_arAdmin[indexDB]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        break;
                    default:
                        break;
                }

                setUIControlConnectionSettings((int)CONN_SETT_TYPE.SOURCE);
                setUIControlConnectionSettings((int)CONN_SETT_TYPE.DEST);
            }
            else
                ;
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e) {
        }

        protected override void setUIControlSourceState()
        {
        }
    }
}
