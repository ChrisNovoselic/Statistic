﻿using System;
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

            PanelTecVzletTDirect.IndexCustomTecView = listTec.Count > 1 ? PanelTecVzletTDirect.INDEX_CUSTOM_TECVIEW.MULTI : PanelTecVzletTDirect.INDEX_CUSTOM_TECVIEW.SINGLE;

            //??? фильтр ТЭЦ-ЛК, Бийская ТЭЦ - т.к. для них нет АИСКУТЭ
            for (i = 0; i < listTec.Count; i++)
                if ((!(listTec[i].m_id > (int)TECComponent.ID.LK))
                    && (!(listTec[i].Type == TEC.TEC_TYPE.BIYSK)))
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

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            return bRes;
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

                    bool bCurrDate = serverTime.Date.Equals(DateTime.Now.Date);
                    int i = -1
                        //, warn = -1, cntWarn = -1
                        , lh = bCurrHour == true ? lastReceivedHour - 1 :
                            bCurrDate == true ? lastReceivedHour - 1 :
                                itemscount
                        , cntHourFactNotValues = -1 // кол-во часов с пропущенными фактическими значениями
                        , cntHourFactRecieved = -1 // кол-во часов с фактически полученными значениями
                        , digitRound = -1; // кол-во знаков для округления
                    double hourFact = -1F, hourDev = -1F // фактическое отклонение за час, значение за крайний час, откл. за крайний час
                        , sumFact = -1F, sumUDGt = -1F, sumDev = -1F; // суммарные значения для факт.темп. и уточненного дисп.графика (для возможности усреднения)
                    string strVal = string.Empty, strNotValue = @"-"
                        , strWarn = string.Empty;

                    Debug.WriteLine(@"DataGridViewVzletTDirectHours::Fill () - serverTime=" + serverTime.ToString()
                        + @"; lastHour=" + lastHour
                        + @"; lastReceivedHour=" + lastReceivedHour);

                    DataGridViewCellStyle curCellStyle;
                    DataGridViewCellStyle normalDevCellStyle = new DataGridViewCellStyle()
                        , errorDevCellStyle = new DataGridViewCellStyle();
                    curCellStyle = normalDevCellStyle;

                    normalDevCellStyle.BackColor = Color.White;
                    errorDevCellStyle.BackColor = Color.Red;

                    cntHourFactNotValues = 0;
                    sumFact =
                    sumUDGt =
                        0F;
                    sumDev = double.NegativeInfinity;
                    for (i = 0; i < itemscount; i++)
                    {
                        // номер часа - уже отображен

                        // зафиксировать отсутствие значения
                        cntHourFactNotValues += ((!(values[i].valuesFact > 0) && (!(i > lh))) ? 1 : 0);

                        // - температура
                        if (values[i].valuesFact > 0)
                        {
                            hourFact = values[i].valuesFact;
                            // факт
                            if (!(i > lh))
                            {
                                sumFact += hourFact;
                                strVal = (Math.Round(hourFact, 2)).ToString();
                            }
                            else
                                strVal = strNotValue;
                        }
                        else
                            strVal = strNotValue;

                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_FACT].Value = strVal;
                        
                        // план
                        showCell(i, INDEX_COLUMNS.TEMPERATURE_PBR, values[i].valuesPmin, 1); // температура
                        // рекомендация
                        showCell(i, INDEX_COLUMNS.REC, values[i].valuesREC, 0);
                        // уточненный дисп./график (??? с учетом рекомендации)
                        showCell(i, INDEX_COLUMNS.UDGt, values[i].valuesUDGe, 1);
                        if (Double.IsNaN(values[i].valuesUDGe) == false)
                            sumUDGt += values[i].valuesUDGe;
                        else
                            ;
                        
                        // - температура
                        if ((values[i].valuesFact > 0)
                            && (values[i].valuesUDGe > 0))
                        {
                            // разность
                            hourDev = values[i].valuesFact - values[i].valuesUDGe;                            
                            
                            if (!(i > lh))
                            {
                                if (double.IsNegativeInfinity(sumDev) == true)
                                    sumDev = 0F;
                                else
                                    ;
                                sumDev += hourDev;
                                strVal = Math.Round(hourDev, 2).ToString();
                            }
                            else
                                strVal = strNotValue;
                        }
                        else
                            strVal = strNotValue;                        
                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = strVal;
                        
                        // визуализация выхода за пределы диапазона
                        curCellStyle = normalDevCellStyle;
                        if ((strVal.Equals(strNotValue) == false)
                            && (!(i > lh)))
                            if (Math.Abs(hourDev) > values[i].valuesDiviation)
                                curCellStyle = errorDevCellStyle;
                            else
                                ; // оставить по умолчанию
                        else
                            ; // оставить по умолчанию
                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Style = curCellStyle;
                    }
                    // кол-во фактически полученных значений в ~ от
                    // кол-ва пропущенных значений
                    // получены ли значения за полные сутки
                    // признака "текущий" час
                    // признака "текущие" сутки
                    // типа БД
                    cntHourFactRecieved = (lh - (cntHourFactNotValues > 0 ? (lh < 24) ? cntHourFactNotValues - 1 : cntHourFactNotValues : (TEC.TypeDbVzlet == TEC.TYPE_DBVZLET.KKS_NAME ? (bCurrHour == true ? -1 : (bCurrDate == true ? -1 : 0)) : TEC.TypeDbVzlet == TEC.TYPE_DBVZLET.GRAFA ? 0 : Int16.MinValue)));
                    //cntHourFactRecieved += ((bCurrHour == true) || (bCurrDate == true)) ? 1 : 0;
                    //Фактическое знач (среднее)
                    digitRound = 2;
                    //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_FACT].Value = Math.Round(sumFact / cntHourFactRecieved, digitRound).ToString(@"F" + digitRound);
                    showCell(i, INDEX_COLUMNS.TEMPERATURE_FACT, sumFact / cntHourFactRecieved, (ushort)digitRound);
                    //Уточненный дисп./график (средний)
                    digitRound = 1;
                    //Rows[i].Cells[(int)INDEX_COLUMNS.UDGt].Value = Math.Round(sumUDGt / itemscount, digitRound).ToString(@"F" + digitRound);
                    showCell (i, INDEX_COLUMNS.UDGt, sumUDGt / itemscount, (ushort)digitRound);
                    //Отклонение за сутки (среднее)
                    digitRound = 2;
                    //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = Math.Round(sumDev / cntHourFactRecieved, digitRound).ToString(@"F" + digitRound);
                    showCell(i, INDEX_COLUMNS.TEMPERATURE_DEVIATION, sumDev / cntHourFactRecieved, (ushort)digitRound);
                    // план для панели оперативной информации
                    PerformDataValues(new DataValuesEventArgs() {
                        m_value1 = (bCurrHour == true ? (sumFact + hourFact) / (cntHourFactRecieved + 1) : sumFact / cntHourFactRecieved)
                        , m_value2 = (bCurrHour == true ? (sumDev + hourDev) / (cntHourFactRecieved + 1) : sumDev / cntHourFactRecieved)
                    });
                }

                private void showCell(int iRow, INDEX_COLUMNS indxCol, double value, ushort digit)
                {
                    //string strValue = @"-";

                    if ((Double.IsNaN(value) == false)
                        && (Double.IsNegativeInfinity(value) == false))
                        Rows[iRow].Cells[(int)indxCol].Value = value.ToString(@"F" + digit.ToString());
                    else
                        Rows[iRow].Cells[(int)indxCol].Value = @"-";
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
                        , lh = -1; // крайний час для отображения
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

                    // копия в 'PanelTecVzletTDirect::HZedGraphControlStandardHours::Draw ()'
                    if (currHour == true)
                        lh = lastHour + 1;
                    else
                        if (HDateTime.ToMoscowTimeZone(DateTime.Now).Date.Equals(serverTime.Date) == true)
                            lh = serverTime.Hour //lastHour + 1
                                ;
                        else
                            lh = 24;

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
                        //Отобразить только завершившиеся часы
                        if (i < lh) {
                            y = values[i].valuesFact;
                            valuesFact.Add(h, y);
                        } else ;

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
                    colorPCurve = Color.Purple;

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
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                    {
                        if (typeConnSett == CONN_SETT_TYPE.DATA_VZLET)
                        //BarItem
                            GraphPane.AddBar(strCurveNameValue, valuesFact, colorPCurve);
                        else
                            // других типов данных для ЛК не предусмотрено
                            ;
                    }
                    else
                        if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                        {
                            if (typeConnSett == CONN_SETT_TYPE.DATA_VZLET)
                            //LineItem
                                GraphPane.AddCurve(strCurveNameValue, valuesFact, colorPCurve);                                
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
                    GraphPane.Title.Text = @"АИСКУТЭ";
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
                    {//??? не проверялось
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
                            (lastHour + 1 + (currHour == true ? 1 : 0)).ToString() + " час";

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

                private class ContextMenuStripZedGraph : HContextMenuStripZedGraph
                {
                    protected override void initializeItemAdding()
                    {
                        int indx = -1;
                    }
                }

                protected override void createContextMenuStrip()
                {
                    this.ContextMenuStrip = new ContextMenuStripZedGraph();
                }

                protected override void initializeContextMenuItemAddingEventHandler(EventHandler fAddingHandler)
                {
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
                                foreColor = Color.Yellow; //LimeGreen
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
                    int indxStartCommonPVal = m_indxStartCommonFirstValueSeries
                        , lastHour = m_parent.m_tecView.currHour == true ? m_parent.m_tecView.lastHour + 1 : m_parent.m_tecView.lastHour
                        ;
                    bool bCurrHour = m_parent.m_tecView.currHour;
                    TG.INDEX_VALUE indxVyvodValue = TG.INDEX_VALUE.COUNT_INDEX_VALUE;
                    double dblValue = -1F;
                    double []arValues = new double[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE - 1];
                    //int[] arIds = new int[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE - 1];
                    int idVyvod = -1;
                    DateTime dtServer = m_parent.m_tecView.serverTime.Date;

                    if (m_parent.m_tecView.IsHourValues(lastHour) == true)
                    {
                        //Температура
                        // значение температуры (текущее)
                        arValues[(int)TG.INDEX_VALUE.FACT] = ((bCurrHour == true) && ((m_parent.m_tecView as DataSource).m_valueCurrHour > 0)) ?
                            (m_parent.m_tecView as DataSource).m_valueCurrHour :
                                double.NegativeInfinity;
                        showValue(ref m_arLabelCommon[(int)CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal]
                            , arValues[(int)TG.INDEX_VALUE.FACT]
                            , 2 //round
                            , false
                            , true
                            , arValues[(int)TG.INDEX_VALUE.FACT] == double.NegativeInfinity ? @"--.--" : string.Empty);
                        // отклонение значения температуры от УДГт (текущее)
                        arValues[(int)TG.INDEX_VALUE.TM] = ((bCurrHour == true) && ((m_parent.m_tecView as DataSource).m_valueCurrHour > 0) && (m_parent.m_tecView.m_valuesHours[lastHour].valuesUDGe > 0)) ?
                            (m_parent.m_tecView as DataSource).m_valueCurrHour - m_parent.m_tecView.m_valuesHours[lastHour].valuesUDGe :
                                double.NegativeInfinity;
                        showValue(ref m_arLabelCommon[(int)CONTROLS.lblDeviatCurrentValue - indxStartCommonPVal]
                            , arValues[(int)TG.INDEX_VALUE.TM]
                            , 2 //round
                            , false
                            , true
                            , arValues[(int)TG.INDEX_VALUE.TM] == double.NegativeInfinity ? @"---" : string.Empty);
                        //Температура
                        // часовое значение температуры (крайний час)
                        arValues[(int)TG.INDEX_VALUE.FACT] = m_parent.m_tecView.m_valuesHours[lastHour].valuesFact > 0 ? m_parent.m_tecView.m_valuesHours[lastHour].valuesFact : double.NegativeInfinity;
                        showValue(ref m_arLabelCommon[(int)CONTROLS.lblTemperatureHourValue - indxStartCommonPVal]
                            , arValues[(int)TG.INDEX_VALUE.FACT]
                            , 2 //round
                            , true
                            , true
                            , arValues[(int)TG.INDEX_VALUE.FACT] == double.NegativeInfinity ? @"--.--" : string.Empty);
                        // отклонение часового значения температуры от УДГт (крайний час)
                        arValues[(int)TG.INDEX_VALUE.TM] = ((m_parent.m_tecView.m_valuesHours[lastHour].valuesUDGe > 0) && (m_parent.m_tecView.m_valuesHours[lastHour].valuesFact > 0)) ?
                            m_parent.m_tecView.m_valuesHours[lastHour].valuesFact - m_parent.m_tecView.m_valuesHours[lastHour].valuesUDGe : double.NegativeInfinity;
                        showValue(ref m_arLabelCommon[(int)CONTROLS.lblDeviatHourValue - indxStartCommonPVal]
                            , arValues[(int)TG.INDEX_VALUE.TM]
                            , 2 //round
                            , false
                            , true
                            , arValues[(int)TG.INDEX_VALUE.TM] == double.NegativeInfinity ? @"---" : string.Empty);

                        //// цвет шрифта для значений температуры, мощности
                        //m_arLabelCommon[(int)CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)CONTROLS.lblTemperatureHourValue - indxStartCommonPVal].ForeColor = getColorValues(TG.INDEX_VALUE.FACT);
                        m_arLabelCommon[(int)CONTROLS.lblTemperatureDateValue - indxStartCommonPVal].ForeColor = dtServer.Equals(HDateTime.ToMoscowTimeZone().Date) == false ?
                            getColorValues(TG.INDEX_VALUE.FACT) :
                                Color.LimeGreen; // если сутки текущие, то оставить как есть
                    }
                    else
                        ; // нет значений для указанного 'lastHour' часа

                    if (PanelTecVzletTDirect.IndexCustomTecView == INDEX_CUSTOM_TECVIEW.SINGLE)
                        // детализация
                        // при текущих значениях поля для вывода значений очищаются (m_parent.m_tecView.currHour == true)
                        foreach (TECComponent g in m_parent.m_tecView.LocalTECComponents)
                            if (g.IsVyvod == true)
                            {//Только ГТП
                                // идентификаторы параметров
                                //arIds[(int)TG.INDEX_VALUE.FACT] =
                                //arIds[(int)TG.INDEX_VALUE.TM] =
                                //    -1;
                                idVyvod = g.m_id;
                                // значения параметров
                                arValues[(int)TG.INDEX_VALUE.FACT] =
                                arValues[(int)TG.INDEX_VALUE.TM] =
                                    -1F;
                                // получить значения для параметров ВЫВОДа
                                foreach (Vyvod.ParamVyvod pv in g.m_listLowPointDev)
                                {//Цикл по списку с парметрами ВЫВОДа
                                    if (!(m_parent.m_tecView.m_dictValuesLowPointDev[pv.m_id].m_power_LastMinutesTM == null))
                                    {
                                        indxVyvodValue = TG.INDEX_VALUE.COUNT_INDEX_VALUE;

                                        switch (pv.m_id_param)
                                        {
                                            case Vyvod.ID_PARAM.G_PV:
                                            case Vyvod.ID_PARAM.G2_PV:
                                                indxVyvodValue = (int)TG.INDEX_VALUE.FACT;
                                                break;
                                            case Vyvod.ID_PARAM.T_PV:
                                            case Vyvod.ID_PARAM.T2_PV:
                                                indxVyvodValue = TG.INDEX_VALUE.TM;
                                                break;
                                            default:
                                                break;
                                        }

                                        //arIds[indxVyvodValue] = pv.m_id;
                                        if (indxVyvodValue < TG.INDEX_VALUE.COUNT_INDEX_VALUE)
                                            if (bCurrHour == true)
                                                // текущее значение
                                                dblValue = (m_parent.m_tecView as DataSource).m_dictCurrValuesLowPointDev[pv.m_id];
                                            else
                                                // ретро-значение
                                                dblValue = m_parent.m_tecView.m_dictValuesLowPointDev[pv.m_id].m_power_LastMinutesTM[lastHour];
                                        else
                                            dblValue = -1F;

                                        if (dblValue > 0)
                                            switch (pv.m_id_param)
                                            {
                                                case Vyvod.ID_PARAM.G_PV:
                                                case Vyvod.ID_PARAM.G2_PV:
                                                    if (arValues[(int)indxVyvodValue] < 0)
                                                        arValues[(int)indxVyvodValue] = 0;
                                                    else
                                                        ;
                                                    arValues[(int)indxVyvodValue] += dblValue;
                                                    break;
                                                case Vyvod.ID_PARAM.T_PV:
                                                case Vyvod.ID_PARAM.T2_PV:
                                                    if (!(arValues[(int)indxVyvodValue] > 0))
                                                        arValues[(int)indxVyvodValue] = dblValue;
                                                    else
                                                        ;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        else
                                            ;
                                    }
                                    else
                                        ; // массив со значениями не инициализирован
                                }

                                if (idVyvod > 0)
                                {
                                    // отобразить значение ВЗВЕШЕННАЯ температура                            
                                    indxVyvodValue = (int)TG.INDEX_VALUE.FACT;
                                    showTDirectValue(idVyvod
                                        , indxVyvodValue
                                        , ((arValues[(int)TG.INDEX_VALUE.FACT] > 0) && (arValues[(int)TG.INDEX_VALUE.TM] > 0)) ?
                                        arValues[(int)TG.INDEX_VALUE.FACT] * arValues[(int)TG.INDEX_VALUE.TM] / (bCurrHour == true ? (m_parent.m_tecView as DataSource).m_SummGpv : m_parent.m_tecView.m_valuesHours[lastHour].valuesTMSNPsum) :
                                                -1F);
                                    // отобразить значение температура
                                    indxVyvodValue = TG.INDEX_VALUE.TM;
                                    showTDirectValue(idVyvod
                                        , indxVyvodValue
                                        , arValues[(int)indxVyvodValue]);
                                }
                                else
                                    ; // идентификатор ВЫВОДа не известен
                            }
                            else
                                ; // только ВЫВОДы
                    else
                        ; // не детализировать при отображении нескольких ТЭЦ
                }

                public override void ShowTMValues()
                {// не используется. Одновременно отображается как FACT, так и TM
                }

                //private Color getColor

                private void showTDirectValue(int id_tg, TG.INDEX_VALUE indx, double powerLastHour)
                {
                    if (powerLastHour > 0)
                        //Отобразить значение
                        showValue(m_tgLabels[id_tg][(int)indx]
                            , powerLastHour, 2, false);
                    else
                        //Отобразить строку - отсутствие значения
                        m_tgLabels[id_tg][(int)indx].Text = "--.--";

                    // установить цвет шрифта для значения
                    m_tgLabels[id_tg][(int)indx].ForeColor =
                        //Color.Green
                        getColorValues(indx)
                            ;
                }

                public void OnSumDataValues(HDataGridViewBase.DataValuesEventArgs ev)
                {
                    int indxStartCommonPVal = m_indxStartCommonFirstValueSeries;

                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblTemperatureDateValue - indxStartCommonPVal]
                            , ev.m_value1
                            , 2 //round
                            , false
                            , true
                            , string.Empty);
                    // отклонение часового значения температуры от УДГт (крайний час)
                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblDeviatDateValue - indxStartCommonPVal]
                        , ev.m_value2
                        , 2 //round
                        , false
                        , true
                        , ev.m_value2 == double.NegativeInfinity ? @"---" : string.Empty);
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

            public enum INDEX_CUSTOM_TECVIEW : short { UNKNOWN = -1, SINGLE, MULTI };
            public static INDEX_CUSTOM_TECVIEW IndexCustomTecView = INDEX_CUSTOM_TECVIEW.UNKNOWN;
            //отобразить часовые таблицу/гистограмму/панель с оперативными данными
            //отобразить часовые таблицу
            private static List<int[]> s_SetCustomTecView = new List<int[]> {new int [] { 0, 1, 0, 1, 0, 1, -1 }, new int [] { 0, 1, 0, 0, -1, 1, -1 }};

            protected override void InitializeComponent()
            {
                components = new System.ComponentModel.Container();

                base.InitializeComponent();

                this.m_ZedGraphHours.InitializeContextMenuItemAddingEventHandler(this.эксельToolStripMenuItemHours_Click, null);

                this.SuspendLayout();

                this.ResumeLayout();
                this.PerformLayout();

                if (!(m_label == null))
                    m_label.PerformRestruct(s_SetCustomTecView[(int)IndexCustomTecView]);
                else
                    OnEventRestruct(s_SetCustomTecView[(int)IndexCustomTecView]);
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

                protected override void Initialize()
                {
                    base.Initialize();

                    m_valueCurrHour =
                    m_SummGpv =
                        -1F;
                    m_dictCurrValuesLowPointDev = new Dictionary<int, double>();
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

                protected override int StateRequest(int state)
                {
                    int iRes = 0;
                    bool bHandled = ((StatesMachine)state == StatesMachine.HoursVzletTDirectValues)
                        || ((StatesMachine)state == StatesMachine.CurrentVzletTDirectValues);
                    //НЕ исключать обработку событий в базовом методе, чтобы отобразить сообщение в строке статуса
                    switch ((StatesMachine)state)
                    {
                        case StatesMachine.HoursVzletTDirectValues:
                            ClearValuesHours();
                            getHoursVzletTDirectRequest(m_curDate.Date);
                            break;
                        case StatesMachine.CurrentVzletTDirectValues:
                            getCurrentVzletTDirectRequest();
                            break;
                        default:
                            break;
                    }

                    base.StateRequest(state);

                    return iRes;
                }

                protected override int StateResponse(int state, object table)
                {
                    int iRes = 0;
                    bool bHandled = ((StatesMachine)state == StatesMachine.HoursVzletTDirectValues)
                        || ((StatesMachine)state == StatesMachine.CurrentVzletTDirectValues);
                    //ВАЖНО исключить обработку событий в базовом методе
                    switch ((StatesMachine)state)
                    {
                        case StatesMachine.HoursVzletTDirectValues:
                            iRes = getHoursVzletTDirectResponse(table as System.Data.DataTable);
                            break;
                        case StatesMachine.CurrentVzletTDirectValues:
                            iRes = getCurrentVzletTDirectResponse(table as System.Data.DataTable);
                            //updateGUI_TM_Gen();
                            break;
                        default:
                            break;
                    }

                    if (bHandled == true)
                        base.StateResponse(state, table);
                    else
                        iRes = base.StateResponse(state, table);

                    return iRes;
                }

                private void getHoursVzletTDirectRequest(DateTime dt)
                {
                    string strQuery = m_tec.GetHoursVzletTDirectQuery(dt);

                    //Debug.WriteLine(DateTime.Now.ToString () + @"; TecView::getHoursVzletTDirectRequest () - query = " + strQuery);
                    Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_VZLET], strQuery);
                }

                private void getCurrentVzletTDirectRequest()
                {
                    string strQuery = m_tec.GetCurrentVzletTDirectQuery ();

                    //Debug.WriteLine(DateTime.Now.ToString() + @"; TecView::getCurrentVzletTDirectRequest () - query = " + strQuery);
                    Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_VZLET], strQuery);
                }

                private static int getIdComponent(string nameField)
                {
                    int iRes = -1;

                    if (Int32.TryParse(nameField.Substring(nameField.IndexOf('_') + 1), out iRes) == false)
                        iRes = -1;
                    else
                        ;

                    return iRes;
                }

                private int getHoursVzletTDirectResponse(DataTable table)
                {
                    int iRes = 0;

                    int iHour = -1, iHourRecieved = -1 // индекс для номера часа, макс. номер часа с полученным значением
                        , id = -1, v = -1, indx = -1 // идентификатор параметра-ВЫВОДА, индекс для вывода, индекс для поля в таблице-результате
                        , typeValue = 0, cntTypeValues = 2 // тип значения (0 - реальные расходы/1 - реальные температуры/2 - взвешенные температуры)
                        , cntVyvod = -1; // количество ВЫВОДов
                    double Gpv = -1F, Tpv = -1F;
                    List<int> listIDLowPointDev = new List<int>(); // список идентификаторов параметров ВЫВОДов, значения которых представлены в столбцах таблицы-результата
                    DataRow[] arDataParamVyvod = null;

                    try
                    {
                        switch (TEC.TypeDbVzlet)
                        {
                            case TEC.TYPE_DBVZLET.KKS_NAME:
                                foreach (TECComponent tc in _localTECComponents)
                                    if ((tc.IsVyvod == true)
                                        && (tc.m_bKomUchet == true))
                                    {
                                        foreach (TECComponentBase lpd in tc.m_listLowPointDev)
                                        {
                                            // получить идентификатор параметра ВЫВОДа
                                            id = lpd.m_id;
                                            // получить все строки со значениями для параметра ВЫВОДа
                                            arDataParamVyvod = table.Select(@"ID_POINT_ASKUTE=" + id);
                                            foreach (DataRow r in arDataParamVyvod)
                                            {
                                                iHour = ((DateTime)r[@"DATETIME"]).Hour;
                                                // запомнить крайний номер часа с полученными значениями
                                                if (iHourRecieved < iHour)
                                                    iHourRecieved = iHour;
                                                else
                                                    ;

                                                m_dictValuesLowPointDev[id].m_power_LastMinutesTM[iHour] = (double)r[@"VALUE"];

                                                if (((lpd as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.G_PV)
                                                    || ((lpd as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.G2_PV))
                                                // суммировать массовые расходы для последующего вычисления массовой доли конкретного ВЫВОДа
                                                    m_valuesHours[iHour].valuesTMSNPsum += (double)r[@"VALUE"];
                                                else
                                                    ;
                                            }
                                        }
                                    }
                                    else
                                        ;

                                //iHourRecieved++;

                                for (iHour = 0; iHour < (iHourRecieved + 1); iHour++)
                                    // цикл по номерам часов с полученными значениями
                                    if (m_valuesHours[iHour].valuesTMSNPsum > 0)
                                        foreach (TECComponent tc in _localTECComponents)
                                            if ((tc.IsVyvod == true) // только ВЫВОДы
                                                && (tc.m_bKomUchet == true)) // только коммерческие
                                            {
                                                Gpv =
                                                Tpv =
                                                    0F;

                                                foreach (TECComponentBase lpd in tc.m_listLowPointDev)
                                                {// цикл по всем конечным устройствам ВЫВОДа
                                                    id = lpd.m_id;
                                                    // получить значение для формулы в ~ от типа параметра
                                                    switch ((lpd as Vyvod.ParamVyvod).m_id_param)
                                                    {
                                                        case Vyvod.ID_PARAM.G_PV:
                                                        case Vyvod.ID_PARAM.G2_PV:
                                                            Gpv += m_dictValuesLowPointDev[id].m_power_LastMinutesTM[iHour];
                                                            break;
                                                        case Vyvod.ID_PARAM.T_PV:
                                                        case Vyvod.ID_PARAM.T2_PV:
                                                            if (Tpv == 0F)
                                                                Tpv = m_dictValuesLowPointDev[id].m_power_LastMinutesTM[iHour];
                                                            else
                                                                ;
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                // вычислить вклад ВЫВОДа в общестанционное значение
                                                if ((Gpv > 0)
                                                    && (Tpv > 0))
                                                    m_valuesHours[iHour].valuesFact += (Gpv / m_valuesHours[iHour].valuesTMSNPsum) * Tpv;
                                                else
                                                    ;
                                            }
                                            else
                                                ; // не ВЫВОД с ком.учетом
                                    else
                                        ; // нет возможности произвести вычисления (знаменатель = 0)
                                break;
                            case TEC.TYPE_DBVZLET.GRAFA:
                            default:
                                cntVyvod = (table.Columns.Count - 1 - 1) / cntTypeValues; // 0-е поле для номера часа, крайнее для СРЕДН_СУММ_РАСХОДЫ, остальные для значений в ~ с типами значений

                                for (v = 1; v < table.Columns.Count; v++)
                                {
                                    id = getIdComponent((string)table.Columns[v].ColumnName);
                                    listIDLowPointDev.Add(id);
                                }

                                foreach (DataRow r in table.Rows)
                                {
                                    iHour = (int)r[@"iHOUR"];
                                    Gpv =
                                    Tpv =
                                        -1F;

                                    if (iHour < 0)
                                        iHour += m_valuesHours.Length/*24*/;
                                    else
                                        if (!(iHour < m_valuesHours.Length))
                                            throw new Exception(string.Format(@"TecView::getHoursVzletTDirectResponse () - HOUR={0} за пределами диапазона...", iHour));
                                        else
                                            ; // индекс часа в пределах диапазона

                                    if (!(r[table.Columns.Count - 1] is DBNull))
                                    {
                                        m_valuesHours[iHour].valuesTMSNPsum = (double)r[table.Columns.Count - 1];

                                        for (v = 1; v < (cntVyvod + 1); v++)
                                        {
                                            // расходы
                                            typeValue = 0;
                                            indx = typeValue * cntVyvod + v;
                                            Gpv = (double)r[indx];
                                            m_dictValuesLowPointDev[listIDLowPointDev[indx - 1]].m_power_LastMinutesTM[iHour] = Gpv;
                                            // температуры
                                            typeValue = 1;
                                            indx = typeValue * cntVyvod + v;
                                            Tpv = (double)r[indx];
                                            m_dictValuesLowPointDev[listIDLowPointDev[indx - 1]].m_power_LastMinutesTM[iHour] = Tpv;
                                            // фактические значения
                                            m_valuesHours[iHour].valuesFact += Tpv * (Gpv / m_valuesHours[iHour].valuesTMSNPsum);
                                        }
                                    }
                                    else
                                        break;
                                }

                                iHourRecieved = iHour;
                                break;
                        }

                        //Определить полные ли сутки в результате запроса
                        if (currHour == true)
                            if (iHourRecieved < (m_valuesHours.Length/*24*/ - 1))
                            {
                                lastHour = iHourRecieved - 1;
                                lastReceivedHour = iHourRecieved;
                            }
                            else
                            {
                                lastHour =
                                lastReceivedHour =
                                    iHourRecieved;
                            }
                        else
                            ;
                    }
                    catch (Exception e)
                    {
                        iRes = -1;

                        Logging.Logg().Exception(e, @"TecView::getHoursVzletTDirectResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return iRes;
                }

                public double m_valueCurrHour
                    , m_SummGpv;
                public Dictionary<int, double> m_dictCurrValuesLowPointDev;

                protected override void ClearValuesHours()
                {
                    base.ClearValuesHours();
                    List<int> keys = new List<int>(m_dictCurrValuesLowPointDev.Keys);

                    m_valueCurrHour =
                    m_SummGpv =
                        0F;
                    foreach (int key in keys)
                        m_dictCurrValuesLowPointDev[key] = 0F;
                }

                protected override void initDictValuesLowPointDev(TECComponent comp)
                {
                    base.initDictValuesLowPointDev(comp);

                    foreach (TECComponentBase dev in comp.m_listLowPointDev)
                        if (m_dictCurrValuesLowPointDev.ContainsKey(dev.m_id) == false)
                            m_dictCurrValuesLowPointDev.Add(dev.m_id, -1F);
                        else
                            ;
                }

                private int getCurrentVzletTDirectResponse(DataTable table)
                {
                    int iRes = 0;

                    int v = -1, id = -1, indx = -1 // идентификатор параметра-ВЫВОДА, индекс для вывода, индекс для поля в таблице-результате
                        , typeValue = 0 // тип значения (0 - реальные расходы/1 - реальные температуры/2 - взвешенные температуры)
                        , cntVyvod = -1; // количество ВЫВОДов
                    double Gpv = -1F, Tpv = -1F;
                    List<int> listIDLowPointDev = new List<int>(); // список идентификаторов параметров ВЫВОДов, значения которых представлены в столбцах таблицы-результата;
                    DataRow[] arDataParamVyvod = null;

                    try
                    {
                        switch (TEC.TypeDbVzlet)
                        {
                            case TEC.TYPE_DBVZLET.KKS_NAME:
                                // для подсчета суммы массового расхода
                                foreach (TECComponent tc in _localTECComponents)
                                    if ((tc.IsVyvod == true)
                                        && (tc.m_bKomUchet == true))
                                    {
                                        foreach (TECComponentBase lpd in tc.m_listLowPointDev)
                                        {
                                            // получить идентификатор параметра ВЫВОДа
                                            id = lpd.m_id;
                                            // получить все строки со значениями для параметра ВЫВОДа
                                            arDataParamVyvod = table.Select(@"ID_POINT_ASKUTE=" + id);

                                            if (arDataParamVyvod.Length == 1)
                                            {
                                                m_dictCurrValuesLowPointDev[id] = (float)arDataParamVyvod[0][@"VALUE"];
                                                // получить значение для формулы в ~ от типа параметра
                                                switch ((lpd as Vyvod.ParamVyvod).m_id_param)
                                                {
                                                    case Vyvod.ID_PARAM.G_PV:
                                                    case Vyvod.ID_PARAM.G2_PV:
                                                        Gpv = (float)arDataParamVyvod[0][@"VALUE"];
                                                        m_SummGpv += Gpv;
                                                        break;
                                                    case Vyvod.ID_PARAM.T_PV:
                                                        // ничего не делать
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                                throw new Exception(string.Format(@"PanelTecVzletTDirect.DataSource::getCurrentVzletTDirectResponse () - для параметра ID={0} строк ...", id));
                                        }
                                    }
                                    else
                                        ; // не ВЫВОД с ком.учетом
                                    // для подсчета итогового значения
                                    foreach (TECComponent tc in _localTECComponents)
                                        if ((tc.IsVyvod == true)
                                            && (tc.m_bKomUchet == true))
                                        {
                                            Gpv =
                                            Tpv =
                                                0F;

                                            foreach (TECComponentBase lpd in tc.m_listLowPointDev)
                                            {
                                                // получить идентификатор параметра ВЫВОДа
                                                id = lpd.m_id;

                                                // получить значение для формулы в ~ от типа параметра
                                                switch ((lpd as Vyvod.ParamVyvod).m_id_param)
                                                {
                                                    case Vyvod.ID_PARAM.G_PV:
                                                    case Vyvod.ID_PARAM.G2_PV:
                                                        Gpv += m_dictCurrValuesLowPointDev[id];
                                                        break;
                                                    case Vyvod.ID_PARAM.T_PV:
                                                    case Vyvod.ID_PARAM.T2_PV:
                                                        if (Tpv == 0)
                                                            Tpv = m_dictCurrValuesLowPointDev[id];
                                                        else
                                                            ;
                                                        break;
                                                    default:
                                                        break;
                                                }                                                
                                            }
                                            // вычислить вклад ВЫВОДа в общестанционное значение
                                            if ((Gpv > 0)
                                                && (Tpv > 0))
                                                m_valueCurrHour += (Gpv / m_SummGpv) * Tpv;
                                            else
                                                ; 
                                        }
                                        else
                                            ; // не ВЫВОД с ком.учетом
                                break;
                            case TEC.TYPE_DBVZLET.GRAFA:
                            default:
                                if (table.Rows.Count == 1)
                                {
                                    cntVyvod = (table.Columns.Count - 1 - 1) / 2;

                                    for (v = 1; v < table.Columns.Count; v++)
                                    {
                                        id = getIdComponent((string)table.Columns[v].ColumnName);
                                        listIDLowPointDev.Add(id);
                                    }

                                    if (TecView.ValidateDatetimeTMValue(serverTime.Add(-HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE), (DateTime)table.Rows[0][0]) == false)
                                        iRes = 1; // не актуальное время крайнего опроса
                                    else
                                        ;

                                    if (!(table.Rows[0][table.Columns.Count - 1] is DBNull))
                                    {
                                        m_SummGpv = (float)table.Rows[0][table.Columns.Count - 1];

                                        for (v = 1; v < (cntVyvod + 1); v++)
                                        {
                                            // расходы
                                            typeValue = 0;
                                            indx = typeValue * cntVyvod + v;
                                            Gpv = (float)table.Rows[0][indx];
                                            m_dictCurrValuesLowPointDev[listIDLowPointDev[indx - 1]] = Gpv;
                                            // температуры
                                            typeValue = 1;
                                            indx = typeValue * cntVyvod + v;
                                            Tpv = (float)table.Rows[0][indx];
                                            m_dictCurrValuesLowPointDev[listIDLowPointDev[indx - 1]] = Tpv;
                                            // фактические значения
                                            m_valueCurrHour += Tpv * (Gpv / m_SummGpv);
                                        }
                                    }
                                    else
                                        iRes = 2; // один из расходов == НУЛЛ
                                }
                                else
                                    iRes = -2;
                                break;                            
                        }
                    }
                    catch (Exception e)
                    {
                        iRes = -1;

                        Logging.Logg().Exception(e, @"TecView::getCurrentVzletTDirectResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return iRes;
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
                    if (currHour == true)
                        AddState((int)TecView.StatesMachine.CurrentVzletTDirectValues);
                    else
                        ;
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

                if (IndexCustomTecView == INDEX_CUSTOM_TECVIEW.MULTI)
                    m_label = new PanelCustomTecView.HLabelCustomTecView(s_SetCustomTecView[(int)IndexCustomTecView]);
                else
                    ;

                SPLITTER_PERCENT_VERTICAL = 35;

                m_arPercRows = new int[] { 5, 78 };

                InitializeComponent();

                m_dgwHours.EventDataValues += new HDataGridViewBase.DataValuesEventHandler((_pnlQuickData as PanelQuickDataVzletTDirect).OnSumDataValues);

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
                if (IndexCustomTecView == INDEX_CUSTOM_TECVIEW.SINGLE)
                    // цикл по всем ВЫВОДам
                    m_tecView.m_tec.list_TECComponents.ForEach(c =>
                    {
                        //if ((c.IsParamVyvod == true)
                        //    && ((c.m_listLowPointDev[0] as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV))
                        if (c.IsVyvod == true)
                            // добавить элементы управления для отображения значений указанного ВЫВОДа
                            _pnlQuickData.AddTGView(c);
                        else
                            ;
                    });
                else
                    ;
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

                if (bRes == true)
                    if (activated == true)
                    {
                    }
                    else
                        ;
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
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] = CONN_SETT_TYPE.UNKNOWN;
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = CONN_SETT_TYPE.DATA_VZLET;
                ; // не требуется, разнотипные источники данных отсутствуют
                return new HMark(0);
            }

            /// <summary>
            /// Обработчик события нажатия на кнопку экспорта
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void эксельToolStripMenuItemHours_Click(object sender, EventArgs e)
            {
                lock (m_tecView.m_lockValue)
                {
                    SaveFileDialog sf = new SaveFileDialog();
                    sf.CheckPathExists = true;
                    //sf.MultiSelect = false;
                    sf.ValidateNames = true;
                    sf.DereferenceLinks = false; // Will return .lnk in shortcuts.
                    sf.DefaultExt = ".xls";
                    sf.Filter = s_DialogMSExcelBrowseFilter;
                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                    }
                    else
                        ;
                }
            }
        }
    }
}