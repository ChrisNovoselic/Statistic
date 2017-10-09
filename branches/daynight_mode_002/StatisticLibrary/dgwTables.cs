using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Threading;
using System.Data;
using System.Globalization;
using System.Drawing;

namespace StatisticCommon
{
    public abstract class HDataGridViewTables : DataGridView
    {
        public struct VALUE
        {
            string value;

            string tool_tip;
        }

        protected bool _bIsItogo;

        /// <summary>
        /// ������� ��� ������ ����� �������
        /// </summary>
        public enum INDEX_CELL_STYLE {
            /// <summary>�����</summary>
            COMMON
            /// <summary>��������������</summary>
            , WARNING
            /// <summary>������</summary>summary>
            , ERROR };

        private static Color[] s_dgvCellColors; 

        /// <summary>
        /// ����� ����� �������
        /// </summary>
        public static DataGridViewCellStyle[] s_dgvCellStyles;

        /// <summary>
        /// ����������� - �������� (� �����������)
        /// </summary>
        /// <param name="bIsItogo">������� ������������� ������ � ��������� ���������</param>
        public HDataGridViewTables(Color[] colors, bool bIsItogo)
            : base()
        {
            _bIsItogo = bIsItogo;

            s_dgvCellStyles = new DataGridViewCellStyle [] {
                new DataGridViewCellStyle(DefaultCellStyle) // COMMON
                , new DataGridViewCellStyle(DefaultCellStyle) // WARNING
                , new DataGridViewCellStyle(DefaultCellStyle) // ERROR
            };

            s_dgvCellStyles [(int)INDEX_CELL_STYLE.WARNING].BackColor = colors[(int)INDEX_CELL_STYLE.WARNING];
            s_dgvCellStyles [(int)INDEX_CELL_STYLE.ERROR].BackColor = colors [(int)INDEX_CELL_STYLE.ERROR];
        }

        public void InitRows(int cnt, bool bIns)
        {
            if (bIns == true)
                while (Rows.Count < (cnt + (_bIsItogo == true ? 1 : 0)))
                    Rows.Insert(0, 1);
            else
                while (Rows.Count > (cnt + (_bIsItogo == true ? 1 : 0)))
                    Rows.RemoveAt(0);
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

                s_dgvCellStyles[(int)INDEX_CELL_STYLE.COMMON].BackColor =
                    //value.Equals (SystemColors.Control) == true
                    //    ? SystemColors.Window :
                            value;
            }
        }

        public void Fill (Dictionary<int, VALUE[]> values)
        {
            foreach (KeyValuePair<int, VALUE []> pair in values) {
                foreach (VALUE value in pair.Value) {

                }
            }
        }

        public void Fill (VALUE [,] values)
        {
        }
    }
 }
