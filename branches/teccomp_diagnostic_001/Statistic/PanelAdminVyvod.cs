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


using StatisticCommon;
using StatisticAlarm;
using ASUTP.Core;
using ASUTP;

namespace Statistic
{
    public partial class PanelAdminVyvod : PanelAdmin
    {
        private enum INDEX_CONTROL_UI { UNKNOWN = -1
            , COUNT
        };

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
            : base(idListener, markQueries, new int[] { 0, (int)TECComponent.ID.LK })
        {
        }

        public override bool Activate(bool activate)
        {
            return base.Activate (activate);
        }

        private TECComponent findTECComponent(int id) { return m_admin.FindTECComponent(id); }
        /// <summary>
        /// ��������� � ��� �������� � �����/������ (����� ������ ����� 'PanelAdminNSS')
        /// </summary>
        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;
            //int offset = -1;
            int hour = 1;
            DataGridViewAdminVyvod.DESC_INDEX col = DataGridViewAdminVyvod.DESC_INDEX.COUNT_COLUMN;

            for (hour = 0; hour < dgwAdminTable.Rows.Count; hour++)
            {
                //offset = m_admin.GetSeasonHourOffset(hour);

                for (col = DataGridViewAdminVyvod.DESC_INDEX.PLAN; col < DataGridViewAdminVyvod.DESC_INDEX.TO_ALL; col++)
                {
                    switch (col)
                    {
                        case DataGridViewAdminVyvod.DESC_INDEX.PLAN: // ����
                            valid = double.TryParse((string)dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value, out value);
                            if (valid == true)
                                m_admin.m_curRDGValues[hour].pmin = value;
                            else
                                ; //m_admin.m_curRDGValues[hour].pmin = 0F;
                            break;
                        case DataGridViewAdminVyvod.DESC_INDEX.UDGt: // ����
                            break;
                        case DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION: // ������������
                            valid = double.TryParse((string)dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].Value, out value);
                            if (valid == true)
                                m_admin.m_curRDGValues[hour].recomendation = value;
                            else
                                ;
                            break;
                        case DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE:
                            if (!(this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value == null))
                                m_admin.m_curRDGValues[hour].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                            else
                                ;
                            break;
                        case DataGridViewAdminVyvod.DESC_INDEX.DEVIATION: // ������������ ����������
                            valid = double.TryParse((string)this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION].Value, out value);
                            if (valid == true)
                                m_admin.m_curRDGValues[hour].deviation = value;
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                }
            }

            // ���������� ������������� �������� ��� ���� ����������
            (m_admin as AdminTS_Vyvod).ReplicateCurRDGValues();

