﻿using System;
using System.Data.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;
using System.Reflection;

namespace CommonAux
{
    /// <summary>
    /// Класс для описания параметров сигнала
    /// </summary>
    public class SIGNAL
    {
        /// <summary>
        /// Ключ сигнала (идентификатор устройства + № канала опроса)
        /// </summary>
        public struct KEY
        {
            public int m_object;

            public int m_item;

            public KEY(int obj, int item)
            {
                m_object = obj;

                m_item = item;
            }

            public KEY(KEY key)
            {
                m_object = key.m_object;

                m_item = key.m_item;
            }

            public static bool operator ==(KEY key1, KEY key2)
            {
                return (key1.m_object == key2.m_object)
                    && (key1.m_item == key2.m_item);
            }

            public static bool operator !=(KEY key1, KEY key2)
            {
                return (!(key1.m_object == key2.m_object))
                    || (!(key1.m_item == key2.m_item));
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(@"[object={0}, item={1}]", m_object, m_item);
            }
        }
        /// <summary>
        /// Описание сигнала
        /// </summary>
        public string m_strDesc;

        public SIGNAL.KEY m_key;
        /// <summary>
        /// Признак использования сигнала при расчете
        /// </summary>
        public bool m_bUse;
        /// <summary>
        /// Конструктор - дополнительный (с параметрами)
        /// </summary>
        /// <param name="desc">Описание сигнала (наименование)</param>
        /// <param name="idUSPD">Идентификатор УСПД</param>
        /// <param name="id">Номер канала опроса</param>
        public SIGNAL(string desc, int idUSPD, int id, bool bUse)
        {
            m_strDesc = desc;
            m_key = new SIGNAL.KEY(idUSPD, id);
            m_bUse = bUse;
        }

        public SIGNAL(string desc, SIGNAL.KEY key, bool bUse)
        {
            m_strDesc = desc;
            m_key = new SIGNAL.KEY(key);
            m_bUse = bUse;
        }
    }

    public class MSExcelIO : HClassLibrary.MSExcelIO
    {
        public enum INDEX_MSEXCEL_COLUMN { APOWER, SNUZHDY }

        public MSExcelIO(string path) : base()
        {
            OpenDocument(path);
        }
        /// <summary>
        /// Записать результат в книгу MS Excel
        /// </summary>
        /// <param name="table">Таблица с данными для записи</param>
        /// <param name="col">Столбец на листе книги</param>
        public void WriteValues(string nameSheet, int col, int sheeftRow, double[] arValues)
        {
            int row = sheeftRow;

            SelectWorksheet(nameSheet);

            foreach (double val in arValues)
            {
                base.WriteValue(nameSheet, col, row, val.ToString(@"F3"));
                row++;
            }
        }

        public void WriteValues(int col, int sheeftRow, double[] arValues)
        {
            int row = sheeftRow;

            foreach (double val in arValues)
            {
                base.WriteValue(col, row, val.ToString(@"F3"));
                row++;
            }
        }

        public void WriteDate(int col, int sheeftRow, DateTime date)
        {
            base.WriteValue(col, sheeftRow, date.ToShortDateString());
        }
    }

    public partial class PanelCommonAux : PanelStatistic
    {
        /// <summary>
        /// Режим отображения (в составе статистики/самостоятельная вкладка)
        /// </summary>
        protected int m_displayMode;
        /// <summary>
        /// Объект с параметрами соединения с источником данных
        /// </summary>
        protected List<ConnectionSettings> m_connSettAIISKUECentre;
        /// <summary>
        /// Список ТЭЦ с параметрами из файла конфигурации
        /// </summary>
        protected List<TEC_LOCAL> m_listTEC;
        /// <summary>
        /// Объект для инициализации входных параметров
        /// </summary>
        protected GetDataFromDB m_GetDataFromDB;
        protected DbConnection m_connConfigDB;
        protected int _iListenerId;
        private enum INDEX_CONTROL : short { LB_TEC, LB_GROUP_SIGNAL }
        /// <summary>
        /// Поля таблицы сигналов
        /// </summary>
        public enum DB_TABLE_DATA
        {
            ID, ID_TEC, GROUP, NAME, DESCRIPTION, USPD, CHANNEL, USE
        };

        private const string MS_EXCEL_FILTER = @"Книга MS Excel 2010 (*.xls, *.xlsx)|*.xls;*.xlsx";
        /// <summary>
        /// Перечисление причин, влияющих на готовность к экспорту значений
        /// </summary>
        private enum INDEX_READY { TEMPLATE, DATE }
        /// <summary>
        /// Объект содержащий признаки готовности к экспорту значений
        /// </summary>
        HMark m_markReady;
        private enum INDEX_MSEXCEL_PARS
        {
            /// <summary>
            /// Полный путь к (исходному) шаблону
            /// </summary>
            TEMPLATE_PATH_DEFAULT
            /// <summary>
            /// Наименование (исходного) шаблона
            /// </summary>
            , TEMPLATE_NAME
            /// <summary>
            /// Наименование листа в книге-шаблоне
            /// </summary>
            , SHEET_NAME
            /// <summary>
            /// Номер столбца для отображения даты
            /// </summary>
            , COL_DATADATE
            /// <summary>
            /// Смещение относительно 1-ой строки в книге MS Excel
            ///  для записи значений за 1-ый день месяца
            /// </summary>
            , ROW_START
            /// <summary>
            /// Количество строк в книге MS Excel на сутки
            /// </summary>
            , ROWCOUNT_DATE
            /// <summary>
            /// Версия шаблона книги MS Excel
            /// </summary>
            , TEMPLATE_VER
            /// <summary>
            /// Количество параметров для шаблона книги MS Excel
            /// </summary>
            , COUNT
        }
        /// <summary>
        /// Массив с параметрами шаблона книги MS Excel
        /// </summary>
        private string[] m_arMSEXEL_PARS;
        /// <summary>
        /// Перечисление возможных состояний приложения
        /// </summary>
        private enum STATE { UNKNOWN = -1, READY, ERROR }

