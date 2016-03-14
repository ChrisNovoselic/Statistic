using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing; //Color..
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public abstract class PanelTecViewStandard : PanelTecViewBase
    {
        public class DataGridViewStandardMins : HDataGridViewStandard
        {
            protected virtual void InitializeComponents()
            {
            }

            public DataGridViewStandardMins()
                //: base (new int [] {15, 16, 16, 16, 19, 16})
                : base(HDateTime.INTERVAL.MINUTES
                    , new ColumnProperies[] { new ColumnProperies (50, 15, @"Мин.", @"Min")
                    , new ColumnProperies (50, 16, @"Факт", @"FactMin")
                    , new ColumnProperies (50, 16, @"ПБР", @"PBRMin")
                    , new ColumnProperies (50, 16, @"ПБРэ", @"PBReMin")
                    , new ColumnProperies (50, 19, @"УДГэ", @"UDGeMin")
                    , new ColumnProperies (50, 16, @"+/-", @"DeviationMin")
            })
            {
                InitializeComponents();

                Name = "m_dgwTableMins";
                RowHeadersVisible = false;
                RowTemplate.Resizable = DataGridViewTriState.False;

                RowsAdd();
            }

            public override void Fill(TecView.valuesTEC[] values, params object[] pars)
            {
                int hour = (int)pars[0]
                    , min = (int)pars[1]; //m_tecView.lastMin;
                double sumFact = 0, sumUDGe = 0, sumDiviation = 0;

                if (!(min == 0))
                    min--;
                else
                    ;

                for (int i = 0; i < values.Length - 1; i++)
                {
                    //Ограничить отображение (для режима АИСКУЭ+СОТИАССО)
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = values[i + 1].valuesFact.ToString("F2");
                    if (i < min)
                    {
                        sumFact += values[i + 1].valuesFact;
                    }
                    else
                        ;

                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.PBR].Value = values[i].valuesPBR.ToString("F2");
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.PBRe].Value = values[i].valuesPBRe.ToString("F2");
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Value = values[i].valuesUDGe.ToString("F2");
                    sumUDGe += values[i].valuesUDGe;
                    if ((i < min) && (!(values[i].valuesUDGe == 0)))
                    {
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value =
                            ((double)(values[i + 1].valuesFact - values[i].valuesUDGe)).ToString("F2");
                        //if (Math.Abs(values.valuesFact[i + 1] - values.valuesUDGe[i]) > values.valuesDiviation[i]
                        //    && values.valuesDiviation[i] != 0)
                        //    Rows[i].Cells[5].Style = dgvCellStyleError;
                        //else
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyleCommon;

                        sumDiviation += values[i + 1].valuesFact - values[i].valuesUDGe;
                    }
                    else
                    {
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyleCommon;
                    }
                }

                int cnt = Rows.Count - 1;
                if (!(min > 0))
                {
                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = 0.ToString("F2");
                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Value = 0.ToString("F2");
                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                }
                else
                {
                    if (min > cnt)
                        min = cnt;
                    else
                        ;

                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = (sumFact / min).ToString("F2");
                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Value = values[0].valuesUDGe.ToString("F2");
                    Rows[cnt].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = (sumDiviation / min).ToString("F2");
                }

                ////Назначить крайней видимой строкой - строку с крайним полученным значением
                //setFirstDisplayedScrollingRowIndex(m_dgwMins, m_tecView.lastMin);
                //Назначить крайней видимой строкой - крайнюю строку
                if (!(DisplayedRowCount(true) == 0))
                    FirstDisplayedScrollingRowIndex = RowCount - DisplayedRowCount(true) + 1;
                else
                    FirstDisplayedScrollingRowIndex = 0;
            }

            public override void Fill(params object[] pars)
            {
                int cnt = Rows.Count - 1
                    , diskretnost = 60 / cnt
                    , i = -1, c = -1;

                for (i = 0; i < cnt; i++)
                {
                    Rows[i].Cells[0].Value = ((i + 1) * diskretnost).ToString();
                    for (c = 1; c < Columns.Count; c++)
                        Rows[i].Cells[c].Value = 0.ToString("F2");
                }

                Rows[cnt].Cells[0].Value = "Итог";
                for (c = 1; c < m_arColumns.Length; c++)
                    switch ((INDEX_COLUMNS)c)
                    {
                        case INDEX_COLUMNS.PBR:
                        case INDEX_COLUMNS.PBRe:
                            Rows[i].Cells[c].Value = @"-";
                            break;
                        default:
                            Rows[i].Cells[c].Value = 0.ToString("F2");
                            break;
                    }
            }
        }

        protected class HZedGraphControlStandardHours : HZedGraphControl
        {
            public HZedGraphControlStandardHours(object obj) : base(obj, FormMain.formGraphicsSettings.SetScale) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"АИСКУЭ";
            }
        }

        public PanelTecViewStandard(TEC tec, int indx_tec, int indx_comp)
            : base(/*TecView.TYPE_PANEL.VIEW, */tec, indx_tec, indx_comp)
        {
            m_arPercRows = new int [] { 5, 71 };
        }

        protected override void createDataGridViewMins()
        {
            this.m_dgwMins = new DataGridViewStandardMins();
        }

        protected override void createZedGraphControlHours(object objLock)
        {
            this.m_ZedGraphHours = new HZedGraphControlStandardHours(objLock);
        }

        protected override void createPanelQuickData()
        {
            this._pnlQuickData = new PanelQuickDataStandard();
        }

        protected PanelQuickDataStandard PanelQuickData { get { return _pnlQuickData as PanelQuickDataStandard; } }
    }
}
