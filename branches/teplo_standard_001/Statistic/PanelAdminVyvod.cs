using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;
using StatisticAlarm;

namespace Statistic
{
    public class PanelAdminVyvod : PanelAdmin
    {
        public class AdminTS_Vyvod : AdminTS
        {
            public AdminTS_Vyvod(bool[] arMarkSavePPBRValues)
                : base(arMarkSavePPBRValues)
            {
            }
        }

        private enum INDEX_CONTROL_UI
        {   UNKNOWN = -1
            , COUNT };

        protected override void InitializeComponents()
        {
            base.InitializeComponents ();

            int posY = 271
                , offsetPosY = m_iSizeY + 2 * m_iMarginY
                , indx = -1;
            Rectangle[] arRectControlUI = new Rectangle[] {
            };

            this.dgwAdminTable = new DataGridViewAdminVyvod();

            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);

            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(714, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();

            this.ResumeLayout();
        }

        public PanelAdminVyvod(int idListener, HMark markQueries)
            : base(idListener, FormChangeMode.MANAGER.DISP, markQueries, new int[] { 0, (int)TECComponent.ID.GTP })
        {
        }

        public override bool Activate(bool activate)
        {
            return base.Activate (activate);
        }

        private TECComponent findTECComponent(int id) { return m_admin.FindTECComponent(id); }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;
            //int offset = -1;

            for (int i = 0; i < dgwAdminTable.Rows.Count; i++)
            {
                //offset = m_admin.GetSeasonHourOffset(i);
                
                for (int j = 0; j < (int)DataGridViewAdminVyvod.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminVyvod.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value, out value);
                            m_admin.m_curRDGValues[i].pmin = 0.0;
                            break;
                        case (int)DataGridViewAdminVyvod.DESC_INDEX.UDGt: // УДГэ
                            break;
                        case (int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION: // Рекомендация
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].Value, out value);
                            m_admin.m_curRDGValues[i].recomendation = value;

