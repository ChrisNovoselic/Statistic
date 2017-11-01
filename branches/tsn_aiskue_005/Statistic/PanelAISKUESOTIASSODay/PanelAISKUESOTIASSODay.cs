﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Drawing; //Color
using System.Data;


using StatisticCommon;
using System.Linq;
using GemBox.Spreadsheet;
using System.IO;

using Microsoft.Office.Interop.Excel;
using ASUTP.PlugIn;
using ASUTP.Core;
using ASUTP;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public partial class PanelAISKUESOTIASSODay : PanelStatistic, IDataHost
    {
        private static CONN_SETT_TYPE[] _types = { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO };
        /// <summary>
        /// Перечисление - состояния для обработки обращений к БД
        /// </summary>
        private enum StatesMachine {
            /// <summary>
            /// Получить текущее время сервера БД
            /// </summary>
            SERVER_TIME
            /// <summary>
            /// Получить список сигналов для источника данных
            /// </summary>
            , LIST_SIGNAL
            /// <summary>
            /// Получить значения для выбранных сигналов источника данных
            /// </summary>
            , VALUES
        }
        /// <summary>
        /// Перечисление - возможные действия с сигналами в списке на панели управления
        /// </summary>
        private enum ActionSignal { SELECT, CHECK }
        /// <summary>
        /// Перечисление - возможные изменения даты/времени, часового пояса
        /// </summary>
        [Flags]
        private enum ActionDateTime { UNKNOWN = 0, VALUE = 0x1, TIMEZONE = 0x2 }
        /// <summary>
        /// Структура для описания сигнала
        /// </summary>
        private struct SIGNAL
        {
            public int id;
            /// <summary>
            /// Уникальный строковый идентификатор сигнала
            ///  , для АИИСКУЭ - формируется динамически по правилу [PREFIX_TEC]#OBJECT[идентификатор_УСПД]_ITEM[номер_канала_в_УСПД]
            ///  , для СОТИАСОО - из источника данных
            /// </summary>
            public string kks_code;
            /// <summary>
            /// Полное наименование сигнала
            /// </summary>
            public string name;
            /// <summary>
            /// Краткое наименование сигнала
            ///  , может быть одинаковое для АИИКУЭ и СОТИАССО
            /// </summary>
            public string name_shr;
        }

        private BackgroundWorker m_threadDraw;
        /// <summary>
        /// Перечисление - целочисленные идентификаторы дочерних элементов управления
        /// </summary>
        private enum KEY_CONTROLS
        {
            UNKNOWN = -1
                , DTP_CUR_DATE, CBX_TEC_LIST, CBX_TIMEZONE, BTN_EXPORT
                , CLB_AIISKUE_SIGNAL, CLB_SOTIASSO_SIGNAL
                , DGV_AIISKUE_VALUE, ZGRAPH_AIISKUE
                , DGV_SOTIASSO_VALUE, ZGRAPH_SOTIASSO
                , COUNT_KEY_CONTROLS
        }
        ///// <summary>
        ///// Объект с признаками обработки типов значений
        ///// , которые будут использоваться фактически (PBR, Admin, AIISKUE, SOTIASSO)
        ///// </summary>
        //private HMark m_markQueries;
        /// <summary>
        /// Объект для обработки запросов/получения данных из/в БД
        /// </summary>
        private HandlerSignalQueue m_HandlerQueue;
        /// <summary>
        /// Словарь с предствалениями для отображения значений выбранных (на панели управления) сигналов
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, HDataGridView> m_dictDataGridViewValues;
        /// <summary>
        /// Панели графической интерпретации значений
        /// 1) АИИСКУЭ "сутки - по-часам для выбранных сигналов, 2) СОТИАССО "сутки - по-часам для выбранных сигналов"
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, HZedGraphControl> m_dictZGraphValues;

        private List<StatisticCommon.TEC> m_listTEC;
        ///// <summary>
        ///// Список индексов компонентов ТЭЦ (ТГ)
        /////  для отображения в субобласти графической интерпретации значений СОТИАССО "минута - по-секундно"
        ///// </summary>
        //private List<int> m_listIdAIISKUEAdvised
        //    , m_listIdSOTIASSOAdvised;
        ///// <summary>
        ///// Событие выбора даты
        ///// </summary>
        //private event Action<DateTime> EvtSetDatetimeHour;
        ///// <summary>
        ///// Делегат для установки даты на панели управляющих элементов управления
        ///// </summary>
        //private Action<DateTime> delegateSetDatetimeHour;
        /// <summary>
        /// Панель для активных элементов управления
        /// </summary>
        private PanelManagement m_panelManagement;
        
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PanelAISKUESOTIASSODay(int iListenerConfigId, List<StatisticCommon.TEC> listTec)
            : base(MODE_UPDATE_VALUES.ACTION, FormMain.formGraphicsSettings.BackgroundColor)
        {
            // фильтр ТЭЦ
            m_listTEC = listTec.FindAll(tec => { return (tec.Type == TEC.TEC_TYPE.COMMON) && (tec.m_id < (int)TECComponent.ID.LK); });

            m_dictDataGridViewValues = new Dictionary<CONN_SETT_TYPE, HDataGridView>();
            m_dictZGraphValues = new Dictionary<CONN_SETT_TYPE, HZedGraphControl>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();

            if (m_listTEC.Count > 0) {
                //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
                m_HandlerQueue = new HandlerSignalQueue(iListenerConfigId, m_listTEC);
                m_HandlerQueue.UserDate = new HandlerSignalQueue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
                //m_HandlerQueue.UpdateGUI_Fact += new Action<CONN_SETT_TYPE, HandlerSignalQueue.EVENT>(handlerSignalQueue_OnEventCompleted);
            } else
                Logging.Logg().Error(@"PanelSOTIASSODay::ctor () - кол-во ТЭЦ = 0...", Logging.INDEX_MESSAGE.NOT_SET);

            m_threadDraw = new BackgroundWorker();
            m_threadDraw.DoWork += threadDraw_DoWork;
            m_threadDraw.RunWorkerCompleted += threadDraw_RunWorkerCompleted;
            m_threadDraw.WorkerReportsProgress = false;
            m_threadDraw.WorkerSupportsCancellation = true;

            #region Дополнительная инициализация панели управления
            m_panelManagement.SetTECList(m_listTEC);
            m_panelManagement.EvtTECListSelectionIndexChanged += new Action<int>(panelManagement_TECListOnSelectionChanged);
            m_panelManagement.EvtDateTimeChanged += new Action<ActionDateTime>(panelManagement_OnEvtDateTimeChanged);
            m_panelManagement.EvtActionSignalItem += new Action<CONN_SETT_TYPE, ActionSignal, int>(panelManagement_OnEvtActionSignalItem);
            m_panelManagement.EvtExportDo += new DelegateFunc(panelManagement_OnEvtExportDo);
            #endregion

            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);
        }

        private void threadDraw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void threadDraw_DoWork(object sender, DoWorkEventArgs e)
        {
            draw((CONN_SETT_TYPE)e.Argument);

            e.Result = 0;
        }

        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelAISKUESOTIASSODay(IContainer container, int iListenerConfigId, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(iListenerConfigId, listTec)
        {
            container.Add(this);
        }
        ///// <summary>
        ///// Деструктор
        ///// </summary>
        //~PanelSOTIASSODay ()
        //{
        //    m_tecView = null;
        //}
        
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
        /// Инициализация и размещение собственных элементов управления
        /// </summary>
        private void initializeComponent()
        {
            CONN_SETT_TYPE type = CONN_SETT_TYPE.UNKNOWN;
            System.Windows.Forms.SplitContainer stctrMain
                , stctrView
                , stctrAIISKUE, stctrSOTIASSO;

            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement(); // панель для размещения элементов управления

            //Создать, настроить размещение таблиц для отображения значений
            type = CONN_SETT_TYPE.DATA_AISKUE;
            m_dictDataGridViewValues.Add(type, new HDataGridView()); // АИИСКУЭ-значения
            m_dictDataGridViewValues[type].Name = KEY_CONTROLS.DGV_AIISKUE_VALUE.ToString();
            m_dictDataGridViewValues[type].Dock = DockStyle.Fill;
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            m_dictDataGridViewValues.Add(type, new HDataGridView()); // СОТИАССО-значения
            m_dictDataGridViewValues[type].Name = KEY_CONTROLS.DGV_SOTIASSO_VALUE.ToString();
            m_dictDataGridViewValues[type].Dock = DockStyle.Fill;

            //Создать, настроить размещение графических панелей
            type = CONN_SETT_TYPE.DATA_AISKUE;
            m_dictZGraphValues.Add(type, new HZedGraphControl()); // графическая панель для отображения АИИСКУЭ-значений
            m_dictZGraphValues [type].Tag = type;
            m_dictZGraphValues [type].Name = KEY_CONTROLS.ZGRAPH_AIISKUE.ToString();
            m_dictZGraphValues[type].Dock = DockStyle.Fill;
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            m_dictZGraphValues.Add(type, new HZedGraphControl()); // графическая панель для отображения СОТИАССО-значений
            m_dictZGraphValues [type].Tag = type;
            m_dictZGraphValues [type].Name = KEY_CONTROLS.ZGRAPH_SOTIASSO.ToString();
            m_dictZGraphValues[type].Dock = DockStyle.Fill;

            //Создать контейнеры-сплиттеры, настроить размещение 
            stctrMain = new SplitContainer(); // для главного контейнера (вертикальный)
            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;
            stctrView = new SplitContainer(); // для вспомогательного (2 панели) контейнера (горизонтальный)
            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;
            stctrAIISKUE = new SplitContainer(); // для вспомогательного (таблица + график) контейнера (вертикальный)
            stctrAIISKUE.Dock = DockStyle.Fill;
            stctrAIISKUE.Orientation = Orientation.Vertical;
            stctrSOTIASSO = new SplitContainer(); // для вспомогательного (таблица + график) контейнера (вертикальный)            
            stctrSOTIASSO.Dock = DockStyle.Fill;
            stctrSOTIASSO.Orientation = Orientation.Vertical;

            //Приостановить прорисовку текущей панели
            this.SuspendLayout();

            //Добавить во вспомогательный контейнер элементы управления АИИСКУЭ
            type = CONN_SETT_TYPE.DATA_AISKUE;
            stctrAIISKUE.Panel1.Controls.Add(m_dictDataGridViewValues[type]);
            stctrAIISKUE.Panel2.Controls.Add(m_dictZGraphValues[type]);
            //Добавить во вспомогательный контейнер элементы управления СОТИАССО
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            stctrSOTIASSO.Panel1.Controls.Add(m_dictDataGridViewValues[type]);
            stctrSOTIASSO.Panel2.Controls.Add(m_dictZGraphValues[type]);
            //Добавить вспомогательные контейнеры
            stctrView.Panel1.Controls.Add(stctrAIISKUE);
            stctrView.Panel2.Controls.Add(stctrSOTIASSO);
            //Добавить элементы управления к главному контейнеру
            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            stctrMain.SplitterDistance = 43;

            //Добавить к текущей панели единственный дочерний (прямой) элемент управления - главный контейнер-сплиттер
            this.Controls.Add(stctrMain);
            //Возобновить прорисовку текущей панели
            this.ResumeLayout(false);
            //Принудительное применение логики макета
            this.PerformLayout();
        }

        private void panelManagement_OnEvtActionSignalItem(CONN_SETT_TYPE type, ActionSignal action, int indxSignal)
        {
            SIGNAL signal;
            bool bCheckStateReverse = action == ActionSignal.CHECK;

            switch (action) {
                case ActionSignal.CHECK:
                    signal = m_HandlerQueue.Signals[type].ElementAt(indxSignal);

                    if (m_dictDataGridViewValues[type].ActionColumn(signal.kks_code
                        , signal.name_shr) == true) {
                        // запросить значения для заполнения нового столбца
                        //??? оптимизация, сравнение с предыдущим, полученным по 'SELECT', набором значений - должны совпадать => запрос не требуется                        
                        m_dictDataGridViewValues [type].Fill (m_HandlerQueue.Values(type));
                    } else
                        // столбец удален - ничего не делаем
                        ;
                    break;
                case ActionSignal.SELECT:
                    //??? оптимизация, поиск в табличном представлении ранее запрошенных/полученных наборов значений
                    //m_HandlerQueue.Push(this, new object[] { new object[] { new object[] { HandlerSignalQueue.EVENT.VALUES, type, indxSignal } } });
                    DataAskedHost(new object[] { new object[] { HandlerSignalQueue.EVENT.CUR_VALUES, type, indxSignal } });
                    break;
                default:
                    break;
            }
        }

        #region Константы для работы с шаблоном отчета
        private const int STARTROW_TEMPLATE_SOTIASSO_DAY = 6;
        private const int STARTCOLUMN_TEMPLATE_SOTIASSO_DAY = 1;
        private const string NAMEFOLDER_TEMPLATE_SOTIASSO_DAY = @"Template";
        private const string NAMEFILE_TEMPLATE_SOTIASSO_DAY = @"Отчет по подинтервальной мощности_TEC_DATE.xls";
        private const string NAMEFOLDER_STATISTIC = @"Statistic";
        private const int I_AGREGATE = 30;
        #endregion

        /// <summary>
        /// Обработчик события - нажатие кнопки "Экспорт"
        /// </summary>
        private void panelManagement_OnEvtExportDo()
        {
            KEY_CONTROLS key_ctrl = KEY_CONTROLS.UNKNOWN;
            HDataGridView dgv;
            string pathRemoteTemplate = string.Empty
                , pathUserTemplate = string.Empty
                , nameFolderRemoteTemplate = NAMEFOLDER_TEMPLATE_SOTIASSO_DAY
                , nameFolderUserTemplate = string.Empty
                , nameFileTemplate = NAMEFILE_TEMPLATE_SOTIASSO_DAY;
            ExcelFile excel;
            System.Data.DataTable tableExportDo;
            int iColumn = STARTCOLUMN_TEMPLATE_SOTIASSO_DAY
                , iStartRow = STARTROW_TEMPLATE_SOTIASSO_DAY
                , i_agregate = I_AGREGATE;
            //Excel Application Object
            Microsoft.Office.Interop.Excel.Application oExcelApp;
            Microsoft.Office.Interop.Excel.Workbook oExcelWorkbook;

            try {
                // определить путь к шаблону отчета
                pathRemoteTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(nameFolderRemoteTemplate, nameFileTemplate));
                // определить каталог для размещения итогового отчета
                nameFolderUserTemplate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Statistic");
                // определить полный путь для размещения итогового отчета
                pathUserTemplate = Path.Combine(nameFolderUserTemplate
                    , nameFileTemplate.Replace(@"TEC", (string)(findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox).SelectedItem)
                        .Replace(@"DATE", m_panelManagement.CurDateTime.Date.ToString(@"yyyyMMdd")));
                // создать при необходимости каталог на ПК пользователя
                if (Directory.Exists(nameFolderUserTemplate) == false)
                    Directory.CreateDirectory(nameFolderUserTemplate);
                else
                    ;
                // проверить: выполняется ли уже MS Excel
                oExcelApp = null;
                try {
                    oExcelApp = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                } catch {
                } finally {                    
                    if (object.Equals(oExcelApp, null) == true)
                    // если не выполняется - создать объект MS Excel
                        oExcelApp = new Microsoft.Office.Interop.Excel.Application();
                    else
                        ;
                }
                // проверить: существует ли уже файл с отчетом
                if (File.Exists(pathUserTemplate) == true)
                    try {
                        // удалить файл, т.к. на его место запишем новый
                        File.Delete(pathUserTemplate);
                    } catch {
                        // нет доступа к файлу, возможно, он уже открыт в MS Excel - найти открытую книгу с известным наименованием
                        oExcelWorkbook = oExcelApp.Workbooks.Cast<Microsoft.Office.Interop.Excel.Workbook>().FirstOrDefault(book => { return book.FullName.Equals(pathUserTemplate) == true; });
                        if (object.Equals(oExcelWorkbook, null) == false)
                        // закрыть - если была найдена
                            oExcelWorkbook.Close(false);
                        else
                            ;
                        // книгу "освободили" - можно удалить
                        try {
                        // но только в том случае, если книга была занята действительно MS Excel
                            File.Delete(pathUserTemplate);
                        } catch {
                        // иначе - выход (книгу блокирует другая программа)
                            return;
                        }
                    }
                else
                    ;
                // копируем шаблон на ПК пользователя
                File.Copy(pathRemoteTemplate, pathUserTemplate);
                // объект для работы с ячейками книги MS Excel
                excel = new GemBox.Spreadsheet.ExcelFile();
                excel.LoadXls(pathUserTemplate);

                //excel.Worksheets[0].Rows[0].Cells[0].Value = i_agregate;
                // устанавливаем дату
                excel.Worksheets[0].Rows[0].Cells[1].Value = m_panelManagement.CurDateTime.Date.ToShortDateString();
                excel.Worksheets[0].Rows[0].Cells[2].Value = m_panelManagement.CurDateTime.Date.AddDays(1).ToShortDateString();

                foreach (CONN_SETT_TYPE conn_sett_type in _types) {
                    key_ctrl = conn_sett_type == CONN_SETT_TYPE.DATA_AISKUE ? KEY_CONTROLS.DGV_AIISKUE_VALUE
                        : conn_sett_type == CONN_SETT_TYPE.DATA_SOTIASSO ? KEY_CONTROLS.DGV_SOTIASSO_VALUE
                            : KEY_CONTROLS.UNKNOWN;

                    if (!(key_ctrl == KEY_CONTROLS.UNKNOWN)) {
                        dgv = findControl(key_ctrl.ToString()) as HDataGridView;

                        try {
                            tableExportDo = dgv.GetValues();
                            if (tableExportDo.Columns.Count > 0) {
                                excel.Worksheets[0].InsertDataTable(tableExportDo, iStartRow - 1, iColumn, true);

                                iColumn += tableExportDo.Columns.Count;
                            } else
                            // нет столбцов - нет строк - нет значений для вставки
                                ;
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, string.Format(@"PanelSOTIASSODay::panelManagement_OnEvtExportDo () - conn_sett_type={0}, заполнение значениями...", conn_sett_type), Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    } else
                        Logging.Logg().Error(string.Format(@"PanelSOTIASSODay::panelManagement_OnEvtExportDo () - не удалось найти элемент графического интерфейса для {0}", conn_sett_type)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }

                excel.SaveXls(pathUserTemplate);

                //System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"excel.exe", string.Format("\"{0}\"", pathUserTemplate)));

                oExcelApp.Workbooks.Open(pathUserTemplate);
                oExcelApp.Visible = true;
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"PanelSOTIASSODay::panelManagement_OnEvtExportDo () - подготовка шаблона..."), Logging.INDEX_MESSAGE.NOT_SET);
            } 
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_HandlerQueue.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_HandlerQueue.Start();
        }
        
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            //Проверить актуальность объекта обработки запросов
            if (!(m_HandlerQueue == null)) {
                if (m_HandlerQueue.Actived == true)
                    //Если активен - деактивировать
                    m_HandlerQueue.Activate(false);
                else
                    ;

                if (m_HandlerQueue.IsStarted == true)
                    //Если выполняется - остановить
                    m_HandlerQueue.Stop();
                else
                    ;

                //m_tecView = null;
            } else
                ;

            //Остановить базовый объект
            base.Stop();
        }
        
        /// <summary>
        /// Переопределение наследуемой функции - активация/деактивация объекта
        /// </summary>
        public override bool Activate(bool active)
        {
            bool bRes = false;

            int dueTime = System.Threading.Timeout.Infinite;
            ComboBox cbxTECList;

            bRes = base.Activate(active);

            m_HandlerQueue.Activate(active);

            if (m_HandlerQueue.Actived == true) {
                dueTime = 0;
            } else {
                m_HandlerQueue.ReportClear(true);
            }

            // признак 1-ой активации можно получить у любого объекта в словаре
            if ((m_HandlerQueue.IsFirstActivated == true)
                    & (IsFirstActivated == true)) {
                cbxTECList = findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox;
                // инициировать начало заполнения дочерних элементов содержанием
                cbxTECList.SelectedIndex = -1;
                if (cbxTECList.Items.Count > 0)
                    cbxTECList.SelectedIndex = 0;
                else
                    Logging.Logg().Error(@"PanelSOTIASSODay::Activate () - не заполнен список с ТЭЦ...", Logging.INDEX_MESSAGE.NOT_SET);
            } else
                ;

            return bRes;
        }
        
        /// <summary>
        /// Обработчик события - изменения даты/номера часа на панели с управляющими элементами
        /// </summary>
        /// <param name="dtNew">Новые дата/номер часа</param>
        private void panelManagement_OnEvtDateTimeChanged(ActionDateTime action_changed)
        {
            KEY_CONTROLS key_ctrl = KEY_CONTROLS.UNKNOWN;
            CheckedListBox clb = null;

            foreach (CONN_SETT_TYPE conn_sett_type in _types) {
                //Очистить графические представления
                m_dictZGraphValues[conn_sett_type].Clear();
                //Очистить табличные представления значений
                m_dictDataGridViewValues[conn_sett_type].Clear();
            }
            //??? либо автоматический опрос в 'm_HandlerDb'
            m_HandlerQueue.UserDate = new HandlerSignalQueue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
            // , либо организация цикла опроса в этой функции
            //...
            // , либо вызов метода с аргументами
            //m_HandlerDb.Request(...);
            foreach (CONN_SETT_TYPE conn_sett_type in _types) {
                key_ctrl = conn_sett_type == CONN_SETT_TYPE.DATA_AISKUE ? KEY_CONTROLS.CLB_AIISKUE_SIGNAL
                    : conn_sett_type == CONN_SETT_TYPE.DATA_SOTIASSO ? KEY_CONTROLS.CLB_SOTIASSO_SIGNAL
                        : KEY_CONTROLS.UNKNOWN;

                if (!(key_ctrl == KEY_CONTROLS.UNKNOWN)) {
                    clb = findControl(key_ctrl.ToString()) as CheckedListBox;

                    foreach (int indx in clb.CheckedIndices.Cast<int>())
                        DataAskedHost(new object[] { new object[] { HandlerSignalQueue.EVENT.CHECK_VALUES, conn_sett_type, indx } });

                    DataAskedHost(new object[] { new object[] { HandlerSignalQueue.EVENT.CUR_VALUES, conn_sett_type, clb.SelectedIndex } });
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay::panelManagement_OnEvtDateTimeChanged () - не удалось найти элемент графического интерфейса для {0}", conn_sett_type)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private void draw(CONN_SETT_TYPE type)
        {
            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;

            if (m_HandlerQueue.Values(type).Count() > 0) {                
                // отобразить
                m_dictZGraphValues[type].Draw(m_HandlerQueue.Values(type)
                    , type == CONN_SETT_TYPE.DATA_AISKUE ? @"АИИСКУЭ" : type == CONN_SETT_TYPE.DATA_SOTIASSO ? @"СОТИАССО" : @"Неизвестный тип", textGraphCurDateTime);
            } else
                Logging.Logg().Error(string.Format(@"PanelSOTIASSODay::draw (type={0}) - нет ни одного значения за [{1}]...", type, m_HandlerQueue.UserDate), Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Текст (часть) заголовка для графической субобласти
        /// </summary>
        private string textGraphCurDateTime
        {
            get {
                return m_panelManagement.CurDateTime.ToShortDateString();
            }
        }
        
        /// <summary>
        /// Перерисовать объекты с графическим представлением данных
        ///  , в зависимости от типа графического представления (гистограмма, график)
        /// </summary>
        /// <param name="type">Тип изменений, выполненных пользователем</param>
        public override void UpdateGraphicsCurrent(int type)
        {
            foreach (CONN_SETT_TYPE conn_sett_type in new CONN_SETT_TYPE[] { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO }) {
                draw(conn_sett_type);
            }
        }
        
        /// <summary>
        /// Обработчик события - изменение выбора строки в списке ТЭЦ
        /// </summary>
        /// <param name="indxTEC">Индекс выбранного элемента</param>
        private void panelManagement_TECListOnSelectionChanged(int indxTEC)
        {
            IEnumerable<TECComponentBase> listAIISKUESignalNameShr = new List<TECComponentBase>()
                , listSOTIASSOSignalNameShr = new List<TECComponentBase>();

            if (!(indxTEC < 0)
                && (indxTEC < m_listTEC.Count)) {
                foreach (CONN_SETT_TYPE conn_sett_type in _types) {
                    //Очистить графические представления
                    m_dictZGraphValues[conn_sett_type].Clear();
                    //Очистить табличные представления значений
                    m_dictDataGridViewValues[conn_sett_type].Clear();
                    //Очистить списки с сигналами
                    m_panelManagement.ClearSignalList(conn_sett_type);
                }
                //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
                foreach (CONN_SETT_TYPE type in _types) {
                    //m_HandlerQueue.Push(this, new object[] { new object[] { new object[] { HandlerSignalQueue.EVENT.LIST_SIGNAL, m_listTEC[indxTEC].m_id, type/*.ToString()*/ } } });
                    DataAskedHost(new object[] { new object[] { HandlerSignalQueue.EVENT.LIST_SIGNAL, m_listTEC[indxTEC].m_id, type/*.ToString()*/ } });
                    //EvtDataAskedHost(new object[] { new object[] { HandlerSignalQueue.EVENT.LIST_SIGNAL, m_listTEC[indxTEC].m_id, type/*.ToString()*/ } });
                }
                //Добавить строки(сигналы) на дочернюю панель(список АИИСКУЭ, СОТИАССО-сигналов)
                // - по возникновению сигнала окончания заппроса                
            } else
                ;
        }

        #region Интерфейс IDataHost
        /// <summary>
        /// Не используется
        /// </summary>
        public event DelegateObjectFunc EvtDataAskedHost;
        /// <summary>
        /// Не используется
        /// </summary>
        /// <param name="par">Аргумент для передачи обработчику очереди событий</param>
        public void DataAskedHost(object par)
        {
            m_HandlerQueue.Push(this, new object[] { par });
        }

        public void OnEvtDataRecievedHost(object res)
        {
            if (InvokeRequired == true)
                Invoke(new Action<EventArgsDataHost>(onEvtCompleted), res as EventArgsDataHost);
            else
                onEvtCompleted(res as EventArgsDataHost);
        }
        #endregion

        private void onEvtCompleted(EventArgsDataHost ev)
        {
            HandlerSignalQueue.EVENT evt;
            CONN_SETT_TYPE type;
            SIGNAL signal;
            string kks_code = string.Empty;

            evt = (HandlerSignalQueue.EVENT)ev.par[0];
            type = (CONN_SETT_TYPE)ev.par[1];

            switch (evt) {
                case HandlerSignalQueue.EVENT.LIST_SIGNAL:
                    m_panelManagement.InitializeSignalList(type, m_HandlerQueue.Signals[type].Select(sgnl => { return sgnl.name_shr; }));
                    break;
                case HandlerSignalQueue.EVENT.CUR_VALUES:
                    draw(type);
                    //m_threadDraw.RunWorkerAsync(type);                    
                    break;
                case HandlerSignalQueue.EVENT.CHECK_VALUES:
                    kks_code = (string)ev.par[2];
                    signal = m_HandlerQueue.Signals[type].FirstOrDefault(sgnl => { return sgnl.kks_code.Equals(kks_code) == true; });

                    if (m_dictDataGridViewValues[type].ActionColumn(signal.kks_code
                        , signal.name_shr) == true) {
                        // запросить значения для заполнения нового столбца
                        //??? оптимизация, сравнение с предыдущим, полученным по 'SELECT', набором значений - должны совпадать => запрос не требуется
                        m_dictDataGridViewValues[type].Fill(m_HandlerQueue.Values(type));
                    } else
                        // столбец удален - ничего не делаем
                        ;
                    break;
                default:
                    break;
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }

            set
            {
                base.BackColor = value;

                if (Equals (m_panelManagement, null) == false)
                    m_panelManagement.BackColor = BackColor;
                else
                    ;
            }
        }
    }
}