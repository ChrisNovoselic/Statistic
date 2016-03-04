using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

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

                this.RowCount = COUNT_ROWS;

                for (int i = 0; i < this.RowCount + 1; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

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
            }

            //protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            //{
            //    throw new NotImplementedException();
            //}

            public override void RestructControl()
            {
                COUNT_LABEL = 0; COUNT_TG_IN_COLUMN = 3; COL_TG_START = 1;

                bool bPowerFactZoom = false;
                int cntCols = 0;

                this.Controls.Add(m_panelEmpty, COL_TG_START + cntCols * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
                this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            public override void AddTGView(TG tg)
            {
            }

            public override void ShowFactValues()
            {
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
        private class DataGridViewLK : DataGridView
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewLK()
                : base()
            {
                initializeComponent();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewLK(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления 
            /// </summary>
            private void initializeComponent()
            {
            }
        }

        private class ZedGraphControlLK : HZedGraphControl
        {
            public ZedGraphControlLK(object lockVal)
                : base(lockVal)
            {
            }
        }

        public override void SetDelegateReport(DelegateStringFunc fErr, DelegateStringFunc fWar, DelegateStringFunc fAct, DelegateBoolFunc fClear)
        {
            m_tecView.SetDelegateReport(fErr, fWar, fAct, fClear);
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