        private bool m_bVisibleLog;
        /// <summary>
        /// Признак видимости элемента управления с содержанием лог-сообщений
        /// </summary>
        private bool VisibleLog { get { return m_bVisibleLog; } set { m_bVisibleLog = value; } }

        //private STATE m_State;
        /// <summary>
        /// Состояние приложения
        /// </summary>
        STATE State { get { return (m_markReady.IsMarked((int)INDEX_READY.TEMPLATE) && m_markReady.IsMarked((int)INDEX_READY.DATE)) == true ? STATE.READY : STATE.ERROR; } }

        private string m_strFullPathTemplate;
        /// <summary>
        /// Строка - полный путь для шаблона MS Excel
        /// </summary>
        /// 
        private string FullPathTemplate
        {
            get { return m_strFullPathTemplate; }

            set
            {
                if ((m_strFullPathTemplate == null)
                    || ((!(m_strFullPathTemplate == null))
                        && (m_strFullPathTemplate.Equals(value) == false)))
                {
                    m_strFullPathTemplate = value;

                    if ((value.Equals(string.Empty) == false)
                        && (Path.GetDirectoryName(value).Equals(string.Empty) == false)
                        && (Path.GetFileName(value).Equals(string.Empty) == false))
                        EventNewPathToTemplate(m_strFullPathTemplate);
                    else
                        ;
                }
                else
                    ;
            }
        }
        /// <summary>
        /// Событие при назначении нового пути для шаблона MS Excel
        /// </summary>
        private event DelegateStringFunc EventNewPathToTemplate;

        private System.Windows.Forms.Button m_btnLoad;
        private System.Windows.Forms.Button m_btnOpen;
        private System.Windows.Forms.Button m_btnExit;
        private System.Windows.Forms.Button m_btnStripButtonExcel;
        private System.Windows.Forms.ListBox m_listBoxTEC;
        private System.Windows.Forms.ListBox m_listBoxGrpSgnl;
        private System.Windows.Forms.MonthCalendar m_monthCalendarStart;
        private System.Windows.Forms.MonthCalendar m_monthCalendarEnd;
        private System.Windows.Forms.Label m_labelTEC;
        private System.Windows.Forms.Label m_labelGrpSgnl;
        private System.Windows.Forms.Label m_labelValues;
        private System.Windows.Forms.Label m_labelStartDate;
        private System.Windows.Forms.Label m_labelEndDate;

        private List<System.Windows.Forms.Label> m_labelsGroup;

        private List<DataGridViewValues> m_dgvValues;
        private DataGridView m_sumValues;
        /// <summary>
        /// Требуется переменная конструктора
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Конструктор панели
        /// </summary>
        public PanelCommonAux(string pathTemplate)
        {
            m_GetDataFromDB = new GetDataFromDB();
            m_connSettAIISKUECentre = new List<ConnectionSettings>();

            InitializeComponents();

            m_listBoxTEC.Tag = INDEX_CONTROL.LB_TEC;
            m_listBoxTEC.SelectedIndexChanged += listBox_SelectedIndexChanged;

            //Установить обработчики событий
            EventNewPathToTemplate += new DelegateStringFunc(onNewPathToTemplate);

            m_arMSEXEL_PARS = pathTemplate.Split(',');
        }

        /// <summary>
        /// Фукнция вызова старта программы
        /// </summary>
        public override void Start()
        {
            base.Start();

            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_sumValues.Rows.Add();
                m_sumValues.Rows[i].Cells[0].Value = "сум " + Enum.GetName(typeof(TEC_LOCAL.INDEX_DATA), i);
                m_sumValues.Rows[i].Cells[1].Value = "0";
            }

            int err = -1;
            // зарегистрировать соединение/получить идентификатор соединения
            _iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            m_connConfigDB = DbSources.Sources().GetConnection(_iListenerId, out err);

            m_listTEC = GetListTEC(new InitTEC_200(_iListenerId, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec);

            foreach (TEC_LOCAL tec in m_listTEC)
            {
                m_listBoxTEC.Items.Add(tec.m_strNameShr);
            }

            m_labelEndDate.Text = m_monthCalendarEnd.SelectionStart.ToShortDateString();
            m_labelStartDate.Text = m_monthCalendarStart.SelectionStart.ToShortDateString();

            //Установить начальные признаки готовности к экспорту
            m_markReady = new HMark(0);

            FullPathTemplate = string.Empty;
        }

        /// <summary>
        /// Функция активация вкладки
        /// </summary>
        /// <param name="activated">параметр</param>
        /// <returns>результат</returns>
        public override bool Activate(bool activated)
        {
            int err = -1; 
            bool bRes = base.Activate(activated);

            if (base.IsFirstActivated == true)
            {
                m_GetDataFromDB.InitChannels(m_connConfigDB, m_listTEC);

                foreach (TEC_LOCAL t in m_listTEC)
                {
                    t.InitSensors();
                    //Получить параметры соединения с источником данных
                    m_connSettAIISKUECentre.Add(m_GetDataFromDB.GetConnSettAIISKUECentre(ref _iListenerId, t.m_IdSource, out err));
                }

                m_listBoxTEC.SelectedIndex = 0;
            }

            return bRes;
        }

