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
        protected bool _bIsItogo;

        /// <summary>
        /// Конструктор - основной (с аргументами)
        /// </summary>
        /// <param name="bIsItogo">Признак необходимости строки с итоговыми значенями</param>
        public HDataGridViewTables(bool bIsItogo)
            : base()
        {
            _bIsItogo = bIsItogo;
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

                DefaultCellStyle.BackColor =
                //s_dgvCellStyleCommon.BackColor =
                    value.Equals (SystemColors.Control) == true
                        ? DefaultBackColor
                            : value;
            }
        }
    }
 }
