﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;

namespace trans_tg
{
    public partial class FormMainTransTG : FormMainTrans
    {
        public FormMainTransTG()
            : base((int)ProgramBase.ID_APP.TRANS_TG
            , new string[] { @"ТипБДКфгНазначение" }
            , new string[] { @"200" })
        {
            InitializeComponentTransSrc(@"Путь РДГ (Excel)");

            //???
            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminNSS();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            // 
            // m_dgwAdminTable
            // 
            this.SuspendLayout();
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 401);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            //m_listIsDataTECComponents = new List<bool> ();

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.TEC;

            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        private void watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
        }

        protected override void start()
        {
            base.Start();
        }

        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettings("connsett_tg.ini", true);

            int iConfigDB = -1;
            string keyTypeConfigDB = @"ТипБДКфгНазначение";
            //FileINI fileINI = new FileINI(@"setup.ini");
            //string sec = "Main (" + ProgramBase.AppName + ")";
            iConfigDB = Int32.Parse(m_sFileINI.GetValueOfKey(keyTypeConfigDB));

            TYPE_DATABASE_CFG iTypeConfigDB = TYPE_DATABASE_CFG.UNKNOWN;

            for (TYPE_DATABASE_CFG t = TYPE_DATABASE_CFG.CFG_190; t < TYPE_DATABASE_CFG.UNKNOWN; t++)
            {
                if (t.ToString().Contains(iConfigDB.ToString()) == true)
                {
                    iTypeConfigDB = t;
                    break;
                }
                else
                    ;
            }

            bool bIgnoreTECInUse = false;

            HMark markQueries = new HMark();
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                if (i == (Int16)CONN_SETT_TYPE.SOURCE)
                    m_arAdmin[i] = new AdminTransTG(new bool [] {false, false});
                else
                    if (i == (Int16)CONN_SETT_TYPE.DEST)
                        m_arAdmin[i] = new AdminTransTG(new bool[] { false, true });
                    else
                        ;

