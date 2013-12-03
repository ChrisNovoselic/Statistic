using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_mc
{
    public partial class FormMainTransMC : FormMainTrans
    {
        private System.Windows.Forms.Label labelSourceServerMC;
        private System.Windows.Forms.TextBox tbxSourceServerMC;
        private System.Windows.Forms.Button buttonServerMC;

        public FormMainTransMC()
        {
            InitializeComponentTransGTP();

            this.Text = "Конвертер данных плана и административных данных (Modes-Centre ГТП)";

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
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic5"))); //$this.Icon
            this.notifyIconMain.Text = "Статистика: конвертер Modes-Centre (ГТП)";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic5"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            CreateFormConnectionSettings("connsett_mc.ini");

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { null, null, null, null, null },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            m_arAdmin = new HAdmin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new AdminMC();
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].InitTEC(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.SOURCE);
            //m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].ReConnSettingsRDGSource(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
            //m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_ignore_connsett_data = true;

            //Получатель
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST] = new AdminTS_KomDisp();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].SetDelegateTECComponent(FillComboBoxTECComponent);
            ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).InitTEC(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST);
            ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_date = true;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_connsett_data = true;

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                if (i == (int)(Int16)CONN_SETT_TYPE.DEST)
                    ((AdminTS)m_arAdmin[i]).StartDbInterface();
                else
                    ;
            }

            //panelMain.Visible = false;

            timerMain.Interval = 666; //Признак первой итерации
            timerMain.Start();
        }

        private void InitializeComponentTransGTP()
        {
            this.labelSourceServerMC = new System.Windows.Forms.Label();
            this.tbxSourceServerMC = new System.Windows.Forms.TextBox();
            this.buttonServerMC = new System.Windows.Forms.Button();

            this.groupBoxSource.Controls.Add(this.labelSourceServerMC);
            this.groupBoxSource.Controls.Add(this.tbxSourceServerMC);
            this.groupBoxSource.Controls.Add(this.buttonServerMC);
            // 
            // labelSourceServerMC
            // 
            this.labelSourceServerMC.AutoSize = true;
            this.labelSourceServerMC.Location = new System.Drawing.Point(11, 28);
            this.labelSourceServerMC.Name = "labelSourceServerMC";
            this.labelSourceServerMC.Size = new System.Drawing.Size(95, 13);
            this.labelSourceServerMC.TabIndex = 20;
            this.labelSourceServerMC.Text = "Имя сервера Modes-Centre";
            // 
            // tbxSourceServerMC
            // 
            this.tbxSourceServerMC.Location = new System.Drawing.Point(11, 55);
            this.tbxSourceServerMC.Name = "tbxSourceServerMC";
            this.tbxSourceServerMC.Size = new System.Drawing.Size(243, 20);
            this.tbxSourceServerMC.TabIndex = 15;
            this.tbxSourceServerMC.TextChanged += new System.EventHandler(this.component_Changed);
            this.tbxSourceServerMC.ReadOnly = true;
            // 
            // buttonServerMC
            // 
            this.buttonServerMC.Location = new System.Drawing.Point(257, 53);
            this.buttonServerMC.Name = "buttonServerMC";
            this.buttonServerMC.Size = new System.Drawing.Size(29, 23);
            this.buttonServerMC.TabIndex = 2;
            this.buttonServerMC.Text = "...";
            this.buttonServerMC.UseVisualStyleBackColor = true;
            //this.buttonServerMC.Click += new System.EventHandler(...);
            this.buttonServerMC.Enabled = false;

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();

            //Идентичный код с панелью Modes-Centre
            base.buttonSourceExport.Location = new System.Drawing.Point(8, 86);

            base.buttonSourceSave.Location = new System.Drawing.Point(151, 86);
            base.buttonSourceSave.Click -= base.buttonSave_Click;
            //base.buttonSourceSave.Click += new EventHandler(this.buttonSavePathExcel_Click);
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

        protected override void CreateFormConnectionSettings(string connSettFileName)
        {
            base.CreateFormConnectionSettings(connSettFileName);

            if (m_formConnectionSettings.Protected == false || m_formConnectionSettings.Count < 2)
            {
                while (m_formConnectionSettings.Count < 2)
                    m_formConnectionSettings.addConnSett(new ConnectionSettings());
            }
            else
                ;
        }
    }
}
