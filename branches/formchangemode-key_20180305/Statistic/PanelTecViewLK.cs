﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using ZedGraph;

//using HClassLibrary;
using StatisticCommon;
using ASUTP;
using ASUTP.Core;

namespace Statistic
{
    /// <summary>
    /// Класс PanelLKView (Вид панели ЛК) наследуется от класса PanelTecViewBase (Вид основной панели ТЭЦ)
    /// </summary>
    class PanelLKView : PanelTecViewBase
    {
        /// <summary>
        /// Конструктор класса "Вид панели ЛК"
        /// </summary>
        /// <param name="tec">ТЭЦ</param>
        /// <param name="key">Ключ компонента</param>
        /// <param name="label">лейбл панели пользовательского вида ТЭЦ</param>
        public PanelLKView(StatisticCommon.TEC tec, FormChangeMode.KeyDevice key, PanelCustomTecView.HLabelCustomTecView label = null)
            // В АИИС КУЭ читаем "мощность", в СОТИАССО - температуру окр.воздуха
            //Вызов конструктора из базового класса PanelTecViewBase, передаем параметры: ТЭЦ, номер ТЭЦ,номер компонента,
            //экземпляр класса HMark (источник данных) с массивом аргументов: администратор, ПБР, данные АИИСКУЭ и СОТИАССО
            : base(tec, key, new HMark(new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO }))
        {
            //Лейбл= null
            m_label = label;
            //Разделитель вертикальных процентов= 30
            SPLITTER_PERCENT_VERTICAL = 30;
            // Массив для хранения пропорций при размещении эл-ов управления 
            m_arPercRows = new int[] { 5, 78 };
            //Инициализировать компоненты контрольных часов
            MainHours.Initialize();

            InitializeComponent ();

            m_dgwHours.EventDataValues += new HDataGridViewBase.DataValuesEventHandler((_pnlQuickData as PanelQuickDataLK).OnPBRDataValues);
        }
        /// <summary>
        /// Метод "Инициализация компонентов" 
        /// </summary>
        protected override void InitializeComponent()
        {
            int[] arProp = new int[] { 0, 1, 0, 1, 0, 1, -1 }; //отобразить часовые таблицу/гистограмму/панель с оперативными данными
            
            base.InitializeComponent();

            if (!(m_label == null))
                //Произвести реструктуризацию
                m_label.PerformRestruct(arProp);
            else
                //Восстановление события
                OnEventRestruct(arProp);
        }
      
        private class TecViewLK : TecView
        {
            /// <summary>
            /// Конструктор TecViewLK 
            /// </summary>
            /// <param name="indx_tec">индекс тэц</param>
            /// <param name="indx_comp">индекс компонента</param>
            public TecViewLK (FormChangeMode.KeyDevice key)
                //Вызов конструктора из базового класса TecView, передаем параметры: индекс тэц, индекс компонента,
                //компонент принадлежащий электрической части тэц
                : base (key, TECComponentBase.TYPE.ELECTRO) 
            {
                m_idAISKUEParNumber = ID_AISKUE_PARNUMBER.FACT_30;
                _tsOffsetToMoscow = HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
            }
            /// <summary>
            /// Метод "Изменение состояния" 
            /// </summary>
            public override void ChangeState()
            {
                lock (m_lockState) { GetRDGValues(FormChangeMode.KeyDeviceEmpty, DateTime.MinValue); }

                base.ChangeState(); //Run
            }

            public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
            {
                ClearStates();

                //ClearValues();

                using_date = false;

                if (m_tec.GetReadySensorsStrings(key.Mode) == true)
                    if (currHour == true)
                        AddState((int)StatesMachine.CurrentTimeView);
                    else
                        ;
                else
                {
                    AddState((int)StatesMachine.InitSensors);
                    AddState((int)StatesMachine.CurrentTimeView);
                }

                AddState((int)TecView.StatesMachine.Hours_Fact);
                AddState((int)TecView.StatesMachine.CurrentMins_Fact);
                AddState((int)TecView.StatesMachine.HoursTMTemperatureValues);
                AddState((int)TecView.StatesMachine.PPBRValues);
                AddState((int)TecView.StatesMachine.AdminValues);
            }
            /// <summary>
            /// Метод получения ретро-значений
            /// </summary>
            public override void GetRetroValues()
            {
                //lock гарантирует, что указанный блок кода, защищенный блокировкой для данного объекта, может быть использован только потоком, 
                //который получает эту блокировку. Все другие потоки остаются заблокированными до тех пор, пока
                //блокировка не будет снята. А снята она будет лишь при выходе из этого блока
                lock (m_lockValue)
                {
                    ClearValues();

                    ClearStates();

                    adminValuesReceived = false;

                    //Часы...
                    AddState((int)StatesMachine.Hours_Fact);

                    //Минуты...
                    AddState((int)StatesMachine.RetroMins_Fact);
                    AddState((int)StatesMachine.HoursTMTemperatureValues);

                    AddState((int)StatesMachine.PPBRValues);
                    AddState((int)StatesMachine.AdminValues);

                    Run(@"TecView::GetRetroValues ()");
                }
            }

