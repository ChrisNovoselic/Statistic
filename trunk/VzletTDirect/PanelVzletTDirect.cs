using HClassLibrary;
using StatisticCommon;
using System;
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

namespace StatisticVzletTDirect
{
    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelVzletTDirect
    {
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.SuspendLayout();

            this.ResumeLayout();
            this.PerformLayout();
        }

        #endregion

    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelVzletTDirect : PanelStatistic
    {
        HAdmin m_DataSource;
        /// <summary>
        /// constructor
        /// </summary>
        public PanelVzletTDirect()
        {
            initialize();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="container"></param>
        public PanelVzletTDirect(IContainer container)
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

            return iRes;
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        public void m_DataSource_EvtRecievedTable(object table)
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
            if (!(m_DataSource == null))
                m_DataSource.SetDelegateReport(ferr, fwar, fact, fclr);
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
    }
}