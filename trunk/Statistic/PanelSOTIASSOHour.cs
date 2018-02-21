﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;

//using HClassLibrary;
using StatisticCommon;
using System.Linq;
using ASUTP.Core;
using ASUTP;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public class PanelSOTIASSOHour : PanelStatistic
    {
        /// <summary>
        /// Максимальное количество(подряд) значений с выходом за пределы установленных ограничений, после которого фиксируется "ошибка"
        /// </summary>
        private static ushort MAX_COUNT_WARNING = 4;

        private class TecViewSOTIASSOHour : TecView
        {
            public enum REASON_RETROVALUES { UNKNOWN = -1, DATE, HOUR, MINUTE, BEGIN_HOUR, COUNT }

            public HMark m_markRetroValues;

            public TecViewSOTIASSOHour(int indx_tec, int indx_comp)
                : base(/*TecView.TYPE_PANEL.SOTIASSO, */indx_tec, indx_comp, TECComponentBase.TYPE.ELECTRO)
            {
                m_markRetroValues = new HMark(0);
            }

            public override void ChangeState()
            {
                lock (m_lockState) { GetRDGValues(-1, DateTime.MinValue); }

                base.ChangeState(); //Run
            }

            public override void GetRDGValues(int indx, DateTime date)
            {
                ClearStates();

                ClearValues();

                if (m_tec.m_bSensorsStrings == false)
                    AddState((int)StatesMachine.InitSensors);
                else ;

                using_date = false;

                AddState((int)TecView.StatesMachine.CurrentTimeAdmin); // без m_curDate = serverTime
                AddState((int)TecView.StatesMachine.CurrentMins_TM);
                AddState((int)TecView.StatesMachine.CurrentMinDetail_TM);

                AddState((int)TecView.StatesMachine.PPBRValues);
                AddState((int)TecView.StatesMachine.AdminValues);
            }

            public void GetRetroMinDetail(int indxMin)
            {
                lock (m_lockValue)
                {
                    //Отладка ???
                    if (indxMin < 0)
                    {
                        string strMes = @"TecView::getRetroMinDetail (indxMin = " + indxMin + @") - ...";
                        //Logging.Logg().Error(strMes);
                        //throw new Exception(strMes);
                    }
                    else ;
                    lastMin = indxMin + 1;

                    foreach (TECComponent comp in _localTECComponents)
                        foreach (TG tg in comp.m_listLowPointDev)
                            clearLowPointDevValuesSecs(m_dictValuesLowPointDev[tg.m_id]);

                    ClearStates();

                    AddState((int)StatesMachine.RetroMinDetail_TM);

                    Run(@"TecView::getRetroMinDetail ()");
                }
            }

            /// <summary>
            /// Вызов из обработчика события - восстановление исходного состояния кнопки мыши, при нажатии ее над 'ZedGraph'-минуты
            ///  только для 'PanelSOTIASSO' (час-в-минутах, минуты-в-секундах)
            /// </summary>
            /// <param name="indx">Индекс значения</param>
            /// <returns>Признак - является ли значение ретроспективным</returns>
            public void IsIndexRetroValues(int indx)
            {
                string strRetro = string.Empty;

                m_markRetroValues.Set((int)REASON_RETROVALUES.DATE, !(m_curDate.Date.Equals(serverTime.Date) == true));
                if (m_markRetroValues.IsMarked((int)REASON_RETROVALUES.DATE) == true)
                    strRetro += @"дате+";
                else
                    ;

                m_markRetroValues.Set((int)REASON_RETROVALUES.HOUR, !(m_curDate.Hour.Equals(serverTime.Hour) == true));
                if (m_markRetroValues.IsMarked((int)REASON_RETROVALUES.HOUR) == true)
                    strRetro += @"часу+";
                else
                    ;

                m_markRetroValues.Set((int)REASON_RETROVALUES.MINUTE, indx < (serverTime.Minute - 1));
                if (m_markRetroValues.IsMarked((int)REASON_RETROVALUES.MINUTE) == true)
                    strRetro += @"минуте+";
                else
                    ;

                m_markRetroValues.Set((int)REASON_RETROVALUES.BEGIN_HOUR, !(serverTime.Minute > 2));
                // 1 - для полож. разности, 2 - для особенности БД_значений: отставание при усреднении
                if (m_markRetroValues.IsMarked((int)REASON_RETROVALUES.BEGIN_HOUR) == true)
                {
                    strRetro = @"началу часа+";
                }
                else
                    ;

                if (strRetro.Equals(string.Empty) == false)
                    strRetro = strRetro.Substring(0, strRetro.Length - 1);
                else
                    ;

                if (! (m_markRetroValues.Value == 0))
                {
                    //Console.WriteLine(@"TecView::IsIndexRetroValues (indxMin=" + indx + @", Minute=" + serverTime.Minute + @") - по " + strRetro + @" = TRUE...");
                    Logging.Logg().Debug(@"TecView::IsIndexRetroValues (indxMin=" + indx + @", Minute=" + serverTime.Minute + @") - по " + strRetro + @" = TRUE...", Logging.INDEX_MESSAGE.NOT_SET);
                }
                else
                    ;
            }
        }
        
        /// <summary>
        /// Перечисление - целочисленные идентификаторы дочерних элементов управления
        /// </summary>
        private enum KEY_CONTROLS
        {
            UNKNOWN = -1                
                , DTP_CUR_DATE, NUD_CUR_HOUR, BTN_SET_NOWDATEHOUR
                , CB_TECCOMPONENT, LABEL_GTP_KOEFF, DGV_TECCOMPONENT_VALUE, ZGRAPH_TECCOMPONENT
                , CLB_TG , DGV_TG_VALUE, ZGRAPH_TG
                    , COUNT_KEY_CONTROLS
        }
        /// <summary>
        /// Объект с признаками обработки типов значений
        /// , которые будут использоваться фактически (PBR, Admin, AIISKUE, SOTIASSO)
        /// </summary>
        private HMark m_markQueries;
        /// <summary>
        /// Объект для обработки запросов/получения данных из/в БД
        /// </summary>
        private TecViewSOTIASSOHour m_tecView;

        System.Windows.Forms.SplitContainer stctrMain
            , stctrView;
        /// <summary>
        /// Панели графической интерпретации значений СОТИАССО
        /// 1) "час - по-минутно для выбранного ГТП", 2) "минута - по-секундно для выбранных ТГ"
        /// </summary>
        private ZedGraph.ZedGraphControl m_zGraph_TECComponent;
        private ZedGraph.ZedGraphControl m_zGraph_TG;
        private List<StatisticCommon.TEC> m_listTEC;
        //private List<TECComponentBase> m_TG_Comp;

        private ManualResetEvent m_evTimerCurrent;
        private
            System.Threading.Timer //Вариант №0
            //System.Windows.Forms.Timer //Вариант №1
                m_timerCurrent;
        /// <summary>
        /// Событие для инициирования процесса обновления значений СОТИАССО
        ///  в субобласти табличного представления данных "час - по-минутно для выбранного ГТП"
        /// </summary>
        private event DelegateObjectFunc EvtValuesMins;
        /// <summary>
        /// Событие для инициирования процесса обновления значений СОТИАССО
        ///  в субобласти табличного представления данных "минута - по-секундно для выбранных ТГ"
        /// </summary>
        private event DelegateObjectFunc EvtValuesSecs;
        /// <summary>
        /// Список индексов компонентов ТЭЦ (ТГ)
        ///  для отображения в субобласти графической интерпретации значений СОТИАССО "минута - по-секундно"
        /// </summary>
        private List<int> m_listIdTGAdvised;
        /// <summary>
        /// Событие выбора даты
        /// </summary>
        private event DelegateDateFunc EvtSetDatetimeHour;
        /// <summary>
        /// Делегат для установки даты на панели управляющих элементов управления
        /// </summary>
        private DelegateDateFunc delegateSetDatetimeHour;
        /// <summary>
        /// Значение коэффициента (для проверки выполнения условий сигнализации "Текущая мощность ГТП")
        /// </summary>
        private decimal m_dcGTPKoeffAlarmPcur;
        /// <summary>
        /// Панель для активных элементов управления
        /// </summary>
        private PanelManagement m_panelManagement;
        
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PanelSOTIASSOHour(List<StatisticCommon.TEC> listTec)
            : base(MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            //m_listTEC = listTec;
            // фильтр ТЭЦ-ЛК
            m_listTEC = new List<TEC>();
            foreach (TEC tec in listTec)
                if (!(tec.m_id > (int)TECComponent.ID.LK))
                    m_listTEC.Add (tec);
                else
                    ;
            //Создать объект с признаками обработки тех типов значений
            // , которые будут использоваться фактически
            m_markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_SOTIASSO });
            //m_markQueries.Marked((int)CONN_SETT_TYPE.ADMIN); //Для получения даты/времени
            //m_markQueries.Marked((int)CONN_SETT_TYPE.PBR); //Для получения даты/времени
            //m_markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);
            //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
            m_tecView = new TecViewSOTIASSOHour(0, -1);
            //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { m_listTEC[0] }, m_markQueries);
            //Установить тип значений
            m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            //Делегат для установки текущего времени на панели 'PanelManagement'
            //m_tecView.SetDelegateDatetime(...);
            //Делегат по окончанию обработки всех состояний 'TecView::ChangeState_SOTIASSO'
            m_tecView.updateGUI_Fact = new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);

            m_listIdTGAdvised = new List<int>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();
            //Назначить обработчики события - создание дескриптора панели
            //this.HandleCreated += new EventHandler(OnHandleCreated);
            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);

            EvtSetDatetimeHour += new DelegateDateFunc(m_panelManagement.Parent_OnEvtSetDatetimeHour);
            delegateSetDatetimeHour = new DelegateDateFunc(setDatetimeHour);

            this.m_zGraph_TECComponent.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphTECComponent_MouseUpEvent);
        }
        
        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelSOTIASSOHour(IContainer container, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(listTec)
        {
            container.Add(this);
        }
        ///// <summary>
        ///// Деструктор
        ///// </summary>
        //~PanelSOTIASSO ()
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
            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement(FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor); // панель для размещения элементов управления
            m_panelManagement.EvtDatetimeHourChanged += new DelegateDateFunc(panelManagement_OnEvtDatetimeHourChanged);
            m_panelManagement.EvtGTPSelectionIndexChanged += new DelegateIntFunc(panelManagement_OnEvtTECComponentSelectionIndexChanged);
            m_panelManagement.EvtTGItemChecked += new DelegateIntFunc(panelManagement_OnEvtTGItemChecked);
            //m_panelManagement.EvtSetNowHour += new DelegateFunc(panelManagement_OnEvtSetNowHour);
            m_zGraph_TECComponent = new ZedGraphControlTECComponent(); // графическая панель для отображения значений ГТП
            m_zGraph_TECComponent.Name = KEY_CONTROLS.ZGRAPH_TECCOMPONENT.ToString();
            m_zGraph_TG = new ZedGraphControlTG(); // графическая панель для отображения значений ТГ
            m_zGraph_TG.Name = KEY_CONTROLS.ZGRAPH_TG.ToString();

            //Создать сплиттеры
            stctrMain = new SplitContainer(); // для главного контейнера (вертикальный)
            stctrView = new SplitContainer(); // для вспомогательного (2 графические панели) контейнера (горизонтальный)
            //Настроить размещение главного контейнера
            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;
            //Настроить размещение вспомогательного контейнера
            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;
            //Настроить размещение графических панелей
            m_zGraph_TECComponent.Dock = DockStyle.Fill;
            m_zGraph_TG.Dock = DockStyle.Fill;

            //Приостановить прорисовку текущей панели
            this.SuspendLayout();

            //Добавить во вспомогательный контейнер графические панели
            stctrView.Panel1.Controls.Add(m_zGraph_TECComponent);
            stctrView.Panel2.Controls.Add(m_zGraph_TG);
            //Добавить элементы управления к главному контейнеру
            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            //stctrMain.FixedPanel = FixedPanel.Panel1;
            stctrMain.SplitterDistance = 43;

            //Добавить к текущей панели единственный дочерний (прямой) элемент управления - главный контейнер-сплиттер
            this.Controls.Add(stctrMain);
            //Возобновить прорисовку текущей панели
            this.ResumeLayout(false);
            //Принудительное применение логики макета
            this.PerformLayout();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
        }
        
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : ASUTP.Control.HPanelCommon {
            private class CheckedListBoxTG : CheckedListBox
            {
                public class CheckListBoxItem {
                    public int Tag;
                    public string Text;
                    public override string ToString ()
                    {
                        return Text;
                    }
                }

                public void AddRange (IEnumerable<TECComponentBase>listTG)
                {
                    var items = (from tg in listTG select new { name_shr = tg.name_shr, id = tg.m_id }).ToList();

                    items.ForEach (item => Items.Add (new CheckListBoxItem () {
                        Tag = item.id
                        , Text = item.name_shr
                    }));
                }

                public int TagOfIndex(int indx)
                {
                    return (indx < Items.Count) ? (Items[indx] as CheckListBoxItem).Tag : -1;
                }
            }

            private class HDateTimePicker : DateTimePicker
            {
                public HDateTimePicker()
                    : base()
                {
                    //this.SetStyle(ControlStyles.UserPaint, true);
                }

                //protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
                //{
                //    Graphics g = e.Graphics;
                //    Rectangle rectText =
                //        new Rectangle(0, 0, ClientRectangle.Width - 17, 16);

                //    TextRenderer.DrawText(
                //        g, @"Мой текст", Font,
                //        rectText, Color.Black,
                //        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                //    //base.OnPaint (e);
                //}

                //[Browsable(true), Description ("Описание ...")]
                //public override Color BackColor
                //{
                //    get { return base.BackColor; }
                //    set { base.BackColor = value; }
                //}
            }
            /// <summary>
            /// Событие изменения текущих даты, номера часа
            /// </summary>
            public event DelegateDateFunc EvtDatetimeHourChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event DelegateIntFunc EvtGTPSelectionIndexChanged;
            /// <summary>
            /// Событие изменения перечня ТГ для отображения выбранного ГТП
            /// </summary>
            public event DelegateIntFunc EvtTGItemChecked;

            //public event DelegateFunc EvtSetNowHour;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            private PanelManagement()
                : base(6, 24)
            {
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

            public PanelManagement (Color foreColor, Color backColor)
                : this ()
            {
                BackColor = backColor;
                ForeColor = foreColor;

                //Инициализировать равномерные высоту/ширину столбцов/строк
                initializeLayoutStyleEvenly ();

                initializeComponent ();
            }

            public PanelManagement (IContainer container, Color foreColor, Color backColor)
                : this (container)
            {
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

            /// <summary>
            /// Инициализация, размещения собственных элементов управления
            /// </summary>
            private void initializeComponent()
            {
                Control ctrl = null;

                //Приостановить прорисовку текущей панели
                // ??? корректней приостановить прорисовку после создания всех дочерних элементов
                // ??? при этом потребуется объявить переменные для каждого из элементов управления
                this.SuspendLayout();

                //Создать дочерние элементы управления
                // календарь для установки текущих даты, номера часа
                ctrl = new HDateTimePicker();
                ctrl.Name = KEY_CONTROLS.DTP_CUR_DATE.ToString();
                //(ctrl as HDateTimePicker).SetStyle(ControlStyles.UserPaint, true);
                (ctrl as HDateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as HDateTimePicker).Format = DateTimePickerFormat.Custom;
                //(ctrl as HDateTimePicker).FormatEx = HDateTimePicker.dtpCustomExtensions.dtpLongTime24Hour;
                (ctrl as HDateTimePicker).CustomFormat = "dd MMM, yyyy";
                //(ctrl as HDateTimePicker).Value = ((ctrl as HDateTimePicker).Value - HDateTime.GetUTCOffsetOfMoscowTimeZone()).AddHours (1);
                //(ctrl as HDateTimePicker).TextChanged += new EventHandler(onDatetimeHour_TextChanged);
                (ctrl as HDateTimePicker).ValueChanged += new EventHandler(onCurDatetime_ValueChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // поле для ввода номера часа
                ctrl = new NumericUpDown();
                ctrl.Name = KEY_CONTROLS.NUD_CUR_HOUR.ToString();
                //(ctrl as NumericUpDown).Minimum = 1;
                //(ctrl as NumericUpDown).Maximum = 24;
                (ctrl as NumericUpDown).ReadOnly = true;
                //(ctrl as NumericUpDown).Value = HDateTime.ToMoscowTimeZone ().Hour + 1;
                (ctrl as NumericUpDown).ValueChanged += new EventHandler(onNumericUpDownCurHour_ValueChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели поле для ввода номера часа
                this.Controls.Add(ctrl, 2, 0);
                this.SetColumnSpan(ctrl, 1); this.SetRowSpan(ctrl, 1);
                // подпись для поля ввода номера часа
                ctrl = new System.Windows.Forms.Label();
                ctrl.Text = @"-й час";
                (ctrl as System.Windows.Forms.Label).TextAlign = ContentAlignment.MiddleLeft;
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели поле для ввода номера часа
                this.Controls.Add(ctrl, 3, 0);
                this.SetColumnSpan(ctrl, 1); this.SetRowSpan(ctrl, 1);

                //// подпись текущего времени
                //ctrl = new System.Windows.Forms.Label ();
                //(ctrl as System.Windows.Forms.Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                //(ctrl as System.Windows.Forms.Label).TextAlign = ContentAlignment.MiddleCenter;
                //(ctrl as System.Windows.Forms.Label).Text = @"--:--:--";
                //ctrl.Name = KEY_CONTROLS.LABEL_CUR_TIME.ToString ();
                //ctrl.Dock = DockStyle.Fill;
                ////Добавить к текущей панели подпись
                //this.Controls.Add(ctrl, 0, 1);
                //this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);
                // кнопка перехода к актуальному часу
                ctrl = new System.Windows.Forms.Button();
                ctrl.Name = KEY_CONTROLS.BTN_SET_NOWDATEHOUR.ToString();
                ctrl.Text = @"Тек./час";
                (ctrl as Button).Click += new EventHandler(onSetNowHour_Click);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели кнопку
                this.Controls.Add(ctrl, 4, 0);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // раскрывающийся список для выбора ГТП
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CB_TECCOMPONENT.ToString();
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(onTECComponent_SelectionIndexChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели список ГТП
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 1);

                // коэффициент для текущего ГТП
                ctrl = new System.Windows.Forms.Label();
                ctrl.Name = KEY_CONTROLS.LABEL_GTP_KOEFF.ToString();
                (ctrl as System.Windows.Forms.Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                (ctrl as System.Windows.Forms.Label).TextAlign = ContentAlignment.MiddleCenter;
                ctrl.Text = @"Коэфф-т: -1";
                //ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Top);
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 4, 1);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSOHour.DataGridViewTECComponent();
                ctrl.Name = KEY_CONTROLS.DGV_TECCOMPONENT_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...
                ctrl.BackColor = BackColor;
                ctrl.ForeColor = ForeColor;
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 2);
                this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 9);

                // разделительная линия
                ctrl = new GroupBox();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                ctrl.Height = 3;
                //Добавить к текущей панели раздел./линию
                this.Controls.Add(ctrl, 0, 11);
                this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 1);

                // список для выбора ТГ
                ctrl = new CheckedListBoxTG();
                ctrl.Name = KEY_CONTROLS.CLB_TG.ToString();
                (ctrl as CheckedListBox).CheckOnClick = true;
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(onTG_ItemCheck);
                ctrl.Dock = DockStyle.Fill;
                ctrl.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                ctrl.ForeColor = ForeColor;
                //Добавить к текущей панели список для выбора ТГ
                this.Controls.Add(ctrl, 0, 12);
                this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 3);

                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSOHour.DataGridViewTG();
                ctrl.Name = KEY_CONTROLS.DGV_TG_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...
                ctrl.BackColor = BackColor;
                ctrl.ForeColor = ForeColor;
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 15);
                this.SetColumnSpan(ctrl, 6); this.SetRowSpan(ctrl, 9);

                ////Приостановить прорисовку текущей панели
                //this.SuspendLayout();

                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
            }

            public DateTime GetCurrDateHour()
            {
                return (this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE.ToString(), true)[0] as HDateTimePicker).Value;
            }

            ///// <summary>
            ///// Присвоить исходные дату/номер часа
            ///// </summary>
            //private void initDatetimeHourValue ()
            //{
            //    HDateTimePicker dtpCurDatetimeHour = this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE_HOUR.ToString(), true)[0] as HDateTimePicker;
            //    DateTime curDatetimeHour = dtpCurDatetimeHour.Value;
            //    curDatetimeHour = curDatetimeHour.AddMilliseconds(-1 * (curDatetimeHour.Minute * 60 * 1000 + curDatetimeHour.Second * 1000 + curDatetimeHour.Millisecond));
            //    dtpCurDatetimeHour.Value = curDatetimeHour;
            //}

            /// <summary>
            /// Изменить дату/номер часа
            /// </summary>
            private void initDatetimeHourValue(DateTime dtVal)
            {
                HDateTimePicker dtpCurDatetimeHour = this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE.ToString(), true)[0] as HDateTimePicker;
                DateTime curDatetimeHour;
                curDatetimeHour = dtVal.AddMilliseconds(-1 * (dtVal.Minute * 60 * 1000 + dtVal.Second * 1000 + dtVal.Millisecond));
                dtpCurDatetimeHour.Value = curDatetimeHour;

                setNumericUpDownValue(dtpCurDatetimeHour.Value.Hour + 1);
            }

            /// <summary>
            /// Обработчик события - дескриптор элемента управления создан
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void Parent_OnHandleCreated(object obj, EventArgs ev)
            {
                initDatetimeHourValue(HDateTime.ToMoscowTimeZone());
            }

            /// <summary>
            /// Заполнение ComboBox данными на основе formChangeMode
            /// </summary>
            /// <param name="listTECComponentNameShr">Таблица с данными из formChangeMode</param>
            public void InitializeTECComponentList(List<FormChangeMode.Item> listTECComponentNameShr)
            {
                //??? способ заполнения, конечно, оригинальный, но не коррелирует с исторически сложившимися способами
                DataTable tableTECComponentNameShr = new DataTable();
                tableTECComponentNameShr.Columns.Add("name_shr");
                tableTECComponentNameShr.Columns.Add("id");

                foreach (FormChangeMode.Item item in listTECComponentNameShr) {
                    tableTECComponentNameShr.Rows.Add(item.name_shr, item.id);
                }

                ComboBox cbxTECComponent = (this.Controls.Find(KEY_CONTROLS.CB_TECCOMPONENT.ToString(), true))[0] as ComboBox;

                BindingSource bs = new BindingSource();
                bs.DataSource = tableTECComponentNameShr;
                cbxTECComponent.DataSource = bs.DataSource;
                cbxTECComponent.DisplayMember = "name_shr";
                cbxTECComponent.ValueMember = "id";
                cbxTECComponent.BindingContext = new BindingContext();
            }

            public void InitializeKoeffAlarmPcur(decimal koeff)
            {
                System.Windows.Forms.Label lblKoeff =
                    (this.Controls.Find(KEY_CONTROLS.LABEL_GTP_KOEFF.ToString(), true))[0] as System.Windows.Forms.Label;

                lblKoeff.Text = lblKoeff.Text.Remove(lblKoeff.Text.LastIndexOfAny(new char[] { ' ' }) + 1);
                lblKoeff.Text += koeff.ToString();
            }

            public void InitializeTGList(List<TECComponentBase> listTG)
            {
                Control ctrl;

                ctrl = (this.Controls.Find(KEY_CONTROLS.CLB_TG.ToString(), true))[0];
                (ctrl as CheckedListBox).Items.Clear();
                (ctrl as CheckedListBoxTG).AddRange(listTG);

                ctrl = findControl (KEY_CONTROLS.DGV_TG_VALUE.ToString ());
                (ctrl as DataGridViewTG).Clear ();
                (ctrl as DataGridViewTG).InitializeTGList(listTG);
            }

            private void onCurDatetime_ValueChanged(object obj, EventArgs ev)
            {
                //EvtDatetimeHourChanged (((this.Controls.Find(KEY_CONTROLS.CUR_DATETIME_HOUR.ToString(), true))[0] as HDateTimePicker).Value);
                EvtDatetimeHourChanged((obj as HDateTimePicker).Value);
            }

            private void setNumericUpDownValue(int val)
            {
                NumericUpDown nudCurHour = this.Controls.Find(KEY_CONTROLS.NUD_CUR_HOUR.ToString(), true)[0] as NumericUpDown;
                nudCurHour.ValueChanged -= onNumericUpDownCurHour_ValueChanged;
                nudCurHour.Value = val;
                nudCurHour.ValueChanged += onNumericUpDownCurHour_ValueChanged;
            }

            private void onNumericUpDownCurHour_ValueChanged(object obj, EventArgs ev)
            {
                DateTimePicker dtpCurDate = this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE.ToString(), true)[0] as DateTimePicker;
                NumericUpDown nudCurHour = obj as NumericUpDown;
                int curHour = (int)nudCurHour.Value;
                dtpCurDate.Value = dtpCurDate.Value.AddHours(curHour - dtpCurDate.Value.Hour - 1);

                int iRes = 0
                    , iHour = -1;

                if (curHour == 0)
                    iRes = -1;
                else
                    if (curHour == 25)
                        iRes = 1;
                    else
                        ;

                if (!(iRes == 0))
                {
                    nudCurHour.ValueChanged -= onNumericUpDownCurHour_ValueChanged;
                    if (iRes < 0)
                        iHour = 24;
                    else
                        if (iRes > 0)
                            iHour = 1;
                        else
                            ;
                    setNumericUpDownValue(iHour);
                }
                else
                    ;
            }

            public void Parent_OnEvtSetDatetimeHour(DateTime dtVal)
            {
                initDatetimeHourValue(dtVal/*.AddHours (1)*/);
            }

            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущий ГТП
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void onTECComponent_SelectionIndexChanged(object obj, EventArgs ev)
            {
                int idGTPSelected = -1;

                if (Equals((findControl(KEY_CONTROLS.CB_TECCOMPONENT.ToString()) as ComboBox).SelectedValue, null) == false) {
                    idGTPSelected = Convert.ToInt32((findControl(KEY_CONTROLS.CB_TECCOMPONENT.ToString()) as ComboBox).SelectedValue);

                    EvtGTPSelectionIndexChanged(idGTPSelected);
                } else
                    ;
            }

            private void onTG_ItemCheck(object obj, ItemCheckEventArgs ev)
            {
                EvtTGItemChecked((obj as CheckedListBoxTG).TagOfIndex (ev.Index));
            }

            private void onSetNowHour_Click(object obj, EventArgs ev)
            {
                initDatetimeHourValue(HDateTime.ToMoscowTimeZone());
            }

            /// <summary>
            /// Обработчик события - отобразить полученные значения
            /// </summary>
            /// <param name="valuesMins">Массив значений для отображения</param>
            public void Parent_OnEvtValuesMins(object obj)
            {
                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        this.BeginInvoke(new DelegateObjectFunc(onEvtValuesMins), new object[] { obj });
                    else
                        onEvtValuesMins(obj);
                else
                    ;
            }

            /// <summary>
            /// Обработчик события - отобразить значения в разрезе минута-секунды
            /// </summary>
            /// <param name="obj">Объект, с данными для отображения</param>
            public void Parent_OnEvtValuesSecs(object obj)
            {
                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        this.BeginInvoke(new DelegateObjectFunc(onEvtValuesSecs), new object[] { obj });
                    else
                        onEvtValuesSecs(obj);
                else
                    ;
            }

            /// <summary>
            /// Отобразить значения в разрезе час-минуты
            /// </summary>
            /// <param name="obj">Объект, с данными для отображения</param>
            private void onEvtValuesMins(object obj)
            {
                TecView.valuesTEC[] valuesMins = (obj as object[])[0] as TecView.valuesTEC[];
                decimal dcGTPKoeffAlarmPcur = (decimal)(obj as object[])[1];
                int lastMin = (int)(obj as object[])[2];

                DataGridViewTECComponent dgvTECComponent = this.Controls.Find(KEY_CONTROLS.DGV_TECCOMPONENT_VALUE.ToString(), true)[0] as DataGridViewTECComponent;
                dgvTECComponent.Fill(valuesMins, (int)dcGTPKoeffAlarmPcur, (int)(obj as object[])[2]);
            }

            /// <summary>
            /// Отобразить значения в разрезе минута-секунды
            /// </summary>
            /// <param name="obj">Объект, с данными для отображения</param>
            private void onEvtValuesSecs(object obj)
            {
                DataGridViewTG dgvTG = this.Controls.Find(KEY_CONTROLS.DGV_TG_VALUE.ToString(), true)[0] as DataGridViewTG;

                dgvTG.Fill (obj as Dictionary<int, TecView.valuesLowPointDev>);
            }

            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    findControl (KEY_CONTROLS.CLB_TG.ToString ()).ForeColor = value;
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

                    //findControl (KEY_CONTROLS.DGV_TECCOMPONENT_VALUE.ToString ()).BackColor =
                    //findControl (KEY_CONTROLS.DGV_TG_VALUE.ToString ()).BackColor =
                    findControl (KEY_CONTROLS.CLB_TG.ToString ()).BackColor =
                         value == SystemColors.Control ? SystemColors.Window : BackColor;
                }
            }
        }
        
        /// <summary>
        /// Класс - общий для графического представления значений СОТИАССО на вкладке
        /// </summary>
        private class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZedGraphControl()
                : base()
            {
                initializeComponent();
            }

            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZedGraphControl(IContainer container)
                : this()
            {
                container.Add(this);
            }

            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
            {
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;

                ForeColor = FormMain.formGraphicsSettings.FontColor;
                BackColor = FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control
                    ? SystemColors.Window
                        : FormMain.formGraphicsSettings.BackgroundColor;

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.onPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.onDoubleClickEvent);
            }

            /// <summary>
            /// Обработчик события - отобразить значения точек
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="pane">Контекст графического представления (полотна)</param>
            /// <param name="curve">Коллекция точек для отображения на полотне</param>
            /// <param name="iPt">Индекс точки в наборе точек для отображения</param>
            /// <returns>Значение для отображения для точки с индексом</returns>
            private string onPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }

            /// <summary>
            /// Обработчик события - двойной "щелчок" мыши
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="e">Вргумент события</param>
            /// <returns>Признак продолжения обработки события</returns>
            private bool onDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }

            protected void getColorZEDGraph (out Color colChart, out Color colP)
            {
                //Значения по умолчанию
                colChart = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.BG_SOTIASSO);
                colP = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.SOTIASSO);
            }

            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    this.SetFontColor (value);
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
                    Color clrChart
                        , clrCurve;

                    base.BackColor = value;

                    GraphPane.Fill = new Fill (value == SystemColors.Control ? SystemColors.Window : value);
                    getColorZEDGraph (out clrChart, out clrCurve);
                    GraphPane.Chart.Fill.Color = clrChart;
                }
            }
        }
        
        /// <summary>
        /// Класс для отображения в графическом представлении
        ///  значений за укзанный (дата/номер часа) 1 час для выбранного ГТП
        /// </summary>
        private class ZedGraphControlTECComponent : HZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public ZedGraphControlTECComponent()
                : base()
            {
                initializeComponent();
            }

            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public ZedGraphControlTECComponent(IContainer container)
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

            ///// <summary>
            ///// Обработчик события - отобразить полученные значения
            ///// </summary>
            ///// <param name="obj">Массив значений для отображения</param>
            //public void Parent_OnEvtValuesMins(object obj)
            //{
            //    if (IsHandleCreated == true)
            //        if (InvokeRequired == true)
            //            this.BeginInvoke(new DelegateObjectFunc(onEvtValuesMins), new object[] { obj });
            //        else
            //            onEvtValuesMins(obj);
            //    else
            //        ;
            //}
            ///// <summary>
            ///// Делегат для отображения значений
            ///// </summary>
            ///// <param name="obj">Массив значений для отображения</param>
            //private void onEvtValuesMins(object obj)
            //{
            //}

            /// <summary>
            /// Обновить содержание в графической субобласти "час по-минутно"
            /// </summary>
            public void Draw (string title, TecView.valuesTEC[]values, decimal koeffAlarmPcur)
            {
                double [] valsMins = null
                    , valsUDGe = null
                    , valsOAlarm = null
                    , valsPAlarm = null;
                int itemscount = -1;
                string [] names = null;
                double minimum
                    , minimum_scale
                    , maximum
                    , maximum_scale
                    , diviation;
                bool noValues = false;

                itemscount = values.Length - 1;

                names = new string [itemscount];

                valsMins = new double [itemscount];
                valsUDGe = new double [itemscount];
                valsOAlarm = new double [itemscount];
                valsPAlarm = new double [itemscount];

                minimum = double.MaxValue;
                maximum = 0;
                noValues = true;

                for (int i = 0; i < itemscount; i++) {
                    names [i] = (i + 1).ToString ();

                    valsMins [i] = values [i + 1].valuesFact;
                    diviation = values [i + 1].valuesUDGe / 100 * (double)koeffAlarmPcur;
                    valsPAlarm [i] = values [i + 1].valuesUDGe + diviation; //values[i + 1].valuesDiviation;
                    valsOAlarm [i] = values [i + 1].valuesUDGe - diviation; //values[i + 1].valuesDiviation;
                    valsUDGe [i] = values [i + 1].valuesUDGe;

                    if ((minimum > valsPAlarm [i]) && (!(valsPAlarm [i] == 0))) {
                        minimum = valsPAlarm [i];
                        noValues = false;
                    }

                    if ((minimum > valsOAlarm [i]) && (!(valsOAlarm [i] == 0))) {
                        minimum = valsOAlarm [i];
                        noValues = false;
                    }

                    if ((minimum > valsUDGe [i]) && (!(valsUDGe [i] == 0))) {
                        minimum = valsUDGe [i];
                        noValues = false;
                    }

                    if ((minimum > valsMins [i]) && (!(valsMins [i] == 0))) {
                        minimum = valsMins [i];
                        noValues = false;
                    }

                    if (maximum < valsPAlarm [i])
                        maximum = valsPAlarm [i];
                    else
                        ;

                    if (maximum < valsOAlarm [i])
                        maximum = valsOAlarm [i];
                    else
                        ;

                    if (maximum < valsUDGe [i])
                        maximum = valsUDGe [i];
                    else
                        ;

                    if (maximum < valsMins [i])
                        maximum = valsMins [i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues) {
                    minimum_scale = 0;
                    maximum_scale = 10;
                } else {
                    if (minimum != maximum) {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    } else {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;
                getColorZEDGraph (out colorChart, out colorPCurve);

                GraphPane pane = GraphPane;
                pane.CurveList.Clear ();
                pane.Chart.Fill = new Fill (colorChart);
                //pane.Fill = new Fill (BackColor);

                //LineItem
                pane.AddCurve ("УДГэ", null, valsUDGe, FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.UDG));
                //LineItem
                pane.AddCurve ("", null, valsOAlarm, HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor);
                //LineItem
                pane.AddCurve ("Граница для сигнализации", null, valsPAlarm, HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor);

                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar) {
                    pane.AddBar ("Мощность", null, valsMins, colorPCurve);
                } else
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear) {
                    ////Вариант №1
                    //double[] valuesFactLinear = new double[itemscount];
                    //for (int i = 0; i < itemscount; i++)
                    //    valuesFactLinear[i] = valsMins[i];
                    //Вариант №2
                    PointPairList ppl = new PointPairList ();
                    for (int i = 0; i < itemscount; i++)
                        if (valsMins [i] > 0)
                            ppl.Add (i, valsMins [i]);
                        else
                            ;
                    //LineItem
                    pane.AddCurve ("Мощность"
                                    ////Вариант №1
                                    //, null, valuesFactLinear
                                    //Вариант №2
                                    , ppl
                                    , colorPCurve);
                } else
                    ;

                //Для размещения в одной позиции ОДНого значения
                pane.BarSettings.Type = BarType.Overlay;

                //...из minutes
                pane.XAxis.Scale.Min = 0.5;
                pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
                pane.XAxis.Scale.MinorStep = 1;
                pane.XAxis.Scale.MajorStep = itemscount / 20;

                pane.XAxis.Type = AxisType.Linear; //...из minutes
                                                   //pane.XAxis.Type = AxisType.Text;
                pane.XAxis.Title.Text = "t, мин";
                pane.YAxis.Title.Text = "P, МВт";
                pane.Title.Text = @"СОТИАССО";
                pane.Title.Text += new string (' ', 29);
                pane.Title.Text += title;

                pane.XAxis.Scale.TextLabels = names;
                pane.XAxis.Scale.IsPreventLabelOverlap = false;

                // Включаем отображение сетки напротив крупных рисок по оси X
                pane.XAxis.MajorGrid.IsVisible = true;
                // Задаем вид пунктирной линии для крупных рисок по оси X:
                // Длина штрихов равна 10 пикселям, ... 
                pane.XAxis.MajorGrid.DashOn = 10;
                // затем 5 пикселей - пропуск
                pane.XAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.XAxis.MajorGrid.PenWidth = 0.1F;
                pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив крупных рисок по оси Y
                pane.YAxis.MajorGrid.IsVisible = true;
                // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                pane.YAxis.MajorGrid.DashOn = 10;
                pane.YAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.YAxis.MajorGrid.PenWidth = 0.1F;
                pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив мелких рисок по оси Y
                pane.YAxis.MinorGrid.IsVisible = true;
                // Длина штрихов равна одному пикселю, ... 
                pane.YAxis.MinorGrid.DashOn = 1;
                pane.YAxis.MinorGrid.DashOff = 2;
                // толщина линий
                pane.YAxis.MinorGrid.PenWidth = 0.1F;
                pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Устанавливаем интересующий нас интервал по оси Y
                pane.YAxis.Scale.Min = minimum_scale;
                pane.YAxis.Scale.Max = maximum_scale;

                AxisChange ();

                Invalidate ();
            }
        }
        
        /// <summary>
        /// Класс для отображения в графическом представлении
        ///  значений за указанную (дата/номер часа/номер минуты) 1 мин для выбранных ТГ, выбранного ГТП
        /// </summary>
        private class ZedGraphControlTG : HZedGraphControl
        {
            /// <summary>
            /// Отобразить графическую интерпретацию значений СОТИАССО
            ///  для указанных компонентов ТЭЦ (ТГ) "минута по-секундно"
            /// </summary>
            public void Draw (string title, IEnumerable<string>tg_names, IEnumerable<double[]> values, int lastMin)
            {
                double [,] valsSecs = null;
                //double[] valsUDGe = null
                //    , valsOAlarm = null
                //    , valsPAlarm = null;
                int tgcount = -1
                    , itemscount = -1
                    , min = -1;
                string [] names = null;
                double minimum
                    , minimum_scale
                    , maximum
                    , maximum_scale;
                bool noValues = false;

                GraphPane pane = GraphPane;
                Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty
                        //, colorPCurveBase = Color.Empty
                        ;

                tgcount = tg_names.Count();
                itemscount = 60;

                getColorZEDGraph (out colorChart, out colorPCurve);
                pane.Chart.Fill = new Fill (colorChart);
                //pane.Fill = new Fill (BackColor);

                minimum = double.MaxValue;
                maximum = 0;
                noValues = true;
                minimum_scale = 0;
                maximum_scale = 10;

                if (tgcount > 0) {
                    names = new string [itemscount];

                    valsSecs = new double [tgcount, itemscount];
                    //valsUDGe = new double[itemscount];
                    //valsOAlarm = new double[itemscount];
                    //valsPAlarm = new double[itemscount];

                    min = lastMin < itemscount ? lastMin : itemscount;

                    for (int i = 0; i < itemscount; i++) {
                        names [i] = (i + 1).ToString ();

                        for (int j = 0; j < tgcount; j++) {
                            valsSecs [j, i] = values.ElementAt (j)[i];
                            if (valsSecs [j, i] < 0)
                                valsSecs [j, i] = 0;
                            else
                                ;

                            if ((minimum > valsSecs [j, i]) && (valsSecs [j, i] > 0)) {
                                minimum = valsSecs [j, i];
                                noValues = false;
                            } else
                                ;

                            if (maximum < valsSecs [j, i])
                                maximum = valsSecs [j, i];
                            else
                                ;
                        }
                    }

                    if (!(FormMain.formGraphicsSettings.scale == true))
                        minimum = 0;
                    else
                        ;

                    if (noValues) {
                        minimum_scale = 0;
                        maximum_scale = 10;
                    } else {
                        if (minimum != maximum) {
                            minimum_scale = minimum - (maximum - minimum) * 0.2;
                            if (minimum_scale < 0)
                                minimum_scale = 0;
                            maximum_scale = maximum + (maximum - minimum) * 0.2;
                        } else {
                            minimum_scale = minimum - minimum * 0.2;
                            maximum_scale = maximum + maximum * 0.2;
                        }
                    }

                    int r = -1, g = -1, b = -1
                        , diffRGB = 255 / tgcount;
                    pane.CurveList.Clear ();

                    ////LineItem
                    //pane.AddCurve("УДГэ", null, valsUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
                    ////LineItem
                    //pane.AddCurve("", null, valsOAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
                    ////LineItem
                    //pane.AddCurve("Граница для сигнализации", null, valsPAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));

                    PointPairList [] ppl = new PointPairList [tgcount];
                    for (int j = 0; j < tgcount; j++) {
                        ppl [j] = new PointPairList ();

                        for (int i = 0; i < itemscount; i++)
                            if (valsSecs [j, i] > 0)
                                ppl [j].Add (i + 1, valsSecs [j, i]);
                            else
                                ;
                        //Вариант №1
                        r = colorPCurve.R;
                        g = colorPCurve.G;
                        b = colorPCurve.B;
                        ////Вариант №2
                        //switch (j % 3)
                        //{
                        //    case 0:
                        //        r = colorPCurve.R + diffRGB;
                        //        if (r > 255)
                        //        {
                        //            g += (r - 255);
                        //            r -= (r - 255);
                        //        }
                        //        else
                        //            g = colorPCurve.G;
                        //        b = colorPCurve.B;
                        //        break;
                        //    case 1:
                        //        r = colorPCurve.R;
                        //        g = colorPCurve.G + diffRGB;
                        //        if (g > 255)
                        //        {
                        //            b += (g - 255);
                        //            g -= (g - 255);
                        //        }
                        //        else
                        //            b = colorPCurve.B;
                        //        break;
                        //    case 2:
                        //        g = colorPCurve.G;
                        //        b = colorPCurve.B + diffRGB;
                        //        if (b > 255)
                        //        {
                        //            r += (b - 255);
                        //            b -= (b - 255);
                        //        }
                        //        else
                        //            r = colorPCurve.R;
                        //        break;
                        //    default:
                        //        break;
                        //}
                        colorPCurve = Color.FromArgb (r, g, b);
                        //LineItem
                        pane.AddCurve (tg_names.ElementAt (j), ppl [j], colorPCurve);
                    }
                } else
                    ; // нет ТГ для отображения

                //Для размещения в одной позиции ОДНого значения
                pane.BarSettings.Type = BarType.Overlay;

                //...из minutes
                pane.XAxis.Scale.Min = 0.5;
                pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
                pane.XAxis.Scale.MinorStep = 1;
                pane.XAxis.Scale.MajorStep = itemscount / 20;

                pane.XAxis.Type = AxisType.Linear; //...из minutes
                                                   //pane.XAxis.Type = AxisType.Text;
                pane.XAxis.Title.Text = "t, сек";
                pane.YAxis.Title.Text = "P, МВт";
                pane.Title.Text = @"СОТИАССО";
                pane.Title.Text += new string (' ', 29);
                pane.Title.Text += title;

                pane.XAxis.Scale.TextLabels = names;
                pane.XAxis.Scale.IsPreventLabelOverlap = false;

                // Включаем отображение сетки напротив крупных рисок по оси X
                pane.XAxis.MajorGrid.IsVisible = true;
                // Задаем вид пунктирной линии для крупных рисок по оси X:
                // Длина штрихов равна 10 пикселям, ... 
                pane.XAxis.MajorGrid.DashOn = 10;
                // затем 5 пикселей - пропуск
                pane.XAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.XAxis.MajorGrid.PenWidth = 0.1F;
                pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив крупных рисок по оси Y
                pane.YAxis.MajorGrid.IsVisible = true;
                // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                pane.YAxis.MajorGrid.DashOn = 10;
                pane.YAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.YAxis.MajorGrid.PenWidth = 0.1F;
                pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Включаем отображение сетки напротив мелких рисок по оси Y
                pane.YAxis.MinorGrid.IsVisible = true;
                // Длина штрихов равна одному пикселю, ... 
                pane.YAxis.MinorGrid.DashOn = 1;
                pane.YAxis.MinorGrid.DashOff = 2;
                // толщина линий
                pane.YAxis.MinorGrid.PenWidth = 0.1F;
                pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.GRID);

                // Устанавливаем интересующий нас интервал по оси Y
                pane.YAxis.Scale.Min = minimum_scale;
                pane.YAxis.Scale.Max = maximum_scale;

                AxisChange ();

                Invalidate ();
            }
        }
        
        /// <summary>
        /// Класс для отображения значений в табличном виде
        ///  в разрезе час-минуты для ГТП
        /// </summary>
        private class DataGridViewTECComponent : DataGridView
        {
            private enum INDEX_COLUMN {
                NUM_MINUTE
                , VALUE
                , UDGe
                , DEVIATION
                    , COUNT
            }

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewTECComponent()
                : base()
            {
                initializeComponent();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewTECComponent(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления 
            /// </summary>
            private void initializeComponent()
            {
                this.Columns.AddRange(new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn () // INDEX_COLUMN.NUM_MINUTE
                        , new DataGridViewTextBoxColumn () // INDEX_COLUMN.VALUE
                        , new DataGridViewTextBoxColumn () // INDEX_COLUMN.UDGe
                        , new DataGridViewTextBoxColumn () // INDEX_COLUMN.DEVIATION
                    });

                this.Columns[(int)INDEX_COLUMN.NUM_MINUTE].HeaderText = @"Мин.";
                this.Columns[(int)INDEX_COLUMN.VALUE].HeaderText = @"Значение";
                this.Columns[(int)INDEX_COLUMN.UDGe].HeaderText = @"УДГэ";
                this.Columns[(int)INDEX_COLUMN.DEVIATION].HeaderText = @"Отклонение";

                this.ReadOnly = true;
                //this.ColumnHeadersVisible =
                this.RowHeadersVisible =
                this.AllowUserToAddRows =
                this.AllowUserToDeleteRows =
                this.AllowUserToOrderColumns =
                this.AllowUserToResizeRows =
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[(int)INDEX_COLUMN.NUM_MINUTE].Width = 38;
                this.Columns[(int)INDEX_COLUMN.NUM_MINUTE].Frozen = true;
                this.Columns[(int)INDEX_COLUMN.VALUE].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.Columns[(int)INDEX_COLUMN.UDGe].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.Columns[(int)INDEX_COLUMN.DEVIATION].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                //Добавить строки по числу мин. в часе
                for (int i = 0; i < 60; i++)
                    this.Rows.Add(new object[] { i + 1 });
            }

            public void SetCurrentCell (int iRow, int iCol = 0)
            {
                this.CurrentCell = Rows[iRow].Cells[iCol];
                this.CurrentCell.Selected = true;
            }

            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    DefaultCellStyle.ForeColor = value;
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

                    DefaultCellStyle.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                }
            }

            /// <summary>
            /// Заполнить значениями элемент управления
            /// </summary>
            /// <param name="values">Значения для отображения</param>
            /// <param name="pars">Дополнительные параметры для отображения</param>
            public void Fill(TecView.valuesTEC []values, params int[]pars)
            {
                DataGridViewCellStyle cellStyle;
                double diviation = -1F;
                decimal dcKoeff = (decimal)pars [0]; //dcGTPKoeffAlarmPcur
                int cntDiviation = 0
                    , iLastMin = (int)pars[1];

                int i = -1;

                for (i = 1; i < values.Length; i++)
                {
                    //// изменяем фон ячеек с неизменяемым значением на случай изменения цветовой схемы "Система - Пользователь"
                    //Rows [i - 1].Cells [(int)INDEX_COLUMN.NUM_MINUTE].Style = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON];

                    //Значения
                    Rows [i - 1].Cells[(int)INDEX_COLUMN.VALUE].Value = values[i].valuesFact.ToString(@"F3");
                    //Rows [i - 1].Cells [(int)INDEX_COLUMN.VALUE].Style = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON];
                    //УДГэ
                    Rows [i - 1].Cells[(int)INDEX_COLUMN.UDGe].Value = values[i].valuesUDGe.ToString(@"F3");
                    //Rows [i - 1].Cells [(int)INDEX_COLUMN.UDGe].Style = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON];
                    //Отклонения
                    // максимальное отклонение
                    diviation = values[i].valuesUDGe / 100 * (double)dcKoeff;
                    //Проверить наличие значения
                    if (values[i].valuesFact > 0)
                    {
                        Rows[i - 1].Cells[(int)INDEX_COLUMN.DEVIATION].Value = (values[i].valuesFact - values[i].valuesUDGe).ToString(@"F3");
                        //Определить цвет ячейки
                        if (Math.Abs(values[i].valuesFact - values[i].valuesUDGe) > diviation)
                        {
                            //Увеличить счетчик случаев выхода за установленные границы
                            cntDiviation++;

                            if (cntDiviation > MAX_COUNT_WARNING)
                                cellStyle = HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR];
                            else
                                cellStyle = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.WARNING];
                        }
                        else
                        {
                            //Установить счетчик случаев выхода за установленные границы в исходное состояние
                            cntDiviation = 0;
                            cellStyle = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON];
                        }
                    }
                    else
                    {
                        Rows[i - 1].Cells[(int)INDEX_COLUMN.DEVIATION].Value = 0.ToString(@"F3");
                        //Установить счетчик случаев выхода за установленные границы в исходное состояние
                        cntDiviation = 0;
                        cellStyle = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON];
                    }
                    //Установить цвет ячейки
                    Rows[i - 1].Cells[(int)INDEX_COLUMN.DEVIATION].Style = cellStyle;
                }
                //Указать активную строку
                i = iLastMin > 60 ? 60 : iLastMin;
                SetCurrentCell(i - 1);
            }
        }
        
        /// <summary>        
        /// Класс для отображения значений в табличном виде
        ///  в разрезе минута-секунды для ТГ
        /// </summary>
        private class DataGridViewTG : DataGridView
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewTG()
                : base()
            {
                initializeComponent();
            }

            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewTG(IContainer container)
                : this()
            {
                container.Add(this);

                initializeComponent ();
            }

            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
            {
                this.Columns.AddRange(new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                    });

                this.Columns[0].HeaderText = @"Сек.";

                this.ReadOnly = true;
                //this.ColumnHeadersVisible =
                this.RowHeadersVisible =
                this.AllowUserToAddRows =
                this.AllowUserToDeleteRows =
                this.AllowUserToOrderColumns =
                this.AllowUserToResizeRows =
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[0].Width = 38;
                
                BackColor = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON].BackColor;
                ForeColor = HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON].ForeColor;

                //Добавить строки по числу сек. в мин.
                for (int i = 0; i < 60; i++)
                    this.Rows.Add(new object[] { i + 1 });
            }

            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    DefaultCellStyle.ForeColor = value;
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

                    DefaultCellStyle.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                }
            }

            public void Fill (Dictionary<int, TecView.valuesLowPointDev> dictValuesTG)
            {
                int i = -1; //Индекс столбца
                bool bRowVisible = false; //Признак видимости строки
                for (int j = 0; j < 60; j++) {
                    i = 0;
                    //// фон ячеек с номером секунды д.б. обновлен
                    //Rows [j].Cells [i].Style.BackColor = BackColor;

                    bRowVisible = false;
                    foreach (int id in dictValuesTG.Keys) {
                        i++; // очередной столбец

                        //Rows [j].Cells [i].Style.BackColor = BackColor;

                        //Проверить наличие значения для ТГ за очередную секунду
                        if (!(dictValuesTG [id].m_powerSeconds [j] < 0)) {//Есть значение
                            // отобразить
                            Rows [j].Cells [i].Value = dictValuesTG [id].m_powerSeconds [j].ToString (@"F3");
                            //При необходимости установить признак видимости строки
                            if (bRowVisible == false)
                                bRowVisible = true;
                            else
                                ;
                        } else
                            //Нет значения - пустая строка
                            Rows [j].Cells [i].Value = string.Empty;
                    }
                    //Установить признак видимости строки
                    // в ~ от наличия значения в ней
                    Rows [j].Visible = bRowVisible;
                }
            }

            public void Clear ()
            {
                while (Columns.Count > 1)
                    Columns.RemoveAt (Columns.Count - 1);
            }

            public void InitializeTGList (IEnumerable<TECComponentBase> listTG)
            {
                for (int i = 0; i < listTG.Count(); i++) {
                    Columns.Add (new DataGridViewTextBoxColumn ());
                    Columns [i + 1].HeaderText = listTG.ElementAt (i).name_shr;
                    //Columns[i + 1].Width = ((Width - Columns[0].Width) / listTGNameShr.Count) - (listTGNameShr.Count + 1);
                    Columns [i + 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
        }
        
        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_tecView.Start();

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new
                System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite)
                //System.Windows.Forms.Timer ()
                ;
            //Вариант №1
            //m_timerCurrent.Tick += new EventHandler (TimerCurrent_Tick);
            //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL;
            //m_timerCurrent.Start ();
        }
        
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            //Проверить актуальность объекта обработки запросов
            if (!(m_tecView == null))
            {
                if (m_tecView.Actived == true)
                    //Если активен - деактивировать
                    m_tecView.Activate(false);
                else
                    ;

                if (m_tecView.IsStarted == true)
                    //Если выполняется - остановить
                    m_tecView.Stop();
                else
                    ;

                //m_tecView = null;
            }
            else
                ;
            //Проверить актуальность объекта синхронизации таймера
            if (!(m_evTimerCurrent == null))
                //Сбросить флаг ожидания
                m_evTimerCurrent.Reset();
            else
                ;
            //Проверить актуальность объекта таймера
            if (!(m_timerCurrent == null))
            {
                //Освободить ресурсы таймера
                m_timerCurrent.Dispose();

                m_timerCurrent = null;
            }
            else
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

            bRes = base.Activate(active);

            m_tecView.Activate(active);

            if (m_tecView.Actived == true)
            {
                dueTime = 0;
            }
            else
            {
                m_tecView.ReportClear(true);
            }
            //??? Проверка текущий/не текущий час
            if (!(m_timerCurrent == null))
                //Вариант №0
                m_timerCurrent.Change(dueTime, System.Threading.Timeout.Infinite);
            ////Вариант №1
            //if (dueTime == System.Threading.Timeout.Infinite)
            //    m_timerCurrent.Stop ();
            //else
            //    if (dueTime == 0)
            //        if (m_timerCurrent.Enabled == false)
            //            m_timerCurrent.Start ();
            //        else
            //            ;
            //    else
            //        ;
            else
                ;

            if ((m_tecView.IsFirstActivated == true)
                && (IsFirstActivated == true)) {
                ComboBox cbxGTP = findControl(KEY_CONTROLS.CB_TECCOMPONENT.ToString()) as ComboBox;
                cbxGTP.SelectedIndex = -1;
                cbxGTP.SelectedIndex = 0;
            } else
                ;

            return bRes;
        }

        /// <summary>
        /// Обработчик события - создание дескриптора панели
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>      
        public void ChangeMode(object obj)
        {
            //Добавить строки на дочернюю панель
            m_panelManagement.InitializeTECComponentList((List<FormChangeMode.Item>)obj);

            EvtValuesMins += new DelegateObjectFunc(m_panelManagement.Parent_OnEvtValuesMins);
            EvtValuesSecs += new DelegateObjectFunc(m_panelManagement.Parent_OnEvtValuesSecs);
            //EvtValuesMins += new DelegateObjectFunc((m_zGraph_GTP as HZEdGraph_GTP).Parent_OnEvtValuesMins); //???отображать значения будем в функции на панели

            DataGridViewTECComponent dgvGTP = findControl(KEY_CONTROLS.DGV_TECCOMPONENT_VALUE.ToString()) as DataGridViewTECComponent;
            dgvGTP.SelectionChanged += new EventHandler(panelManagement_dgvGTPOnSelectionChanged);
        }

        /// <summary>
        /// Установить значение даты/времени на дочерней панели с активными элементами управления
        /// </summary>
        /// <param name="val">Новое значение даты/времени</param>
        private void setDatetimeHour(DateTime val)
        {
            EvtSetDatetimeHour(val);
        }
        
        /// <summary>
        /// Метод обратного вызова для таймера 'm_timerCurrent'
        /// </summary>
        /// <param name="obj">Параметр при вызове метода</param>
        private void TimerCurrent_Tick(object obj)
        //private void TimerCurrent_Tick(object obj, EventArgs ev)
        {
            if (m_tecView.Actived == true)
                if (m_tecView.currHour == true)
                {
                    m_tecView.ChangeState();

                    m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                }
                else
                    if (m_tecView.m_markRetroValues.IsMarked ((int)TecViewSOTIASSOHour.REASON_RETROVALUES.BEGIN_HOUR) == true)
                        if ((m_tecView.lastMin > 60) && (m_tecView.serverTime.Minute > 2))
                            if (m_tecView.adminValuesReceived == true) //Признак успешного выполнения операций для состояния 'TecView.AdminValues'
                                if (IsHandleCreated/*InvokeRequired*/ == true)
                                    // переход на следующий час
                                    Invoke(delegateSetDatetimeHour, m_tecView.serverTime);
                                else
                                    return;
                            else
                                ; //m_tecView.adminValuesReceived == false
                        else
                        {
                            m_tecView.ChangeState();

                            m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                        }
                    else
                        ;
            else
                ; //m_tecView.Actived == false
        }
        
        /// <summary>
        /// Установить текущие дату/час для объекта обработки запросов к БД
        /// </summary>
        /// <param name="dtNew">Новое значение даты/времени</param>
        private void setCurrDateHour(DateTime dtNew)
        {
            m_tecView.m_curDate = dtNew;
            m_tecView.lastHour =
                dtNew.Hour// - 1; //- (int)HDateTime.GetUTCOffsetOfMoscowTimeZone().TotalHours //- 3
                ;
            if (m_tecView.lastHour < 0)
            {
                m_tecView.m_curDate = m_tecView.m_curDate.AddDays(-1);
                m_tecView.lastHour += 24;
            }
            else
                ;
        }
        
        /// <summary>
        /// Обработчик события - изменения даты/номера часа на панели с управляющими элементами
        /// </summary>
        /// <param name="dtNew">Новые дата/номер часа</param>
        private void panelManagement_OnEvtDatetimeHourChanged(DateTime dtNew)
        {
            setCurrDateHour(dtNew);
            //Проверить наличие даты/времени полученного на сервере (хотя бы один раз)
            if (m_tecView.serverTime.Equals(DateTime.MinValue) == false)
                if ((m_tecView.m_curDate.Date.Equals(m_tecView.serverTime.Date) == true)
                    && (m_tecView.lastHour.Equals(m_tecView.serverTime.Hour) == true))
                {
                    m_tecView.adminValuesReceived = false; //Чтобы не выполнилась ветвь - переход к след./часу
                    m_tecView.currHour = true;

                    if (!(m_timerCurrent == null))
                        m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                    else
                        ;
                }
                else
                {
                    m_tecView.currHour = false;

                    m_tecView.ChangeState();
                }
            else
                // не выполнен НИ один успешный запрос к БД_значений
                ;
        }

        /// <summary>
        /// Обработчик события - выбор компонента ТЭЦ (ГТП) на панели с управляющими элементами
        /// </summary>
        /// <param name="id">Идентификатор ТЭЦ (компонента ГТП)</param>
        private void panelManagement_OnEvtTECComponentSelectionIndexChanged(int id)
        {
            if (this.Actived == true)
            {
                //Передать информацию 'PanelManagement' для заполнения списка ТГ
                //List<string> listTGNameShr = new List<string>();
                int indxTEC = -1 //Индекс ТЭЦ в списке из БД конфигурации
                    , indxTECComponent = -1 //Индекс компонента ТЭЦ (ГТП) - локальный в пределах ТЭЦ
                    ;
                List<TECComponentBase> listTG_Comp = new List<TECComponentBase>();

                indxTEC =
                    0;
                #region Хряпин А.Н. - Заполнение списка наименований ТГ

                //foreach (TEC t in m_listTEC)
                //{
                //    //В каждой ТЭЦ индекс локальный - обнулить
                //    indxTECComponent = 0;
                //    //Цикл для поиска выбранного пользователем компонента ТЭЦ (ГТП)
                //    // заполнения списка наименований подчиненных (ТГ) элементов
                //    foreach (TECComponent tc in t.list_TECComponents)
                //    {
                //        //Определить тип компонента (по диапазону идентификатора)
                //        if (tc.IsGTP == true)
                //        {//Только ГТП
                //            if (indxGTP == indx)
                //            {
                //                foreach (TG tg in tc.m_listLowPointDev)
                //                    listTGNameShr.Add(/*tc.name_shr + @" " + */tg.name_shr);

                //                m_dcGTPKoeffAlarmPcur = tc.m_dcKoeffAlarmPcur;
                //                indxGTP = -1; //Признак завершения внешнего цикла
                //                break;
                //            }
                //            else
                //                ;
                //            //Увеличить индекс ГТП сквозной
                //            indxGTP++;
                //        }
                //        else
                //            ; // не ГТП

                //        //Увеличить индекс компонента ТЭЦ локальный
                //        indxTECComponent++;
                //    }
                //    //Проверить признак прекращения выполнения цикла
                //    if (indxGTP < 0)
                //    {
                //        indxGTP = indx; //Возвратить найденное значение
                //        // прекратить выполнение цикла
                //        break;
                //    }
                //    else
                //        ;
                //    //Увеличить индекс ТЭЦ
                //    indxTEC++;
                //}

                #endregion

                #region Апельганс А.В. - поиск минимального коэффициента

                // комментарий Хряпин А.Н.
                //!!! не объявлять переменные в середине  кода, тем более внутри цикла
                //!!! зачем объявлена таблица и манипуляции со строками
                //!!! как выходить их внешнего цикла

                m_dcGTPKoeffAlarmPcur = decimal.MaxValue;

                foreach (TEC t in m_listTEC)
                {
                    // есть специальное свойство для проверки 't.m_bSensorsStrings'
                    //if (t.m_listLowPointDev == null)
                    if (t.m_bSensorsStrings == false)
                        t.InitSensorsTEC();
                    // проверить идентификатор ТЭЦ
                    if (t.m_id == id)
                    {
                        indxTEC = m_listTEC.IndexOf(t);
                        indxTECComponent = -1;

                        foreach (TG tg in t.GetListLowPointDev(TECComponentBase.TYPE.ELECTRO))
                            listTG_Comp.Add(tg);

                        foreach (TECComponent tc in t.list_TECComponents)
                            if (tc.IsGTP == true)
                            {
                                if (m_dcGTPKoeffAlarmPcur > tc.m_dcKoeffAlarmPcur)
                                    m_dcGTPKoeffAlarmPcur = tc.m_dcKoeffAlarmPcur;
                                else
                                    ;
                            }
                            else
                                ;
                        break;
                    }
                    else
                    {// идентификатор ТЭЦ не удовлетворяет условию - искать среди компонентов
                        foreach (TECComponent tc in t.list_TECComponents)
                        {
                            if (tc.m_id == id)
                            {
                                if (tc.IsGTP == true)
                                {
                                    indxTECComponent = t.list_TECComponents.IndexOf(tc);
                                    indxTEC = m_listTEC.IndexOf(t);
                                    m_dcGTPKoeffAlarmPcur = tc.m_dcKoeffAlarmPcur;

                                    foreach (TG tg in tc.m_listLowPointDev)
                                        listTG_Comp.Add(tg);
                                    
                                    break;
                                }
                                else
                                {
                                    if (tc.IsPC == true)
                                    {
                                        indxTECComponent = t.list_TECComponents.IndexOf(tc);
                                        indxTEC = m_listTEC.IndexOf(t);

                                        foreach (TG tg in tc.m_listLowPointDev)
                                            listTG_Comp.Add(tg);

                                        foreach (TECComponent tcc in t.list_TECComponents)
                                            if ((tcc.IsGTP == true)
                                                && (tcc.tec.m_id == tc.tec.m_id))
                                                if (m_dcGTPKoeffAlarmPcur > tcc.m_dcKoeffAlarmPcur)
                                                    m_dcGTPKoeffAlarmPcur = tcc.m_dcKoeffAlarmPcur;
                                                else
                                                    ;
                                            else
                                                ;

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //!!! выход из цикла, если задача поиска 'm_dcGTPKoeffAlarmPcur' решена
                    //!!!  или проверить количество элементов 'listTG_Comp'
                    if (m_dcGTPKoeffAlarmPcur < decimal.MaxValue)
                        break;
                    else
                        ;
                }

                #endregion

                //Инициализировать значение коэффициента для выполнения условия сигнализации
                // по мгновенному значению ГТП
                m_panelManagement.InitializeKoeffAlarmPcur(m_dcGTPKoeffAlarmPcur);
                //Инициализировать элементами список с наименованиями ТГ            
                m_panelManagement.InitializeTGList(listTG_Comp);
                //Очистить список с отмеченными ТГ для отображения
                m_listIdTGAdvised.Clear();
                //Проверить актуальность объекта обработки запросов
                if (!(m_tecView == null))
                    //Проверить наличие изменений при новом выборе компонента ТЭЦ
                    if ((!(m_tecView.m_indx_TEC == indxTEC))
                        || (!(m_tecView.indxTECComponents == indxTECComponent)))
                    {//Только, если есть изменения
                        //Деактивация/останов объекта обработки запросов
                        m_tecView.Activate(false);
                        m_tecView.Stop();

                        //m_tecView = null;
                        m_tecView.ReInitTEC(m_listTEC[indxTEC], indxTEC, indxTECComponent, m_markQueries);

                        //Запуск/активация объекта обработки запросов
                        m_tecView.Start();
                        m_tecView.Activate(true);
                        //???при 1-й активации некорректно повторный вызов
                        if (!(m_timerCurrent == null))
                            ////Вариант №0
                            //m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                            ////Вариант №1
                            //m_timerCurrent.Start ();
                            //Вариант №2
                            m_tecView.ChangeState();
                        else
                            ;
                    }
                    else
                        ;
                else
                    ;
            }
        }

        /// <summary>
        /// Обработчик события выбора ТГ в списке ТЭЦ-ТГ
        /// </summary>
        /// <param name="id">Идентификатор выбранного компонента ТЭЦ (ТГ)</param>
        private void panelManagement_OnEvtTGItemChecked(int id)
        {
            if (m_listIdTGAdvised.IndexOf(id) < 0)
                m_listIdTGAdvised.Add(id);
            else
                m_listIdTGAdvised.Remove(id);
            //Обновить графическую интерпретацию "минута - по-секундно" значений СОТИАССО
            zedGraphTG_draw ();
        }

        private void zedGraphTG_draw ()
        {
            var values = from pair in m_tecView.m_dictValuesLowPointDev
                         where m_listIdTGAdvised.Contains (pair.Key)
                         select new {
                             Key = pair.Key
                             , name_shr = m_tecView.FindTECComponent (pair.Key).name_shr
                             , value = pair.Value.m_powerSeconds
                         };
            (m_zGraph_TG as ZedGraphControlTG).Draw (textGraphMinDetail
                , from value in values select value.name_shr
                , from value in values select value.value
                , m_tecView.lastMin);
        }

        /// <summary>
        /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
        /// </summary>
        /// <param name="hour">Номер часа в запросе</param>
        /// <param name="min">Номер минуты в звпросе</param>
        /// <returns>Признак результата выполнения функции</returns>
        private int onEvtHandlerStatesCompleted(int hour, int min)
        {
            int iRes = 0;

            //string msg = @"PanelSOTIASSO::updateGUI_Fact () - lastHour=" + hour + @", lastMin=" + min + @" ...";
            //Console.WriteLine (msg);
            //Logging.Logg ().Debug (msg, Logging.INDEX_MESSAGE.NOT_SET);

            if (!(hour < 0))
            {
                // Событие для панели управления (PanelManagement) - заполнить DataGridView
                EvtValuesMins(new object[] { m_tecView.m_valuesMins, m_dcGTPKoeffAlarmPcur, min });
                // те же аргументы передать для графической интерапретации значений
                (m_zGraph_TECComponent as ZedGraphControlTECComponent).Draw(textGraphMins, m_tecView.m_valuesMins, m_dcGTPKoeffAlarmPcur);
            }
            else
                ;

            // событие для панели управления ('PanelManagement') для табличной интерпретации значений
            EvtValuesSecs(m_tecView.m_dictValuesLowPointDev);
            // те же аргументы передать для графической интеп=рперетации
            // , событие не требуется, т.к. m_zGraph_TG дочерний элемент непосредственно для текущей панели
            zedGraphTG_draw ();

            return iRes;
        }

        /// <summary>
        /// Текст (часть) заголовка для графической субобласти "час по-минутно"
        /// </summary>
        private string textGraphMins
        {
            get
            {
                return CurrDateHour.ToShortDateString() + @", "
                    + (CurrDateHour.Hour + 1) + @"-й ч";
            }
        }

        /// <summary>
        /// Текст (часть) заголовка для графической субобласти "минута по-секундно"
        /// </summary>
        private string textGraphMinDetail
        {
            get
            {
                string strRes = string.Empty
                    , strHour = string.Empty
                    , strMin = string.Empty;
                int iLastMin = 60;

                bool bLastMin = !(m_tecView.lastMin < iLastMin) //61
                    , bCurrHour = m_tecView.currHour;
                //Подпись для номера часа
                if (bLastMin == false)
                    strHour += (CurrDateHour.Hour + 1);
                else
                    if (bCurrHour == true)
                        strHour += (CurrDateHour.Hour + 2);
                    else
                        strHour += (CurrDateHour.Hour + 1);
                strHour +=  @"-й ч";
                //Подпись для номера минуты
                if (bLastMin == false)
                    if (bCurrHour == true)
                        strMin += m_tecView.lastMin;
                    else
                        strMin += (m_tecView.lastMin - 1);
                else
                    if (bCurrHour == true)
                        strMin += (m_tecView.lastMin - (iLastMin - 1)); //60
                    else
                        strMin += 60;
                strMin += @"-я мин";
                //Результирующая подпись
                strRes = strHour + @", " + strMin;

                return strRes;
                //return ((m_tecView.lastMin < 61) ? (CurrDateHour.Hour + 1) : (m_tecView.currHour == true ? CurrDateHour.Hour + 2 : CurrDateHour.Hour + 1)) + @"-й ч"
                //    + @", " + ((m_tecView.lastMin < 61) ? (m_tecView.currHour == true ? m_tecView.lastMin : (m_tecView.lastMin - 1)) :
                //        (m_tecView.currHour == true ? m_tecView.lastMin - 60 : 60)) + @"-я мин";
            }
        }        

        /// <summary>
        /// Перерисовать объекты с графическим представлением данных
        ///  , в зависимости от типа графического представления (гистограмма, график)
        /// </summary>
        /// <param name="type">Тип графической информации, подвергшейся изменениям</param>
        public override void UpdateGraphicsCurrent(int type)
        {
            (m_zGraph_TECComponent as ZedGraphControlTECComponent).Draw (textGraphMins, m_tecView.m_valuesMins, m_dcGTPKoeffAlarmPcur);
            zedGraphTG_draw ();
        }
        
        /// <summary>
        /// Текущие дата/час, выбранные пользователем
        /// </summary>
        private DateTime CurrDateHour
        {
            get { return m_panelManagement.GetCurrDateHour(); }
        }
        
        /// <summary>
        /// Обработчик события - освобождение кн. мыши над 'zedGraphMins'
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        /// <returns>Признак дальнейшей обработки события</returns>
        private bool zedGraphTECComponent_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (! (e.Button == MouseButtons.Left))
                // только для левой кн.
                return true; // обработать далее стандартным обработчиком
            else
                ;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;
            //Сопоставдение точки отображаемому объекту
            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (
                (!(obj is BarItem))
                && !(obj is LineItem)
                )
                // только для гистограмм
                return true; // обработать далее стандартным обработчиком
            else
                ;

            if ((!(m_tecView == null)) && (found == true))
            {
                //if (!(delegateStartWait == null)) delegateStartWait(); else ;

                //panelManagement_dgvGTPOnSelectionChanged (this.Controls.Find(KEY_CONTROLS.DGV_GTP_VALUE.ToString(), true)[0], new MinSelectedChangedEventArgs () { m_index = index });
                (this.Controls.Find(KEY_CONTROLS.DGV_TECCOMPONENT_VALUE.ToString(), true)[0] as DataGridViewTECComponent).SetCurrentCell(index);

                //if (!(delegateStopWait == null)) delegateStopWait(); else ;
            }

            return true;
        }
        
        /// <summary>
        /// Обработчик события - изменение выбора строки в 'DataGridViewGTP'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void panelManagement_dgvGTPOnSelectionChanged (object obj, EventArgs ev)
        {
            int index = -1;
            DataGridViewTECComponent dgvGTP = obj as DataGridViewTECComponent;
            //Проверить был ли отправлен/обработан запрос для основных "час по-минутно" данных
            if (m_tecView.adminValuesReceived == true)
                //Проверить есть выбранные строки
                if (dgvGTP.SelectedRows.Count > 0)
                {
                    //Определить индекс выбранной строки (+1 для )
                    index = dgvGTP.SelectedRows[0].Index + 1;
                    //Проверить является выбранный 1-мин интервал (строка) ретроспективой
                    m_tecView.IsIndexRetroValues(index);
                    if (! (m_tecView.m_markRetroValues.Value == 0))
                        // для ретроспективных интервалов
                        if ((m_tecView.m_markRetroValues.IsMarked((int)TecViewSOTIASSOHour.REASON_RETROVALUES.DATE) == true)
                            || (m_tecView.m_markRetroValues.IsMarked ((int)TecViewSOTIASSOHour.REASON_RETROVALUES.HOUR) == true)
                            || (m_tecView.m_markRetroValues.IsMarked ((int)TecViewSOTIASSOHour.REASON_RETROVALUES.MINUTE) == true))
                        {
                            m_tecView.currHour = false;
                            //Установить дату/час
                            setCurrDateHour(CurrDateHour);
                            //Инициировать запрос для получения ретроспективных значений за интервал
                            m_tecView.GetRetroMinDetail(index);
                        }
                        else
                            if (m_tecView.m_markRetroValues.IsMarked((int)TecViewSOTIASSOHour.REASON_RETROVALUES.BEGIN_HOUR) == true)
                                m_tecView.currHour = false
                                    ;
                            else
                                ;
                    else
                        // для текущего интервала
                        if (m_tecView.currHour == false)
                            //Установить текущий интервал в соответствии с актуальными датой/временем
                            (this.Controls.Find(KEY_CONTROLS.BTN_SET_NOWDATEHOUR.ToString(), true)[0] as System.Windows.Forms.Button).PerformClick();
                        else
                            ;
                }
                else
                    ;
            else
                ;
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor = value;

                if (Equals (m_panelManagement, null) == false)
                    m_panelManagement.ForeColor = value;
                else
                    ;

                if (Equals (m_zGraph_TECComponent, null) == false)
                    m_zGraph_TECComponent.ForeColor = value;
                else
                    ;
                if (Equals (m_zGraph_TG, null) == false)
                    m_zGraph_TG.ForeColor = value;
                else
                    ;
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

                if (Equals (m_zGraph_TECComponent, null) == false)
                    m_zGraph_TECComponent.BackColor = value;
                else
                    ;
                if (Equals (m_zGraph_TG, null) == false)
                    m_zGraph_TG.BackColor = value;
                else
                    ;
            }
        }            
    }
}