                            break;
                        case (int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE:
                                if (!(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value == null))
                                    m_admin.m_curRDGValues[i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                else
                                    m_admin.m_curRDGValues[i].deviationPercent = false;

                            break;
                        case (int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION].Value, out value);
                            m_admin.m_curRDGValues[i].deviation = value;

                            break;
                        default:
                            break;
                    }
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        public override void setDataGridViewAdmin(DateTime date)
        {
            int offset = -1;
            string strFmtDatetime = string.Empty;

            //??? не очень изящное решение
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {
                m_evtAdminTableRowCount.Reset();
                this.BeginInvoke(new DelegateFunc(normalizedTableHourRows));
                m_evtAdminTableRowCount.WaitOne(System.Threading.Timeout.Infinite);
            }
            else
                Logging.Logg().Error(@"PanelTAdminKomDisp::setDataGridViewAdmin () - ... BeginInvoke (normalizedTableHourRows) - ...", Logging.INDEX_MESSAGE.D_001);

            ((DataGridViewAdminVyvod)this.dgwAdminTable).m_PBR_0 = m_admin.m_curRDGValues_PBR_0;

            for (int i = 0; i < m_admin.m_curRDGValues.Length; i++)
            {
                strFmtDatetime = m_admin.GetFmtDatetime (i);
                offset = m_admin.GetSeasonHourOffset (i + 1);

                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDatetime);

                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value = m_admin.m_curRDGValues[i].pbr.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].ToolTipText = m_admin.m_curRDGValues[i].pbr_number;
                if (i > 0)
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.UDGt].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues[i - 1].pbr) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                else
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.UDGt].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues_PBR_0) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].Value = m_admin.m_curRDGValues[i].recomendation.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].ToolTipText = m_admin.m_curRDGValues[i].dtRecUpdate.ToString ();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value = m_admin.m_curRDGValues[i].deviationPercent.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION].Value = m_admin.m_curRDGValues[i].deviation.ToString("F2");
            }

            m_admin.CopyCurToPrevRDGValues();
        }

        public override void ClearTables()
        {
            this.dgwAdminTable.ClearTables();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
        }      

        protected override void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.comboBoxTecComponent_SelectionChangeCommitted (sender, e);
        }

        private class DataGridViewAdminVyvod : DataGridViewAdmin
        {
            public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, UDGt, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
            private static string[] arDescStringIndex = { "DateHour", "Plan", @"UDGt", "Recomendation", "DeviationType", "Deviation", "ToAll" };
            private static string[] arDescRusStringIndex = { "Дата, час", "План", @"УДГт", "Рекомендация", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };
            private static string[] arDefaultValueIndex = { string.Empty, string.Empty, string.Empty, string.Empty, false.ToString(), string.Empty };

            public double m_PBR_0;

            /// <summary>
            /// Инициализация компонентов DataGridView
            /// </summary>
            protected override void InitializeComponents()
            {
                base.InitializeComponents();

                int col = -1;
                Columns.AddRange(new DataGridViewColumn[(int)DESC_INDEX.COUNT_COLUMN] {
                    new DataGridViewTextBoxColumn()
                    , new DataGridViewTextBoxColumn()
                    , new DataGridViewTextBoxColumn()
                    , new DataGridViewComboBoxColumn()
                    , new DataGridViewCheckBoxColumn()
                    , new DataGridViewTextBoxColumn()                    
                    , new DataGridViewButtonColumn()
                });
                col = 0;
                for (col = 0; col < (int)DESC_INDEX.RECOMENDATION; col++)
                {
                    Columns[col].Frozen = true;
                    Columns[col].HeaderText = arDescRusStringIndex[col];
                    Columns[col].Name = arDescStringIndex[col];
                    Columns[col].ReadOnly = true;
                    Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                }

                for (col = (int)DESC_INDEX.RECOMENDATION; col < (int)DESC_INDEX.COUNT_COLUMN; col++)
                {
                    Columns[col].Frozen = false;
                    Columns[col].HeaderText = arDescRusStringIndex[col];
                    Columns[col].Name = arDescStringIndex[col];
                    Columns[col].ReadOnly = false;
                    Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                }

                //dgvCellStyleError = new DataGridViewCellStyle();
                //dgvCellStyleError.BackColor = Color.Red;

                //dgvCellStyleGTP = new DataGridViewCellStyle();
                //dgvCellStyleGTP.BackColor = Color.Yellow;

                this.Dock = DockStyle.Fill;

                this.CellValueChanged += new DataGridViewCellEventHandler(DataGridViewAdminVyvod_CellValueChanged);

                this.HorizontalScrollBar.Visible = true;
            }

            protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs ev)
            {
                double value;
                bool valid;

                switch (ev.ColumnIndex)
                {
                    case (int)DESC_INDEX.PLAN: // План
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value, out value);
                        if ((valid == false) || (value > maxRecomendationValue))
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = 0.ToString("F2");
                        else
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = value.ToString("F2");
                        break;
                    case (int)DESC_INDEX.UDGt: //Не редактируется
                        break;
                    case (int)DESC_INDEX.RECOMENDATION: // Рекомендация
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                        if ((valid == false) || (value > maxRecomendationValue))
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        else
                        {
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");

                            double prevPbr
                                , Pbr = double.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value.ToString());
                            if (ev.RowIndex > 0)
                                prevPbr = double.Parse(Rows[ev.RowIndex - 1].Cells[(int)DESC_INDEX.PLAN].Value.ToString());
                            else
                                prevPbr = m_PBR_0;

                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.UDGt].Value = (((Pbr + prevPbr) / 2) + value).ToString("F2");
                        }
                        break;
                    case (int)DESC_INDEX.DEVIATION_TYPE:
                        break;
                    case (int)DESC_INDEX.DEVIATION: // Максимальное отклонение
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value, out value);
                        bool isPercent = bool.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        double maxValue = -1F;

                        if (isPercent == true)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if ((valid == false) || (value < 0) || (value > maxValue))
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = 0.ToString("F2");
                        else
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = value.ToString("F2");
                        break;
                    default:
                        break;
                }
            }

            public override void ClearTables()
            {
                //for (int i = 0; i < Rows.Count; i++)
                foreach (DataGridViewRow r in Rows)
                    for (int j = (int)DESC_INDEX.DATE_HOUR; j < ((int)DESC_INDEX.TO_ALL + 0); j++)
                        r.Cells[j].Value = arDefaultValueIndex[j];
            }

            private void DataGridViewAdminVyvod_CellValueChanged(object obj, DataGridViewCellEventArgs ev)
            {
            }
        }
    }
}

