﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;


using StatisticCommon;
using System.Linq;
using System.Data.Common;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    partial class PanelAISKUESOTIASSODay
    {
        /// <summary>
        /// Класс - общий для графического представления значений СОТИАССО на вкладке
        /// </summary>
        private class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZedGraphControl()
                : base()
            {
                initializeComponent();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZedGraphControl(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
            {
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.onPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.onDoubleClickEvent);

                BackColor = SystemColors.Window; // SystemColors.Window

                GraphPane pane = this.GraphPane;
                // Подпишемся на событие, которое будет вызываться при выводе каждой отметки на оси
                pane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(xAxis_OnScaleFormatEvent);
            }
            /// <summary>
            /// Обработчик события - отобразить значения точек
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="pane">Контекст графического представления (полотна)</param>
            /// <param name="curve">Коллекция точек для отображения на полотне</param>
            /// <param name="iPt">Индекс точки в наборе точек для отображения</param>
            /// <returns>Значение для отображения для точки с индексом</returns>
            private string onPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }
            /// <summary>
            /// Обработчик события - двойной "щелчок" мыши
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="e">Вргумент события</param>
            /// <returns>Признак продолжения обработки события</returns>
            private bool onDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }

            /// <summary>
            /// Константа для настройки отображения сетки по оси X
            /// </summary>
            private const int DIV_MAJOR_STEP = 4;

            /// <summary>
            /// Метод, который вызывается, когда надо отобразить очередную метку по оси
            /// </summary>
            /// <param name="pane">Указатель на текущий GraphPane</param>
            /// <param name="axis">Указатель на ось</param>
            /// <param name="val">Значение, которое надо отобразить</param>
            /// <param name="index">Порядковый номер данного отсчета</param>
            /// <returns>Метод должен вернуть строку, которая будет отображаться под данной меткой</returns>
            private string xAxis_OnScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
            {
                //if (index > 0)
                    return string.Format(@"{0}", new DateTime(TimeSpan.FromMinutes(((index + 1) * DIV_MAJOR_STEP) * 30).Ticks).ToString("HH:mm"));
                //else
                //    return string.Empty;
            }

            private Color ColorChart
            {
                get
                {
                    Color clrRes = Color.Empty
                        , clrNotReq = Color.Empty;

                    getColorZEDGraph (out clrRes, out clrNotReq);

                    return clrRes;
                }
            }

            private void getColorZEDGraph (out Color colorChart, out Color colValue)
            {
                getColorZEDGraph (Tag == null ? CONN_SETT_TYPE.DATA_AISKUE : (CONN_SETT_TYPE)Tag, out colorChart, out colValue);
            }

            private void getColorZEDGraph (CONN_SETT_TYPE type, out Color colorChart, out Color colValue)
            {
                FormGraphicsSettings.INDEX_COLOR indxBackGround = FormGraphicsSettings.INDEX_COLOR.COUNT_INDEX_COLOR
                    , indxChart = FormGraphicsSettings.INDEX_COLOR.COUNT_INDEX_COLOR;

                //Значения по умолчанию
                switch (type) {
                    default:
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        indxBackGround = FormGraphicsSettings.INDEX_COLOR.BG_ASKUE;
                        indxChart = FormGraphicsSettings.INDEX_COLOR.ASKUE;
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        indxBackGround = FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO;
                        indxChart = FormGraphicsSettings.INDEX_COLOR.SOTIASSO;
                        break;
                }

                colorChart = FormMain.formGraphicsSettings.COLOR (indxBackGround);
                colValue = FormMain.formGraphicsSettings.COLOR (indxChart);
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

                    if (Equals (GraphPane, null) == false) {
                        GraphPane.Chart.Fill = new ZedGraph.Fill (ColorChart);
                        GraphPane.Fill = new ZedGraph.Fill (BackColor == SystemColors.Control ? SystemColors.Window : BackColor);
                    } else
                        ;
                }
            }

            /// <summary>
            /// Обновить содержание в графической субобласти "сутки по-часам"
            /// </summary>
            public void Draw(IEnumerable<HandlerSignalQueue.VALUE> srcValues
                , string textConnSettType, string textDate)
            {
                double[] values = null;
                int itemscount = -1;
                Color colorChart
                    , colorPCurve;
                double minimum
                    , minimum_scale
                    , maximum
                    , maximum_scale;
                bool noValues = false;

                itemscount = srcValues.Count();

                //names = new string[itemscount];

                values = new double[itemscount];

                minimum = double.MaxValue;
                maximum = double.MinValue;
                noValues = true;

                for (int i = 0; i < itemscount; i++) {
                    //names[i] = string.Format(@"{0}", new DateTime(TimeSpan.FromMinutes((i + 1) * 30).Ticks).ToString("HH:mm"));

                    values[i] = srcValues.ElementAt(i).value < 0 ? -1 * srcValues.ElementAt(i).value : srcValues.ElementAt(i).value;

                    if ((minimum > values[i]) && (!(values[i] == 0))) {
                        minimum = values[i];
                        noValues = false;
                    } else
                        ;

                    if (maximum < values[i])
                        maximum = values[i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues) {
                    minimum_scale = 0;
                    maximum_scale = 10;
                } else {
                    if (minimum != maximum) {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    } else {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                // получить цветовую гамму
                getColorZEDGraph ((CONN_SETT_TYPE)Tag, out colorChart, out colorPCurve);

                GraphPane pane = GraphPane;
                pane.CurveList.Clear();
                pane.Chart.Fill = new Fill(colorChart);
                pane.Fill = new Fill (BackColor);

                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar) {
                    pane.AddBar("Мощность", null, values, colorPCurve);
                } else
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear) {
                    ////Вариант №1
                    //double[] valuesFactLinear = new double[itemscount];
                    //for (int i = 0; i < itemscount; i++)
                    //    valuesFactLinear[i] = valsMins[i];
                    //Вариант №2
                    PointPairList ppl = new PointPairList();
                    for (int i = 0; i < itemscount; i++)
                        if (values[i] > 0)
                            ppl.Add(i, values[i]);
                        else
                            ;
                    //LineItem
                    pane.AddCurve("Мощность"
                        ////Вариант №1
                        //, null, valuesFactLinear
                        //Вариант №2
                        , ppl
                        , colorPCurve);
                } else
                    ;

                //// Ось X будет пересекаться с осью Y на уровне Y = 0
                //pane.XAxis.Cross = 65.0;
                //// Отключим отображение первых и последних меток по осям
                pane.XAxis.Scale.IsSkipFirstLabel = false;
                pane.XAxis.Scale.IsSkipLastLabel = true;
                //// Отключим отображение меток в точке пересечения с другой осью
                //pane.XAxis.Scale.IsSkipCrossLabel = true;
                //// Спрячем заголовки осей
                //pane.XAxis.Title.IsVisible = false;

                //Для размещения в одной позиции ОДНого значения
                pane.BarSettings.Type = BarType.Overlay;

                //...из minutes
                pane.XAxis.Scale.Min = 0.5;
                pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
                pane.XAxis.Scale.MinorStep = 1;
                pane.XAxis.Scale.MajorStep = itemscount /  (itemscount / DIV_MAJOR_STEP);

                pane.XAxis.Type = AxisType.Linear; //...из minutes
                                                   //pane.XAxis.Type = AxisType.Text;
                pane.XAxis.Title.Text = "t, ЧЧ:мм";
                pane.YAxis.Title.Text = "P, кВт";
                pane.Title.Text = textConnSettType;
                pane.Title.Text += new string(' ', 29);
                pane.Title.Text += textDate;

                //pane.XAxis.Scale.TextLabels = names;
                pane.XAxis.Scale.IsPreventLabelOverlap = true;

                // Включаем отображение сетки напротив крупных рисок по оси X
                pane.XAxis.MajorGrid.IsVisible = true;
                // Задаем вид пунктирной линии для крупных рисок по оси X:
                // Длина штрихов равна 10 пикселям, ... 
                pane.XAxis.MajorGrid.DashOn = 10;
                // затем 5 пикселей - пропуск
                pane.XAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.XAxis.MajorGrid.PenWidth = 0.1F;
                pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Включаем отображение сетки напротив крупных рисок по оси Y
                pane.YAxis.MajorGrid.IsVisible = true;
                // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                pane.YAxis.MajorGrid.DashOn = 10;
                pane.YAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.YAxis.MajorGrid.PenWidth = 0.1F;
                pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Включаем отображение сетки напротив мелких рисок по оси Y
                pane.YAxis.MinorGrid.IsVisible = true;
                // Длина штрихов равна одному пикселю, ... 
                pane.YAxis.MinorGrid.DashOn = 1;
                pane.YAxis.MinorGrid.DashOff = 2;
                // толщина линий
                pane.YAxis.MinorGrid.PenWidth = 0.1F;
                pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Устанавливаем интересующий нас интервал по оси Y
                pane.YAxis.Scale.Min = minimum_scale;
                pane.YAxis.Scale.Max = maximum_scale;

                AxisChange();

                Invalidate();
            }

            public void Clear()
            {
                GraphPane.CurveList.Clear();
            }
        }
     }
}