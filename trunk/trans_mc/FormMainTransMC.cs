using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
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

        List<bool> m_listIsDataTECComponents;

        public FormMainTransMC() : base ()
        {
            InitializeComponentTransMC();

            this.Text = "Конвертер данных плана и административных данных (Modes-Centre ГТП)";

            //???
            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminMC();
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

            m_listIsDataTECComponents = new List<bool>();

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic5"))); //$this.Icon
            this.notifyIconMain.Text = "Статистика: конвертер (Modes-Centre ГТП)";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic5")));

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            CreateFormConnectionSettings("connsett_mc.ini");

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { null, null, null, null, null},
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            m_arAdmin = new HAdmin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            bool bIgnoreTECInUse = false;
            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new AdminMC();
            ((AdminMC)m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE]).InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, bIgnoreTECInUse);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].ReConnSettingsRDGSource(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
            //((AdminMC)m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_ignore_date = false;
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_ignore_connsett_data = true;

            //Получатель
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST] = new AdminTS_KomDisp();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].SetDelegateTECComponent(FillComboBoxTECComponent);
            ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, bIgnoreTECInUse);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].connSettConfigDB = m_formConnectionSettings.getConnSett();
            ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_date = true;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_connsett_data = true;

            setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
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
                //else
                //    ;
            }

            ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).StartDbInterface();

            //panelMain.Visible = false;

            timerMain.Interval = 666; //Признак первой итерации
            timerMain.Start();
        }

        private void InitializeComponentTransMC()
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
            this.labelSourceServerMC.Text = "Наименование сервера Modes-Centre";
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

            //Идентичный код с панелью Modes-Centre
            base.buttonSourceExport.Location = new System.Drawing.Point(8, 86);

            base.buttonSourceSave.Location = new System.Drawing.Point(151, 86);
            //base.buttonSourceSave.Click -= base.buttonSave_Click;
            //base.buttonSourceSave.Click += new EventHandler(this.buttonSaveServerMC_Click);
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

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)) &&
                (m_listTECComponentIndex.Count > 0) && (!(comboBoxTECComponent.SelectedIndex < 0)))
            {
                ClearTables();

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        switch (m_IndexDB) {
                            case (Int16)CONN_SETT_TYPE.SOURCE:
                                ((AdminMC)m_arAdmin[m_IndexDB]).GetRDGValues(-1, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                                break;
                            case (Int16)CONN_SETT_TYPE.DEST:
                                ((AdminTS)m_arAdmin[m_IndexDB]).GetRDGValues((int)((AdminTS)m_arAdmin[m_IndexDB]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                                break;
                            default:
                                break;
                        }
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
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
                base.component_Changed(sender, e);
            }
            else ;
        }

        //protected /*override*/ void buttonSavePathExcel_Click(object sender, EventArgs e)
        //{
        //}

        protected override void setUIControlSourceState()
        {
            //tbxSourceServerMC.Text = ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[((AdminTS_NSS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_listTECComponentIndexDetail[0]].tec.m_path_rdg_excel;
            enabledButtonSourceExport(tbxSourceServerMC.Text.Length > 0 ? true : false);
        }

        private int GetIndexGTPOwner(int indx_tg)
        {
            return -1;
        }

        protected override void getDataGridViewAdmin(int indxDB) //indxDB = DEST (ВСЕГДА)
        {
        }

        protected override void setDataGridViewAdmin(DateTime date)
        {
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTransMC));
            this.panelMain.SuspendLayout();
            this.groupBoxSource.SuspendLayout();
            this.groupBoxDest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).BeginInit();
            this.SuspendLayout();
            // 
            // FormMainTransMC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(841, 568);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMainTransMC";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void buttonClose_Click(object sender, EventArgs e)
        {
            if (!(m_arAdmin[(int)CONN_SETT_TYPE.DEST] == null)) ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).StopDbInterface(); else ;

            for (int i = 0; (i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) && (!(m_arAdmin == null)); i++)
            {
                //if (!(m_arAdmin[i] == null)) ((AdminTS)m_arAdmin[i]).StopDbInterface(); else ;
            }

            Close();
        }
    }
}
