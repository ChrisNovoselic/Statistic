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

        public FormMainTransTG()
        {
            InitializeComponentTransTG();

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.TG;

            CreateFormConnectionSettings("connsett_tg.ini");

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { null, null, null, null, null},
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            m_arAdmin = new Admin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            bool bIgnoreTECInUse = false;
            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new AdminTransTG();
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, bIgnoreTECInUse);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].ReConnSettingsRDGSource(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_typeFields = Admin.TYPE_FIELDS.DYNAMIC;

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

            this.groupBoxDest.Location = new System.Drawing.Point(8, 196);

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            m_dgwAdminTable.Size = new System.Drawing.Size(498, 392);

            base.panelMain.Size = new System.Drawing.Size(822, 404);

            //base.buttonClose.Anchor = AnchorStyles.Left;
            base.buttonClose.Location = new System.Drawing.Point(733, 434);

            this.Size = new System.Drawing.Size(849, 514);
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
            tbxSourcePathExcel.Text = m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].tec.m_path_rdg_excel;
            buttonSourceExport.Enabled = tbxSourcePathExcel.Text.Length > 0 ? true : false;
        }

        protected override void getDataGridViewAdmin(int indxDB) //indxDB = DEST (ВСЕГДА)
        {
            base.getDataGridViewAdmin(indxDB);

            ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_curTimezoneOffsetRDGExcelValues = new Admin.RDGStruct[((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curTimezoneOffsetRDGExcelValues.Length];
            ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curTimezoneOffsetRDGExcelValues.CopyTo(((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_curTimezoneOffsetRDGExcelValues, 0);
        }
    }
}