        /// <summary>
        /// Обработчик события установки нового значения для пути к шаблону
        /// </summary>
        /// <param name="path">Строка - полный путь к шаблону</param>
        private void onNewPathToTemplate(string path)
        {
            //Проверить строку на наличие в ней значения - изменить состояние программы
            m_markReady.Set((int)INDEX_READY.TEMPLATE, path.Equals(string.Empty) == false);
            //Установить признак дотупности для элементов интерфейса экспорта в книгу MS Excel
            // п. главного меню + кнопка на панели быстрого доступа
            enableBtnExcel(State == STATE.READY);
        }

        /// <summary>
        /// Включить/отключить доступность интерфейса экспорта в книгу MS Excel
        /// </summary>
        /// <param name="bEnabled">Признак включения/отключения</param>
        private void enableBtnExcel(bool bEnabled)
        {
            m_btnStripButtonExcel.Enabled =
                bEnabled;
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            if (!(m_GetDataFromDB == null))
            {
                m_GetDataFromDB.SetDelegateReport(ferr, fwar, fact, fclr);
            }
        }

        /// <summary>
        /// Возвраить список объектов ТЭЦ (без ЛК38)
        /// </summary>
        /// <returns>Список объектов ТЭЦ</returns>
        public List<TEC_LOCAL> GetListTEC(List<TEC> tec)
        {
            List<TEC_LOCAL> listRes = new List<TEC_LOCAL>();
            TEC_LOCAL tec_local;

            for (int i = 0; i < tec.Count - 1; i++)
            {
                tec_local = new TEC_LOCAL();

                tec_local.m_Id = tec[i].m_id;
                tec_local.m_strNameShr = tec[i].name_shr;
                tec_local.m_IdSource = Convert.ToInt32(tec[i].GetAddingParameter(TEC.ADDING_PARAM_KEY.ID_SOURCE_AISKUE_TSN));
                List<int> list_column = new List<int>();

                foreach (string s in tec[i].GetAddingParameter(TEC.ADDING_PARAM_KEY.COLUMN_TSN_EXCEL).ToString().Split(','))
                {
                    list_column.Add(Convert.ToInt32(s));
                }

                tec_local.m_arMSExcelNumColumns = list_column.ToArray();

                for (int j = 0; j < tec_local.m_arListSgnls.Count(); j++)
                {
                    tec_local.m_arListSgnls[j] = new List<SIGNAL>();
                }

                listRes.Add(tec_local);
            }

            return listRes;
        }

        private int setFullPathTemplate(string strFullPathTemplate)
        {
            int iRes = 0; // исходное состояние - нет ошибки

            iRes = validateTemplate(strFullPathTemplate);
            if (iRes == 0)
            {
                // сохранить каталог с крайним прошедшим
                FullPathTemplate =
                    strFullPathTemplate;
            }
            else
                ;

            return iRes;
        }

        /// <summary>
        /// Определить размеры ячеек макета панели
        /// </summary>
        /// <param name="cols">Количество столбцов в макете</param>
        /// <param name="rows">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            //initializeLayoutStyleEvenly(100, 100);
            initializeLayoutStyleEvenly(20, 25);
            //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        }

