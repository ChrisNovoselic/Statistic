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
        public override void UpdateGraphicsCurrent(int type)
        {
            base.UpdateGraphicsCurrent(type);

            lock (m_tecView.m_lockValue)
            {
                m_dgwHours.Fill(m_tecView.m_valuesHours
                , m_tecView.lastHour
                , m_tecView.lastReceivedHour
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_tec.m_id
                , m_tecView.currHour
                , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                , m_tecView.serverTime);
            }
            PanelQuickData.UpdateColorPbr();
        }

        public class DataGridViewStandardMins : HDataGridViewStandard
        {
            protected virtual void InitializeComponents()
            {
            }

            public DataGridViewStandardMins()
                //: base (new int [] {15, 16, 16, 16, 19, 16})
                : base(HDateTime.INTERVAL.MINUTES
                        , new ColumnProperies[] {
                            new ColumnProperies (50, 15, @"���.", @"Min")
                            , new ColumnProperies (50, 16, @"����", @"FactMin")
                            , new ColumnProperies (50, 16, @"���", @"PBRMin")
                            , new ColumnProperies (50, 16, @"����", @"PBReMin")
                            , new ColumnProperies (50, 19, @"����", @"UDGeMin")
                            , new ColumnProperies (50, 16, @"+/-", @"DeviationMin")
                        }
                    , true)
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
                    , min = (int)pars[1] //m_tecView.lastMin;
                    , cnt = -1;
                double sumFact = 0, sumUDGe = 0, sumDiviation = 0;

                if (!(min == 0))
                    min--;
                else
                    ;

                cnt = Rows.Count - 1;

                for (int i = 0; i < values.Length - 1; i++)
                {
                    //���������� ����������� (��� ������ ������+��������)
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = values[i + 1].valuesFact.ToString("F2");
                    if (i < min)
                    {
                        sumFact += values[i + 1].valuesFact/* / cnt*/;
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

                        sumDiviation += (values[i + 1].valuesFact - values[i].valuesUDGe)/* / cnt*/;
                    }
                    else
                    {
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                        Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyleCommon;
                    }
                }

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

                ////��������� ������� ������� ������� - ������ � ������� ���������� ���������
                //setFirstDisplayedScrollingRowIndex(m_dgwMins, m_tecView.lastMin);
                //��������� ������� ������� ������� - ������� ������
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

                Rows[cnt].Cells[0].Value = "����";
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
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"������";
            }

            public override void Draw(TecView.valuesTEC[] values, params object[] pars)
            {
                bool currHour = (bool)pars[0]; //m_tecView.currHour
                CONN_SETT_TYPE typeConnSett = (CONN_SETT_TYPE)pars[1]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]
                int lastHour = (int)pars[2]; //m_tecView.lastHour
                bool bCurDateSeason = (bool)pars[3]; //m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                IntDelegateIntFunc delegateSeasonHourOffset = new IntDelegateIntFunc((IntDelegateIntFunc)pars[4]); //m_tecView.GetSeasonHourOffset
                DateTime serverTime = (DateTime)pars[5]; //m_tecView.serverTime                
                string strTitle = (string)pars[6]; //_pnlQuickData.dtprDate.Value.ToShortDateString()

                GraphPane.CurveList.Clear();

                int itemscount = values.Length;

                string[] names = new string[itemscount];

                double[] valuesPDiviation = new double[itemscount];
                double[] valuesODiviation = new double[itemscount];
                double[] valuesUDGe = new double[itemscount];
                double[] valuesFact = new double[itemscount];

                double minimum = double.MaxValue, minimum_scale;
                double maximum = 0, maximum_scale;
                bool noValues = true;
                for (int i = 0; i < itemscount; i++)
                {
                    if (bCurDateSeason == true)
                    {
                        names[i] = (i + 1 - delegateSeasonHourOffset(i + 1)).ToString();

                        if ((i + 0) == HAdmin.SeasonDateTime.Hour)
                            names[i] += @"*";
                        else
                            ;
                    }
                    else
                        names[i] = (i + 1).ToString();

                    valuesPDiviation[i] = values[i].valuesUDGe + values[i].valuesDiviation;
                    valuesODiviation[i] = values[i].valuesUDGe - values[i].valuesDiviation;
                    valuesUDGe[i] = values[i].valuesUDGe;
                    valuesFact[i] = values[i].valuesFact;

                    if ((minimum > valuesPDiviation[i]) && (!(valuesPDiviation[i] == 0)))
                    {
                        minimum = valuesPDiviation[i];
                        noValues = false;
                    }

                    if ((minimum > valuesODiviation[i]) && (!(valuesODiviation[i] == 0)))
                    {
                        minimum = valuesODiviation[i];
                        noValues = false;
                    }

                    if ((minimum > valuesUDGe[i]) && (!(valuesUDGe[i] == 0)))
                    {
                        minimum = valuesUDGe[i];
                        noValues = false;
                    }

                    if ((minimum > valuesFact[i]) && (!(valuesFact[i] == 0)))
                    {
                        minimum = valuesFact[i];
                        noValues = false;
                    }

                    if (maximum < valuesPDiviation[i])
                        maximum = valuesPDiviation[i];
                    else
                        ;

                    if (maximum < valuesODiviation[i])
                        maximum = valuesODiviation[i];
                    else
                        ;

                    if (maximum < valuesUDGe[i])
                        maximum = valuesUDGe[i];
                    else
                        ;

                    if (maximum < valuesFact[i])
                        maximum = valuesFact[i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues)
                {
                    minimum_scale = 0;
                    maximum_scale = 10;
                }
                else
                {
                    if (minimum != maximum)
                    {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    }
                    else
                    {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;
                getColorZedGraph(typeConnSett, out colorChart, out colorPCurve);

                GraphPane.Chart.Fill = new Fill(colorChart);

                //LineItem
                GraphPane.AddCurve("����", null, valuesUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
                //LineItem
                GraphPane.AddCurve("", null, valuesODiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
                //LineItem
                GraphPane.AddCurve("��������� ����������", null, valuesPDiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));

                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                {
                    if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        //BarItem
                        GraphPane.AddBar("��������", null, valuesFact, colorPCurve);
                    else
                    {
                        // ����� � 'PanelTecVzletTDirect::ZedGraphControlVzletTDirect::Draw ()'
                        int lh = -1;
                        if (currHour == true)
                            lh = lastHour;
                        else
                            if (HDateTime.ToMoscowTimeZone(DateTime.Now).Date.Equals(serverTime.Date) == true)
                                lh = serverTime.Hour;
                            else
                                lh = 24;

                        double[] valuesASKUE = new double[lh]
                            , valuesSOTIASSO = new double[lh + 1];
                        for (int i = 0; i < lh + 1; i++)
                        {
                            if (i < lh - 0)
                            {
                                valuesASKUE[i] = valuesFact[i];
                                valuesSOTIASSO[i] = 0;
                            }
                            else
                            {
                                if (i < valuesFact.Length)
                                    valuesSOTIASSO[i] = valuesFact[i];
                                else
                                    ;
                            }
                        }

                        GraphPane.AddBar("��������(������)", null, valuesASKUE, colorPCurve);
                        GraphPane.AddBar("��������(��������)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                    }
                }
                else
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                    {
                        if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        {
                            double[] valuesFactLinear = new double[lastHour];
                            for (int i = 0; i < lastHour; i++)
                                valuesFactLinear[i] = valuesFact[i];

                            //LineItem
                            GraphPane.AddCurve("��������", null, valuesFactLinear, colorPCurve);
                        }
                        else
                        {
                            PointPairList valuesASKUE = new PointPairList()
                                , valuesSOTIASSO = new PointPairList();

                            for (int i = 0; i < lastHour + 1; i++)
                            {
                                if (i < lastHour - 0)
                                {
                                    valuesASKUE.Add((double)(i + 1), valuesFact[i]);
                                }
                                else
                                {
                                    valuesSOTIASSO.Add((double)(i + 1), valuesFact[i]);
                                }
                            }

                            //LineItem
                            GraphPane.AddCurve("��������(������)", valuesASKUE, colorPCurve);
                            //LineItem
                            GraphPane.AddCurve("��������(��������)", valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                        }
                    }

                //��� ���������� � ����� ������� ������ ��������
                GraphPane.BarSettings.Type = BarType.Overlay;

                //...�� minutes
                GraphPane.XAxis.Scale.Min = 0.5;
                GraphPane.XAxis.Scale.Max = GraphPane.XAxis.Scale.Min + itemscount;
                GraphPane.XAxis.Scale.MinorStep = 1;
                GraphPane.XAxis.Scale.MajorStep = 1; //itemscount / 20;

                GraphPane.XAxis.Type = AxisType.Linear; //...�� minutes
                //GraphPane.XAxis.Type = AxisType.Text;
                GraphPane.XAxis.Title.Text = "";
                GraphPane.YAxis.Title.Text = "";
                //�� ������� ���-���������� �������� - �������� ������  05.12.2014
                //GraphPane.Title.Text = @"(" + m_ZedGraphHours.SourceDataText + @")";
                GraphPane.Title.Text = SourceDataText;
                GraphPane.Title.Text += new string(' ', 29);
                GraphPane.Title.Text +=
                    //"�������� " +
                    ////�� ������� ������������� ������ - �������� ������
                    ////@"(" + m_ZedGraphHours.SourceDataText  + @") " +
                    //@"�� " +
                    strTitle;

                GraphPane.XAxis.Scale.TextLabels = names;
                GraphPane.XAxis.Scale.IsPreventLabelOverlap = false;

                // �������� ����������� ����� �������� ������� ����� �� ��� X
                GraphPane.XAxis.MajorGrid.IsVisible = true;
                // ������ ��� ���������� ����� ��� ������� ����� �� ��� X:
                // ����� ������� ����� 10 ��������, ... 
                GraphPane.XAxis.MajorGrid.DashOn = 10;
                // ����� 5 �������� - �������
                GraphPane.XAxis.MajorGrid.DashOff = 5;
                // ������� �����
                GraphPane.XAxis.MajorGrid.PenWidth = 0.1F;
                GraphPane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // �������� ����������� ����� �������� ������� ����� �� ��� Y
                GraphPane.YAxis.MajorGrid.IsVisible = true;
                // ���������� ������ ��� ���������� ����� ��� ������� ����� �� ��� Y
                GraphPane.YAxis.MajorGrid.DashOn = 10;
                GraphPane.YAxis.MajorGrid.DashOff = 5;
                // ������� �����
                GraphPane.YAxis.MajorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // �������� ����������� ����� �������� ������ ����� �� ��� Y
                GraphPane.YAxis.MinorGrid.IsVisible = true;
                // ����� ������� ����� ������ �������, ... 
                GraphPane.YAxis.MinorGrid.DashOn = 1;
                GraphPane.YAxis.MinorGrid.DashOff = 2;
                // ������� �����
                GraphPane.YAxis.MinorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // ������������� ������������ ��� �������� �� ��� Y
                GraphPane.YAxis.Scale.Min = minimum_scale;
                GraphPane.YAxis.Scale.Max = maximum_scale;

                AxisChange();

                Invalidate();
            }
        }

        protected class HZedGraphControlStandardMins : HZedGraphControl
        {
            public HZedGraphControlStandardMins(object obj) : base(obj, FormMain.formGraphicsSettings.SetScale) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"������";

                this.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            }

            public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
            {
                int diskretnost = -1;
                if (pane.CurveList.Count > 0)
                    diskretnost = 60 / pane.CurveList[0].Points.Count;
                else
                    diskretnost = 60 / 20;

                return ((val) * diskretnost).ToString();
            }

            public override void Draw(TecView.valuesTEC[] values, params object[] pars)
            {
                bool currHour = (bool)pars[0]; //m_tecView.currHour
                CONN_SETT_TYPE typeConnSett = (CONN_SETT_TYPE)pars[1]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES]
                int lastMin = (int)pars[2]; //m_tecView.lastMin
                bool bCurDateSeason = (bool)pars[3]; //m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                IntDelegateIntFunc delegateSeasonHourOffset = new IntDelegateIntFunc((IntDelegateIntFunc)pars[4]); //m_tecView.GetSeasonHourOffset
                int hour = (int)pars[5]; //hour
                bool adminValuesReceived = (bool)pars[6]; //(m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true)
                double recomendation = (double)pars[7]; //m_tecView.recomendation                

                GraphPane.CurveList.Clear();

                int itemscount = values.Length - 1
                    , diskretnost = 60 / itemscount;

                string[] names = new string[itemscount];

                double[] valuesRecommend = new double[itemscount];

                double[] valuesUDGe = new double[itemscount];

                double[] valuesFact = new double[itemscount];

                for (int i = 0; i < itemscount; i++)
                {
                    valuesFact[i] = values[i + 1].valuesFact;
                    valuesUDGe[i] = values[i + 1].valuesUDGe;
                }

                //double[] valuesPDiviation = new double[itemscount];

                //double[] valuesODiviation = new double[itemscount];

                double minimum = double.MaxValue, minimum_scale;
                double maximum = 0, maximum_scale;
                bool noValues = true;
                int iName = -1;

                for (int i = 0; i < itemscount; i++)
                {
                    iName = ((i + 1) * diskretnost);
                    if (iName % 3 == 0)
                        names[i] = iName.ToString();
                    else
                        names[i] = string.Empty;
                    //valuesPDiviation[i] = m_valuesMins.valuesUDGe[i] + m_valuesMins.valuesDiviation[i];
                    //valuesODiviation[i] = m_valuesMins.valuesUDGe[i] - m_valuesMins.valuesDiviation[i];

                    if (currHour == true)
                        valuesRecommend[i] = ((i < (lastMin - 1)) || (!(adminValuesReceived == true))) ? 0 : recomendation;
                    else
                        ;

                    //if (minimum > valuesPDiviation[i] && valuesPDiviation[i] != 0)
                    //{
                    //    minimum = valuesPDiviation[i];
                    //    noValues = false;
                    //}
                    //else
                    //    ;

                    //if (minimum > valuesODiviation[i] && valuesODiviation[i] != 0)
                    //{
                    //    minimum = valuesODiviation[i];
                    //    noValues = false;
                    //}
                    //else
                    //    ;

                    if (currHour == true)
                    {
                        if (minimum > valuesRecommend[i] && valuesRecommend[i] != 0)
                        {
                            minimum = valuesRecommend[i];
                            noValues = false;
                        }
                    }
                    else
                        ;

                    if (minimum > valuesUDGe[i] && valuesUDGe[i] != 0)
                    {
                        minimum = valuesUDGe[i];
                        noValues = false;
                    }
                    else
                        ;

                    if (minimum > valuesFact[i] && valuesFact[i] != 0)
                    {
                        minimum = valuesFact[i];
                        noValues = false;
                    }
                    else
                        ;

                    //if (maximum < valuesPDiviation[i])
                    //    maximum = valuesPDiviation[i];
                    //else
                    //    ;

                    //if (maximum < valuesODiviation[i])
                    //    maximum = valuesODiviation[i];
                    //else
                    //    ;

                    if (currHour == true)
                    {
                        if (maximum < valuesRecommend[i])
                            maximum = valuesRecommend[i];
                        else
                            ;
                    }
                    else
                        ;

                    if (maximum < valuesUDGe[i])
                        maximum = valuesUDGe[i];
                    else
                        ;

                    if (maximum < valuesFact[i])
                        maximum = valuesFact[i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues)
                {
                    minimum_scale = 0;
                    maximum_scale = 10;
                }
                else
                {
                    if (minimum != maximum)
                    {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        else
                            ;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    }
                    else
                    {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;
                getColorZedGraph(typeConnSett, out colorChart, out colorPCurve);
                GraphPane.Chart.Fill = new Fill(colorChart);

                LineItem curve2 = GraphPane.AddCurve("����", null, valuesUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
                //LineItem curve4 = GraphPane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
                //LineItem curve3 = GraphPane.AddCurve("��������� ����������", null, valuesPDiviation, graphSettings.divColor);

                switch (FormMain.formGraphicsSettings.m_graphTypes)
                {
                    case FormGraphicsSettings.GraphTypes.Bar:
                        if ((!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                            || (currHour == false))
                        {
                            //BarItem
                            GraphPane.AddBar("��������", null, valuesFact, colorPCurve);
                            //BarItem
                            GraphPane.AddBar("������������� ��������", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                        }
                        else
                        {
                            bool order = false; //������� "������������" ��������...
                            double[] valuesSOTIASSO = null;
                            switch (lastMin)
                            {
                                case 0:
                                    valuesSOTIASSO = new double[valuesFact.Length];
                                    valuesSOTIASSO[lastMin] = valuesFact[lastMin];
                                    valuesFact[lastMin] = 0F;
                                    //������� "������������" ��������
                                    if (valuesRecommend[lastMin] > valuesSOTIASSO[lastMin])
                                        order = true;
                                    else
                                        ;
                                    break;
                                case 21:
                                    //valuesFact - ��������,
                                    //valuesRecommend = 0
                                    break;
                                default:
                                    try
                                    {
                                        valuesSOTIASSO = new double[valuesFact.Length];
                                        valuesSOTIASSO[lastMin - 1] = valuesFact[lastMin - 1];
                                        valuesFact[lastMin - 1] = 0F;
                                        //������� "������������" ��������
                                        if (valuesRecommend[lastMin - 1] > valuesSOTIASSO[lastMin - 1])
                                            order = true;
                                        else
                                            ;
                                    }
                                    catch (Exception e)
                                    {
                                        Logging.Logg().Exception(e, @"PanelTecViewBase::DrawGraphMins (hour=" + hour + @") - ... lastMin(>0)=" + lastMin, Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                    break;
                            }

                            //BarItem
                            GraphPane.AddBar("��������(������)", null, valuesFact, colorPCurve);
                            if (order == true)
                            {
                                //BarItem
                                GraphPane.AddBar("��������(��������)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                                //BarItem
                                GraphPane.AddBar("������������� ��������", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                            }
                            else
                            {
                                //BarItem
                                GraphPane.AddBar("������������� ��������", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                                //BarItem                        
                                GraphPane.AddBar("��������(��������)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                            }
                        }
                        break;
                    case FormGraphicsSettings.GraphTypes.Linear:
                        PointPairList listValuesSOTIASSO = null
                            , listValuesAISKUE = null
                            , listValuesRec = null;
                        if ((!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                            || (currHour == false))
                        {
                            switch (lastMin)
                            {
                                case 0:
                                    //LineItem
                                    listValuesRec = new PointPairList();
                                    if ((adminValuesReceived == true) && (currHour == true))
                                        for (int i = 0; i < itemscount; i++)
                                            listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                    else
                                        ;
                                    break;
                                default:
                                    listValuesAISKUE = new PointPairList();
                                    for (int i = 0; i < lastMin - 1; i++)
                                        listValuesAISKUE.Add((double)(i + 1), valuesFact[i]);

                                    listValuesRec = new PointPairList();
                                    if ((adminValuesReceived == true) && (currHour == true))
                                        for (int i = lastMin - 1; i < itemscount; i++)
                                            listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                    else
                                        ;
                                    break;
                            }

                            //LineItem
                            GraphPane.AddCurve("��������", listValuesAISKUE, colorPCurve);
                            //LineItem
                            GraphPane.AddCurve("������������� ��������", listValuesRec, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                        }
                        else
                        {
                            switch (lastMin)
                            {
                                case 0:
                                    if (valuesFact[lastMin] > 0)
                                    {
                                        listValuesSOTIASSO = new PointPairList();
                                        listValuesSOTIASSO.Add(1F, valuesFact[lastMin]);
                                        if ((adminValuesReceived == true) && (currHour == true))
                                            for (int i = 1; i < itemscount; i++)
                                                listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                        else
                                            ;
                                    }
                                    else
                                        ;
                                    break;
                                default:
                                    listValuesAISKUE = new PointPairList();
                                    for (int i = 0; i < lastMin - 1; i++)
                                        listValuesAISKUE.Add((double)(i + 1), valuesFact[i]);
                                    if (valuesFact[lastMin - 1] > 0)
                                    {
                                        listValuesSOTIASSO = new PointPairList();
                                        listValuesSOTIASSO.Add((double)lastMin - 0, valuesFact[lastMin - 1]);

                                        if ((adminValuesReceived == true) && (currHour == true))
                                        {
                                            listValuesRec = new PointPairList();
                                            for (int i = lastMin - 0; i < itemscount; i++)
                                                listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                        }
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        if ((adminValuesReceived == true) && (currHour == true))
                                        {
                                            listValuesRec = new PointPairList();
                                            for (int i = lastMin - 1; i < itemscount; i++)
                                                listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                        }
                                        else
                                            ;
                                    }
                                    break;
                            }

                            GraphPane.AddCurve("��������(������)", listValuesAISKUE, colorPCurve);
                            GraphPane.AddCurve("��������(��������)", listValuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                            GraphPane.AddCurve("������������� ��������", listValuesRec, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                        }
                        break;
                    default:
                        break;
                }

                GraphPane.BarSettings.Type = BarType.Overlay;

                GraphPane.XAxis.Type = AxisType.Linear;

                GraphPane.XAxis.Title.Text = "";
                GraphPane.YAxis.Title.Text = "";

                //�� ������� ���-���������� �������� - �������� ������ 05.12.2014
                //GraphPane.Title.Text = @" (" + m_ZedGraphMins.SourceDataText + @")";
                GraphPane.Title.Text = SourceDataText;
                // ���.����������� (�� ������ ����)
                GraphPane.Title.Text += new string(' ', 29);
                if (bCurDateSeason == true)
                {
                    int offset = delegateSeasonHourOffset(hour + 1);
                    GraphPane.Title.Text += //"������� �������� �� " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                        (hour + 1 - offset).ToString();
                    if (HAdmin.SeasonDateTime.Hour == hour)
                        GraphPane.Title.Text += "*";
                    else
                        ;

                    GraphPane.Title.Text += @" ���";
                }
                else
                    GraphPane.Title.Text += //"������� �������� �� " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                        (hour + 1).ToString() + " ���";

                //�� ������� ������������� ������ - �������� ������
                //GraphPane.Title.Text += @" (" + m_ZedGraphMins.SourceDataText + @")";

                GraphPane.XAxis.Scale.Min = 0.5;
                GraphPane.XAxis.Scale.Max = GraphPane.XAxis.Scale.Min + itemscount;
                GraphPane.XAxis.Scale.MinorStep = 1;
                GraphPane.XAxis.Scale.MajorStep = itemscount / 20;

                GraphPane.XAxis.Scale.TextLabels = names;
                GraphPane.XAxis.Scale.IsPreventLabelOverlap = false;

                // �������� ����������� ����� �������� ������� ����� �� ��� X
                GraphPane.XAxis.MajorGrid.IsVisible = true;
                // ������ ��� ���������� ����� ��� ������� ����� �� ��� X:
                // ����� ������� ����� 10 ��������, ... 
                GraphPane.XAxis.MajorGrid.DashOn = 10;
                // ����� 5 �������� - �������
                GraphPane.XAxis.MajorGrid.DashOff = 5;
                // ������� �����
                GraphPane.XAxis.MajorGrid.PenWidth = 0.1F;
                GraphPane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // �������� ����������� ����� �������� ������� ����� �� ��� Y
                GraphPane.YAxis.MajorGrid.IsVisible = true;
                // ���������� ������ ��� ���������� ����� ��� ������� ����� �� ��� Y
                GraphPane.YAxis.MajorGrid.DashOn = 10;
                GraphPane.YAxis.MajorGrid.DashOff = 5;
                // ������� �����
                GraphPane.YAxis.MajorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // �������� ����������� ����� �������� ������ ����� �� ��� Y
                GraphPane.YAxis.MinorGrid.IsVisible = true;
                // ����� ������� ����� ������ �������, ... 
                GraphPane.YAxis.MinorGrid.DashOn = 1;
                GraphPane.YAxis.MinorGrid.DashOff = 2;
                // ������� �����
                GraphPane.YAxis.MinorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // ������������� ������������ ��� �������� �� ��� Y
                GraphPane.YAxis.Scale.Min = minimum_scale;
                GraphPane.YAxis.Scale.Max = maximum_scale;

                AxisChange();

                Invalidate();
            }
        }

        public PanelTecViewStandard(TEC tec, int indx_tec, int indx_comp, HMark markQueries)
            : base(/*TecView.TYPE_PANEL.VIEW, */tec, indx_tec, indx_comp, markQueries)
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

        protected override void createZedGraphControlMins(object objLock)
        {
            this.m_ZedGraphMins = new HZedGraphControlStandardMins(m_tecView.m_lockValue);
        }

        protected override void createPanelQuickData()
        {
            this._pnlQuickData = new PanelQuickDataStandard();
        }

        protected PanelQuickDataStandard PanelQuickData { get { return _pnlQuickData as PanelQuickDataStandard; } }

        //protected override void DrawGraphHours()
        //{
        //    m_ZedGraphHours.Draw(m_tecView.m_valuesHours
        //        , new object[] {
        //            m_tecView.currHour
        //            , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]
        //            , m_tecView.lastHour
        //            , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
        //            , (IntDelegateIntFunc)m_tecView.GetSeasonHourOffset                    
        //            , m_tecView.serverTime                    
        //            , _pnlQuickData.dtprDate.Value.ToShortDateString()
        //        }
        //    );
        //}

        protected override HMark enabledSourceData_ToolStripMenuItems()
        {
            //bool [] arRes = new bool [] {false, false};
            HMark markRes = new HMark (0);

            if (FormMain.formGraphicsSettings.m_connSettType_SourceData == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) {
                //������ ���� �������� ��� ������
                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO]).Enabled =
                    HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO);

                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN]).Enabled =
                    HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_SOTIASSO_3_MIN);

                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE]).Enabled =                 
                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE]).Enabled =                
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN]).Enabled =
                    true;

                //�������� "��� ����", �� �������� �������� ������ ��� ��������� ��������������
            } else {
                //������ ���� ���������� ��� ������
                //������������� ���������� �������� ������
                if (! (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == FormMain.formGraphicsSettings.m_connSettType_SourceData))
                {
                    m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                    //arRes [(int)HDateTime.INTERVAL.MINUTES] = true;
                    markRes.Marked ((int)HDateTime.INTERVAL.MINUTES);
                } else {
                }

                if (!(m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == FormMain.formGraphicsSettings.m_connSettType_SourceData))
                {
                    m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                    //arRes[(int)HDateTime.INTERVAL.HOURS] = true;
                    markRes.Marked((int)HDateTime.INTERVAL.HOURS);
                }
                else
                {
                }

                //if (arRes[(int)HDateTime.INTERVAL.MINUTES] == true) {
                if (markRes.IsMarked ((int)HDateTime.INTERVAL.MINUTES) == true)
                {
                    initTableMinRows ();

                    enabledSourceData_ToolStripMenuItems (m_ZedGraphMins.ContextMenuStrip, m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES]);
                }
                else ;

                //if (arRes[(int)HDateTime.INTERVAL.HOURS] == true)
                if (markRes.IsMarked ((int)HDateTime.INTERVAL.HOURS) == true) {
                    enabledSourceData_ToolStripMenuItems(m_ZedGraphHours.ContextMenuStrip, m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]);
                }
                else ;
            }

            //return arRes[(int)HDateTime.INTERVAL.MINUTES] || arRes[(int)HDateTime.INTERVAL.HOURS];
            //return arRes;
            return markRes;
        }

        private void enabledSourceData_ToolStripMenuItems(ContextMenuStrip menu, CONN_SETT_TYPE type)
        {
            int indx = -1;

            //�������� ���������� ������ ������������ ����
            // ��� ����������� �������� �������� ������
            // (������������ ��� ������� �������������)
            for (indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                menu.Items[indx].Enabled =
                    true;

            switch (type)
            {
                case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                    break;
                case CONN_SETT_TYPE.DATA_AISKUE:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                    break;
                default:
                    break;
            }

            //�������� �������� ������
            ((ToolStripMenuItem)menu.Items[indx]).PerformClick();

            //��������������� "�������������" ������� ������������ ����
            for (indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                menu.Items[indx].Enabled =
                    false;
        }
    }
}