                try {
                    ((AdminTS)m_arAdmin[i]).InitTEC(idListener, FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, iTypeConfigDB, markQueries, bIgnoreTECInUse);
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "FormMainTransTG::FormMainTransTG ()");
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                //((AdminTS)m_arAdmin[i]).connSettConfigDB = m_formConnectionSettings.getConnSett();
                ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                if (i == (Int16)CONN_SETT_TYPE.SOURCE)
                    m_arAdmin[i].m_ignore_date = false;
                else
                    if (i == (Int16)CONN_SETT_TYPE.DEST)
                        m_arAdmin[i].m_ignore_date = true;
                    else
                        ;

                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор
            }

            DbSources.Sources().UnRegister(idListener);

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                    m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
                    m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                    m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                    //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                    m_arAdmin[i].Start();
                }

                /*
                // This needs to be declared in a place where it will not go out of scope.
                // For example, it would be a class variable in a form class.
                System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
                // This code would go in one of the initialization methods of the class.
                watcher.Path = "c:\\";
                // Watch only for changes to *.txt files.
                watcher.Filter = "*.txt";
                watcher.IncludeSubdirectories = false;
                // Enable the component to begin watching for changes.
                watcher.EnableRaisingEvents = true;
                // Filter for Last Write changes.
                watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                // Example of watching more than one type of change.
                watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.Size;

                //Асинхронно
                watcher.Changed += new System.IO.FileSystemEventHandler(this.watcher_Changed);

                //Сихронно
                watcher.WaitForChanged(System.IO.WatcherChangeTypes.All);
                */

                //panelMain.Visible = false;

                start ();
            }
            else
                ;
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged () == true)
            {
                ClearTables();

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        switch (m_IndexDB)
                        {
                            case (int)CONN_SETT_TYPE.SOURCE:
                                m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].ResetRDGExcelValues();

                                ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).fillListIndexTECComponent(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]);
                                ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).fillListIndexTECComponent(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]);
                                int countComp = ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_listTECComponentIndexDetail.Count;

                                //if (m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].tec.m_path_rdg_excel.Length > 0)
                                //{
                                ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).ImpRDGExcelValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                                //}
                                //else
                                //    ;
                                break;
                            case (int)CONN_SETT_TYPE.DEST:
                                ((AdminTS)m_arAdmin[m_IndexDB]).GetRDGValues((int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                                break;
                            default:
                                break;
                        }

                        setUIControlSourceState();
                        setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        protected override void component_Changed(object sender, EventArgs e)
        {
            if (m_IndexDB == (short)CONN_SETT_TYPE.DEST)
            {
                /*
                uint indxDB = (uint)m_IndexDB;
                ConnectionSettings connSett = new ConnectionSettings();

                connSett.server = tbxDestServerIP.Text;
                connSett.port = (int)nudnDestPort.Value;
                connSett.dbName = tbxDestNameDatabase.Text;
                connSett.userName = tbxDestUserId.Text;
                connSett.password = tbxDestPassword.Text;
                connSett.ignore = false;

                m_formConnectionSettings.ConnectionSettingsEdit = connSett;
                */

                base.component_Changed(sender, e);
            }
            else ;
        }

        protected override void setUIControlSourceState()
        {
            m_arUIControls [(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[((AdminTS_NSS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_listTECComponentIndexDetail[0]].tec.m_path_rdg_excel;
            enabledButtonSourceExport(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text.Length > 0 ? true : false);
        }

        private int GetIndexGTPOwner(int indx_tg)
        {
            int indxDB = m_IndexDB,
                id_gtp_owner = ((DataGridViewAdminNSS)m_dgwAdminTable).GetIdGTPOwner(indx_tg);

            foreach (int indx in ((AdminTransTG)m_arAdmin[indxDB]).m_listTECComponentIndexDetail)
            {
                if (((AdminTransTG)m_arAdmin[indxDB]).allTECComponents[indx].m_id == id_gtp_owner)
                {
                    return ((AdminTransTG)m_arAdmin[indxDB]).m_listTECComponentIndexDetail.IndexOf(indx);
                }
                else
                    ;
            }

            return -1;
        }

        protected override void getDataGridViewAdmin(int indxDB) //indxDB = DEST (ВСЕГДА)
        {
            double value;
            bool valid;

            ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues.Clear ();

            foreach (int indx in ((AdminTransTG)m_arAdmin[indxDB]).m_listTECComponentIndexDetail)
            {
                int indx_comp = ((AdminTransTG)m_arAdmin[indxDB]).m_listTECComponentIndexDetail.IndexOf(indx),
                    indx_owner = GetIndexGTPOwner(indx_comp);

                if (!(indx_comp < 0))
                {
                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues.Add(new AdminTS.RDGStruct[24]);

                    if (((AdminTransTG)m_arAdmin[indxDB]).modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                    {
                        ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_listCurRDGValues[indx_comp].CopyTo(((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp], 0);
                    }
                    else
                        if (((AdminTransTG)m_arAdmin[indxDB]).modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                        {
                            if (!(indx_owner < 0))
                                for (int i = 0; i < 24; i++)
                                {
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].pbr = Convert.ToDouble(m_dgwAdminTable.Rows[i].Cells[indx_comp + 1].Value); // '+ 1' за счет DateTime

                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].recomendation = 0.0;
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].deviationPercent = ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_owner][i].deviationPercent;
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].deviation = ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_owner][i].deviation;
                                }
                            else
                                ;
                        }
                        else
                            ;
                }
                else
                    ;
            }

            //((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).getDataGridViewAdmin(((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]));
            ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).getCurTimezoneOffsetRDGExcelValues(((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]));
        }

        protected override void FillComboBoxTECComponent()
        {
            comboBoxTECComponent.Items.Clear();

            if (m_listTECComponentIndex.Count > 0) {
                comboBoxTECComponent.Items.AddRange(((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).GetListNameTEC());

                if (comboBoxTECComponent.Items.Count > 0)
                {
                    ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).indxTECComponents = m_listTECComponentIndex[0];
                    comboBoxTECComponent.SelectedIndex = 0;
                }
                else
                    ;
            }
            else
                ;
        }

        private void addTextBoxColumn(DateTime date)
        {
            int indxDB = m_IndexDB,
                indx = ((AdminTS_NSS)m_arAdmin[indxDB]).m_listTECComponentIndexDetail[m_dgwAdminTable.Columns.Count - 2];
            ((DataGridViewAdminNSS)m_dgwAdminTable).addTextBoxColumn(((AdminTS_NSS)m_arAdmin[indxDB]).GetNameTECComponent(indx),
                                                                        ((AdminTS_NSS)m_arAdmin[indxDB]).GetIdTECComponent(indx),
                                                                        ((AdminTS_NSS)m_arAdmin[indxDB]).GetIdGTPOwnerTECComponent(indx),
                                                                        date);

            DataGridViewCellEventArgs ev;

            for (int i = 0; i < 24; i++)
            {
                if (m_dgwAdminTable.Columns.Count == 3) //Только при добавлении 1-го столбца
                    m_dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("dd-MM-yyyy HH:00");
                else
                    ;

                m_dgwAdminTable.Rows[i].Cells[m_dgwAdminTable.Columns.Count - 2].Value = ((AdminTS_NSS)m_arAdmin[indxDB]).m_listCurRDGValues[m_dgwAdminTable.Columns.Count - 3][i].pbr.ToString("F2");
                ev = new DataGridViewCellEventArgs(m_dgwAdminTable.Columns.Count - 2, i);
                ((DataGridViewAdminNSS)m_dgwAdminTable).DataGridViewAdminNSS_CellValueChanged(null, ev);
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();

            //m_dgwAdminTable.Invalidate();
        }

        private void SaveChanges ()
        {
            ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).SaveChanges();
        }

        protected override void SaveRDGValues(bool bCallback)
        {
            ((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
            //((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SaveChanges();
        }

        protected override void updateDataGridViewAdmin(DateTime date)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateDateFunc(addTextBoxColumn), date);
            else
                Logging.Logg().Error(@"FormMainTransTG::updateDataGridViewAdmin () - ... BeginInvoke (addTextBoxColumn) - ...");
        }

        protected override void buttonClear_Click(object sender, EventArgs e)
        {
            //m_IndexDB = только DEST
            ((AdminTS)m_arAdmin[m_IndexDB]).ClearRDGValues(dateTimePickerMain.Value.Date);
            
            m_dgwAdminTable.ClearTables ();

            ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).GetRDGValues((int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
        }

        protected override void saveDataGridViewAdminComplete()
        {
            bool bCompletedSaveChanges = false;

            //Вариант №1
            //bCompletedSaveChanges = ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).CompletedSaveChanges;

            //Вариант №2
            bCompletedSaveChanges = WaitHandle.WaitAny(new WaitHandle[] { ((AdminTS_NSS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_evSaveChangesComplete }, 0) == 0;

            Logging.Logg().Debug(@"FormMainTransTG::saveDataGridViewAdminComplete () - CompletedSaveChanges=" + bCompletedSaveChanges.ToString());

            if (bCompletedSaveChanges == true)
            {
                //Logging.Logg().Debug(@"FormMainTransTG::saveDataGridViewAdminComplete () - SuccessSaveChanges=" + ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SuccessSaveChanges.ToString());
                //if (((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SuccessSaveChanges == true) {
                    base.saveDataGridViewAdminComplete();
                //} else ;
            }
            else
                ;
                
        }

        protected override void setDataGridViewAdmin(DateTime date)
        {
            //if (WindowState == FormWindowState.Minimized)
            //if (m_bTransAuto == true)
            //if (m_modeMashine == MODE_MASHINE.AUTO || m_modeMashine == MODE_MASHINE.SERVICE)
            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                if (((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).CompletedGetRDGValues == true)
                {
                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);

                    if (IsHandleCreated/*InvokeRequired*/ == true)
                        this.BeginInvoke(new DelegateBoolFunc(SaveRDGValues), false);
                    else
                        Logging.Logg().Error(@"FormMainTransTG::setDataGridViewAdmin () - ... BeginInvoke (SaveRDGValues) - ...");

                    //this.BeginInvoke(new DelegateFunc(trans_auto_next));
                }
                else
                    ;
            }
            else
            {
                //this.BeginInvoke(new DelegateDateFunction(addTextBoxColumn), date);
                updateDataGridViewAdmin(date);
            }
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
        }
    }
}