        protected virtual void InitializeComponents()
        {
            #region Инициализация переменных
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_btnLoad = new System.Windows.Forms.Button();
            this.m_btnOpen = new System.Windows.Forms.Button();
            this.m_btnExit = new System.Windows.Forms.Button();
            this.m_btnStripButtonExcel = new System.Windows.Forms.Button();

            this.m_listBoxTEC = new System.Windows.Forms.ListBox();
            this.m_listBoxGrpSgnl = new System.Windows.Forms.ListBox();
            this.m_monthCalendarStart = new System.Windows.Forms.MonthCalendar();
            this.m_monthCalendarEnd = new System.Windows.Forms.MonthCalendar();
            this.m_labelTEC = new System.Windows.Forms.Label();
            this.m_labelGrpSgnl = new System.Windows.Forms.Label();
            this.m_labelValues = new System.Windows.Forms.Label();
            this.m_labelStartDate = new System.Windows.Forms.Label();
            this.m_labelEndDate = new System.Windows.Forms.Label();

            m_sumValues = new DataGridView();

            this.m_dgvValues = new List<DataGridViewValues>();
            this.m_labelsGroup = new List<System.Windows.Forms.Label>();
            for ( int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues.Add(new DataGridViewValues());
                ((System.ComponentModel.ISupportInitialize)(this.m_dgvValues[i])).BeginInit();

                m_labelsGroup.Add(new System.Windows.Forms.Label());
            }

            ((System.ComponentModel.ISupportInitialize)(this.m_sumValues)).BeginInit();

            #endregion

            components = new System.ComponentModel.Container();

            this.SuspendLayout();

            //this.Controls.Add(m_btnLoad, 81, 40); this.SetColumnSpan(m_btnLoad, 18); this.SetRowSpan(m_btnLoad, 5);
            //this.Controls.Add(m_btnOpen, 81, 46); this.SetColumnSpan(m_btnOpen, 18); this.SetRowSpan(m_btnOpen, 5);
            //this.Controls.Add(m_btnExit, 81, 94); this.SetColumnSpan(m_btnExit, 18); this.SetRowSpan(m_btnExit, 5);
            //this.Controls.Add(m_btnStripButtonExcel, 81, 52); this.SetColumnSpan(m_btnStripButtonExcel, 18); this.SetRowSpan(m_btnStripButtonExcel, 5);
            //this.Controls.Add(m_listBoxTEC, 61, 40); this.SetColumnSpan(m_listBoxTEC, 18); this.SetRowSpan(m_listBoxTEC, 20);
            //this.Controls.Add(m_monthCalendarStart, 60, 8); this.SetColumnSpan(m_monthCalendarStart, 15); this.SetRowSpan(m_monthCalendarStart, 15);
            //this.Controls.Add(m_monthCalendarEnd, 80, 8); this.SetColumnSpan(m_monthCalendarEnd, 15); this.SetRowSpan(m_monthCalendarEnd, 15);
            //this.Controls.Add(m_labelTEC, 62, 37); this.SetColumnSpan(m_labelTEC, 11); this.SetRowSpan(m_labelTEC, 2);
            //this.Controls.Add(m_labelValues, 8, 2); this.SetColumnSpan(m_labelValues, 30); this.SetRowSpan(m_labelValues, 2);
            //this.Controls.Add(m_labelStartDate, 65, 6); this.SetColumnSpan(m_labelStartDate, 8); this.SetRowSpan(m_labelStartDate, 2);
            //this.Controls.Add(m_labelEndDate, 85, 6); this.SetColumnSpan(m_labelEndDate, 8); this.SetRowSpan(m_labelEndDate, 2);

            //for (int i = 0; i < m_dgvValues.Count; i++)
            //{
            //    this.Controls.Add(m_dgvValues[i], 8, 4 + i * 16); this.SetColumnSpan(m_dgvValues[i], 50); this.SetRowSpan(m_dgvValues[i], 16);
            //    this.Controls.Add(m_labelsGroup[i], 2, 7 + i * 16); this.SetColumnSpan(m_labelsGroup[i], 5); this.SetRowSpan(m_labelsGroup[i], 2);
            //}
            //this.Controls.Add(m_sumValues, 61, 60); this.SetColumnSpan(m_sumValues, 38); this.SetRowSpan(m_sumValues, 40);

            this.Controls.Add(m_btnLoad, 16, 9); this.SetColumnSpan(m_btnLoad, 4); this.SetRowSpan(m_btnLoad, 2);
            this.Controls.Add(m_btnOpen, 16, 11); this.SetColumnSpan(m_btnOpen, 4); this.SetRowSpan(m_btnOpen, 2);
            //this.Controls.Add(m_btnExit, 81, 94); this.SetColumnSpan(m_btnExit, 18); this.SetRowSpan(m_btnExit, 5);
            this.Controls.Add(m_btnStripButtonExcel, 16, 13); this.SetColumnSpan(m_btnStripButtonExcel, 4); this.SetRowSpan(m_btnStripButtonExcel, 2);
            this.Controls.Add(m_listBoxTEC, 12, 9); this.SetColumnSpan(m_listBoxTEC, 4); this.SetRowSpan(m_listBoxTEC, 6);
            this.Controls.Add(m_monthCalendarStart, 12, 1); this.SetColumnSpan(m_monthCalendarStart, 1); this.SetRowSpan(m_monthCalendarStart, 1);
            this.Controls.Add(m_monthCalendarEnd, 16, 1); this.SetColumnSpan(m_monthCalendarEnd, 1); this.SetRowSpan(m_monthCalendarEnd, 1);
            this.Controls.Add(m_labelTEC, 12, 8); this.SetColumnSpan(m_labelTEC, 2); this.SetRowSpan(m_labelTEC, 1);
            this.Controls.Add(m_labelValues, 2, 0); this.SetColumnSpan(m_labelValues, 5); this.SetRowSpan(m_labelValues, 1);
            this.Controls.Add(m_labelStartDate, 12, 0); this.SetColumnSpan(m_labelStartDate, 3); this.SetRowSpan(m_labelStartDate, 1);
            this.Controls.Add(m_labelEndDate, 16, 0); this.SetColumnSpan(m_labelEndDate, 3); this.SetRowSpan(m_labelEndDate, 1);

            for (int i = 0; i < m_dgvValues.Count; i++)
            {
                this.Controls.Add(m_dgvValues[i], 1, 1 + i * 4); this.SetColumnSpan(m_dgvValues[i], 11); this.SetRowSpan(m_dgvValues[i], 4);
                this.Controls.Add(m_labelsGroup[i], 0, 2 + i * 4); this.SetColumnSpan(m_labelsGroup[i], 1); this.SetRowSpan(m_labelsGroup[i], 1);
            }
            this.Controls.Add(m_sumValues, 12, 15); this.SetColumnSpan(m_sumValues, 8); this.SetRowSpan(m_sumValues, 10);

            this.ResumeLayout();

            initializeLayoutStyle();

            #region Параметры элементов управления
            // 
            // m_btnLoad
            // 
            this.m_btnLoad.Name = "m_btnLoad";
            this.m_btnLoad.TabIndex = 0;
            this.m_btnLoad.Text = "Загрузить";
            this.m_btnLoad.UseVisualStyleBackColor = true;
            this.m_btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            this.m_btnLoad.Dock = DockStyle.Fill;
            // 
            // m_btnOpen
            // 
            this.m_btnOpen.Name = "m_btnSave";
            this.m_btnOpen.TabIndex = 1;
            this.m_btnOpen.Text = "Открыть";
            this.m_btnOpen.UseVisualStyleBackColor = true;
            this.m_btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            this.m_btnOpen.Dock = DockStyle.Fill;
            this.m_btnOpen.Width = m_listBoxTEC.Width;
            // 
            // m_btnExit
            // 
            this.m_btnExit.Name = "m_btnExit";
            this.m_btnExit.TabIndex = 2;
            this.m_btnExit.Text = "Выход";
            this.m_btnExit.UseVisualStyleBackColor = true;
            this.m_btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.m_btnExit.Width = m_monthCalendarStart.Width;
            this.m_btnExit.Visible = false;
            // 
            // m_btnStripButtonExcel
            // 
            this.m_btnStripButtonExcel.Name = "m_btnStripButtonExcel";
            this.m_btnStripButtonExcel.TabIndex = 0;
            this.m_btnStripButtonExcel.Text = "Экспорт";
            this.m_btnStripButtonExcel.UseVisualStyleBackColor = true;
            this.m_btnStripButtonExcel.Click += new System.EventHandler(this.btnStripButtonExcel_Click);
            this.m_btnStripButtonExcel.Enabled = false;
            this.m_btnStripButtonExcel.Dock = DockStyle.Fill;
            this.m_btnStripButtonExcel.Width = m_listBoxTEC.Width;
            // 
            // m_listBoxTEC
            // 
            this.m_listBoxTEC.FormattingEnabled = true;
            this.m_listBoxTEC.Name = "m_listBoxTEC";
            this.m_listBoxTEC.TabIndex = 3;
            this.m_listBoxTEC.Width = m_monthCalendarStart.Width;
            this.m_listBoxTEC.Dock = DockStyle.Fill;
            // 
            // m_listBoxGrpSgnl
            // 
            this.m_listBoxGrpSgnl.FormattingEnabled = true;
            this.m_listBoxGrpSgnl.Name = "m_listBoxGrpSgnl";
            this.m_listBoxGrpSgnl.TabIndex = 4;
            // 
            // m_monthCalendar
            // 
            this.m_monthCalendarStart.Name = "m_monthCalendar";
            this.m_monthCalendarStart.TabIndex = 5;
            // 
            // monthCalendar2
            // 
            this.m_monthCalendarEnd.Name = "monthCalendar2";
            this.m_monthCalendarEnd.TabIndex = 6;
            // 
            // m_labelTEC
            // 
            this.m_labelTEC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_labelTEC.AutoSize = true;
            this.m_labelTEC.Name = "m_labelTEC";
            this.m_labelTEC.TabIndex = 2;
            this.m_labelTEC.Text = "Подразделение";
            // 
            // m_labelGrpSgnl
            // 
            this.m_labelGrpSgnl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_labelGrpSgnl.AutoSize = true;
            this.m_labelGrpSgnl.Name = "m_labelGrpSgnl";
            this.m_labelGrpSgnl.TabIndex = 3;
            this.m_labelGrpSgnl.Text = "Группа сигналов";
            // 
            // m_labelValues
            // 
            this.m_labelValues.AutoSize = true;
            this.m_labelValues.Name = "m_labelValues";
            this.m_labelValues.TabIndex = 5;
            this.m_labelValues.Text = "Значения сигналов (сутки)";
            this.m_labelValues.Dock = DockStyle.Bottom;
            // 
            // m_labelStartDate
            // 
            this.m_labelStartDate.AutoSize = true;
            this.m_labelStartDate.Name = "m_labelStartDate";
            this.m_labelStartDate.TabIndex = 5;
            this.m_labelStartDate.Text = "";
            this.m_labelStartDate.Visible = false;
            // 
            // m_labelEndDate
            // 
            this.m_labelEndDate.AutoSize = true;
            this.m_labelEndDate.Name = "m_labelEndDate";
            this.m_labelEndDate.TabIndex = 5;
            this.m_labelEndDate.Text = "";
            this.m_labelEndDate.Visible = false;
            // 
            // m_labelsGroup
            // 
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                this.m_labelsGroup[i].Name = Enum.GetName(typeof(TEC_LOCAL.INDEX_DATA), i).ToString();
                this.m_labelsGroup[i].TabIndex = 5;
                this.m_labelsGroup[i].Text = Enum.GetName(typeof(TEC_LOCAL.INDEX_DATA), i).ToString();
            }
            // 
            // m_dgvValues
            // 
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                this.m_dgvValues[i].AllowUserToAddRows = false;
                this.m_dgvValues[i].AllowUserToDeleteRows = false;
                this.m_dgvValues[i].AllowUserToOrderColumns = true;
                this.m_dgvValues[i].AllowUserToResizeColumns = false;
                this.m_dgvValues[i].AllowUserToResizeRows = false;
                this.m_dgvValues[i].Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.m_dgvValues[i].AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
                this.m_dgvValues[i].AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
                this.m_dgvValues[i].ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
                dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
                dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
                dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
                dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
                this.m_dgvValues[i].DefaultCellStyle = dataGridViewCellStyle1;
                this.m_dgvValues[i].Name = "m_dgvValues" + i.ToString();
                this.m_dgvValues[i].RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                this.m_dgvValues[i].RowTemplate.ReadOnly = true;
                this.m_dgvValues[i].RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.m_dgvValues[i].SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                this.m_dgvValues[i].TabIndex = 4;
            }
            // 
            // m_sumValues
            // 
            this.m_sumValues.RowHeadersVisible = false;
            this.m_sumValues.AllowUserToAddRows = false;
            this.m_sumValues.AllowUserToDeleteRows = false;
            this.m_sumValues.AllowUserToOrderColumns = true;
            this.m_sumValues.AllowUserToResizeColumns = false;
            this.m_sumValues.AllowUserToResizeRows = false;
            this.m_sumValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_sumValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.m_sumValues.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            //this.m_sumValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.m_sumValues.DefaultCellStyle = dataGridViewCellStyle2;
            this.m_sumValues.Name = "m_sumValues";
            //this.m_sumValues.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.m_sumValues.RowTemplate.ReadOnly = true;
            this.m_sumValues.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_sumValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_sumValues.TabIndex = 4;
            this.m_sumValues.ColumnCount = 2;
            this.m_sumValues.ColumnHeadersVisible = false;



