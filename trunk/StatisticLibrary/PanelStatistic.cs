﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Drawing; //Color..

using HClassLibrary;

namespace StatisticCommon
{
    public abstract class PanelStatistic : HPanelCommon
    {
        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        protected static string s_DialogMSExcelBrowseFilter = @"Книга MS Excel 2003|*.xls|Книга MS Excel 2010|*.xlsx";

        public PanelStatistic(int cCols = -1, int cRows = -1)
            : base(cCols, cRows)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;
        }

        public static volatile int POOL_TIME = -1
            , ERROR_DELAY = -1;

        public virtual void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public abstract void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr);

        public virtual bool MayToClose()
        {
            return true;
        }

        //protected List<TEC> m_listTEC;
    }

    public abstract class PanelContainerStatistic : PanelStatistic
    {
        private Type _controlType;

        public PanelContainerStatistic(Type type)
            : base()
        {
            registredControlType(type);
        }

        private void registredControlType(Type type)
        {
            _controlType = type;
        }
        /// <summary>
        /// Старт опроса
        /// </summary>
        public override void Start()
        {
            base.Start();

            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Start();
                else
                    ;
        }
        /// <summary>
        /// Остановка опроса
        /// </summary>
        public override void Stop()
        {
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Stop();
                else
                    ;

            base.Stop();
        }
        /// <summary>
        /// Активация панели
        /// </summary>
        /// <param name="active">Установка состояния панели</param>
        /// <returns>Возвращает состояние после выполнения операции</returns>
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == false)
                return bRes;
            else
                ;

            int i = 0;
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Activate(active);
                else
                    ;

            return bRes;
        }
        /// <summary>
        /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
        ///  (должна быть вызвана явно)
        /// </summary>
        /// <param name="col">Количество столбцов в макете</param>
        /// <param name="row">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;

            initializeLayoutStyleEvenly();
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
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }
    }

    public abstract class PanelStatisticWithTableHourRows : PanelStatistic
    {
        protected abstract void initTableHourRows();
    }
}
