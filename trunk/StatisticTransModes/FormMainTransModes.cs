﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using StatisticCommon;
using StatisticTrans;

namespace StatisticTransModes
{
    public abstract class FormMainTransModes : FormMainTrans
    {
        public FormMainTransModes()
            : base(new string[] { @"ИгнорДатаВремя-techsite", @"РДГФорматТаблицаНазначение", @"ТипБДКфгНазначение" },
                                    new string[] { false.ToString(), AdminTS.TYPE_FIELDS.DYNAMIC.ToString(), @"200" })
        {
            InitializeComponentTransModes();

            //m_SetupINI = new SETUP_INI ();

            //???
            this.m_dgwAdminTable = new StatisticTransModes.DataGridViewAdminModes();
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

            //m_listIsDataTECComponents = new List<bool>();

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            m_arAdmin = new HAdmin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        private void InitializeComponentTransModes()
        {
        }

        protected override void start()
        {
            base.Start();
        }

        protected override void updateDataGridViewAdmin(DateTime date)
        {
            int indxDB = m_IndexDB
                , offset = 0;

            //this.BeginInvoke(new HClassLibrary.DelegateIntFunc(initAdminTableRows), indxDB);

            string strFmtDateHour = string.Empty;
            for (int i = 0; i < m_arAdmin[indxDB].m_curRDGValues.Length; i++)
            {
                strFmtDateHour = m_arAdmin[indxDB].GetFmtDatetime(i, out offset);

                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDateHour);

                switch (indxDB)
                {
                    case (int)CONN_SETT_TYPE.SOURCE:
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].Value = m_arAdmin[indxDB].m_curRDGValues[i].pbr.ToString("F2");
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMIN].Value = m_arAdmin[indxDB].m_curRDGValues[i].pmin.ToString("F2");
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMAX].Value = m_arAdmin[indxDB].m_curRDGValues[i].pmax.ToString("F2");
                        break;
                    case (int)CONN_SETT_TYPE.DEST:
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].Value = m_arAdmin[indxDB].m_curRDGValues[i].pbr.ToString("F2");
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMIN].Value = m_arAdmin[indxDB].m_curRDGValues[i].pmin.ToString("F2");
                        this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMAX].Value = m_arAdmin[indxDB].m_curRDGValues[i].pmax.ToString("F2");
                        break;
                    default:
                        break;
                }
            }

            //m_arAdmin[indxDB].CopyCurToPrevRDGValues ();

            //this.m_dgwAdminTable.Invalidate();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            ClearTables();

            switch (m_modeTECComponent)
            {
                case FormChangeMode.MODE_TECCOMPONENT.GTP:
                    switch (m_IndexDB)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[m_IndexDB].GetRDGValues(-1, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                            break;
                        case (Int16)CONN_SETT_TYPE.DEST:
                            m_arAdmin[m_IndexDB].GetRDGValues((int)((AdminTS)m_arAdmin[m_IndexDB]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
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

        protected override void component_Changed(object sender, EventArgs e)
        {
            if (m_IndexDB == (short)CONN_SETT_TYPE.DEST)
            {
                base.component_Changed(sender, e);
            }
            else ;
        }

        private int GetIndexGTPOwner(int indx_tg)
        {
            return -1;
        }

        protected override void getDataGridViewAdmin(int indxDB) //indxDB = DEST (ВСЕГДА)
        {
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminModes.DESC_INDEX.PBR: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = value;

                            m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST].m_curRDGValues[i].pbr_number = m_arAdmin[(int)(Int16)CONN_SETT_TYPE.SOURCE].m_curRDGValues[i].pbr_number;
                            break;
                        case (int)DataGridViewAdminModes.DESC_INDEX.PMIN: // Pmin
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMIN].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmin = value;
                            break;
                        case (int)DataGridViewAdminModes.DESC_INDEX.PMAX: // Pmax
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMAX].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmax = value;
                            break;
                        default:
                            break;
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTransModes));
            this.panelMain.SuspendLayout();
            this.groupBoxSource.SuspendLayout();
            this.groupBoxDest.SuspendLayout();
            //((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).BeginInit();
            this.SuspendLayout();
            // 
            // FormMainTransMC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(841, 568);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMainTransModes";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            //((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void buttonClose_Click(object sender, EventArgs e)
        {
            for (int i = 0; (i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) && (!(m_arAdmin == null)); i++)
            {
                if (!(m_arAdmin[i] == null)) (m_arAdmin[i]).Stop(); else ;
            }

            Close();
        }
    }

}