            #endregion

            m_listBoxTEC.Tag = INDEX_CONTROL.LB_TEC;
            m_listBoxTEC.SelectedIndexChanged += listBox_SelectedIndexChanged;
            m_listBoxGrpSgnl.Tag = INDEX_CONTROL.LB_GROUP_SIGNAL;
            m_listBoxGrpSgnl.SelectedIndexChanged += listBox_SelectedIndexChanged;
            m_monthCalendarStart.DateChanged += monthCalendarStart_DateChanged;
            m_monthCalendarEnd.DateChanged += monthCalendarEnd_DateChanged;
        }

        /// <summary>
        /// Проверка допустимости выбранного диапазона дат
        /// </summary>
        /// <returns>Результат проверки</returns>
        private int validateDates()
        {
            int iRes = 0;

            if (!(m_monthCalendarStart.SelectionStart.Year == m_monthCalendarEnd.SelectionStart.Year))
                iRes = -1;
            else
                if (!(m_monthCalendarStart.SelectionStart.Month == m_monthCalendarEnd.SelectionStart.Month))
                iRes = -2;
            else
                    if (m_monthCalendarStart.SelectionStart.Day > m_monthCalendarEnd.SelectionStart.Day)
                iRes = 1;
            else
                ;

            return iRes;
        }

        private void monthCalendarEnd_DateChanged(object sender, DateRangeEventArgs e)
        {
            m_labelEndDate.Text = m_monthCalendarEnd.SelectionStart.ToShortDateString();

            int iRes = validateDates();

            if (iRes == 0)
            {
                m_btnStripButtonExcel.Enabled = true;
                m_GetDataFromDB.ReportClear(true);
            }
            else
            {
                m_btnStripButtonExcel.Enabled = false;
                m_GetDataFromDB.ErrorReport("Некорректный временной диапазон");
            }
        }

        private void monthCalendarStart_DateChanged(object sender, DateRangeEventArgs e)
        {
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearValues();
            }
            m_labelStartDate.Text = m_monthCalendarStart.SelectionStart.ToShortDateString();

            int iRes = validateDates();

            if (iRes == 0)
            {
                m_btnStripButtonExcel.Enabled = true;
                m_GetDataFromDB.ReportClear(true);
            }
            else
            {
                m_btnStripButtonExcel.Enabled = false;
                m_GetDataFromDB.ErrorReport("Некорректный временной диапазон");
            }
        }

        private void updateRowData()
        {
            int rowWight = 200;
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearRows();
                m_dgvValues[i].AddRowData(m_listTEC[m_listBoxTEC.SelectedIndex].m_arListSgnls[i]);
                m_dgvValues[i].RowHeadersWidthSizeMode =
                DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                m_dgvValues[i].RowHeadersWidth = rowWight;
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            { 
                m_sumValues.Rows[i].Cells[1].Value = "0";
            }
            if ((INDEX_CONTROL)((Control)sender).Tag == INDEX_CONTROL.LB_TEC)
            {
                updateRowData();
            }
            else
                if ((INDEX_CONTROL)((Control)sender).Tag == INDEX_CONTROL.LB_GROUP_SIGNAL)
                updateRowData();
            else
                ;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку на панели быстрого доступа "Открыть"
        /// </summary>
        /// <param name="sender">Объект-инициатор события</param>
        /// <param name="e">Аргумент события</param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            bool date_tmp;
            date_tmp = ((object)1 == (object)1);

            //Создать форму диалога выбора файла-шаблона MS Excel
            using (FileDialog formChoiсeTemplate = new OpenFileDialog())
            {
                string strPathToTemplate = FullPathTemplate;
                int iErr = 0;

                //Установить исходные параметры для формы диалога
                if (FullPathTemplate.Equals(string.Empty) == true)
                    FullPathTemplate =
                        //Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments)
                        m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_PATH_DEFAULT]
                        + @"\"
                        + m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_NAME]
                        ;
                else
                    ;
                formChoiсeTemplate.InitialDirectory = Path.GetDirectoryName(FullPathTemplate);
                formChoiсeTemplate.Title = @"Указать книгу MS Excel-шаблон";
                formChoiсeTemplate.CheckPathExists =
                formChoiсeTemplate.CheckFileExists =
                    true;
                formChoiсeTemplate.Filter = MS_EXCEL_FILTER;

                m_GetDataFromDB.ActionReport("Проверка шаблона");
                //Отобразить форму диалога
                if (formChoiсeTemplate.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    iErr = setFullPathTemplate(formChoiсeTemplate.FileName);
                }
                else
                    iErr = 1;

                if (!(iErr == 0))
                    switch (iErr)
                    {
                        case 1: //...отмена действия
                            //labelLog.Text += @"отменено пользователем..." + @"шаблон: " + (FullPathTemplate.Length > 0 ? FullPathTemplate : @"не указан...");
                            m_GetDataFromDB.ActionReport(@"Отменено пользователем..." + @"шаблон: " + (FullPathTemplate.Length > 0 ? FullPathTemplate : @"не указан..."));
                            m_GetDataFromDB.ReportClear(true);
                            break;
                        case -1: //...шаблон не прошел проверку
                            MessageBox.Show("Ошибка при проверке шаблона.\r\nОбратитесь в службу поодержки (тел.: 0-4444, 289-04-37).", @"Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            m_GetDataFromDB.ActionReport("Ошибка при проверке шаблона");
                            m_GetDataFromDB.ReportClear(true);
                            //labelLog.Text += @"ошибка проверки..." + @"шаблон: " + (FullPathTemplate.Length > 0 ? FullPathTemplate : @"не указан...");
                            break;
                        case -2:
                            m_GetDataFromDB.ReportClear(true);
                            break;
                        default:
                            m_GetDataFromDB.ReportClear(true);
                            break;
                    }
                else
                {
                    m_GetDataFromDB.ActionReport("Проверка шаблона успешно завершена");
                    m_GetDataFromDB.ReportClear(true);
                    m_btnStripButtonExcel.Enabled = true;
                };
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            int iRes = -1
                , iListenerId = -1;
            string msg = string.Empty;
            TEC_LOCAL.INDEX_DATA indx;
            TEC_LOCAL tec = m_listTEC[m_listBoxTEC.SelectedIndex];
            TEC_LOCAL.VALUES_DATE.VALUES_GROUP dictIndxValues;
            //m_GetDataFromDB = new GetDataFromDB();

            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearValues();
            }

            //delegateStartWait();

            //Установить соединение с источником данных
            iListenerId = DbSources.Sources().Register(m_connSettAIISKUECentre[m_listBoxTEC.SelectedIndex], false, @"");
            if (!(iListenerId < 0))
            {
                Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);

                for (indx = 0; indx <= TEC_LOCAL.INDEX_DATA.GRVIII; indx++)
                {
                    if (m_listBoxTEC.SelectedIndex != m_listTEC.Count-2 && indx == TEC_LOCAL.INDEX_DATA.GRVIII)
                    {
                        indx++; break;
                    }

                    tec.ClearValues(m_monthCalendarStart.SelectionStart.Date, indx);

                    iRes = m_GetDataFromDB.Request(tec, iListenerId
                        , m_monthCalendarStart.SelectionStart.Date //SelectionStart всегда == SelectionEnd, т.к. MultiSelect = false
                        , m_monthCalendarStart.SelectionEnd.Date.AddDays(1)
                        , indx);

                    if (!(iRes < 0))
                    {
                        dictIndxValues = tec.m_listValuesDate.Find(item => { return item.m_dataDate == m_monthCalendarStart.SelectionStart.Date; }).m_dictData[indx];
                        switch (indx)
                        {
                            case TEC_LOCAL.INDEX_DATA.TG:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.TSN:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVI:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVIII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Rows.Count - 1].Cells[24].Value);
                                break;
                        }

                        if (iRes == 0)
                        {
                            msg = @"Получены значения для: " + tec.m_strNameShr;
                            Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                        {
                            msg = @"Ошибка при получении значений для: " + tec.m_strNameShr;
                            Logging.Logg().Error(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        Logging.Logg().Warning(string.Format(@"FormMain::btnLoadClick () - нет результата запроса за {0} либо группа сигналов INDEX={1} пустая..."
                                , m_monthCalendarStart.SelectionStart.Date, indx)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
            {
                //delegateStopWait();
                throw new Exception(@"FormMain::btnLoad_Click () - не установлено соединение с источником данных...");
            }

            //delegateStopWait();

            //Разорвать соединение с источником данных
            DbSources.Sources().UnRegister(iListenerId);
            Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.FindForm().Close();
        }

        /// <summary>
        /// Обработчик событий: нажатие на кнопку на панели быстрого доступа "экспортВMSExcel"
        /// </summary>
        /// <param name="sender">Объект - инициатор события</param>
        /// <param name="e">Аргумент события</param>
        private void btnStripButtonExcel_Click(object sender, EventArgs e)
        {
            int iRes = -1, iErr = -1
                , iListenerId = -1;
            double[] arWriteValues;
            HMark markErr = new HMark(0);
            string msg = string.Empty;
            Logging.Logg().Action(@"Экспорт в MS Excel - начало ...", Logging.INDEX_MESSAGE.NOT_SET);
            m_GetDataFromDB.ActionReport("Экспорт в MS Excel - получение данных");

            if (m_connSettAIISKUECentre == null)
                throw new Exception(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - нет параметров для установки соединения с источником данных...");
            else
            { }

            //delegateStartWait();

            //Установить соединение с источником данных
            iListenerId = DbSources.Sources().Register(m_connSettAIISKUECentre, false, @"");
            if (!(iListenerId < 0))
            {
                Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

                foreach (TEC_LOCAL t in m_listTEC)
                {
                    iRes = m_GetDataFromDB.Request(t, iListenerId, m_monthCalendarStart.SelectionStart.Date, m_monthCalendarEnd.SelectionStart.Date.AddDays(1));

                    if (!(iRes < 0))
                    {
                        msg = string.Format(@"Получены значения для: {0}", t.m_strNameShr);
                        Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                    {
                        msg = @"Ошибка при получении значений для: " + t.m_strNameShr;
                        Logging.Logg().Error(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        markErr.Set(m_listTEC.IndexOf(t), true);
                    }
                }
            }
            else
            {
                //delegateStopWait();
                throw new Exception(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - не установлено соединение с источником данных...");
            }

            //delegateStopWait();

            //Разорвать соединение с источником данных
            DbSources.Sources().UnRegister(iListenerId);

            m_GetDataFromDB.ActionReport("Экспорт в MS Excel - запись данных");

            //Создать форму диалога выбора файла-шаблона MS Excel
            using (FileDialog formChoiseResult = new SaveFileDialog())
            {
                msg = @"Сохранить результат в: ";

                //Установить исходные параметры для формы диалога
                //formChoiseResult.InitialDirectory =                
                formChoiseResult.Title = @"Указать книгу MS Excel-результат";
                formChoiseResult.CheckPathExists = true;
                formChoiseResult.CheckFileExists = false;
                formChoiseResult.Filter = MS_EXCEL_FILTER;
                //Отобразить диалог для выбора книги MS Excel для сохранения рез-та
                if (formChoiseResult.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //delegateStartWait();

                    if (File.Exists(formChoiseResult.FileName) == false)
                        File.Copy(FullPathTemplate, formChoiseResult.FileName);
                    else
                        ;

                    MSExcelIO excel = new MSExcelIO(formChoiseResult.FileName);
                    //Проверить корректность шаблона
                    if (validateTemplate(excel) == 0)
                    {
                        //Установить текущим лист книги с именем из конфигурационного файла
                        excel.SelectWorksheet(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.SHEET_NAME]);
                        //Установить начальные значения для строк на листе книги MS Excel
                        int iColDataDate = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.COL_DATADATE])
                            , iRowStartMSExcel = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.ROW_START])
                            , iRowCountDateMSExcel = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.ROWCOUNT_DATE])
                            //, indx = -1
                            , iRowStart;
                        List<DateTime> listWriteDates = new List<DateTime>();
                        // сохранить активную мощность
                        foreach (TEC_LOCAL t in m_listTEC)
                            if (markErr.IsMarked(m_listTEC.IndexOf(t)) == false)
                                foreach (TEC_LOCAL.VALUES_DATE valsDate in t.m_listValuesDate)
                                {
                                    try
                                    {
                                        //Определить начальную строку по дате набора значений
                                        iRowStart = iRowStartMSExcel + (valsDate.m_dataDate.Day - 1) * iRowCountDateMSExcel;

                                        if (listWriteDates.IndexOf(valsDate.m_dataDate) < 0)
                                        {
                                            listWriteDates.Add(valsDate.m_dataDate);

                                            excel.WriteDate(iColDataDate, iRowStart, valsDate.m_dataDate);
                                        }
                                        else
                                            ;

                                        //Сохранить набор значений на листе книги MS Excel
                                        excel.WriteValues(t.m_arMSExcelNumColumns[(int)GetDataFromDB.INDEX_MSEXCEL_COLUMN.APOWER]
                                            , iRowStart
                                            , valsDate.m_dictData[TEC_LOCAL.INDEX_DATA.TG].m_summaHours);
                                        // получить набор значений для записи в соответствии с вариантом расчета
                                        arWriteValues = valsDate.GetValues(out iErr);
                                        if (iErr == 0)
                                            //Сохранить набор значений на листе книги MS Excel
                                            excel.WriteValues(t.m_arMSExcelNumColumns[(int)GetDataFromDB.INDEX_MSEXCEL_COLUMN.SNUZHDY]
                                                , iRowStart
                                                , arWriteValues);
                                        else
                                            Logging.Logg().Error(string.Format(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - TEC.ИД={0}, дата={1}, отсутствуют необходимые для расчета группы..."
                                                    , t.m_Id, valsDate.m_dataDate)
                                                , Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                    catch (Exception exc)
                                    {
                                        Logging.Logg().Exception(exc
                                            , string.Format(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - TEC.ИД={0}, дата={1}"
                                                , t.m_Id, valsDate.m_dataDate)
                                            , Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                }
                            else
                                ;

                        excel.SaveExcel(formChoiseResult.FileName);

                        //Закрыть книгу MS Excel
                        excel.Dispose();
                        excel = null;

                        msg += formChoiseResult.FileName;

                    }
                    else
                        ;

                    //delegateStopWait();
                }
                else
                {
                    msg += @"отменено пользователем...";
                }
                Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);
            }
            m_GetDataFromDB.ReportClear(true);
        }

        /// <summary>
        /// Проверить шаблон на корректность использования
        /// </summary>
        /// <param name="path">Строка - полный путь для шаблона</param>
        /// <returns>Признак проверки (0 - успех)</returns>
        private int validateTemplate(string path)
        {
            int iRes = 0;

            MSExcelIO excel = null;

            if (File.Exists(path) == true)
            {
                try
                {
                    excel = new MSExcelIO(path);
                }
                catch (Exception e)
                {
                    iRes = -1;

                    Logging.Logg().Exception(e, @"FormMain::validateTemplate (" + path + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (iRes == 0)
                    iRes = validateTemplate(excel as MSExcelIO);
                else
                    ;

                excel.CloseExcelDoc();
                excel.Dispose();
            }
            else
                iRes = -2;

            return iRes;
        }

        /// <summary>
        /// Проверить шаблон на возможность использования по назначению
        /// </summary>
        /// <param name="excel">Шаблон для экспорта данных</param>
        /// <returns>Признак проверки (0 - успех)</returns>
        private int validateTemplate(MSExcelIO excel)
        {
            int iRes = 0;

            try
            {
                excel.SelectWorksheet(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.SHEET_NAME]);
            }
            catch (Exception e)
            {
                iRes = -2;

                Logging.Logg().Exception(e, @"FormMain::validateTemplate (" + @"???ПУТЬ_К_ФАЙЛУ " + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            string ver = string.Empty;

            if (iRes == 0)
            {
                ver = excel.ReadValue(1, 1);

                iRes = ver.Equals(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_VER]) == true ? 0 : -3;
            }
            else
                ;

            return iRes;
        }
    }
}
