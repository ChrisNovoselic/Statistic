using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public class PanelSOTIASSO : PanelStatistic
    {
        private TecView m_tecView;

        System.Windows.Forms.SplitContainer stctrMain
            , stctrView;
        ZedGraph.ZedGraphControl m_zGraph_GTP
            , m_zGraph_TG;

        /// <summary>
        /// Панель для активных элементов управления
        /// </summary>
        private PanelManagement m_panelManagement;
        /// <summary>
        /// Инициализация панели с установкой кол-ва столбцов, строк
        /// </summary>
        /// <param name="cols">Количество столбцов</param>
        /// <param name="rows">Количество строк</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PanelSOTIASSO () : base ()
        {
            m_tecView = new TecView (TecView.TYPE_PANEL.SOTIASSO, -1, -1);

            initializeComponent ();
        }
        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelSOTIASSO (IContainer container) : this ()
        {
            container.Add (this);
        }
        ///// <summary>
        ///// Деструктор
        ///// </summary>
        //~PanelSOTIASSO ()
        //{
        //    m_tecView = null;
        //}
        /// <summary>
        /// Инициализация и размещение собственных элементов управления
        /// </summary>
        private void initializeComponent ()
        {
            m_panelManagement = new PanelManagement ();
            m_zGraph_GTP = new ZedGraph.ZedGraphControl();
            m_zGraph_TG = new ZedGraph.ZedGraphControl();

            stctrMain = new SplitContainer ();
            stctrView = new SplitContainer();

            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;

            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;

            m_zGraph_GTP.Dock = DockStyle.Fill;
            m_zGraph_TG.Dock = DockStyle.Fill;

            this.SuspendLayout ();

            stctrView.Panel1.Controls.Add(m_zGraph_GTP);
            stctrView.Panel2.Controls.Add(m_zGraph_TG);

            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            this.Controls.Add(stctrMain);

            this.ResumeLayout (false);
            this.PerformLayout();
        }
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : HPanelCommon
        {
            /// <summary>
            /// Элемент управления - каледарь
            ///  для установки даты отображаемой информации
            /// </summary>
            private DateTimePicker m_dtpCurrent;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagement() : base (-1, -1)
            {
                initializeComponent ();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public PanelManagement(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация, размещения собственных элементов управления
            /// </summary>
            private void initializeComponent()
            {
                m_dtpCurrent = new DateTimePicker();
                m_dtpCurrent.DropDownAlign = LeftRightAlignment.Right;
                m_dtpCurrent.Format = DateTimePickerFormat.Custom;
                m_dtpCurrent.CustomFormat = @"HH-й час, dd MMMM, yyyy";
                m_dtpCurrent.Dock = DockStyle.Fill;

                this.SuspendLayout();

                this.Controls.Add(m_dtpCurrent);

                this.ResumeLayout(false);
                this.PerformLayout();
            }
            /// <summary>
            /// Инициализация панели с установкой кол-ва столбцов, строк
            /// </summary>
            /// <param name="cols">Количество столбцов</param>
            /// <param name="rows">Количество строк</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new System.NotImplementedException();
            }
        }
        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start ();
        }
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            base.Stop ();
        }
        /// <summary>
        /// Переопределение наследуемой функции - активация/деактивация объекта
        /// </summary>
        public override bool Activate(bool active)
        {
            bool bRes = false;

            bRes = base.Activate(active);

            return bRes;
        }
    }
}