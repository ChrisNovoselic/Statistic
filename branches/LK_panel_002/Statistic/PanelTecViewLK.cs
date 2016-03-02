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
            }

            //protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            //{
            //    throw new NotImplementedException();
            //}

            public override void RestructControl()
            {
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
            throw new NotImplementedException();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }
    }
}
