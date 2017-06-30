using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;

using HClassLibrary;
using StatisticCommon;
using System.Linq;
using System.Data.Common;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    partial class PanelSOTIASSODay
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
            /// Обновить содержание в графической субобласти "сутки по-часам"
            /// </summary>
            public void Draw(IEnumerable<HandlerSignalQueue.VALUE> srcValues
                , string textConnSettType, string textDate
                , Color colorChart
                , Color colorPCurve)
            {
                double[] values = null;
                int itemscount = -1;
                string[] names = null;
                double minimum
                    , minimum_scale
                    , maximum
                    , maximum_scale;
                bool noValues = false;

                itemscount = srcValues.Count() - 1;

                names = new string[itemscount];

                values = new double[itemscount];

                minimum = double.MaxValue;
                maximum = 0;
                noValues = true;

                for (int i = 0; i < itemscount; i++) {
                    names[i] = (i + 1).ToString();

                    values[i] = srcValues.ElementAt(i + 1).value;

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

                GraphPane pane = GraphPane;
                pane.CurveList.Clear();
                pane.Chart.Fill = new Fill(colorChart);

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

                //Для размещения в одной позиции ОДНого значения
                pane.BarSettings.Type = BarType.Overlay;

                //...из minutes
                pane.XAxis.Scale.Min = 0.5;
                pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
                pane.XAxis.Scale.MinorStep = 1;
                pane.XAxis.Scale.MajorStep = itemscount / 20;

                pane.XAxis.Type = AxisType.Linear; //...из minutes
                                                   //pane.XAxis.Type = AxisType.Text;
                pane.XAxis.Title.Text = "t, мин";
                pane.YAxis.Title.Text = "P, МВт";
                pane.Title.Text = textConnSettType;
                pane.Title.Text += new string(' ', 29);
                pane.Title.Text += textDate;

                pane.XAxis.Scale.TextLabels = names;
                pane.XAxis.Scale.IsPreventLabelOverlap = false;

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
            }
        }
     }
}