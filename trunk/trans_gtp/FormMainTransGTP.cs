﻿using System;
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

            CreateFormConnectionSettings("connsett_gtp.ini", true);

            //Добавление необходимого кол-ва элементов настроек для соединения с БД конфигурации
            //if (m_formConnectionSettingsConfigDB.Count < 2)
            //{
            //    while (!(m_formConnectionSettingsConfigDB.Count < 2))
            //        m_formConnectionSettingsConfigDB.addConnSett(m_formConnectionSettingsConfigDB.Count);
            //    конфигурацияБДToolStripMenuItem.PerformClick();

            //    return;
            //}
            //else
            //    ;

            m_fileINI.Add (@"ТипБДКфгИсточник", @"190");
            m_fileINI.Add (@"РДГФорматТаблицаИсточник", @"STATIC");

            ////Для переназначения идентификаторов источников данных БийскТЭЦ
            //m_fileINI.Add(@"ID_БДНазначение_ASKUE", @"6,");
            //m_fileINI.Add(@"ID_БДНазначение_SOTIASSO", @"6,");
            //m_fileINI.Add(@"ID_БДНазначение_PPBR_PBR", @"6,103");
            //m_fileINI.Add(@"ID_БДНазначение_PPBR_ADMIN", @"6,");

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

            m_fileINI.Add(@"ТЭЦПараметрыНазначение", @"{}");

            int idListener = -1;
            //Инициализация объектов получения данных
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arAdmin[i] = new AdminTS_KomDisp(new bool[] { false, true });
                idListener = DbSources.Sources().Register(m_formConnectionSettingsConfigDB.getConnSett(i), false, @"CONFIG_DB");
                try
                {
                    //((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true, false);
                    m_arAdmin[i].InitTEC(idListener, m_modeTECComponent, arTypeConfigDB[i], m_markQueries, true);
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "FormMainTransGTP::FormMainTransGTP ()");
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }

                ////Для переназначения идентификаторов источников данных БийскТЭЦ
                //int j = -1;
                //string val = m_fileINI.GetValueOfKey (@"ID_БДНазначение_PPBR_PBR");
                //val = val.Split (',')[0];
                //for (j = 0; j < m_arAdmin[i].m_list_tec.Count; j ++) {
                //    if (m_arAdmin[i].m_list_tec[j].m_id == Int32.Parse (val))
                //        break;
                //    else
                //        ;
                //}

                //if (j < m_arAdmin[i].m_list_tec.Count) {
                //} else {
                //}

                if (i == (int)CONN_SETT_TYPE.DEST) {
                    string strTECParametersDest = m_fileINI.GetValueOfKey(@"ТЭЦПараметрыНазначение");
                    if (strTECParametersDest.Equals (string.Empty) == false) {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                        string prefix_admin = @""
                            , prefix_pbr = @"BiTEC";
                        int err = -1
                            , indx = -1
                            , indx_tec = 0;

                        //Создание объекта ТЭЦ
                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].m_arNameTableAdminValues[(int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields] = @"";
                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].m_arNameTableUsedPPBRvsPBR[(int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields] = @"BiPPBRvsPBR";
                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].prefix_admin = prefix_admin;
                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].prefix_pbr = prefix_pbr;

                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].SetNamesField(@"", //ADMIN_DATETIME
                                            @"", //ADMIN_REC
                                            @"", //ADMIN_IS_PER
                                            @"", //ADMIN_DIVIAT
                                            @"Date_time", //PBR_DATETIME
                                            @"PBR", //PPBRvsPBR
                                            @"PBR_number");

                        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].connSettings(ConnectionSettingsSource.GetConnectionSettings(InitTECBase.TYPE_DATABASE_CFG.CFG_190, idListener, 103, -1, out err), (int)StatisticCommon.CONN_SETT_TYPE.PBR);

                        if (err == 0)
                        {
                            for (int c = 0; c < m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents.Count; c ++) {
                                if ((m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents [c].m_id > 100) && (m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents [c].m_id < 500)) {
                                    switch (m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents [c].m_id) {
                                        case 117:
                                            prefix_pbr = @"TG1";
                                            break;
                                        case 118:
                                            prefix_pbr = @"TG28";
                                            break;
                                        default:
                                            break;
                                    }
                                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents [c].prefix_admin = prefix_admin;
                                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].list_TECComponents[c].prefix_pbr = prefix_pbr;
                                } else {
                                }
                            }
                            
                        }
                        else
                            ; //Ошибка получения параметров соединений с БД
                    } else ;
                }
                else {
                }

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

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
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