            /// <summary>
            /// Метод "Возвратить сумму фактических значений для всех ТГ"
            /// </summary>
            /// <param name="hour">Номер часа за который расчитывается сумма</param>
            /// <returns>Результат суммирования</returns>
            public override double GetSummaFactValues(int hour)
            {
                double dblRes = -1F;
                double[] arTGRes = null;
                uint[] arTGcounter = null;
                int id = -1
                    , min = -1
                    , iter = IntervalMultiplier
                    , cntInterval = -1; ;

                arTGRes = new double[ListLowPointDev.Count];
                arTGcounter = new uint[ListLowPointDev.Count];
                cntInterval = 60 / 3;

                for (int t = 0; t < ListLowPointDev.Count; t++)
                {
                    id = ListLowPointDev[t].m_id;

                    arTGRes[t] = -1F;
                    arTGcounter[t] = 0;

                    for (min = 1; min < cntInterval; min += iter)
                        if (!(m_dictValuesLowPointDev[id].m_powerMinutes[min] < 0))
                        {
                            if (arTGRes[t] < 0F)
                                arTGRes[t] = 0F;
                            else
                                ;

                            arTGRes[t] += m_dictValuesLowPointDev[id].m_powerMinutes[min];
                            arTGcounter[t]++;
                        }
                        else
                            ;

                    if (!(arTGRes[t] < 0))
                    {
                        if (dblRes < 0F)
                            dblRes = 0;
                        else
                            ;

                        arTGRes[t] /= arTGcounter[t];
                        dblRes += arTGRes[t];
                    }
                    else
                        ;
                }

                return dblRes;
            }
        }
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelQuickDataLK : HPanelQuickData
        {
            /// <summary>
            /// Конструктор "Быстрые данные панели ЛК"
            /// </summary>
            public PanelQuickDataLK(Color foreColor, Color backColor)
                : base (foreColor, backColor)
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                //Количество виртуальных строк, на которые  разбита панель
                COUNT_ROWS = 3;
                SZ_COLUMN_LABEL = 58F;
                // Текущая температура
                m_indxStartCommonFirstValueSeries = (int)CONTROLS.lblTemperatureCurrent;
                // Текущая мощность
                m_indxStartCommonSecondValueSeries = (int)CONTROLS.lblPowerCurrent;
                m_iCountCommonLabels = (int)CONTROLS.lblPowerDateValue - (int)CONTROLS.lblTemperatureCurrent + 1;

                // Количество и параметры строк макета панели
                this.RowCount = COUNT_ROWS;
                for (int i = 0; i < this.RowCount + 1; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

                this.m_arLabelCommon = new System.Windows.Forms.Label[m_iCountCommonLabels];

                //
                // btnSetNow
                // Кнопка "Текущий час"
                this.Controls.Add(this.btnSetNow, 0, 0);
                // 
                // dtprDate
                // Дата
                this.Controls.Add(this.dtprDate, 0, 1);
                // 
                // lblServerTime
                // Время на сервере
                this.Controls.Add(this.lblServerTime, 0, 2);

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
                        //лейбл для t тек
                        case CONTROLS.lblTemperatureCurrent:
                        //лейбл для t пр/ч
                        case CONTROLS.lblTemperatureHour:
                        //лейбл для t пр/сут
                        case CONTROLS.lblTemperatureDate:
                            // цвет заднего плана пустой
                            backClolor = Color.Empty;
                            // цвет переднего плана черный
                            foreColor = Color.Black;
                            szFont = 8F;
                            //Выравнивание надписи слева по центру
                            align = ContentAlignment.MiddleLeft;
                            break;
                        //значение t тек
                        case CONTROLS.lblTemperatureCurrentValue:
                        // значение t пр/ч
                        case CONTROLS.lblTemperatureHourValue:
                        // значение пр/сут
                        case CONTROLS.lblTemperatureDateValue:
                            backClolor = Color.Black;
                            foreColor = Color.LimeGreen;
                            szFont = 15F;
                            //Выравнивание значений по середине по центру
                            align = ContentAlignment.MiddleCenter;
                            break;
                            //По умолчанию 
                        default:
                            backClolor = Color.Red;
                            foreColor = Color.Yellow;
                            szFont = 6F;
                            align = ContentAlignment.MiddleCenter;
                            break;
                    }

                    switch (i)
                    {    // Создание текста для надписи
                        case CONTROLS.lblTemperatureCurrent:
                            text = @"t тек"; //@"P тек";
                            break;
                        case CONTROLS.lblTemperatureHour:
                            text = @"t пр/ч";
                            break;
                        case CONTROLS.lblTemperatureDate:
                            text = @"t пр/сут";
                            break;
                        default:
                            text = string.Empty;
                            break;
                    }

                    createLabel((int)i, text, foreColor, backClolor, szFont, align);
                }
                #endregion

