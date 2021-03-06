﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

using ZedGraph;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    class PanelLKView : PanelTecViewBase
    {        
        public PanelLKView(StatisticCommon.TEC tec, int num_tec, int num_comp)
            : base(tec, num_tec, num_comp)
        {
            SPLITTER_PERCENT_VERTICAL = 30;

            m_arPercRows = new int[] { 5, 78 };

            MainHours.Initialize();

            InitializeComponent ();

            (m_dgwHours as DataGridViewLKHours).EventPBRDateValues += new DataGridViewLKHours.PBRDateValuesEventHandler((_pnlQuickData as PanelQuickDataLK).OnPBRDateValues);
        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();

            OnEventRestruct(
                //PanelCustomTecView.HLabelCustomTecView.s_propViewDefault
                new int[] { 0, 1, 0, 1, 0, 1, -1 } //отобразить часовые таблицу/гистограмму/панель с оперативными данными
                );            
        }

        private class TecViewLK : TecView
        {
            public TecViewLK (int indx_tec, int indx_comp)
                : base (indx_tec, indx_comp) 
            {
                m_idAISKUEParNumber = ID_AISKUE_PARNUMBER.FACT_30;
                _tsOffsetToMoscow = HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
            }

            public override void ChangeState()
            {
                lock (m_lockState) { GetRDGValues(-1, DateTime.MinValue); }

                base.ChangeState(); //Run
            }

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

                AddState((int)TecView.StatesMachine.Hours_Fact);
                AddState((int)TecView.StatesMachine.CurrentMins_Fact);
                AddState((int)TecView.StatesMachine.HoursTMTemperatureValues);
                AddState((int)TecView.StatesMachine.PPBRValues);
                AddState((int)TecView.StatesMachine.AdminValues);
            }

            public override double GetSummaFactValues(int hour)
            {
                double dblRes = -1F;
                double[] arTGRes = null;
                uint[] arTGcounter = null;
                int iter = m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_03 ? 1 :
                    m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_30 ? 10 : -1;

                arTGRes = new double[listTG.Count];
                arTGcounter = new uint[listTG.Count];

                for (int t = 0; t < listTG.Count; t++)
                {
                    arTGRes[t] = -1F;
                    arTGcounter[t] = 0;

                    for (int j = 10; j < ((60 / 3) + 1); j += iter)
                        if (!(m_dictValuesTG[listTG[t].m_id].m_powerMinutes[j] < 0))
                        {
                            if (arTGRes[t] < 0F)
                                arTGRes[t] = 0F;
                            else
                                ;

                            arTGRes[t] += m_dictValuesTG[listTG[t].m_id].m_powerMinutes[j];
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
            public PanelQuickDataLK()
                : base (/*-1, -1*/)
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                COUNT_ROWS = 3;

                SZ_COLUMN_LABEL = 58F;

                m_indxStartCommonFirstValueSeries = (int)CONTROLS.lblTemperatureCurrent;
                m_indxStartCommonSecondValueSeries = (int)CONTROLS.lblPowerCurrent;
                m_iCountCommonLabels = (int)CONTROLS.lblPowerDateValue - (int)CONTROLS.lblTemperatureCurrent + 1;

                m_tgLabels = new Dictionary<int, System.Windows.Forms.Label[]>();

                // количество и параметры строк макета панели
                this.RowCount = COUNT_ROWS;
                for (int i = 0; i < this.RowCount + 1; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

                this.m_arLabelCommon = new System.Windows.Forms.Label[m_iCountCommonLabels];

                //
                // btnSetNow
                //
                this.Controls.Add(this.btnSetNow, 0, 0);
                // 
                // dtprDate
                // 
                this.Controls.Add(this.dtprDate, 0, 1);
                // 
                // lblServerTime
                // 
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
                        case CONTROLS.lblPowerCurrent:
                        case CONTROLS.lblPowerHour:
                        case CONTROLS.lblPowerDate:
                            foreColor = Color.Black;
                            backClolor = Color.Empty;
                            szFont = 8F;
                            align = ContentAlignment.MiddleLeft;
                            break;
                        case CONTROLS.lblPowerCurrentValue:
                        case CONTROLS.lblPowerHourValue:
                        case CONTROLS.lblPowerDateValue:
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
                COUNT_ROW_LABELCOMMON = 1;

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
                this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            private Color clrLabel { get { return m_parent.m_tecView.currHour == true ? Color.LimeGreen : Color.OrangeRed; } }

            public void OnPBRDateValues(DataGridViewLKHours.PBRDateValuesEventArgs ev)
            {
                // температура
                showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureDateValue - m_indxStartCommonFirstValueSeries]
                    , (double)ev.m_temperatureDate //> 0 ? (double)val : double.NegativeInfinity
                    , 2 //round
                    , false
                    , true
                    , string.Empty);
                // мощность
                showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerDateValue - m_indxStartCommonFirstValueSeries]
                    , (double)ev.m_powerDate > 0 ? (double)ev.m_powerDate : double.NegativeInfinity
                    , 2 //round
                    , false
                    , true
                    , string.Empty);

                m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureDateValue - m_indxStartCommonFirstValueSeries].ForeColor =
                m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerDateValue - m_indxStartCommonFirstValueSeries].ForeColor =
                    clrLabel;
            }

            private void showFactTGValue(int id_tg, double[] powerLastHourMinutes)
            {
                int min = -1
                    , cntMinValues = 0;
                double powerLastHour = 0F;

                for (min = 10; min < powerLastHourMinutes.Length; min += 10)
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
                    m_tgLabels[id_tg][(int)TG.INDEX_VALUE.FACT].Text = "---";

                // установить цвет шрифта для значения
                m_tgLabels[id_tg][(int)TG.INDEX_VALUE.FACT].ForeColor =
                    clrLabel
                    //getColorFactValues()
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
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesLastMinutesTM
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // плановое значение температуры (час)
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureHourValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesPmin
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // плановое значение температуры (сутки) - отображается при обработке события 'DataGridViewLKHours::EventTemperaturePBRDay'
                    
                    //Мощность
                    // текущее значение мощности (час)
                    powerLastHour = m_parent.m_tecView.GetSummaFactValues(lastHour);
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerCurrentValue - indxStartCommonPVal]
                        , powerLastHour < 0 ? double.NegativeInfinity : powerLastHour * 1000
                        , 2 //round
                        , false
                        , true
                        , powerLastHour < 0 ? @"---" : string.Empty);
                    // плановое значение мощности (час)
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerHourValue - indxStartCommonPVal]
                        , m_parent.m_tecView.m_valuesHours[lastHour].valuesPBR > 0 ? m_parent.m_tecView.m_valuesHours[lastHour].valuesPBR : double.NegativeInfinity
                        , 2 //round
                        , false
                        , true
                        , string.Empty);
                    // цвет шрифта для значений температуры, мощности
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureCurrentValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureHourValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblTemperatureDateValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerCurrentValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerHourValue - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerDateValue - indxStartCommonPVal].ForeColor =
                        clrLabel;
                    // текущее значение мощности для компонентов-ТГ-фидеров (час)
                    //ShowTGValue
                    if (m_parent.indx_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
                    {
                        foreach (TECComponent g in m_parent.m_tecView.m_localTECComponents)
                            if (g.IsGTP == true)
                            //Только ГТП
                                foreach (TG tg in g.m_listTG) {
                                //Цикл по списку с ТГ
                                    powerLastHourMinutes = m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerMinutes;
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
                        foreach (TECComponent comp in m_parent.m_tecView.m_localTECComponents) {
                        //Цикл по списку ТГ в компоненте ТЭЦ (ГТП, ЩУ)
                            powerLastHourMinutes = m_parent.m_tecView.m_dictValuesTG[comp.m_listTG[0].m_id].m_powerMinutes;
                            //Проверить возможность получения значения
                            if (!(powerLastHourMinutes == null))
                                showFactTGValue (comp.m_listTG[0].m_id, powerLastHourMinutes);
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
            private enum INDEX_COLUMNS : short { PART_TIME, TEMPERATURE_FACT, POWER_FACT_SUM, TEMPERATURE_PBR, POWER_PBR, TEMPERATURE_DEVIATION, POWER_DEVIATION
                , COUNT_COLUMN }

            public class PBRDateValuesEventArgs : EventArgs
            {
                public double m_temperatureDate;
                public double m_powerDate;
            }

            public delegate void PBRDateValuesEventHandler(PBRDateValuesEventArgs ev);

            public event PBRDateValuesEventHandler EventPBRDateValues;

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewLKHours()
                //: base(new int[] { 8, 15, 15, 15, 15, 15, 15 })
                : base(
                    HDateTime.INTERVAL.HOURS
                    , new ColumnProperies[] { new ColumnProperies (27, 10, @"Час", @"Hour")
                    , new ColumnProperies (47, 15, @"t час", @"TemperatureFact")
                    , new ColumnProperies (47, 15, @"P час", @"PowerFactSum")
                    , new ColumnProperies (47, 15, @"t пр/ч", @"TemperaturePBR")
                    , new ColumnProperies (47, 15, @"P пл/ч", @"PowerPBR")
                    , new ColumnProperies (42, 15, @"t +/-", @"TemperatureDevHour")
                    , new ColumnProperies (42, 15, @"P +/-", @"PowerDevHour")
            })
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
                double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
                int lastHour = (int)pars[0]; //m_tecView.lastHour;
                int lastReceivedHour = (int)pars[1]; //m_tecView.lastReceivedHour;
                int itemscount = (int)pars[2]; //m_tecView.m_valuesHours.Length;
                bool bPmin = (int)pars[3] == 5
                    , bCurrHour = (bool)pars[4] //m_tecView.currHour
                    , bIsTypeConnSettAISKUEHour = (bool)pars[5]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                DateTime serverTime = (DateTime)pars[6]; //m_tecView.serverTime.Date.Equals(HDateTime.ToMoscowTimeZone(DateTime.Now.Date))

                int i = -1
                    , warn = -1, cntWarn = -1
                    , lh = bCurrHour == true ? lastReceivedHour - 1 : lastReceivedHour;
                double t_pbr = -1F, p_pbr = -1F;
                string strWarn = string.Empty;
                

                DataGridViewCellStyle curCellStyle;
                DataGridViewCellStyle regularHourCellStyle = new DataGridViewCellStyle()
                    , mainHourCellStyle = new DataGridViewCellStyle();

                regularHourCellStyle.BackColor = Color.White;
                mainHourCellStyle.BackColor = Color.LimeGreen;
                //// полужирный на основе 1-ой ячейки                
                //mainHourCellStyle.Font = new System.Drawing.Font(RowsDefaultCellStyle.Font, FontStyle.Bold);

                cntWarn = 0;
                t_pbr = 0;
                for (i = 0; i < itemscount; i++)
                {
                    // номер часа
                    curCellStyle = (MainHours.IsMain(serverTime, i + 1) == true) ? mainHourCellStyle :
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
                    // разность
                    if (i < lastHour)
                    {
                        // - температура
                        if ((!(values[i].valuesLastMinutesTM == 0))
                            && (!(values[i].valuesPmin == 0)))
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = (values[i].valuesPmin - values[i].valuesLastMinutesTM).ToString(@"F2");                            
                        else
                            Rows[i].Cells[(int)INDEX_COLUMNS.TEMPERATURE_DEVIATION].Value = @"-";                        
                        // - мощность
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
                EventPBRDateValues(new PBRDateValuesEventArgs() { m_temperatureDate = t_pbr, m_powerDate = p_pbr });
            }
        }

        private class MainHours
        {
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
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            listRanges = new List<RANGE_HOURS>(new RANGE_HOURS[] { new RANGE_HOURS() { Begin = 8, End = 12 }, new RANGE_HOURS() { Begin = 17, End = 21 } });
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

        private class ZedGraphControlLK : HZedGraphControl
        {
            public ZedGraphControlLK(object lockVal)
                : base(lockVal, FormMain.formGraphicsSettings.SetScale)
            {
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
                    , iMainIntervalCount = MainHours.GetCountMainInterval (serverTime.Date)
                    , i = -1, h = -1
                    , indxItemMain = -1, indxHourMain = -1
                    , iMainIntervalChanged = -1;
                double y = -1F;

                string[] names = new string[itemscount];

                PointPairList[] valuesMainFact = null //new double[itemscount]
                    , valuesRegularFact = null //new double[itemscount]
                    ;
                PointPairList [] valuesPlan = null //new PointPairList () //new double[itemscount]
                    , valuesPDiviation = null //new PointPairList () //new double[itemscount]
                    , valuesODiviation = null //new PointPairList () //new double[itemscount]
                    ;

                // выделить память для
                if (iMainIntervalCount > 0)
                {
                    valuesMainFact = new PointPairList[iMainIntervalCount];
                    valuesRegularFact = new PointPairList[iMainIntervalCount + 1];

                    for (i = 0; i < valuesMainFact.Length; i++)
                        valuesMainFact[i] = new PointPairList();

                    for (i = 0; i < valuesRegularFact.Length; i++)
                        valuesRegularFact[i] = new PointPairList();
                    
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

                Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;
                getColorZedGraph(typeConnSett, out colorChart, out colorPCurve);

                GraphPane.Chart.Fill = new Fill(colorChart);

                //LineItem - план/отклонения
                string strCurveNamePPlan = "P план"
                    , strCurveNameDeviation = "Возможное отклонение";
                for (i = 0; i < iMainIntervalCount; i++)
                {
                    GraphPane.AddCurve(strCurveNamePPlan, /*null,*/ valuesPlan[i], FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
                    //LineItem
                    GraphPane.AddCurve(string.Empty, /*null,*/ valuesODiviation[i], FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
                    //LineItem
                    GraphPane.AddCurve(strCurveNameDeviation, /*null,*/ valuesPDiviation[i], FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
                    // чтобы повторно не добавить подпись в легенду
                    strCurveNamePPlan =
                    strCurveNameDeviation =
                        string.Empty;
                }
                //Значения
                string strCurveNameMain = "Мощность(контр.)"
                    , strCurveNameReg = "Мощность(рег.)";
                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                {
                    if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    {//BarItem
                        for (i = 0; i < valuesMainFact.Length; i++)
                        {
                            GraphPane.AddBar(strCurveNameMain, valuesMainFact[i], colorPCurve);
                            // чтобы повторно не добавить подпись в легенду
                            strCurveNameMain =
                                string.Empty;
                        }

                        for (i = 0; i < valuesRegularFact.Length; i++)
                        {
                            GraphPane.AddBar(strCurveNameReg, valuesRegularFact[i], Color.White);
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
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                    {
                        if (!(typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        {//LineItem
                            for (i = 0; i < valuesMainFact.Length; i++)
                            {
                                GraphPane.AddCurve(strCurveNameMain, valuesMainFact[i], colorPCurve);
                                // чтобы повторно не добавить подпись в легенду
                                strCurveNameMain =
                                    string.Empty;
                            }

                            for (i = 0; i < valuesRegularFact.Length; i++)
                            {
                                GraphPane.AddCurve(strCurveNameReg, valuesRegularFact[i], Color.White);
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

            //protected override string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            //{
            //    return curve[iPt].Y.ToString("F3");
            //}

            public override bool FindNearestObject(PointF p, Graphics g, out object obj, out int index)
            {
                bool bRes = false;

                bRes = base.FindNearestObject(p, g, out obj, out index);

                if (!(obj == null))
                    //if ((obj as CurveItem).IsLine == true)
                        index = (int)(obj as CurveItem).Points[index].X - 1;
                    //else ;
                else
                    ;

                return bRes;
            }
        }

        //public override void SetDelegateReport(DelegateStringFunc fErr, DelegateStringFunc fWar, DelegateStringFunc fAct, DelegateBoolFunc fClear)
        //{
        //    m_tecView.SetDelegateReport(fErr, fWar, fAct, fClear);
        //}

        protected override void createTecView(int indx_tec, int indx_comp)
        {
            m_tecView = new TecViewLK(indx_tec, indx_comp);
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
            this._pnlQuickData = new PanelQuickDataLK ();
        }

        protected override HMark enabledSourceData_ToolStripMenuItems()
        {
            HMark markRes = new HMark(0);

            return markRes;
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
