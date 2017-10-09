using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

using HClassLibrary;
using System.Drawing;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridViewAdmin
    {
        public enum COLUMN_INDEX : ushort { DATE_HOUR, PLAN, UDGe, RECOMENDATION, FOREIGN_CMD, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };

        /// <summary>
        /// �������� �������� ������� ������������� ��� ������������� �� ��������
        /// </summary>
        struct COLUMN_PROPERTIES {
            /// <summary>
            /// ��� �������
            /// </summary>
            public Type _type;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public string _name;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public string _headerText;
            /// <summary>
            /// �������� �� ���������
            /// </summary>
            public string DefaultValue;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public bool _frozen;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public bool _readOnly;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public DataGridViewTriState _resizable;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public int _width;
            /// <summary>
            /// ������������� ������������ ��������
            /// </summary>
            public System.Windows.Forms.DataGridViewColumnSortMode _sortMode;
        }

        public double m_PBR_0;

        public DataGridViewAdminKomDisp(System.Drawing.Color []colors) : base(colors)
        {
            this.CellMouseMove += new DataGridViewCellMouseEventHandler (dgwAdminTable_CellMouseMove);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Columns.AddRange (new DataGridViewColumn [] {
                new DataGridViewTextBoxColumn () { // DATE_HOUR
                Name = COLUMN_INDEX.DATE_HOUR.ToString()
                , HeaderText = "����, ���"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = true
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn () { // PLAN
                Name = COLUMN_INDEX.PLAN.ToString()
                , HeaderText = "����"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                , Width = 56
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // UDGe
                Name = COLUMN_INDEX.UDGe.ToString()
                , HeaderText = "����"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                , Width = 56
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // RECOMENDATION
                Name = COLUMN_INDEX.RECOMENDATION.ToString()
                , HeaderText = "������������"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewCheckBoxColumn() { // FOREIGN_CMD
                Name = COLUMN_INDEX.FOREIGN_CMD.ToString()
                , HeaderText = "�����. ���-��"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = false }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewCheckBoxColumn() { // DEVIATION_TYPE
                Name = COLUMN_INDEX.DEVIATION_TYPE.ToString()
                , HeaderText = "���������� � ���������"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = false }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // DEVIATION
                Name = COLUMN_INDEX.DEVIATION.ToString()
                , HeaderText = "�������� ������������� ����������"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.True
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewButtonColumn() { // TO_ALL
                Name = COLUMN_INDEX.TO_ALL.ToString()
                , HeaderText = "�����������"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }});

            //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

            (Columns [(int)COLUMN_INDEX.TO_ALL] as DataGridViewButtonColumn).DefaultCellStyle.BackColor = SystemColors.Control;

            BackColor = SystemColors.Window;
        }

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs ev)
        {
            double value;
            bool valid;

            switch (ev.ColumnIndex)
            {
                case (int)COLUMN_INDEX.PLAN: // ����
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value = 0.ToString("F2");
                    else
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value = value.ToString("F2");
                    break;
                case (int)COLUMN_INDEX.UDGe: //�� �������������
                    break;
                case (int)COLUMN_INDEX.RECOMENDATION: // ������������
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                    else {
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value = value.ToString("F2");

                        double prevPbr
                            , Pbr = double.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value.ToString ());
                        if (ev.RowIndex > 0)
                            prevPbr = double.Parse(Rows[ev.RowIndex - 1].Cells[(int)COLUMN_INDEX.PLAN].Value.ToString());
                        else
                            prevPbr = m_PBR_0;

                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.UDGe].Value = (((Pbr + prevPbr) / 2) + value).ToString("F2");
                    }
                    break;
                case (int)COLUMN_INDEX.FOREIGN_CMD:
                    bool fCmd = false;
                    try {
                        fCmd = bool.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.FOREIGN_CMD].Value.ToString());
                    }
                    catch (Exception e) {
                        Logging.Logg().Exception(e, @"DataGridViewAdminKomDisp::CellValidate () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value, out value);
                    if ((valid == false) /*|| (value == 0F)*/ || (value > maxRecomendationValue))
                        fCmd = false;
                    else
                        ;
                    Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.FOREIGN_CMD].Value = fCmd;
                    break;
                case (int)COLUMN_INDEX.DEVIATION_TYPE:
                    break;
                case (int)COLUMN_INDEX.DEVIATION: // ������������ ����������
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value, out value);
                    bool isPercent = bool.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION_TYPE].Value.ToString());
                    double maxValue = -1F;

                    if (isPercent == true)
                        maxValue = maxDeviationPercentValue;
                    else
                        maxValue = maxDeviationValue; // ������ ��� �������� �� �����������, �� ��� ������������ ������� ���������

                    if ((valid == false) || (value < 0) || (value > maxValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value = 0.ToString("F2");
                    else
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value = value.ToString("F2");
                    break;
                default:
                    break;
            }
        }

        public override void ClearTables()
        {
            ////for (int i = 0; i < Rows.Count; i++)
            //foreach (DataGridViewRow r in Rows)
            //    for (int j = (int)COLUMN_INDEX.DATE_HOUR; j < ((int)COLUMN_INDEX.TO_ALL + 0); j++)
            //        r.Cells[j].Value = _columnProperties[j].DefaultValue;
        }

        private void dgwAdminTable_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case (int)COLUMN_INDEX.DATE_HOUR:
                case (int)COLUMN_INDEX.PLAN:
                case (int)COLUMN_INDEX.UDGe:
                    Cursor = Cursors.Help;
                    break;
                case (int)COLUMN_INDEX.RECOMENDATION:
                case (int)COLUMN_INDEX.DEVIATION:
                    Cursor = Cursors.IBeam;
                    break;
                case (int)COLUMN_INDEX.FOREIGN_CMD:
                case (int)COLUMN_INDEX.DEVIATION_TYPE:
                case (int)COLUMN_INDEX.TO_ALL:
                    Cursor = Cursors.Hand;
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
            }
        }
    }
}