                #region добавить поля для значений МОЩНОСТИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblPowerDateValue + 1; i++)
                {
                    switch (i)
                    {
                        //Лейбл для P час
                        case CONTROLS.lblPowerCurrent:
                        //Лейбл для P пл/ч
                        case CONTROLS.lblPowerHour:
                        //Лейбл для P сети
                        case CONTROLS.lblPowerDate:
                            // Цвет переднего плана черный, заднего пустой
                            backClolor = Color.Empty;
                            foreColor = Color.Black;
                            szFont = 8F;
                            //Выравнивание по середине слева
                            align = ContentAlignment.MiddleLeft;
                            break;
                            //Значения
                        case CONTROLS.lblPowerCurrentValue:
                        case CONTROLS.lblPowerHourValue:
                        case CONTROLS.lblPowerDateValue:
                            //Цвета значений
                            backClolor = Color.Black;
                            foreColor = Color.LimeGreen;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            break;
                            //По умолчанию
                        default:
                            backClolor = Color.Red;
                            foreColor = Color.Yellow;
                            szFont = 6F;
                            align = ContentAlignment.MiddleCenter;
                            break;
                    }

                    switch (i)
                    {
                        // Создание текста для надписи
                        case CONTROLS.lblPowerCurrent:
                            text = @"P час";
                            break;
                        case CONTROLS.lblPowerHour:
                            text = @"P пл/ч";
                            break;
                        case CONTROLS.lblPowerDate:
                            text = @"P сети";
                            break;
                        default:
                            text = string.Empty;
                            break;
                    }

                    createLabel((int)i, text, foreColor, backClolor, szFont, align);
                }
                #endregion
            }
            /// <summary>
            ///Перечисление контролируемых величин
            /// </summary>
            public enum CONTROLS : short
            {
                unknown = -1
                , lblTemperatureCurrent, lblTemperatureCurrentValue
                , lblTemperatureHour, lblTemperatureHourValue
                , lblTemperatureDate, lblTemperatureDateValue
                , lblPowerCurrent, lblPowerCurrentValue
                , lblPowerHour, lblPowerHourValue
                , lblPowerDate, lblPowerDateValue
                , dtprDate
                , lblServerTime
                , btnSetNow
                    , COUNT_CONTROLS
            }

            //protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            //{
            //    throw new NotImplementedException();
            //}
            /// <summary>
            /// Метод "Расположение  в ячейках таблицы"
            /// </summary>
            /// <param name="indx">Индекс ячейки в layout</param>
            /// <returns>Объект-адрес размещения</returns>
            protected override TableLayoutPanelCellPosition getPositionCell(int indx)
            {   //ряд
                int row = -1,
                // колонки
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
                        row = 1; col = 1;
                        break;
                    case CONTROLS.lblTemperatureHourValue:
                        row = 1; col = 2;
                        break;
                    case CONTROLS.lblTemperatureDate:
                        row = 2; col = 1;
                        break;
                    case CONTROLS.lblTemperatureDateValue:
                        row = 2; col = 2;
                        break;
                    case CONTROLS.lblPowerCurrent:
                        row = 0; col = 3;
                        break;
                    case CONTROLS.lblPowerCurrentValue:
                        row = 0; col = 4;
                        break;
                    case CONTROLS.lblPowerHour:
                        row = 1; col = 3;
                        break;
                    case CONTROLS.lblPowerHourValue:
                        row = 1; col = 4;
                        break;
                    case CONTROLS.lblPowerDate:
                        row = 2; col = 3;
                        break;
                    case CONTROLS.lblPowerDateValue:
                        row = 2; col = 4;
                        break;
                    default:
                        break;
                }

                return new TableLayoutPanelCellPosition(col, row);
            }

            public override void RestructControl()
            {
                COUNT_LABEL = 2; COUNT_TG_IN_COLUMN = 3; COL_TG_START = 5;
                COUNT_ROWSPAN_LABELCOMMON = 1;

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
                    //this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROW_LABELCOMMON);
                }

                //Ширина столбцов группы "Температура"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                #endregion

                #region Добавить столбцы группы "Мощность"
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblPowerDateValue + 1; i++)
                {
                    //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                    this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                    this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                    //this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROW_LABELCOMMON);
                }

                //Ширина столбцов группы "Мощность"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                #endregion

                cntCols = ((m_tgLabels.Count / COUNT_TG_IN_COLUMN) + ((m_tgLabels.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));

                for (int i = 0; i < cntCols; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                }

                if (m_tgLabels.Count > 0)
                    addTGLabels(false);
                else
                    ;

                this.Controls.Add(m_panelEmpty, COL_TG_START + cntCols * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
                //Задать диапазон строк
                this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            //private Color clrLabel { get { return m_parent.m_tecView.currHour == true ? Color.LimeGreen : Color.OrangeRed; } }

            public void OnPBRDataValues(HDataGridViewBase.DataValuesEventArgs ev)
            {
                // температура
                showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureDateValue - m_indxStartCommonFirstValueSeries]
                    , (double)ev.m_value1 //> 0 ? (double)val : double.NegativeInfinity
                    , 2 //round
                    , false
                    , true
                    , string.Empty);
                // мощность
                showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerDateValue - m_indxStartCommonFirstValueSeries]
                    , (double)ev.m_value2 > 0 ? (double)ev.m_value2 : double.NegativeInfinity
                    , 2 //round
                    , false
                    , true
                    , string.Empty);

                m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureDateValue - m_indxStartCommonFirstValueSeries].ForeColor =
                m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerDateValue - m_indxStartCommonFirstValueSeries].ForeColor =
                    getColorValues (TG.INDEX_VALUE.FACT);
            }

            private void showFactTGValue(int id_tg, double[] powerLastHourMinutes)
            {
                int min = -1
                    , cntMinValues = 0;
                double powerLastHour = 0F;

                for (min = 1; min < powerLastHourMinutes.Length; min += 10)
                    //Проверить возможность получения значения
                    if (! (powerLastHourMinutes[min] < 0)) {
                        powerLastHour += powerLastHourMinutes[min];

                        cntMinValues++;
                    }
                    else
                        ;

                if (cntMinValues > 0)
                    //Отобразить значение
                    showValue(m_tgLabels[id_tg][(int)TG.INDEX_VALUE.FACT]
                        , powerLastHour * 1000 / cntMinValues, 2, false);
                else
                    //Отобразить строку - отсутствие значения
                    m_tgLabels[id_tg][(int)TG.INDEX_VALUE.FACT].Text = "--.--";

                // Установить цвет шрифта для значения
                m_tgLabels[id_tg][(int)TG.INDEX_VALUE.FACT].ForeColor =
                    //clrLabel
                    getColorValues(TG.INDEX_VALUE.FACT)
                        ;
            }

            public override void ShowFactValues()
            {
                int indxStartCommonPVal = m_indxStartCommonFirstValueSeries
                    , lastHour = m_parent.m_tecView.lastHour //m_parent.m_tecView.currHour == true ? m_parent.m_tecView.lastHour + 1 : m_parent.m_tecView.lastHour;
                    ;
                double[] powerLastHourMinutes;
                double powerLastHour = -1F;

                if (m_parent.m_tecView.IsHourValues(lastHour) == true)
                {
                    //Температура
                    // текущее значение температуры (час)
                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesLastMinutesTM
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // плановое значение температуры (час)
                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblTemperatureHourValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesPmin
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // плановое значение температуры (сутки) - отображается при обработке события 'DataGridViewLKHours::EventTemperaturePBRDay'
                    
                    //Мощность
                    // текущее значение мощности (час)
                    powerLastHour = m_parent.m_tecView.GetSummaFactValues(lastHour);
                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblPowerCurrentValue - indxStartCommonPVal]
                        , powerLastHour < 0 ? double.NegativeInfinity : powerLastHour * 1000
                        , 2 //round
                        , false
                        , true
                        , powerLastHour < 0 ? @"---" : string.Empty);
                    // плановое значение мощности (час)
                    showValue(ref m_arLabelCommon[(int)CONTROLS.lblPowerHourValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesPBR > 0 ? m_parent.m_tecView.m_valuesHours[lastHour].valuesPBR : double.NegativeInfinity
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // цвет шрифта для значений температуры, мощности
                    m_arLabelCommon[(int)CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)CONTROLS.lblTemperatureHourValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)CONTROLS.lblTemperatureDateValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)CONTROLS.lblPowerCurrentValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)CONTROLS.lblPowerHourValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)CONTROLS.lblPowerDateValue - indxStartCommonPVal].ForeColor =
                        getColorValues(TG.INDEX_VALUE.FACT);
                    // текущее значение мощности для компонентов-ТГ-фидеров (час)
                    //ShowTGValue
                    if (m_parent.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC) // значит этот view будет суммарным для всех ГТП
                    {
                        foreach (TECComponent g in m_parent.m_tecView.LocalTECComponents)
                            if (g.IsGTP == true)
                            //Только ГТП
                                foreach (TG tg in g.ListLowPointDev) {
                                //Цикл по списку с ТГ
                                    powerLastHourMinutes = m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerMinutes;
                                    //Проверить возможность получения значения
                                    if (!(powerLastHourMinutes == null))
                                        showFactTGValue(tg.m_id, powerLastHourMinutes);
                                    else
                                        ;
                                }
                            else
                                ;
                    }
                    else
                        foreach (TECComponent comp in m_parent.m_tecView.LocalTECComponents) {
                        //Цикл по списку ТГ в компоненте ТЭЦ (ГТП, ЩУ)
                            powerLastHourMinutes = m_parent.m_tecView.m_dictValuesLowPointDev[comp.ListLowPointDev[0].m_id].m_powerMinutes;
                            //Проверить возможность получения значения
                            if (!(powerLastHourMinutes == null))
                                showFactTGValue (comp.ListLowPointDev[0].m_id, powerLastHourMinutes);
                            else
                                ;
                        }
                }
                else
                    ;
            }

            /// <summary>
            /// Отобразить текущие значения
            /// </summary>
            public override void ShowTMValues()
            {
            }
        }

        /// <summary>
        /// Класс для отображения значений в табличном виде
        ///  в разрезе час-минуты для ГТП
        /// </summary>
        public class DataGridViewLKHours : HDataGridViewBase
        {
            /// <summary>
            ///Перечисление колонок таблицы
            /// </summary>
            private enum INDEX_COLUMNS : short { PART_TIME, TEMPERATURE_FACT, POWER_FACT_SUM, TEMPERATURE_PBR, POWER_PBR, TEMPERATURE_DEVIATION, POWER_DEVIATION
                , COUNT_COLUMN }

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewLKHours()
                //: base(new int[] { 8, 15, 15, 15, 15, 15, 15 })
                : base(
                    HDateTime.INTERVAL.HOURS
                      //Название колонок таблицы
                    , new ColumnProperies[] { new ColumnProperies (27, 10, @"Час", @"Hour")
                    , new ColumnProperies (47, 15, @"t час", @"TemperatureFact")
                    , new ColumnProperies (47, 15, @"P час", @"PowerFactSum")
                    , new ColumnProperies (47, 15, @"t пр/ч", @"TemperaturePBR")
                    , new ColumnProperies (47, 15, @"P пл/ч", @"PowerPBR")
                    , new ColumnProperies (42, 15, @"t +/-", @"TemperatureDevHour")
                    , new ColumnProperies (42, 15, @"P +/-", @"PowerDevHour")
            }, true)
            {
                InitializeComponents();

                Name = "m_dgwTableHours";
                RowHeadersVisible = false;
                RowTemplate.Resizable = DataGridViewTriState.False;

                RowsAdd();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewLKHours(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления 
            /// </summary>
            private void InitializeComponents()
            {
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
                        Rows[i].Cells[c].Value = 0.ToString("F2");
                }

                Rows[count].Cells[0].Value = "Сумма";
                for (c = 1; c < m_arColumns.Length; c++)
                    switch ((INDEX_COLUMNS)c)
                    {
                        case INDEX_COLUMNS.TEMPERATURE_FACT:
                        case INDEX_COLUMNS.POWER_FACT_SUM:
                        case INDEX_COLUMNS.TEMPERATURE_DEVIATION:
                        case INDEX_COLUMNS.POWER_DEVIATION:
                            Rows[i].Cells[c].Value = @"-".ToString();
                            break;
                        default:
                            Rows[i].Cells[c].Value = 0.ToString("F2");
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
                        serverTime.Date.Equals(DateTime.Now.Date) == true ? lastReceivedHour - 1 : //??? '.Now' не работает, если часовой пояс не НСК (например, МСК - на сервере)
                            itemscount;
                double t_pbr = -1F, p_pbr = -1F;
                string strWarn = string.Empty;

                //Debug.WriteLine(@"DataGridViewLKHours::Fill () - serverTime=" + serverTime.ToString()
                //    + @"; lastHour=" + lastHour
                //    + @"; lastReceivedHour=" + lastReceivedHour);

                DataGridViewCellStyle curCellStyle;

                // Создание обычных (regularHour) и контрольных часов (mainHour)  В ТАБЛИЦЕ
                DataGridViewCellStyle regularHourCellStyle = new DataGridViewCellStyle()
                    , mainHourCellStyle = new DataGridViewCellStyle();

                regularHourCellStyle.BackColor = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.ASKUE_LK_REGULAR);
                mainHourCellStyle.BackColor = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.ASKUE);
                //// полужирный на основе 1-ой ячейки
                //mainHourCellStyle.Font = new System.Drawing.Font(RowsDefaultCellStyle.Font, FontStyle.Bold);

                //cntWarn = 0;
                t_pbr = 0;
                for (i = 0; i < itemscount; i++)
                {
                    // Текущий стиль ячейки
                    curCellStyle = (MainHours.IsMain(serverTime, i+ 1) == true) ? mainHourCellStyle :
                        regularHourCellStyle;
                    //Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Style = curCellStyle; // стиль определен для всей строки
                    Rows[i].DefaultCellStyle = curCellStyle;
                    // факт
                    if (!(i > lh)) {
                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_FACT].Value = (values[i].valuesLastMinutesTM).ToString(@"F2"); // температура
                        //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_FACT].Style = curCellStyle; // стиль определен для всей строки
                        Rows[i].Cells[(int)INDEX_COLUMNS.POWER_FACT_SUM].Value = (values[i].valuesFact * 1000).ToString(@"F2"); // мощность
                        //Rows[i].Cells[(int)INDEX_COLUMNS.POWER_FACT_SUM].Style = curCellStyle; // стиль определен для всей строки
                    } else ;
                    // план
                    Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_PBR].Value = (values[i].valuesPmin).ToString(@"F2"); // температура
                    //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_PBR].Style = curCellStyle; // стиль определен для всей строки
                    t_pbr += values[i].valuesPmin;
                    Rows[i].Cells[(int)INDEX_COLUMNS.POWER_PBR].Value = (values[i].valuesPBR).ToString(@"F2"); // мощность
                    //Rows[i].Cells[(int)INDEX_COLUMNS.POWER_PBR].Style = curCellStyle; // стиль определен для всей строки
                    // мощность сети (максимальная из загруженных)
                    if ((values[i].valuesPBR > 0)
                        && (p_pbr < values[i].valuesPBR))
                        p_pbr = values[i].valuesPBR;
                    else
                        ;
                    // Разность
                    if (!(i > lh))
                    {
                        // - Температура
                        if ((!(values[i].valuesLastMinutesTM == 0))
                            || (!(values[i].valuesPmin == 0)))
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = (values[i].valuesPmin - values[i].valuesLastMinutesTM).ToString(@"F2");
                        else
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = @"-";
                        // - Мощность
                        if ((values[i].valuesFact > 0)
                            && (values[i].valuesPBR > 0))
                            Rows[i].Cells[(int)INDEX_COLUMNS.POWER_DEVIATION].Value = (values[i].valuesPBR - values[i].valuesFact * 1000).ToString(@"F2");
                        else
                            Rows[i].Cells[(int)INDEX_COLUMNS.POWER_DEVIATION].Value = @"-";
                    }
                    else
                    {
                        Rows[i].Cells[(int)INDEX_COLUMNS.POWER_DEVIATION].Value =
                        Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value =
                            @"-";
                    }
                    //Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Style = curCellStyle; // стиль определен для всей строки
                    //Rows[i].Cells[(int)INDEX_COLUMNS.POWER_DEVIATION].Style = curCellStyle; // стиль определен для всей строки
                }

                t_pbr /= itemscount;
                t_pbr = Math.Round(t_pbr, 0);
                // план
                Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_PBR].Value = t_pbr.ToString(@"F2"); // температура
                PerformDataValues(new DataValuesEventArgs() { m_value1 = t_pbr, m_value2 = p_pbr });
            }
        }
        private class MainHours
        {
            //Диапазон  часов
            private struct RANGE_HOURS
            {
                public int Begin;
                public int End;
            }

            public static void Initialize()
            {
                if (!(_dictValues == null))
                {
                    _dictValues.Clear();
                    _dictValues = null;
                }
                else
                    ;

                _dictValues = new Dictionary<int, List<RANGE_HOURS>>();

                List <RANGE_HOURS> listRanges;
                for (int m = 0; m < 12; m++)
                {
                    switch (m + 1)
                    {
                        case 1:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 16, End = 21 } });
                            break;
                        case 2:
                        case 3:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 17, End = 21 } });
                            break;
                        case 4:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 18, End = 21 } });
                            break;
                        case 5:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 13 }, new RANGE_HOURS() { Begin = 19, End = 21 } });
                            break;
                        case 6:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 17 }, new RANGE_HOURS() { Begin = 19, End = 21 } });
                            break;
                        case 7:
                        case 8:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 9, End = 21 }});
                            break;
                        case 9:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 16, End = 21 } });
                            break;
                        case 10:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 11 }, new RANGE_HOURS() { Begin = 16, End = 21 } });
                            break;
                        case 11:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 11 }, new RANGE_HOURS() { Begin = 15, End = 21 } });
                            break;
                        case 12:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 15, End = 21 } });
                            break;
                        default:
                            throw new Exception (@"MainHours::Initialize () - неизвестный номер месяца...");
                            break;
                    }

                    _dictValues.Add(m + 1, listRanges);
                }
            }

            private static Dictionary<int, List<RANGE_HOURS>> _dictValues = new Dictionary<int, List<RANGE_HOURS>>();

            public static bool IsMain(DateTime curDate, int hour)
            {
                bool bRes = false;

                if ((!(curDate.DayOfWeek == DayOfWeek.Saturday))
                    && (!(curDate.DayOfWeek == DayOfWeek.Sunday)))
                {
                    foreach (RANGE_HOURS range in _dictValues[curDate.Month])
                        if ((!(hour < range.Begin))
                            && (!(hour > range.End)))
                        {
                            bRes = true;

                            break;
                        }
                        else
                            ;
                }
                else
                    ;

                return bRes;
            }
  
            public static int GetCountMainInterval(DateTime curDate)
            {
                int iRes = 0;

                if ((!(curDate.DayOfWeek == DayOfWeek.Saturday))
                    && (!(curDate.DayOfWeek == DayOfWeek.Sunday)))
                    iRes = _dictValues[curDate.Month].Count;
                else
                    ;

                return iRes;
            }

            public static int GetIndexItemInterval(DateTime curDate, int hour)
            {
                int iRes = -1;

                if ((!(curDate.DayOfWeek == DayOfWeek.Saturday))
                    && (!(curDate.DayOfWeek == DayOfWeek.Sunday)))
                    foreach (RANGE_HOURS range in _dictValues[curDate.Month])
                        if ((!(hour < range.Begin))
                            && (!(hour > range.End)))
                        {
                            iRes = _dictValues[curDate.Month].IndexOf(range);

                            break;
                        }
                        else
                            ;
                else
                    ;

                return iRes;
            }
        }
        /// <summary>
        /// Класс "Отрисовка гистограммы панели ЛК"
        /// </summary>
        private class ZedGraphControlLK : HZedGraphControl
        {
            /// <summary>
            /// Конструктор ZedGraphControlLK принимает аргумент типа object
            /// </summary>
            /// <param name="lockVal">объект для обеспечения синхронизации при обращении к отображаемым данным</param>
            public ZedGraphControlLK(object lockVal)
                //Вызов конструктора из базового класса HZedGraphControl
                : base(lockVal, FormMain.formGraphicsSettings.SetScale)
            {
                InitializeComponent();
            }

            public override void Draw(TecView.valuesTEC []values, params object[] pars)
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
                    , iMainIntervalCount = MainHours.GetCountMainInterval (serverTime.Date) // кол-во главных интервалов (со значенями ПБР)
                    , i = -1, h = -1
                    , indxItemMain = -1, indxHourMain = -1 // индекс главного интервала, индекс часа в главном интервале
                    , iMainIntervalChanged = -1; // признак смены типа интервала главный-обычный/обычный-главный
                double y = -1F; // значение ПБР

                string[] names = new string[itemscount];

                //Список пар точек
                PointPairList[] valuesMainFact = null //new double[itemscount]
                    , valuesRegularFact = null //new double[itemscount]
                    ;
                PointPairList [] valuesPlan = null //new PointPairList () //new double[itemscount]
                    , valuesPDiviation = null //new PointPairList () //new double[itemscount]
                    , valuesODiviation = null //new PointPairList () //new double[itemscount]
                    ;

                // Выделить память для
                // регулярных часов - безусловно
                valuesRegularFact = new PointPairList[iMainIntervalCount + 1];
                //Для каждого регулярного часа создать пару точек 
                for (i = 0; i < valuesRegularFact.Length; i++)
                    valuesRegularFact[i] = new PointPairList();
                // Для остальных при необходимости
                if (iMainIntervalCount > 0)
                {
                    valuesMainFact = new PointPairList[iMainIntervalCount];
                    //Для каждого контрольного часа создать пару точек 
                    for (i = 0; i < valuesMainFact.Length; i++)
                        valuesMainFact[i] = new PointPairList();
                    
                    valuesPlan = new PointPairList[iMainIntervalCount];
                    valuesPDiviation = new PointPairList[iMainIntervalCount];
                    valuesODiviation = new PointPairList[iMainIntervalCount];

                    for (i = 0; i < iMainIntervalCount; i++)
                    {
                        valuesPlan[i] = new PointPairList();
                        valuesPDiviation[i] = new PointPairList();
                        valuesODiviation[i] = new PointPairList();
                    }
                }
                else
                    ;

                double minimum = double.MaxValue, minimum_scale;
                double maximum = 0, maximum_scale;
                bool noValues = true;
                for (i = 0, h = 1, indxItemMain = 0, indxHourMain = -1, iMainIntervalChanged = 0; i < itemscount; i++, h++)
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
                    // Плановые значения указываются только для часов по графику
                    if (values[i].valuesPBR > 0)
                    {
                        indxHourMain++;
                        y = values[i].valuesPBR;

                        indxItemMain = MainHours.GetIndexItemInterval (serverTime.Date, h);
                        if (!(indxItemMain < 0))
                        {
                            valuesPlan[indxItemMain].Add(h, y);
                            valuesPDiviation[indxItemMain].Add(h, y + values[i].valuesDiviation);
                            valuesODiviation[indxItemMain].Add(h, y - values[i].valuesDiviation);
                        }
                        else
                            Logging.Logg().Error(@"ZedGraphControlLK::Draw () - hour=" + h, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                        indxHourMain = -1;

                    y = values[i].valuesFact * 1000;
                    if (MainHours.IsMain(serverTime.Date, h) == true)
                    {
                        if (iMainIntervalChanged % 2 == 0)
                            iMainIntervalChanged++;
                        else
                            ;

                        valuesMainFact[(int)(iMainIntervalChanged / 2)].Add(h, y);
                    }
                    else
                    {
                        if (iMainIntervalChanged % 2 == 1)
                            iMainIntervalChanged++;
                        else
                            ;

                        valuesRegularFact[(int)(iMainIntervalChanged / 2)].Add(h, y);
                    }

                    if (values[i].valuesPBR > 0)
                    {
                        if ((!(indxItemMain < 0))
                            && (indxItemMain < iMainIntervalCount))
                        {
                            if (minimum > valuesPDiviation[indxItemMain][indxHourMain].Y)
                            {
                                minimum = valuesPDiviation[indxItemMain][indxHourMain].Y;
                                noValues = false;
                            }
                            else
                                ;

                            if (minimum > valuesODiviation[indxItemMain][indxHourMain].Y)
                            {
                                minimum = valuesODiviation[indxItemMain][indxHourMain].Y;
                                noValues = false;
                            }
                            else
                                ;

                            if (minimum > valuesPlan[indxItemMain][indxHourMain].Y)
                            {
                                minimum = valuesPlan[indxItemMain][indxHourMain].Y;
                                noValues = false;
                            }
                            else
                                ;
                        }
                        else
                        {
                            Logging.Logg().Error(@"ZedGraphControlLK::Draw () - план мощности не совпадает с графиком контрольных часов...", Logging.INDEX_MESSAGE.NOT_SET);

                            break;
                        }
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

                    if (values[i].valuesPBR > 0)
                    {
                        if (maximum < valuesPDiviation[indxItemMain][indxHourMain].Y)
                            maximum = valuesPDiviation[indxItemMain][indxHourMain].Y;
                        else
                            ;

                        if (maximum < valuesODiviation[indxItemMain][indxHourMain].Y)
                            maximum = valuesODiviation[indxItemMain][indxHourMain].Y;
                        else
                            ;

                        if (maximum < valuesPlan[indxItemMain][indxHourMain].Y)
                            maximum = valuesPlan[indxItemMain][indxHourMain].Y;
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
                    // Цвет диаграммы= пусто
                Color colorChart = Color.Empty
                    //Цвет контрольной кривой
                    , colorPMainCurve = Color.Empty
                    //Цвет обычной кривой
              , colorPRegularCurve = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.ASKUE_LK_REGULAR);
               
                //Получить цвет гистограммы
                getColorZedGraph(typeConnSett, out colorChart, out colorPMainCurve);

                GraphPane.Chart.Fill = new Fill(colorChart);
                //GraphPane.Fill = new Fill (BackColor);

                //LineItem - план/отклонения
                //Надпись кривой "P план"
                string strCurveNamePPlan = "P план"
                    //Надпись кривой "Возможное отклонение"
                    , strCurveNameDeviation = "Возможное отклонение";
                for (i = 0; i < iMainIntervalCount; i++)
                {
                    //Цвета кривых УДГ, Отклонение
                    GraphPane.AddCurve(strCurveNamePPlan, /*null,*/ valuesPlan[i], FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.UDG));
                    //LineItem
                    GraphPane.AddCurve(string.Empty, /*null,*/ valuesODiviation[i], HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor);
                    //LineItem
                    GraphPane.AddCurve(strCurveNameDeviation, /*null,*/ valuesPDiviation[i], HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor);
                    //Чтобы повторно не добавить подпись в легенду
                    strCurveNamePPlan =
                    strCurveNameDeviation =
                        string.Empty;
                }
                //Подпись гистограммы
                string strCurveNameMain = "Мощность(контр.)"
                    , strCurveNameReg = "Мощность(рег.)";
                //Если тип графика -гистограмма
                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                {
                    if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    {//BarItem
                        if (!(valuesMainFact == null))
                            for (i = 0; i < valuesMainFact.Length; i++)
                            {
                                //Область графика, добавить столбик
                                GraphPane.AddBar(strCurveNameMain, valuesMainFact[i], colorPMainCurve);
                                // чтобы повторно не добавить подпись в легенду
                                strCurveNameMain =
                                    string.Empty;
                            }
                        else
                            ; // отображать контольные часы не требуется

                        for (i = 0; i < valuesRegularFact.Length; i++)
                        {
                            GraphPane.AddBar(strCurveNameReg, valuesRegularFact[i], colorPRegularCurve);
                            // чтобы повторно не добавить подпись в легенду
                            strCurveNameReg =
                                string.Empty;
                        }
                    }
                    else
                        // других типов данных для ЛК не предусмотрено
                        ;
                }
                else
                //Если тип графика линейный
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                    {
                        if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        {//LineItem
                            if (!(valuesMainFact == null))
                                for (i = 0; i < valuesMainFact.Length; i++)
                                {
                                    GraphPane.AddCurve(strCurveNameMain, valuesMainFact[i], colorPMainCurve);
                                    // чтобы повторно не добавить подпись в легенду
                                    strCurveNameMain =
                                        string.Empty;
                                }
                            else
                                ; // отображать контольные часы не требуется

                            for (i = 0; i < valuesRegularFact.Length; i++)
                            {
                                GraphPane.AddCurve(strCurveNameReg, valuesRegularFact[i], colorPRegularCurve);
                                // чтобы повторно не добавить подпись в легенду
                                strCurveNameReg =
                                    string.Empty;
                            }
                        }
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
                GraphPane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив крупных рисок по оси Y
                GraphPane.YAxis.MajorGrid.IsVisible = true;
                // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                GraphPane.YAxis.MajorGrid.DashOn = 10;
                GraphPane.YAxis.MajorGrid.DashOff = 5;
                // толщина линий
                GraphPane.YAxis.MajorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив мелких рисок по оси Y
                GraphPane.YAxis.MinorGrid.IsVisible = true;
                // Длина штрихов равна одному пикселю, ... 
                GraphPane.YAxis.MinorGrid.DashOn = 1;
                GraphPane.YAxis.MinorGrid.DashOff = 2;
                // толщина линий
                GraphPane.YAxis.MinorGrid.PenWidth = 0.1F;
                GraphPane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Устанавливаем интересующий нас интервал по оси Y
                GraphPane.YAxis.Scale.Min = minimum_scale;
                GraphPane.YAxis.Scale.Max = maximum_scale;

                AxisChange();

                Invalidate();
            }

            //protected override string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            //{
            //    return curve[iPt].Y.ToString("F3");
            //}

            public override bool FindNearestObject(PointF p, Graphics g, out object obj, out int index)
            {
                bool bRes = false;

                bRes = base.FindNearestObject(p, g, out obj, out index);

                if ((!(obj == null))
                    && ((obj is CurveItem) == true))
                    //if ((obj as CurveItem).IsLine == true)
                        index = (int)(obj as CurveItem).Points[index].X - 1;
                    //else ;
                else
                    ;

                return bRes;
            }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"АИСКУЭ";
                //// только для минут
                //this.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            }
        }

        //public override void SetDelegateReport(DelegateStringFunc fErr, DelegateStringFunc fWar, DelegateStringFunc fAct, DelegateBoolFunc fClear)
        //{
        //    m_tecView.SetDelegateReport(fErr, fWar, fAct, fClear);
        //}

        protected override void createTecView(FormChangeMode.KeyDevice key)
        {
            m_tecView = new TecViewLK(key);
        }

        protected override void createDataGridViewMins() { } // заглушка

        protected override void createDataGridViewHours()
        {
            this.m_dgwHours = new DataGridViewLKHours();
        }

        protected override void createZedGraphControlHours(object objLock)
        {
            this.m_ZedGraphHours = new ZedGraphControlLK(objLock);
        }

        protected override void createZedGraphControlMins(object objLock)
        {
            // минутная гистограмма не требуется
        }

        protected override void createPanelQuickData()
        {
            this._pnlQuickData = new PanelQuickDataLK (ForeColor, BackColor);
        }

        protected override ASUTP.Core.HMark enabledSourceData_ToolStripMenuItems ()
        {
            ASUTP.Core.HMark markRes = new ASUTP.Core.HMark (0);

            return markRes;
        }

        /// <summary>
        /// Метод непосредственного применения параметров графического представления данных
        /// </summary>
        /// <param name="type">Тип изменившихся параметров</param>
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
        }

        //protected override void initTableHourRows()
        //{
        //    //Ничего не делаем, т.к. составные элементы самостоятельно настраивают кол-во строк в таблицах
        //}        

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }
    }
}
