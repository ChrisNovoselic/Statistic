using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_tg
{
    public partial class FormMainTransTG : FormMainTrans
    {
        private System.Windows.Forms.Label labelSourcePathExcel;
        private System.Windows.Forms.TextBox tbxSourcePathExcel;
        private System.Windows.Forms.Button buttonPathExcel;

        List <bool> m_listIsDataTECComponents;

        public FormMainTransTG()
        {
            InitializeComponentTransTG();

            this.Text = "Конвертер данных плана и административных данных (ТГ)";

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
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            m_listIsDataTECComponents = new List<bool> ();

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            m_dgwAdminTable.Size = new System.Drawing.Size(498, 392);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.TEC;

            CreateFormConnectionSettings("connsett_tg.ini");

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { null, null, null, null, null},
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            m_arAdmin = new Admin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            bool bIgnoreTECInUse = false;
            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new AdminTransTG ();
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, bIgnoreTECInUse);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].ReConnSettingsRDGSource(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_typeFields = Admin.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_ignore_date = false;

            //Получатель
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST] = new AdminTransTG();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].SetDelegateTECComponent(FillComboBoxTECComponent);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, bIgnoreTECInUse);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].connSettConfigDB = m_formConnectionSettings.getConnSett();
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_typeFields = Admin.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_date = true;

            setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete (saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].StartDbInterface();
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

            timerMain.Interval = 666; //Признак первой итерации
            timerMain.Start();
        }

        private void watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
        }

        private void InitializeComponentTransTG()
        {
            this.labelSourcePathExcel = new System.Windows.Forms.Label();
            this.tbxSourcePathExcel = new System.Windows.Forms.TextBox();
            this.buttonPathExcel = new System.Windows.Forms.Button();

            this.groupBoxSource.Controls.Add(this.labelSourcePathExcel);
            this.groupBoxSource.Controls.Add(this.tbxSourcePathExcel);
            this.groupBoxSource.Controls.Add(this.buttonPathExcel);
            // 
            // labelSourcePathExcel
            // 
            this.labelSourcePathExcel.AutoSize = true;
            this.labelSourcePathExcel.Location = new System.Drawing.Point(11, 28);
            this.labelSourcePathExcel.Name = "labelSourcePathExcel";
            this.labelSourcePathExcel.Size = new System.Drawing.Size(95, 13);
            this.labelSourcePathExcel.TabIndex = 20;
            this.labelSourcePathExcel.Text = "Путь РДГ (Excel)";
            // 
            // tbxSourcePathExcel
            // 
            this.tbxSourcePathExcel.Location = new System.Drawing.Point(11, 55);
            this.tbxSourcePathExcel.Name = "tbxSourcePathExcel";
            this.tbxSourcePathExcel.Size = new System.Drawing.Size(243, 20);
            this.tbxSourcePathExcel.TabIndex = 15;
            this.tbxSourcePathExcel.TextChanged += new System.EventHandler(this.component_Changed);
            this.tbxSourcePathExcel.ReadOnly = true;
            // 
            // buttonPathExcel
            // 
            this.buttonPathExcel.Location = new System.Drawing.Point(257, 53);
            this.buttonPathExcel.Name = "buttonPathExcel";
            this.buttonPathExcel.Size = new System.Drawing.Size(29, 23);
            this.buttonPathExcel.TabIndex = 2;
            this.buttonPathExcel.Text = "...";
            this.buttonPathExcel.UseVisualStyleBackColor = true;
            //this.buttonPathExcel.Click += new System.EventHandler(...);
            this.buttonPathExcel.Enabled = false;

            base.buttonSourceExport.Location = new System.Drawing.Point(8, 86);
            
            base.buttonSourceSave.Location = new System.Drawing.Point(151, 86);
            base.buttonSourceSave.Click -= base.buttonSave_Click;
            base.buttonSourceSave.Click += new EventHandler (this.buttonSavePathExcel_Click);
            base.buttonSourceSave.Enabled = false;

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();

            this.groupBoxSource.Size = new System.Drawing.Size(300, 120);

            this.groupBoxDest.Location = new System.Drawing.Point(3, 196);

            base.panelMain.Size = new System.Drawing.Size(822, 404);

            //base.buttonClose.Anchor = AnchorStyles.Left;
            base.buttonClose.Location = new System.Drawing.Point(733, 434);

            this.Size = new System.Drawing.Size(849, 514);

            this.m_checkboxModeMashine.Location = new System.Drawing.Point(13, 434);
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

        protected /*override*/ void buttonSavePathExcel_Click(object sender, EventArgs e)
        {
        }

        protected override void setUIControlSourceState()
        {
            tbxSourcePathExcel.Text = m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[((AdminNSS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_listTECComponentIndexDetail[0]].tec.m_path_rdg_excel;
            enabledButtonSourceExport (tbxSourcePathExcel.Text.Length > 0 ? true : false);
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
                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues.Add(new Admin.RDGStruct[24]);

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
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].plan = Convert.ToDouble(m_dgwAdminTable.Rows[i].Cells[indx_comp + 1].Value); // '+ 1' за счет DateTime

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
                comboBoxTECComponent.Items.AddRange(((AdminNSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).GetListNameTEC());

                if (comboBoxTECComponent.Items.Count > 0)
                {
                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].indxTECComponents = m_listTECComponentIndex[0];
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
                indx = ((AdminNSS)m_arAdmin[indxDB]).m_listTECComponentIndexDetail[m_dgwAdminTable.Columns.Count - 2];
            ((DataGridViewAdminNSS)m_dgwAdminTable).addTextBoxColumn(((AdminNSS)m_arAdmin[indxDB]).GetNameTECComponent(indx),
                                                                        ((AdminNSS)m_arAdmin[indxDB]).GetIdTECComponent(indx),
                                                                        ((AdminNSS)m_arAdmin[indxDB]).GetIdGTPOwnerTECComponent(indx),
                                                                        date);

            DataGridViewCellEventArgs ev;

            for (int i = 0; i < 24; i++)
            {
                if (m_dgwAdminTable.Columns.Count == 3) //Только при добавлении 1-го столбца
                    m_dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                else
                    ;

                m_dgwAdminTable.Rows[i].Cells[m_dgwAdminTable.Columns.Count - 2].Value = ((AdminNSS)m_arAdmin[indxDB]).m_listCurRDGValues[m_dgwAdminTable.Columns.Count - 3][i].plan.ToString("F2");
                ev = new DataGridViewCellEventArgs(m_dgwAdminTable.Columns.Count - 2, i);
                ((DataGridViewAdminNSS)m_dgwAdminTable).DataGridViewAdminNSS_CellValueChanged(null, ev);
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        private void SaveChanges ()
        {
            ((AdminNSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).SaveChanges();
        }

        protected override void SaveRDGValues(bool bCallback)
        {
            m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST].SaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
            //((AdminNSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SaveChanges();
        }

        protected override void setDataGridViewAdmin(DateTime date)
        {
            int indxDB = -1;

            m_countDataTECComponents ++;

            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                if (m_countDataTECComponents == ((AdminNSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_listTECComponentIndexDetail.Count)
                {
                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);
                    
                    this.BeginInvoke(new DelegateBoolFunc(SaveRDGValues), false);

                    //this.BeginInvoke(new DelegateFunc(SaveChanges));
                }
                else
                    ;
            }
            else
            {
                this.BeginInvoke(new DelegateDateFunction(addTextBoxColumn), date);
            }
        }

        protected override void buttonClear_Click(object sender, EventArgs e)
        {
            m_arAdmin[m_IndexDB].ClearRDGValues(dateTimePickerMain.Value.Date);
            
            m_dgwAdminTable.ClearTables ();

            m_arAdmin[(int)CONN_SETT_TYPE.DEST].GetRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
        }
    }
}
