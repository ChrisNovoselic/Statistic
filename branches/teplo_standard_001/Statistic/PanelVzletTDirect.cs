using System;
using System.Collections;
using System.ComponentModel;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms; //TableLayoutPanel

using ZedGraph;

using HClassLibrary;
using StatisticCommon;
using System.Collections.Generic;

namespace Statistic
{
    partial class PanelVzletTDirect
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelVzletTDirect : PanelContainerStatistic
    {
        /// <summary>
        /// Конструктор-основной
        /// </summary>
        /// <param name="listTec">Лист ТЭЦ</param>
        public PanelVzletTDirect(List<TEC> listTec)
            : base(typeof(PanelTecVzletTDirect))
        {
            InitializeComponent();

            PanelTecVzletTDirect ptvtd;

            int i = -1;

            initializeLayoutStyle(listTec.Count / 2
                , listTec.Count);
            // фильтр ТЭЦ-ЛК
            for (i = 0; i < listTec.Count; i++)
                if (!(listTec[i].m_id > (int)TECComponent.ID.LK))
                {
                    ptvtd = new PanelTecVzletTDirect(listTec[i], i, -1);
                    this.Controls.Add(ptvtd, i % this.ColumnCount, i / this.ColumnCount);
                }
                else
                    ;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="container">Объект владелец для вкладки</param>
        /// <param name="listTec">Список ТЭЦ</param>
        public PanelVzletTDirect(IContainer container, List<TEC> listTec)
            : this(listTec)
        {
            container.Add(this);
        }

        public void UpdateGraphicsCurrent(int type)
        {
            foreach (Control ptvtd in this.Controls)
                if ((ptvtd is PanelTecVzletTDirect) == true) (ptvtd as PanelTecVzletTDirect).UpdateGraphicsCurrent(type); else ;
        }

        public partial class PanelTecVzletTDirect : PanelTecViewBase
        {
            private class DataGridViewVzletTDirectHours : HDataGridViewBase
            {
                private enum INDEX_COLUMNS : short
                {
                    PART_TIME, TEMPERATURE_FACT, TEMPERATURE_PBR, REC, UDGt, TEMPERATURE_DEVIATION
                        , COUNT_COLUMN
                }

                public DataGridViewVzletTDirectHours()
                    : base(HDateTime.INTERVAL.HOURS, new HDataGridViewBase.ColumnProperies[]
                    {//??? в сумме ширина = 310, проценты = 98, 
                        new HDataGridViewBase.ColumnProperies (27, 8, @"Час", @"Hour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"Факт", @"FactHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"План", @"PBRHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"Рек.(%)", @"RecHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"УДГт", @"UDGtHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"+/-", @"DeviationHour")
                    })
                {
                    Name = "dgvTableTDirectHours";
                    RowHeadersVisible = false;
                    RowTemplate.Resizable = DataGridViewTriState.False;

                    RowsAdd();
                }

                public override void Fill(params object[] pars)
                {
                    int count = (int)pars[1]
                    , hour = -1
                    , offset = -1
                    , i = -1, c = -1;
                    DateTime dtCurrent = (DateTime)pars[0];
                    bool bSeasonDate = (bool)pars[2];

                    Rows.Clear();

                    Rows.Add(count + 1);

                    for (i = 0; i < count; i++)
                    {
                        hour = i + 1;
                        if (bSeasonDate == true)
                        {
                            offset = HAdmin.GetSeasonHourOffset(dtCurrent, hour);

                            Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Value = (hour - offset).ToString();
                            if ((hour - 1) == HAdmin.SeasonDateTime.Hour)
                                Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Value += @"*";
                            else
                                ;
                        }
                        else
                            Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Value = (hour).ToString();

                        for (c = 1; c < m_arColumns.Length; c++)
                            Rows[i].Cells[c].Value = 0.ToString("F1");
                    }

                    Rows[count].Cells[0].Value = "Средн.";
                    for (c = 1; c < m_arColumns.Length; c++)
                        switch ((INDEX_COLUMNS)c)
                        {
                            case INDEX_COLUMNS.TEMPERATURE_FACT:
                            case INDEX_COLUMNS.TEMPERATURE_PBR:
                            case INDEX_COLUMNS.REC:
                            case INDEX_COLUMNS.UDGt:
                            case INDEX_COLUMNS.TEMPERATURE_DEVIATION:
                                Rows[i].Cells[c].Value = @"-".ToString();
                                break;
                            default:
                                Rows[i].Cells[c].Value = 0.ToString("F1");
                                break;
                        }
                }

                public override void Fill(TecView.valuesTEC[] values, params object[] pars)
                {
                    //double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
                    int lastHour = (int)pars[0]; //m_tecView.lastHour;
                    int lastReceivedHour = (int)pars[1]; //m_tecView.lastReceivedHour;
                    int itemscount = (int)pars[2]; //m_tecView.m_valuesHours.Length;
                    bool bPmin = (int)pars[3] == 5
                        , bCurrHour = (bool)pars[4] //m_tecView.currHour
                        , bIsTypeConnSettAISKUEHour = (bool)pars[5]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                    DateTime serverTime = (DateTime)pars[6]; //m_tecView.serverTime.Date.Equals(HDateTime.ToMoscowTimeZone(DateTime.Now.Date))

                    int i = -1
                        //, warn = -1, cntWarn = -1
                        , lh = bCurrHour == true ? lastReceivedHour - 1 :
                            serverTime.Date.Equals(DateTime.Now.Date) == true ? lastReceivedHour - 1 :
                                itemscount;
                    double udg_t = -1F;
                    string strWarn = string.Empty;

                    Debug.WriteLine(@"DataGridViewVzletTDirectHours::Fill () - serverTime=" + serverTime.ToString()
                        + @"; lastHour=" + lastHour
                        + @"; lastReceivedHour=" + lastReceivedHour);

                    DataGridViewCellStyle curCellStyle;
                    DataGridViewCellStyle normalHourCellStyle = new DataGridViewCellStyle()
                        , errorHourCellStyle = new DataGridViewCellStyle();

                    normalHourCellStyle.BackColor = Color.White;
                    errorHourCellStyle.BackColor = Color.Red;
                    //// полужирный на основе 1-ой ячейки                
                    //mainHourCellStyle.Font = new System.Drawing.Font(RowsDefaultCellStyle.Font, FontStyle.Bold);

                    //cntWarn = 0;
                    udg_t = 0;
                    for (i = 0; i < itemscount; i++)
                    {
                        // номер часа
                        curCellStyle = normalHourCellStyle;
                        //Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Style = curCellStyle; // стиль определен для всей строки
                        Rows[i].DefaultCellStyle = curCellStyle;
                        // факт
                        if (!(i > lh))
                        {
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_FACT].Value = (values[i].valuesFact).ToString(@"F2"); // температура
                        }
                        else ;
                        // план
                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_PBR].Value = (values[i].valuesPmin).ToString(@"F1"); // температура
                        //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_PBR].Style = curCellStyle; // стиль определен для всей строки                        
                        // рекомендация
                        Rows[i].Cells[(int)INDEX_COLUMNS.REC].Value = (values[i].valuesREC).ToString(@"F0");
                        // уточненный дисп./график
                        Rows[i].Cells[(int)INDEX_COLUMNS.UDGt].Value = (values[i].valuesUDGe).ToString(@"F1");
                        udg_t += values[i].valuesUDGe;
                        // разность
                        if (!(i > lh))
                        {
                            // - температура
                            if ((values[i].valuesFact > 0)
                                && (values[i].valuesUDGe > 0))
                                Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = (values[i].valuesUDGe - values[i].valuesFact).ToString(@"F2");
                            else
                                Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = @"-";                            
                        }
                        else
                        {
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value =
                                @"-";
                        }
                        //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Style = curCellStyle; // стиль определен для всей строки
                        //Rows[i].Cells[(int)INDEX_COLUMNS.POWER_DEVIATION].Style = curCellStyle; // стиль определен для всей строки
                    }

                    udg_t /= itemscount;
                    udg_t = Math.Round(udg_t, 1);
                    // Уточненный дисп./график (средний)
                    Rows[i].Cells[(int)INDEX_COLUMNS.UDGt].Value = udg_t.ToString(@"F1");
                    //// план для панели оперативной информации
                    //EventPBRDateValues(new PBRDateValuesEventArgs() { m_temperatureDate = t_pbr, m_powerDate = p_pbr });
                }
            }

            private class ZedGraphControlVzletTDirect : HZedGraphControl
            {
                public ZedGraphControlVzletTDirect(object lockVal)
                    : base(lockVal, FormMain.formGraphicsSettings.SetScale)
                {
                }

                public override void Draw(TecView.valuesTEC[] values, params object[] pars)
                {
                    bool currHour = (bool)pars[0]; //m_tecView.currHour
                    CONN_SETT_TYPE typeConnSett = (CONN_SETT_TYPE)pars[1]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]
                    int lastHour = (int)pars[2]; //m_tecView.lastHour
                    bool bCurDateSeason = (bool)pars[3]; //m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                    IntDelegateIntFunc delegateSeasonHourOffset = new IntDelegateIntFunc((IntDelegateIntFunc)pars[4]); //m_tecView.GetSeasonHourOffset
                    DateTime serverTime = ((DateTime)pars[5]); //m_tecView.serverTime                
                    string strTitle = (string)pars[6]; //_pnlQuickData.dtprDate.Value.ToShortDateString()

                    GraphPane.CurveList.Clear();

                    int itemscount = values.Length
                        , i = -1, h = -1
                        ;
                    double y = -1F;

                    string[] names = new string[itemscount];

                    PointPairList valuesFact = null
                        ;
                    PointPairList valuesPlan = null
                        , valuesPDiviation = null 
                        , valuesODiviation = null
                        ;

                    double minimum = double.MaxValue, minimum_scale;
                    double maximum = 0, maximum_scale;
                    bool noValues = true;

                    // выделить память для
                    // регулярных часов - безусловно
                    valuesFact = new PointPairList();

                    valuesPlan = new PointPairList();
                    valuesPDiviation = new PointPairList();
                    valuesODiviation = new PointPairList();

                    for (i = 0, h = 1; i < itemscount; i++, h++)
                    {
                        if (bCurDateSeason == true)
                        {
                            names[i] = (h - delegateSeasonHourOffset(h)).ToString();

                            if ((i + 0) == HAdmin.SeasonDateTime.Hour)
                                names[i] += @"*";
                            else
                                ;
                        }
                        else
                            names[i] = h.ToString();

                        if (values[i].valuesPmin > 0)
                        {
                            y = values[i].valuesUDGe;

                            valuesPlan.Add(h, y);
                            valuesPDiviation.Add(h, y + values[i].valuesDiviation);
                            valuesODiviation.Add(h, y - values[i].valuesDiviation);
                        }
                        else
                            //??? ошибка
                            ;

                        y = values[i].valuesFact;
                        valuesFact.Add(h, y);

                        if (values[i].valuesPmin > 0)
                        {
                            if (minimum > valuesPDiviation[i].Y)
                            {
                                minimum = valuesPDiviation[i].Y;
                                noValues = false;
                            }
                            else
                                ;

                            if (minimum > valuesODiviation[i].Y)
                            {
                                minimum = valuesODiviation[i].Y;
                                noValues = false;
                            }
                            else
                                ;

                            if (minimum > valuesPlan[i].Y)
                            {
                                minimum = valuesPlan[i].Y;
                                noValues = false;
                            }
                            else
                                ;                            
                        }
                        else
                            ;

                        if ((minimum > y) && (!(y == 0)))
                        {
                            minimum = y;
                            noValues = false;
                        }
                        else
                            ;

                        if (values[i].valuesPmin > 0)
                        {
                            if (maximum < valuesPDiviation[i].Y)
                                maximum = valuesPDiviation[i].Y;
                            else
                                ;

                            if (maximum < valuesODiviation[i].Y)
                                maximum = valuesODiviation[i].Y;
                            else
                                ;

                            if (maximum < valuesPlan[i].Y)
                                maximum = valuesPlan[i].Y;
                            else
                                ;
                        }
                        else
                            ;

                        if (maximum < y)
                            maximum = y;
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

                    //LineItem - план/отклонения
                    string strCurveNamePlan = "Тпр план"
                        , strCurveNameDeviation = "Возможное отклонение";
                    GraphPane.AddCurve(strCurveNamePlan, /*null,*/ valuesPlan, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
                    //LineItem
                    GraphPane.AddCurve(string.Empty, /*null,*/ valuesODiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
                    //LineItem
                    GraphPane.AddCurve(strCurveNameDeviation, /*null,*/ valuesPDiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));                    

                    //Значения
                    string strCurveNameValue = "Температура";
                    Color clrData = Color.Purple;
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                    {
                        if (typeConnSett == CONN_SETT_TYPE.DATA_VZLET)
                        //BarItem
                            GraphPane.AddBar(strCurveNameValue, valuesFact, clrData);
                        else
                            // других типов данных для ЛК не предусмотрено
                            ;
                    }
                    else
                        if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                        {
                            if (typeConnSett == CONN_SETT_TYPE.DATA_VZLET)
                            //LineItem
                                GraphPane.AddCurve(strCurveNameValue, valuesFact, clrData);                                
                            else
                                // других типов данных для ЛК не предусмотрено
                                ;
                        }

                    //Для размещения в одной позиции ОДНого значения
                    GraphPane.BarSettings.Type = BarType.Overlay;

                    //...из minutes
                    GraphPane.XAxis.Scale.Min = 0.5;
                    GraphPane.XAxis.Scale.Max = GraphPane.XAxis.Scale.Min + itemscount;
                    GraphPane.XAxis.Scale.MinorStep = 1;
                    GraphPane.XAxis.Scale.MajorStep = 1; //itemscount / 20;

                    GraphPane.XAxis.Type = AxisType.Linear; //...из minutes
                    //GraphPane.XAxis.Type = AxisType.Text;
                    GraphPane.XAxis.Title.Text = "";
                    GraphPane.YAxis.Title.Text = "";
                    //По просьбе НСС-машинистов ДОБАВИТЬ - источник данных  05.12.2014
                    //GraphPane.Title.Text = @"(" + m_ZedGraphHours.SourceDataText + @")";
                    GraphPane.Title.Text = SourceDataText;
                    GraphPane.Title.Text += new string(' ', 29);
                    GraphPane.Title.Text +=
                        //"Мощность " +
                        ////По просьбе пользователей УБРАТЬ - источник данных
                        ////@"(" + m_ZedGraphHours.SourceDataText  + @") " +
                        //@"на " +
                        strTitle;
                    // доп.нинформация (по номеру часу) - копия из 'HZedGraphControlStandardMins::Draw'
                    GraphPane.Title.Text += new string(' ', 29);
                    if (bCurDateSeason == true)
                    {
                        int offset = delegateSeasonHourOffset(lastHour + 1);
                        GraphPane.Title.Text += //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                            (lastHour + 1 - offset).ToString();
                        if (HAdmin.SeasonDateTime.Hour == lastHour)
                            GraphPane.Title.Text += "*";
                        else
                            ;

                        GraphPane.Title.Text += @" час";
                    }
                    else
                        GraphPane.Title.Text += //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                            (lastHour + 1).ToString() + " час";

                    GraphPane.XAxis.Scale.TextLabels = names;
                    GraphPane.XAxis.Scale.IsPreventLabelOverlap = false;

                    // Включаем отображение сетки напротив крупных рисок по оси X
                    GraphPane.XAxis.MajorGrid.IsVisible = true;
                    // Задаем вид пунктирной линии для крупных рисок по оси X:
                    // Длина штрихов равна 10 пикселям, ... 
                    GraphPane.XAxis.MajorGrid.DashOn = 10;
                    // затем 5 пикселей - пропуск
                    GraphPane.XAxis.MajorGrid.DashOff = 5;
                    // толщина линий
                    GraphPane.XAxis.MajorGrid.PenWidth = 0.1F;
                    GraphPane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                    // Включаем отображение сетки напротив крупных рисок по оси Y
                    GraphPane.YAxis.MajorGrid.IsVisible = true;
                    // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                    GraphPane.YAxis.MajorGrid.DashOn = 10;
                    GraphPane.YAxis.MajorGrid.DashOff = 5;
                    // толщина линий
                    GraphPane.YAxis.MajorGrid.PenWidth = 0.1F;
                    GraphPane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                    // Включаем отображение сетки напротив мелких рисок по оси Y
                    GraphPane.YAxis.MinorGrid.IsVisible = true;
                    // Длина штрихов равна одному пикселю, ... 
                    GraphPane.YAxis.MinorGrid.DashOn = 1;
                    GraphPane.YAxis.MinorGrid.DashOff = 2;
                    // толщина линий
                    GraphPane.YAxis.MinorGrid.PenWidth = 0.1F;
                    GraphPane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                    // Устанавливаем интересующий нас интервал по оси Y
                    GraphPane.YAxis.Scale.Min = minimum_scale;
                    GraphPane.YAxis.Scale.Max = maximum_scale;

                    AxisChange();

                    Invalidate();
                }
            }
            /// <summary>
            /// Класс для размещения активных элементов управления
            /// </summary>
            private class PanelQuickDataVzletTDirect : HPanelQuickData
            {
                private enum INDEX_VALUE {  }
                
                public PanelQuickDataVzletTDirect()
                    : base(/*-1, -1*/)
                {
                    InitializeComponents();
                }

                private void InitializeComponents()
                {
                    COUNT_ROWS = 12;

                    int count_ui = 3
                        , count_rowspan_ui = COUNT_ROWS / count_ui;

                    // значение 'SZ_COLUMN_LABEL' устанавливается индивидуально
                    /*SZ_COLUMN_LABEL = 48F;*/ SZ_COLUMN_LABEL_VALUE = 78F;
                    SZ_COLUMN_TG_LABEL = 40F; SZ_COLUMN_TG_LABEL_VALUE = 75F;

                    m_indxStartCommonFirstValueSeries = (int)CONTROLS.lblTemperatureCurrent;
                    m_indxStartCommonSecondValueSeries = (int)CONTROLS.lblDeviatCurrent;
                    m_iCountCommonLabels = (int)CONTROLS.lblDeviatDateValue - (int)CONTROLS.lblTemperatureCurrent + 1;

                    // количество и параметры строк макета панели
                    this.RowCount = COUNT_ROWS;
                    for (int i = 0; i < this.RowCount + 1; i++)
                        this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

                    this.m_arLabelCommon = new System.Windows.Forms.Label[m_iCountCommonLabels];

                    //
                    // btnSetNow
                    //
                    this.Controls.Add(this.btnSetNow, 0, 0);
                    this.SetRowSpan(this.btnSetNow, count_rowspan_ui);
                    // 
                    // dtprDate
                    // 
                    this.Controls.Add(this.dtprDate, 0, 1 * count_rowspan_ui);
                    this.SetRowSpan(this.dtprDate, count_rowspan_ui);
                    // 
                    // lblServerTime
                    // 
                    this.Controls.Add(this.lblServerTime, 0, 2 * count_rowspan_ui);
                    this.SetRowSpan(this.lblServerTime, count_rowspan_ui);

                    //Ширина столбца группы "Элементы управления"
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

                    //Создание ОБЩих элементов управления
                    Color foreColor, backClolor;
                    float szFont;
                    ContentAlignment align;
                    string text = string.Empty;

                    #region добавить поля для значений ТЕМПЕРАТУРЫ и их подписи
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblTemperatureDateValue + 1; i++)
                    {
                        //szFont = 6F;

                        switch (i)
                        {
                            case CONTROLS.lblTemperatureCurrent:
                            case CONTROLS.lblTemperatureHour:
                            case CONTROLS.lblTemperatureDate:
                                foreColor = Color.Black;
                                backClolor = Color.Empty;
                                szFont = 8F;
                                align = ContentAlignment.MiddleLeft;
                                break;
                            case CONTROLS.lblTemperatureCurrentValue:
                            case CONTROLS.lblTemperatureHourValue:
                            case CONTROLS.lblTemperatureDateValue:
                                foreColor = Color.LimeGreen;
                                backClolor = Color.Black;
                                szFont = 15F;
                                align = ContentAlignment.MiddleCenter;
                                break;
                            default:
                                foreColor = Color.Yellow;
                                backClolor = Color.Red;
                                szFont = 6F;
                                align = ContentAlignment.MiddleCenter;
                                break;
                        }

                        switch (i)
                        {
                            case CONTROLS.lblTemperatureCurrent:
                                text = @"t тек";
                                break;
                            case CONTROLS.lblTemperatureHour:
                                text = @"t час";
                                break;
                            case CONTROLS.lblTemperatureDate:
                                text = @"t сут";
                                break;
                            default:
                                text = string.Empty;
                                break;
                        }

                        createLabel((int)i, text, foreColor, backClolor, szFont, align);
                    }
                    #endregion

                    #region добавить поля для значений отклонения и их подписи
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDeviatDateValue + 1; i++)
                    {
                        switch (i)
                        {
                            case CONTROLS.lblDeviatCurrent:
                            case CONTROLS.lblDeviatHour:
                            case CONTROLS.lblDeviatDate:
                                foreColor = Color.Black;
                                backClolor = Color.Empty;
                                szFont = 8F;
                                align = ContentAlignment.MiddleLeft;
                                break;
                            case CONTROLS.lblDeviatCurrentValue:
                            case CONTROLS.lblDeviatHourValue:
                            case CONTROLS.lblDeviatDateValue:
                                foreColor = Color.LimeGreen;
                                backClolor = Color.Black;
                                szFont = 15F;
                                align = ContentAlignment.MiddleCenter;
                                break;
                            default:
                                foreColor = Color.Yellow;
                                backClolor = Color.Red;
                                szFont = 6F;
                                align = ContentAlignment.MiddleCenter;
                                break;
                        }

                        switch (i)
                        {
                            case CONTROLS.lblDeviatCurrent:
                                text = @"Откл.тек";
                                break;
                            case CONTROLS.lblDeviatHour:
                                text = @"Откл.час";
                                break;
                            case CONTROLS.lblDeviatDate:
                                text = @"Откл.сут";
                                break;
                            default:
                                text = string.Empty;
                                break;
                        }

                        createLabel((int)i, text, foreColor, backClolor, szFont, align);
                    }
                    #endregion
                }

                public enum CONTROLS : short
                {
                    unknown = -1
                    , lblTemperatureCurrent, lblTemperatureCurrentValue
                    , lblTemperatureHour, lblTemperatureHourValue
                    , lblTemperatureDate, lblTemperatureDateValue
                    , lblDeviatCurrent, lblDeviatCurrentValue
                    , lblDeviatHour, lblDeviatHourValue
                    , lblDeviatDate, lblDeviatDateValue
                    , btnSetNow
                    , dtprDate
                    , cbxTimeZone
                    , lblServerTime
                        , COUNT_CONTROLS
                }

                //protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
                //{
                //    throw new NotImplementedException();
                //}

                protected override TableLayoutPanelCellPosition getPositionCell(int indx)
                {
                    int row = -1,
                    col = -1;

                    switch ((CONTROLS)indx)
                    {
                        case CONTROLS.lblTemperatureCurrent:
                            row = 0; col = 1;
                            break;
                        case CONTROLS.lblTemperatureCurrentValue:
                            row = 0; col = 2;
                            break;
                        case CONTROLS.lblTemperatureHour:
                            row = 1 * COUNT_ROWSPAN_LABELCOMMON; col = 1;
                            break;
                        case CONTROLS.lblTemperatureHourValue:
                            row = 1 * COUNT_ROWSPAN_LABELCOMMON; col = 2;
                            break;
                        case CONTROLS.lblTemperatureDate:
                            row = 2 * COUNT_ROWSPAN_LABELCOMMON; col = 1;
                            break;
                        case CONTROLS.lblTemperatureDateValue:
                            row = 2 * COUNT_ROWSPAN_LABELCOMMON; col = 2;
                            break;
                        case CONTROLS.lblDeviatCurrent:
                            row = 0; col = 3;
                            break;
                        case CONTROLS.lblDeviatCurrentValue:
                            row = 0; col = 4;
                            break;
                        case CONTROLS.lblDeviatHour:
                            row = 1 * COUNT_ROWSPAN_LABELCOMMON; col = 3;
                            break;
                        case CONTROLS.lblDeviatHourValue:
                            row = 1 * COUNT_ROWSPAN_LABELCOMMON; col = 4;
                            break;
                        case CONTROLS.lblDeviatDate:
                            row = 2 * COUNT_ROWSPAN_LABELCOMMON; col = 3;
                            break;
                        case CONTROLS.lblDeviatDateValue:
                            row = 2 * COUNT_ROWSPAN_LABELCOMMON; col = 4;
                            break;
                        default:
                            break;
                    }

                    return new TableLayoutPanelCellPosition(col, row);
                }

                public override void AddTGView(TECComponentBase comp)
                {
                    int cnt = -1
                        , id = -1;
                    HLabel hlblValue;

                    id = comp.m_id;

                    m_tgLabels.Add(id, new System.Windows.Forms.Label[(int)Vyvod.ParamVyvod.INDEX_VALUE.COUNT]);
                    m_tgToolTips.Add(id, new ToolTip[(int)Vyvod.ParamVyvod.INDEX_VALUE.COUNT]);
                    cnt = m_tgLabels.Count;

                    m_tgLabels[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.LABEL_DESC] = HLabel.createLabel(comp.name_shr,
                                                                            new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                            Color.Black, Color.Empty,
                                                                            8F, ContentAlignment.MiddleRight));

                    hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, Color.Black, 13F, ContentAlignment.MiddleCenter));
                    hlblValue.Text = @"---.--"; //name_shr + @"_Fact";
                    hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
                    //m_tgToolTips[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT].SetToolTip(hlblValue, tg.name_shr + @"[" + tg.m_SensorsStrings_ASKUE[0] + @"]: " + (tg.m_TurnOnOff == Vyvod.ParamVyvod.INDEX_TURNOnOff.ON ? @"вкл." : @"выкл."));
                    m_tgLabels[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT] = (System.Windows.Forms.Label)hlblValue;
                    m_tgToolTips[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT] = new ToolTip();

