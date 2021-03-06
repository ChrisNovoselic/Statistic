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
            : base(TecView.TYPE_PANEL.LK, tec, num_tec, num_comp)
        {
            SPLITTER_PERCENT_VERTICAL = 30;

            m_arPercRows = new int[] { 5, 78 };

            InitializeComponent ();
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
                : base (TYPE_PANEL.LK, indx_tec, indx_comp) 
            {
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
                m_iCountCommonLabels = (int)CONTROLS.lblPowerHourValue - (int)CONTROLS.lblTemperatureCurrent + 1;

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
                            text = @"t пл/ч";
                            break;
                        case CONTROLS.lblTemperatureDate:
                            text = @"t пл/сут";
                            break;
                        default:
                            text = string.Empty;
                            break;
                    }

                    createLabel((int)i, text, foreColor, backClolor, szFont, align);
                }
                #endregion

                #region добавить поля для значений МОЩНОСТИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblPowerHourValue + 1; i++)
                {
                    switch (i)
                    {
                        case CONTROLS.lblPowerCurrent:
                        case CONTROLS.lblPowerHour:
                            foreColor = Color.Black;
                            backClolor = Color.Empty;
                            szFont = 8F;
                            align = ContentAlignment.MiddleLeft;
                            break;
                        case CONTROLS.lblPowerCurrentValue:
                        case CONTROLS.lblPowerHourValue:
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
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblPowerHourValue + 1; i++)
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

            public override void ShowFactValues()
            {
                int indxStartCommonPVal = m_indxStartCommonFirstValueSeries;
                double dblSum = m_parent.m_tecView.GetSummaFactValues();

                showValue(ref m_arLabelCommon[(int)PanelQuickDataLK.CONTROLS.lblPowerCurrentValue - indxStartCommonPVal], dblSum, true, string.Empty);
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
            private enum INDEX_COLUMNS : short { PART_TIME, TEMPERATURE, POWER_SUM, POWER_F1, POWER_F2, DEVIATION
                , COUNT_COLUMN }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewLKHours()
                //: base(new int[] { 8, 15, 15, 15, 15, 15, 15 })
                : base(
                    HDateTime.INTERVAL.HOURS
                    , new ColumnProperies[] { new ColumnProperies (27, 10, @"Час", @"Hour")
                    , new ColumnProperies (47, 15, @"t час", @"TemperaturePBR")
                    , new ColumnProperies (47, 15, @"P час", @"PowerPBR")
                    , new ColumnProperies (47, 15, @"t пл/ч", @"TemperatureFact")
                    , new ColumnProperies (47, 15, @"P пл/ч", @"PowerFact")
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
                            Rows[i].Cells[0].Value += @"*";
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
                        case INDEX_COLUMNS.POWER_SUM:
                        case INDEX_COLUMNS.POWER_F1:
                        case INDEX_COLUMNS.POWER_F2:
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
                int receivedHour = (int)pars[1]; //m_tecView.lastReceivedHour;
                int itemscount = (int)pars[2]; //m_tecView.m_valuesHours.Length;
                int warn = -1,
                    cntWarn = -1;
                string strWarn = string.Empty;
                bool bPmin = (int)pars[3] == 5
                    , bCurrHour = (bool)pars[4] //m_tecView.currHour
                    , bIsTypeConnSettAISKUEHour = (bool)pars[5] //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                    , bIsNowMoscowDate = (bool)pars[6]; //m_tecView.serverTime.Date.Equals(HDateTime.ToMoscowTimeZone(DateTime.Now.Date))

                DataGridViewCellStyle curCellStyle;
                cntWarn = 0;
                for (int i = 0; i < itemscount; i++)
                {
                    Rows[i].Cells[(int)INDEX_COLUMNS.POWER_SUM].Value = values[i].valuesFact.ToString(@"F2");
                }
            }
        }

        private class ZedGraphControlLK : HZedGraphControl
        {
            public ZedGraphControlLK(object lockVal)
                : base(lockVal, FormMain.formGraphicsSettings.SetScale)
            {
            }
        }

        public override void SetDelegateReport(DelegateStringFunc fErr, DelegateStringFunc fWar, DelegateStringFunc fAct, DelegateBoolFunc fClear)
        {
            m_tecView.SetDelegateReport(fErr, fWar, fAct, fClear);
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

        protected override void createPanelQuickData()
        {
            this._pnlQuickData = new PanelQuickDataLK ();
        }

        protected override void initTableHourRows()
        {
            //Ничего не делаем, т.к. составные элементы самостоятельно настраивают кол-во строк в таблицах
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }
    }
}
