﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using StatisticCommon;
using StatisticTrans;
using ASUTP;
using System.ServiceModel;

namespace StatisticTransModes
{
    public abstract class FormMainTransModes : FormMainTrans
    {
        public FormMainTransModes(ASUTP.Helper.ProgramBase.ID_APP id_app, KeyValuePair<string, string> [] config)
            : base(id_app
                , new System.Collections.Generic.KeyValuePair<string, string> [] {
                    new System.Collections.Generic.KeyValuePair<string, string> (@"ИгнорДатаВремя-techsite", false.ToString())
                }.Concat(config).ToArray())
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

            ///??? в конструкторе
            //Start ();
            Load += FormMainTransModes_OnLoad;
        }

        private void FormMainTransModes_OnLoad (object sender, EventArgs e)
        {
            Start();
        }

        private void InitializeComponentTransModes()
        {
        }

        protected override void start()
        {
            base.Start();
        }

        /// <summary>
        /// Обновить значения в представлении
        /// </summary>
        /// <param name="date">Дата за которую требуется обновить значения</param>
        /// <param name="bNewValues">Признак наличия новых значений (false - обновление оформления представления со старыми значениями при изменении цветовой схемы)</param>
        /// <param name="values">Значения для отображения</param>
        protected override void updateDataGridViewAdmin (DateTime date, bool bNewValues, IList<HAdmin.RDGStruct> values)
        {
            StatisticTrans.CONN_SETT_TYPE indxDB = m_IndexDB;
            int offset = 0
                , iRow = -1
                , iSkiped = 0;

            //HAdmin.RDGStruct[] values = _client.GetRDGValues (m_IndexDB);

            string strFmtDateHour = string.Empty;
            for (int i = 0; i < values.Count; i++)
            {
                strFmtDateHour = _client.GetFormatDatetime (indxDB, i);
                offset = _client.GetSeasonHourOffset(indxDB, i);

                if (string.IsNullOrEmpty (values [i].pbr_number) == true) {
                    iSkiped++;

                    continue;
                } else
                    ;

                iRow = i - iSkiped;

                this.m_dgwAdminTable.Rows[iRow].Cells[(int)DataGridViewAdminModes.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDateHour);

                this.m_dgwAdminTable.Rows[iRow].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].Value = values[i].pbr.ToString("F2");
                this.m_dgwAdminTable.Rows[iRow].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].ToolTipText = values[i].pbr_number;
                this.m_dgwAdminTable.Rows[iRow].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMIN].Value = values[i].pmin.ToString("F2");
                this.m_dgwAdminTable.Rows[iRow].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMAX].Value = values[i].pmax.ToString("F2");
            }
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            ASUTP.Logging.Logg ().Debug ($"FormMainTransModes::comboBoxTECComponent_SelectedIndexChanged () - m_IndexDB={m_IndexDB}, ModeTECComponent={m_modeTECComponent.ToString()}, date={dateTimePickerMain.Value.Date.ToShortDateString()}..."
                , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

            ClearTables ();

            switch (m_modeTECComponent)
            {
                case FormChangeMode.MODE_TECCOMPONENT.GTP:
                    _client.GetRDGValues (m_IndexDB, SelectedItemKey, dateTimePickerMain.Value.Date);
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
            if (m_IndexDB == StatisticTrans.CONN_SETT_TYPE.DEST)
            {
                base.component_Changed(sender, e);
            }
            else ;
        }

        private int getIndexGTPOwner(int indx_tg)
        {
            int iRes = -1;
            
            return iRes;
        }

        protected override void getDataGridViewAdmin(StatisticTrans.CONN_SETT_TYPE indxDB) //indxDB = DEST (ВСЕГДА)
        {
            double value;
            bool valid;
            HAdmin.RDGStruct [] values;
            HAdmin.RDGStruct rdgValue;

            for (int i = 0; i < 24; i++)
            {
                rdgValue = new HAdmin.RDGStruct ();

                for (int j = 0; j < (int)DataGridViewAdminKomDisp.COLUMN_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminModes.DESC_INDEX.PBR: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PBR].Value, out value);
                            rdgValue.pbr = value;
                            ////??? где источник, где назначение
                            //rdgValue.pbr_number = _client.GetPBRNumber (StatisticTrans.CONN_SETT_TYPE.SOURCE, i);
                            break;
                        case (int)DataGridViewAdminModes.DESC_INDEX.PMIN: // Pmin
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMIN].Value, out value);
                            rdgValue.pmin = value;
                            break;
                        case (int)DataGridViewAdminModes.DESC_INDEX.PMAX: // Pmax
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminModes.DESC_INDEX.PMAX].Value, out value);
                            rdgValue.pmax = value;
                            break;
                        default:
                            break;
                    }
                }

                //_client.SetCurrentRDGValue (indxDB, i, rdgValue);
            }

            //_client.CopyCurToPrevRDGValues(indxDB);
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
            _client.Stop ();

            Close();
        }
    }

}