                    hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.Green, Color.Black, 13F, ContentAlignment.MiddleCenter));
                    hlblValue.Text = @"---.--"; //name_shr + @"_TM";
                    hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
                    m_tgLabels[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.DEVIAT] = (System.Windows.Forms.Label)hlblValue;

                    m_tgToolTips[id][(int)Vyvod.ParamVyvod.INDEX_VALUE.DEVIAT] = new ToolTip();
                }

                new protected void addTGLabels(bool bIsDeviation = true)
                {
                    int r = -1, c = -1
                        , i = 0;

                    foreach (int key in m_tgLabels.Keys)
                    {
                        i++;

                        this.Controls.Add(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.LABEL_DESC]);
                        c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 0); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                        this.SetCellPosition(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.LABEL_DESC], new TableLayoutPanelCellPosition(c, r));
                        this.SetRowSpan(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.LABEL_DESC], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                        this.Controls.Add(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT]);
                        c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 1); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                        this.SetCellPosition(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT], new TableLayoutPanelCellPosition(c, r));
                        this.SetRowSpan(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.FACT], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                        if (bIsDeviation == true)
                        {
                            this.Controls.Add(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.DEVIAT]);
                            c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 2); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                            this.SetCellPosition(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.DEVIAT], new TableLayoutPanelCellPosition(c, r));
                            this.SetRowSpan(m_tgLabels[key][(int)Vyvod.ParamVyvod.INDEX_VALUE.DEVIAT], (COUNT_ROWS / COUNT_TG_IN_COLUMN));
                        }
                        else
                            ;
                    }
                }

                public override void RestructControl()
                {
                    COUNT_LABEL = (int)Vyvod.ParamVyvod.INDEX_VALUE.COUNT; COUNT_TG_IN_COLUMN = 4; COL_TG_START = 5;
                    COUNT_ROWSPAN_LABELCOMMON = 4;

                    bool bPowerFactZoom = false;
                    int cntCols = 0;

                    TableLayoutPanelCellPosition pos;

                    //Удаление ОБЩих элементов управления
                    // температуры
                    removeFirstCommonLabels((int)CONTROLS.lblTemperatureDateValue);
                    ////??? мощности
                    //removeSecondCommonLabels((int)CONTROLS.lblPowerHourValue);

                    //Удаление ПУСТой панели
                    if (!(this.Controls.IndexOf(m_panelEmpty) < 0)) this.Controls.Remove(m_panelEmpty); else ;

                    //Удаление стилей столбцов
                    while (this.ColumnStyles.Count > 1)
                        this.ColumnStyles.RemoveAt(this.ColumnStyles.Count - 1);

                    ////??? отображается ли Мощность (Отклонения) - для текущей вкладки неактуально
                    //COL_TG_START -= 2; // вне ~ от контекстного меню, т.к. контекстного меню нет

                    ////??? отображается ли ТМ - для текущей вкладки неактуально
                    //COUNT_LABEL--; // вне ~ от контекстного меню, т.к. контекстного меню нет
                    //COL_TG_START--; // отображение ТМ для этой вкладки не предусмотрено

                    #region Температура
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblTemperatureDateValue + 1; i++)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        pos = getPositionCell((int)i);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], pos);
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROWSPAN_LABELCOMMON);
                    }

                    //Ширина столбцов группы "Температура"
                    SZ_COLUMN_LABEL = 48F; /*SZ_COLUMN_LABEL_VALUE = 78F;*/
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                    #endregion

                    #region Отклонение
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDeviatDateValue + 1; i++)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        pos = getPositionCell((int)i);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], pos);
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROWSPAN_LABELCOMMON);
                    }

                    //Ширина столбцов группы "Отклонение"
                    SZ_COLUMN_LABEL = 63F; /*SZ_COLUMN_LABEL_VALUE = 78F;*/
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                    #endregion

                    cntCols = ((m_tgLabels.Count / COUNT_TG_IN_COLUMN) + ((m_tgLabels.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));

                    for (int i = 0; i < cntCols; i++)
                    {
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL)); // для подписи вывода
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE)); // для значения
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE)); // для отклонения
                    }

                    if (m_tgLabels.Count > 0)
                        addTGLabels();
                    else
                        ;

                    this.Controls.Add(m_panelEmpty, COL_TG_START + cntCols * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
                    this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                }

                public override void ShowFactValues()
                {
                    ;
                }

                public override void ShowTMValues()
                {
                    ;
                }
            }
            /// <summary>
            /// Определить размеры ячеек макета панели
            /// </summary>
            /// <param name="cols">Количество столбцов в макете</param>
            /// <param name="rows">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }
            /// <summary>
            /// Требуется переменная конструктора
            /// </summary>
            private System.ComponentModel.IContainer components = null;
            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            protected override void InitializeComponent()
            {
                components = new System.ComponentModel.Container();

                int[] arProp = new int[] { 0, 1, 0, 1, 0, 1, -1 }; //отобразить часовые таблицу/гистограмму/панель с оперативными данными

                base.InitializeComponent();

                this.SuspendLayout();

                this.ResumeLayout();
                this.PerformLayout();

                if (!(m_label == null))
                    m_label.PerformRestruct(arProp);
                else
                    OnEventRestruct(arProp);
            }

            #endregion
        }

        /// <summary>
        /// Класс для описания панели с информацией
        ///  по дианостированию состояния ИС
        /// </summary>
        public partial class PanelTecVzletTDirect : PanelTecViewBase
        {
            private class DataSource : TecView
            {
                public DataSource(int indx_tec, int indx_comp = -1) : base (indx_tec, indx_comp, TECComponentBase.TYPE.TEPLO)
                {
                    m_idAISKUEParNumber = ID_AISKUE_PARNUMBER.FACT_30;
                }

                //protected override int StateCheckResponse(int state, out bool error, out object outobj)
                //{
                //    throw new NotImplementedException();
                //}

                //protected override int StateRequest(int state)
                //{
                //    throw new NotImplementedException();
                //}

                //protected override int StateResponse(int state, object obj)
                //{
                //    throw new NotImplementedException();
                //}

                //protected override void StateWarnings(int state, int req, int res)
                //{
                //    throw new NotImplementedException();
                //}

                //protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
                //{
                //    throw new NotImplementedException();
                //}

                public override void ChangeState()
                {
                    lock (m_lockState) { GetRDGValues(-1, DateTime.MinValue); }

                    base.ChangeState(); //Run
                }

                //protected override void getPPBRValuesRequest()
                //{
                //    string strQuery = string.Empty;

                //    strQuery =
                //        m_tec.GetPBRValueQuery(indxTECComponents, m_curDate.Date.Add(-m_tsOffsetToMoscow), _type) //TECComponentBase.TYPE.TEPLO
                //        ;

                //    Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.PBR], strQuery);
                //}

                //protected override void getAdminValuesRequest()
                //{
                //    string strQuery = string.Empty;

                //    strQuery =
                //        m_tec.GetAdminValueQuery(indxTECComponents, m_curDate.Date.Add(-m_tsOffsetToMoscow), _type)
                //        ;

                //    Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN], strQuery);
                //}

                public override void GetRDGValues(int indx, DateTime date)
                {
                    ClearStates();

                    //ClearValues();

                    using_date = false;

                    if (m_tec.m_bSensorsStrings == true)
                        if (currHour == true)
                            AddState((int)StatesMachine.CurrentTimeView);
                        else
                            ;
                    else
                    {
                        AddState((int)StatesMachine.InitSensors);
                        AddState((int)StatesMachine.CurrentTimeView);
                    }

                    // ...
                    AddState((int)TecView.StatesMachine.HoursVzletTDirectValues);
                    // ...
                    AddState((int)TecView.StatesMachine.PPBRValues);
                    AddState((int)TecView.StatesMachine.AdminValues);
                }

                //public override bool WasChanged()
                //{
                //    throw new NotImplementedException();
                //}

                protected override void setValuesHours(TECComponentBase.ID idComp = TECComponentBase.ID.MAX)
                {
                    double currPBRt = -1F;
                    int cntResponsibled = -1;

                    for (int i = 0; i < m_valuesHours.Length; i++) //??? m_valuesHours.Length == m_dictValuesTECComponent.Length + 1
                    {
                        cntResponsibled = 0;

                        foreach (int id in m_dictValuesTECComponent[i + 1].Keys)
                        {
                            if (m_dictValuesTECComponent[i + 1][id].valuesPmin > 5)
                            {// responsible
                                cntResponsibled++;

                                // PBR, PBRmax не рассматриваются
                                m_valuesHours[i].valuesPmin += m_dictValuesTECComponent[i + 1][id].valuesPmin;
                                if (i == 0)
                                {
                                    currPBRt = (m_dictValuesTECComponent[i + 1][id].valuesPmin + m_dictValuesTECComponent[0][id].valuesPmin) / 2;
                                }
                                else
                                {
                                    currPBRt = (m_dictValuesTECComponent[i + 1][id].valuesPmin + m_dictValuesTECComponent[i][id].valuesPmin) / 2;
                                }

                                m_dictValuesTECComponent[i + 1][id].valuesPBRe = currPBRt;
                                m_valuesHours[i].valuesPBRe += currPBRt;

                                // valuesForeignCommand не рассматриваются

                                m_valuesHours[i].valuesREC += m_dictValuesTECComponent[i + 1][id].valuesREC; // всегда в %

                                m_dictValuesTECComponent[i + 1][id].valuesUDGe = currPBRt + (currPBRt * m_dictValuesTECComponent[i + 1][id].valuesREC / 100);
                                m_valuesHours[i].valuesUDGe += m_dictValuesTECComponent[i + 1][id].valuesUDGe;

                                if (m_dictValuesTECComponent[i + 1][id].valuesISPER == 1)
                                {
                                    m_dictValuesTECComponent[i + 1][id].valuesDiviation =
                                        (currPBRt + m_dictValuesTECComponent[i + 1][id].valuesREC) * m_dictValuesTECComponent[i + 1][id].valuesDIV / 100;
                                }
                                else
                                {
                                    m_dictValuesTECComponent[i + 1][id].valuesDiviation = m_dictValuesTECComponent[i + 1][id].valuesDIV;
                                }
                                m_valuesHours[i].valuesDiviation += m_dictValuesTECComponent[i + 1][id].valuesDiviation;
                            }
                            else
                                ; // not responsible
                        }

                        m_valuesHours[i].valuesPmin /= cntResponsibled;
                        m_valuesHours[i].valuesREC /= cntResponsibled;
                        m_valuesHours[i].valuesPBRe /= cntResponsibled;
                        m_valuesHours[i].valuesUDGe /= cntResponsibled;
                        m_valuesHours[i].valuesDiviation /= cntResponsibled;
                    }
                }
            }
            /// <summary>
            /// constructor
            /// </summary>
            public PanelTecVzletTDirect(TEC tec, int indx_tec, int indx_comp)
                : base(tec, indx_tec, indx_comp, new HMark(new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_VZLET }))
            {
                initialize();
            }
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="container">Родительский объект по отношению к создаваемому</param>
            public PanelTecVzletTDirect(IContainer container, TEC tec, int indx_tec, int indx_comp, HMark markQueries)
                : base(tec, indx_tec, indx_comp, markQueries)
            {
                container.Add(this);
                initialize();
            }
            /// <summary>
            /// Инициализация подключения к БД
            /// и компонентов панели.
            /// </summary>
            /// <returns></returns>
            private int initialize()
            {
                int iRes = 0;

                SPLITTER_PERCENT_VERTICAL = 35;

                m_arPercRows = new int[] { 5, 78 };

                InitializeComponent();

                return iRes;
            }
            /// <summary>
            /// Создать объект для обращения к БД
            /// </summary>
            /// <param name="indx_tec">Индекс ТЭЦ в глобальном списке</param>
            /// <param name="indx_comp">Индекс компонента ТЭЦ (внутренний для ТЭЦ), если -1, то ТЭЦ в целом</param>
            protected override void createTecView(int indx_tec, int indx_comp)
            {
                m_tecView = new DataSource(indx_tec, indx_comp);
            }
            /// <summary>
            /// Создать объекты панели оперативной информации для отображения значений
            ///  в соответствии с составом объекта отображения в целом
            /// </summary>
            public override void AddTGView()
            {
                // цикл по всем ВЫВОДам
                m_tecView.m_tec.list_TECComponents.ForEach (v => {
                    if (v.IsVyvod == true)
                        // добавить элементы управления для отображения значений указанного ВЫВОДа
                        _pnlQuickData.AddTGView(v);
                    else
                        ;
                });
            }
            /// <summary>
            /// Обработчик события - получение данных при запросе к БД
            /// </summary>
            /// <param name="table">Результат выполнения запроса - таблица с данными</param>
            private void dataSource_OnEvtRecievedTable(object table)
            {
            }
            /// <summary>
            /// Назначить делегаты по отображению сообщений в строке статуса
            /// </summary>
            /// <param name="ferr">Делегат для отображения в строке статуса ошибки</param>
            /// <param name="fwar">Делегат для отображения в строке статуса предупреждения</param>
            /// <param name="fact">Делегат для отображения в строке статуса описания действия</param>
            /// <param name="fclr">Делегат для удаления из строки статуса сообщений</param>
            public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
            {
                if (!(m_tecView == null))
                    m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    throw new Exception(@"PanelVzletTDirect::SetDelegateReport () - целевой объект не создан...");
            }
            /// <summary>
            /// Фукнция вызова старта программы
            /// (создание таймера и получения данных)
            /// </summary>
            public override void Start()
            {
                base.Start();

                start();
            }
            /// <summary>
            /// Вызов функций для заполнения 
            /// элементов панели данными
            /// </summary>
            private void start()
            {
            }
            /// <summary>
            /// Остановка работы панели
            /// </summary>
            public override void Stop()
            {
                if (Started == true)
                {
                    stop();

                    base.Stop();
                }
                else
                    ;
            }
            /// <summary>
            /// Остановка работы таймера
            /// и обработки запроса к БД
            /// </summary>
            private void stop()
            {
            }
            /// <summary>
            /// Функция активация Вкладки
            /// </summary>
            /// <param name="activated">параметр</param>
            /// <returns>результат</returns>
            public override bool Activate(bool activated)
            {
                bool bRes = base.Activate(activated);

                if (activated == true)
                {
                }
                else
                    ;

                return bRes;
            }
            /// <summary>
            /// Создать панель отображения оперативных данных
            /// </summary>
            protected override void createPanelQuickData()
            {
                _pnlQuickData = new PanelQuickDataVzletTDirect();
            }
            /// <summary>
            /// Создать таблицу-представление для отображения значений в разрезе "сутки - час"
            /// </summary>
            protected override void createDataGridViewHours()
            {
                m_dgwHours = new DataGridViewVzletTDirectHours();
            }
            /// <summary>
            /// Создать таблицу-представление для отображения значений в разрезе "час - минуты"
            /// </summary>
            protected override void createDataGridViewMins()
            {
                ; // не требуется
            }
            /// <summary>
            /// Создать объект для графической интерпретации данных в разрезе "сутки - час"
            /// </summary>
            /// <param name="objLock">Объект синхронизации(блокировки) при обновлении информации на объекте</param>
            protected override void createZedGraphControlHours(object objLock)
            {
                m_ZedGraphHours = new ZedGraphControlVzletTDirect(new object());
            }
            /// <summary>
            /// Создать объект для графической интерпретации данных в разрезе "час - минуты"
            /// </summary>
            /// <param name="objLock">Объект синхронизации(блокировки) при обновлении информации на объекте</param>
            protected override void createZedGraphControlMins(object objLock)
            {
                ; // не требуется
            }

            protected override HMark enabledSourceData_ToolStripMenuItems()
            {
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] =
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] =
                    CONN_SETT_TYPE.DATA_VZLET;
                ; // не требуется, разнотипные источники данных отсутствуют
                return new HMark(0);
            }

            public void UpdateGraphicsCurrent(int type)
            {
                lock (m_tecView.m_lockValue)
                {
                    //??? Проверка 'type' TYPE_UPDATEGUI
                    HMark markChanged = enabledSourceData_ToolStripMenuItems();
                    if (markChanged.IsMarked() == false)
                    {
                        //DrawGraphMins(m_tecView.lastHour);
                        DrawGraphHours();
                    }
                    else
                    {
                        if (m_tecView.currHour == true)
                            NewDateRefresh();
                        else
                        {//m_tecView.currHour == false
                            updateGraphicsRetro(markChanged);
                        }
                    }
                }
            }
        }
    }
}