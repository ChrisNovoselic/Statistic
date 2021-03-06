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
        public PanelVzletTDirect(List<StatisticCommon.TEC> listTec)
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
        /// <param name="container"></param>
        /// <param name="listTec">Лист ТЭЦ</param>
        public PanelVzletTDirect(IContainer container, List<StatisticCommon.TEC> listTec)
            : this(listTec)
        {
            container.Add(this);
        }

        public partial class PanelTecVzletTDirect : PanelTecViewBase
        {
            private class DataGridViewVzletTDirectHours : HDataGridViewBase
            {
                public DataGridViewVzletTDirectHours(HDateTime.INTERVAL interval, ColumnProperies[] arColumns)
                    : base(interval, arColumns)
                {
                }

                public override void Fill(TecView.valuesTEC[] values, params object[] pars)
                {
                    throw new NotImplementedException();
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
                }
            }
            /// <summary>
            /// Класс для размещения активных элементов управления
            /// </summary>
            private class PanelQuickDataVzletTDirect : HPanelQuickData
            {
                public PanelQuickDataVzletTDirect()
                    : base(/*-1, -1*/)
                {
                    InitializeComponents();
                }

                private void InitializeComponents()
                {
                    COUNT_ROWS = 3;

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
                        case CONTROLS.lblDeviatCurrent:
                            row = 0; col = 3;
                            break;
                        case CONTROLS.lblDeviatCurrentValue:
                            row = 0; col = 4;
                            break;
                        case CONTROLS.lblDeviatHour:
                            row = 1; col = 3;
                            break;
                        case CONTROLS.lblDeviatHourValue:
                            row = 1; col = 4;
                            break;
                        case CONTROLS.lblDeviatDate:
                            row = 2; col = 3;
                            break;
                        case CONTROLS.lblDeviatDateValue:
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
                    SZ_COLUMN_LABEL = 48F; /*SZ_COLUMN_LABEL_VALUE = 78F;*/
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                    #endregion

                    #region Отклонение
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDeviatDateValue + 1; i++)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                        //this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROW_LABELCOMMON);
                    }

                    //Ширина столбцов группы "Отклонение"
                    SZ_COLUMN_LABEL = 63F; /*SZ_COLUMN_LABEL_VALUE = 78F;*/
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                    #endregion

                    cntCols = ((m_tgLabels.Count / COUNT_TG_IN_COLUMN) + ((m_tgLabels.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));

                    for (int i = 0; i < cntCols; i++)
                    {
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL));
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                    }

                    //if (m_tgLabels.Count > 0)
                    //    addTGLabels(false);
                    //else
                    //    ;

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
                public DataSource(int indx_tec, int indx_comp = -1) : base (indx_tec, indx_comp)
                {
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

                protected override void GetPPBRDatesRequest(DateTime date)
                {
                    throw new NotImplementedException();
                }

                protected override int GetPPBRDatesResponse(DataTable table, DateTime date)
                {
                    throw new NotImplementedException();
                }

                protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date)
                {
                    throw new NotImplementedException();
                }

                protected override int GetPPBRValuesResponse(DataTable table, DateTime date)
                {
                    throw new NotImplementedException();
                }

                public override void GetRDGValues(int indx, DateTime date)
                {
                    throw new NotImplementedException();
                }

                public override bool WasChanged()
                {
                    throw new NotImplementedException();
                }
            }
            /// <summary>
            /// constructor
            /// </summary>
            public PanelTecVzletTDirect(TEC tec, int indx_tec, int indx_comp)
                : base(tec, indx_tec, indx_comp)
            {
                initialize();
            }
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="container"></param>
            public PanelTecVzletTDirect(IContainer container, TEC tec, int indx_tec, int indx_comp)
                : base(tec, indx_tec, indx_comp)
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

            protected override void createTecView(int indx_tec, int indx_comp)
            {
                m_tecView = new DataSource(indx_tec, indx_comp);
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

            protected override void createPanelQuickData()
            {
                _pnlQuickData = new PanelQuickDataVzletTDirect();
            }

            protected override void createDataGridViewHours()
            {
                m_dgwHours = new DataGridViewVzletTDirectHours(HDateTime.INTERVAL.HOURS, new HDataGridViewBase.ColumnProperies[]
                    {//??? в сумме ширина = 310, проценты = 98, 
                        new HDataGridViewBase.ColumnProperies (27, 8, @"Час", @"Hour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"Факт", @"FactHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"План", @"PBRHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"Рек.", @"RecHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"УДГэ", @"UDGeHour")
                        , new HDataGridViewBase.ColumnProperies (47, 18, @"+/-", @"DeviationHour")
                    }
                );
            }

            protected override void createDataGridViewMins()
            {
                ; // не требуется
            }

            protected override void createZedGraphControlHours(object objLock)
            {
                m_ZedGraphHours = new ZedGraphControlVzletTDirect(new object());
            }

            protected override void createZedGraphControlMins(object objLock)
            {
                ; // не требуется
            }

            protected override HMark enabledSourceData_ToolStripMenuItems()
            {
                ; // не требуется, разнотипные источники данных отсутствуют
                return new HMark(0);
            }
        }
    }
}