            //m_admin.CopyCurRDGValues();
        }

        /// <summary>
        /// ��������� ������������� ���������� 
        ///  , ��� ������������� ��������� ���������� � ����������� �����
        ///  ��� ������������� ������� � ��������� ����������
        /// </summary>
        /// <param name="date">���� ������������ ��������</param>
        /// <param name="bNewValues">������� ������� ����� ��������, ����� ��������� �������� ���������� �������������</param>
        public override void setDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            int hour = -1
                , offset = -1;
            string strFmtDatetime = string.Empty;
            double PBR_0 = 0F;
            HAdmin.RDGStruct[] arSumCurRDGValues = null;

            (m_admin as AdminTS_Vyvod).SummatorRDGValues();

            if ((m_admin as AdminTS_TG).CompletedGetRDGValues == true)
            {
                //??? �� ����� ������� �������
                if (IsHandleCreated == true) {
                    if (InvokeRequired == true) {
                        m_evtAdminTableRowCount.Reset ();
                        this.BeginInvoke (new DelegateBoolFunc (normalizedTableHourRows), InvokeRequired);
                        m_evtAdminTableRowCount.WaitOne (System.Threading.Timeout.Infinite);
                    } else
                        normalizedTableHourRows(InvokeRequired);
                }
                else
                    Logging.Logg().Error(@"PanelAdminVyvod::setDataGridViewAdmin () - ... BeginInvoke (normalizedTableHourRows) - ...", Logging.INDEX_MESSAGE.D_001);

                // �������� �������� �� ������� ��� ��������� � ������
                PBR_0 =
                (this.dgwAdminTable as DataGridViewAdminVyvod).m_PBR_0 =
                    (m_admin as AdminTS_Vyvod).m_SumRDGValues_PBR_0;
                arSumCurRDGValues = new HAdmin.RDGStruct[(m_admin as AdminTS_Vyvod).m_arSumRDGValues.Length];
                (m_admin as AdminTS_Vyvod).m_arSumRDGValues.CopyTo(arSumCurRDGValues, 0);

                this.dgwAdminTable.DefaultCellStyle.BackColor = this.dgwAdminTable.BackColor;

                for (hour = 0; hour < arSumCurRDGValues.Length; hour++)
                {
                    strFmtDatetime = m_admin.GetFmtDatetime(hour);
                    offset = m_admin.GetSeasonHourOffset(hour + 1);

                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DATE_HOUR].Value = date.AddHours(hour + 1 - offset).ToString(strFmtDatetime);
                    //this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.DATE_HOUR].Style.BackColor = this.dgwAdminTable.BackColor;

                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value = arSumCurRDGValues[hour].pmin.ToString("F2");
                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].ToolTipText = arSumCurRDGValues[hour].pbr_number;
                    //this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Style.BackColor = this.dgwAdminTable.BackColor;

                    // UDGt ����������� � 'DataGridViewAdminVyvod::onCellValueChanged'

                    (this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION] as DataGridViewComboBoxCell).Value = (object)(((int)arSumCurRDGValues[hour].recomendation).ToString());
                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].ToolTipText = arSumCurRDGValues[hour].dtRecUpdate.ToString();
                    //this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Value = arSumCurRDGValues[hour].deviationPercent.ToString();
                    //this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION_TYPE].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION].Value = arSumCurRDGValues[hour].deviation.ToString("F2");
                    //this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.DEVIATION].Style.BackColor = this.dgwAdminTable.BackColor;

                    //if (bNewValues == false) {
                    //// �������������� �������� ���� ����, �.�. � ���� �������� ������ "�����������" ��� ��������/��������� ��������
                    //    this.dgwAdminTable.Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.UDGt].Style.BackColor = this.dgwAdminTable.BackColor;
                    //} else
                    //    ;
                }
            }
            else
                ; // ���� ����������, �� ��� �����������(���������) ��������

            if (bNewValues == true)
                m_admin.CopyCurToPrevRDGValues ();
            else
                ;
        }

        public override void ClearTables()
        {
            this.dgwAdminTable.ClearTables();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {//??? ����� 'AdminTS_NSS'
            base.InitializeComboBoxTecComponent (mode);

            if (m_listTECComponentIndex.Count > 0) {
                comboBoxTecComponent.Items.AddRange (((AdminTS_TG)m_admin).GetListNameTEC ());

                if (comboBoxTecComponent.Items.Count > 0)
                {
                    m_admin.indxTECComponents = m_listTECComponentIndex[0];
                    comboBoxTecComponent.SelectedIndex = 0;
                }
                else
                    ;
            }
            else
                ;
        }      

        protected override void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.comboBoxTecComponent_SelectionChangeCommitted (sender, e);
        }

        protected override void createAdmin ()
        {
            //����������� �������������� �������� ���: ��������� ���������� (��������� ���������� �� ������), ������ �� ���������
            m_admin = new PanelAdminVyvod.AdminTS_Vyvod (new bool[] { true, false });
        }

        private class DataGridViewAdminVyvod : DataGridViewAdmin
        {
            public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, UDGt, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
            private static string[] arDescStringIndex = { "DateHour", "Plan", @"UDGt", "Recomendation", "DeviationType", "Deviation", "ToAll" };
            private static string[] arDescRusStringIndex = { "����, ���", "����", @"����", "������������", "���������� � ���������", "�������� ������������� ����������", "�����������" };
            private static object[] arDefaultValueIndex = { string.Empty, string.Empty, string.Empty, string.Empty/*0.ToString()*/, false.ToString(), string.Empty };

            public double m_PBR_0;

            protected override int INDEX_COLUMN_BUTTON_TO_ALL { get { return (int)DataGridViewAdminVyvod.DESC_INDEX.TO_ALL; } }

            public DataGridViewAdminVyvod ()
                : base (FormMain.formGraphicsSettings.FontColor
                      , new Color [] {
                          SystemColors.Window
                          , Color.Yellow
                          , FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.DIVIATION)
                      })
            {
            }

            /// <summary>
            /// ������������� ����������� DataGridView
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

                DataGridViewComboBoxCell cellTemplateRec = new DataGridViewComboBoxCell();
                cellTemplateRec.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                cellTemplateRec.Items.AddRange(new object[] { "-3", "0", "3" });
                cellTemplateRec.Value = 0;
                Columns[(int)DESC_INDEX.RECOMENDATION].CellTemplate = cellTemplateRec;

                this.Dock = DockStyle.Fill;

                //this.BackColor = SystemColors.Window;
                this.CellValueChanged += new DataGridViewCellEventHandler(onCellValueChanged);

                this.HorizontalScrollBar.Visible = true;

                InitializeColumnToAll ();
            }

            protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs ev)
            {
                double value;
                bool valid;

                switch (ev.ColumnIndex)
                {
                    case (int)DESC_INDEX.PLAN: // ����
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value, out value);
                        if ((valid == false) || (value > maxRecomendationValue))
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = 0.ToString("F2");
                        else
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = value.ToString("F2");
                        break;
                    case (int)DESC_INDEX.UDGt: //�� �������������
                        break;
                    case (int)DESC_INDEX.RECOMENDATION: // ������������
                        //valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                        //if ((valid == false) || (value > maxRecomendationValue))
                        //    Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        //else
                        //{
                        //    Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");

                        //    double prevPbr
                        //        , Pbr = double.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value.ToString());
                        //    if (ev.RowIndex > 0)
                        //        prevPbr = double.Parse(Rows[ev.RowIndex - 1].Cells[(int)DESC_INDEX.PLAN].Value.ToString());
                        //    else
                        //        prevPbr = m_PBR_0;

                        //    Rows[ev.RowIndex].Cells[(int)DESC_INDEX.UDGt].Value = (((Pbr + prevPbr) / 2) + value).ToString("F2");
                        //}
                        break;
                    case (int)DESC_INDEX.DEVIATION_TYPE:
                        break;
                    case (int)DESC_INDEX.DEVIATION: // ������������ ����������
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value, out value);
                        bool isPercent = bool.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        double maxValue = -1F;

                        if (isPercent == true)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // ������ ��� �������� �� �����������, �� ��� ������������ ������� ���������

                        if ((valid == false) || (value < 0) || (value > maxValue))
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = 0.ToString("F2");
                        else
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = value.ToString("F2");
                        break;
                    default:
                        break;
                }
            }

            private void onCellValueChanged(object sender, DataGridViewCellEventArgs ev)
            {
                int hour = -1;
                double valPrevPBR = -1F
                    , valCurPBR = -1F
                    , valRec = -1F;

                if (ev.ColumnIndex == (int)DESC_INDEX.RECOMENDATION)
                {
                    hour = ev.RowIndex;
                    // �������� ���������� ��������
                    if (hour == 0)
                        valPrevPBR = m_PBR_0;
                    else
                        double.TryParse((string)Rows[hour - 1].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value, out valPrevPBR);
                    // �������� ������� ��������
                    double.TryParse((string)Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.PLAN].Value, out valCurPBR);
                    // �������� ���������� �������� ������������
                    if ((!(valPrevPBR < 0))
                        && (!(valCurPBR < 0))
                        && (double.TryParse((string)Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].Value, out valRec) == true))
                    {
                        //Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.RECOMENDATION].ToolTipText = HDateTime.ToMoscowTimeZone().ToString();
                        Rows[hour].Cells[(int)DataGridViewAdminVyvod.DESC_INDEX.UDGt].Value = (((valCurPBR + valPrevPBR) / 2) + ((valCurPBR + valPrevPBR) / 2) * (valRec / 100)).ToString("F2");
                        Rows [hour].Cells [(int)DataGridViewAdminVyvod.DESC_INDEX.UDGt].Style.BackColor = BackColor;
                    }
                    else
                        ;
                }
                else
                    ;
            }

            public override void ClearTables()
            {
                //for (int i = 0; i < Rows.Count; i++)
                foreach (DataGridViewRow r in Rows)
                    for (int j = (int)DESC_INDEX.DATE_HOUR; j < ((int)DESC_INDEX.TO_ALL + 0); j++)
                        r.Cells[j].Value = arDefaultValueIndex[j];
            }

            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value;

                    //??? ������� �1 - �����������
                    DefaultCellStyle.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                    ////??? ������� �2 - ���������� (�� �������������)
                    //if ((INDEX_COLUMN_BUTTON_TO_ALL > 0)
                    //    && (RowCount > 0))
                    //    for (int col = 0; col < (int)INDEX_COLUMN_BUTTON_TO_ALL; col++)
                    //        for (int i = 0; i < 24; i++) {
                    //        // ����������� �� ��������� ����� ���� � ������ ���
                    //        // ��������, ������������ � ������ �� ������� ��������� �������� - ���� ����� ����� �������� ������
                    //            Rows [i].Cells [col].Style.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                    //        }
                    //else
                    //// ��� ��������/����� - ��� �������� �� ��������� ����� ���� �����
                    //    ;
                }
            }
        }
    }
}

