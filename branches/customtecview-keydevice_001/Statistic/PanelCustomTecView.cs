﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


using StatisticCommon;
using ASUTP.Core;
using ASUTP.Control;

namespace Statistic
{
    partial class PanelCustomTecView
    {
        const Char CHAR_DELIM_PROP = '+'
                , CHAR_DELIM_LABEL = '&'
                , CHAR_DELIM_ARRAYITEM = ',';

        public class HLabelCustomTecView : Label {
            public static string s_msg = @"Добавить выбором пункта контекстного меню...";
            private LabelViewProperties m_propView;
            //public static LabelViewProperties s_propViewDefault = new LabelViewProperties () { Item1 = 0 };
            //public List<int> m_listIdContextMenuItems;
            //private static string[] s_arContentMenuItems = { @"Таблица(мин)", @"Таблица(час)", @"График(мин)", @"График(час)", @"Ориентация", @"Оперативные значения", @"Таблица+Гистограмма" };
            /// <summary>
            /// Событие - инициирует измекнение структуры элемента управления
            /// </summary>
            public event DelegateObjectFunc EventRestruct;

            /// <summary>
            /// Метод инициирующий возникновение события из-вне
            /// </summary>
            /// <param name="obj">Аргумент события</param>
            public void PerformRestruct(object obj)
            {
                EventRestruct (obj);
            }

            /// <summary>
            /// Метод инициирующий возникновение события из-вне
            /// </summary>
            public void PerformRestruct()
            {
                EventRestruct (m_propView);
            }

            /// <summary>
            /// Значение признака ориентации размещения таблиц, графиков
            /// </summary>
            private LabelViewProperties.VALUE m_prevViewOrientation;

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HLabelCustomTecView(int []arProp = null)
            {
                this.Dock = DockStyle.Fill;
                this.Text = s_msg;
                this.BorderStyle = BorderStyle.Fixed3D;
                this.TextAlign = ContentAlignment.MiddleCenter;
                //this.AutoSize = false; // по умолчанию 'false'
                this.SetAutoSizeMode(AutoSizeMode.GrowOnly);
                _fontDefault =
                _fontActual =
                    this.Font;

                //m_listIdContextMenuItems = new List<int>();

                if (arProp == null)
                    m_propView = new LabelViewProperties ();
                else
                    ;

                m_prevViewOrientation = m_propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION);

                ForeColorChanged += new EventHandler (onForeColorChanged);
                BackColorChanged += new EventHandler (onBackColorChanged);

                //this.SizeChanged += new EventHandler (onSizeChanged);
            }

            /// <summary>
            /// Обработчик события - выбор п. меню
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void OnMenuItem_Content(object obj, EventArgs ev)
            {
                int indx = ((MenuItem)obj).Index;

                if ((((MenuItem)obj).Parent is MenuItem) && (! (((MenuItem)obj).Parent.MenuItems.Count == 2)))
                    setProperty(((MenuItem)obj).Index, ((MenuItem)obj).Checked == true ? 0 : 1);
                else
                    setProperty((int)LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION, indx);

                EventRestruct (m_propView);

                ContentMenuStateChange ();

                ((PanelCustomTecView)Parent.Parent).EventContentChanged ();
            }

            /// <summary>
            /// Установить новое значение для свойства
            /// </summary>
            /// <param name="indx">Индекс свойства</param>
            /// <param name="newVal">Новое значение свойства</param>
            private void setProperty (int indx, int newVal) {
                m_propView.SetValue((LabelViewProperties.INDEX_PROPERTIES_VIEW)indx, (LabelViewProperties.VALUE)newVal);

                int i = -1
                    , cnt = 0;
                LabelViewProperties.INDEX_PROPERTIES_VIEW indxStart = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN
                    , indxEnd = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;

                cnt = m_propView.CountOn;

                if (cnt > 1) {
                    if (cnt > 2) {
                        //if (cnt > 3) {
                            indxStart = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                            indxEnd = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                            if (indx < (int)LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS)
                            { //3-й установленный признак - таблица: снять с отображения графики
                                indxStart = LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS;
                                indxEnd = LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS;
                            }
                            else
                            { //3-й установленный признак - график: снять с отображения таблицы
                                indxStart = LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS;
                                indxEnd = LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS;
                            }

                            for (i = (int)indxStart; ! (i > (int)indxEnd); i ++) {
                                cnt -= m_propView [i] ;
                                m_propView.SetValue ((LabelViewProperties.INDEX_PROPERTIES_VIEW)i, LabelViewProperties.VALUE.Zero); //Снять с отображения
                        }

                        //} else ;
                    }
                    else
                        ;

                    if (cnt > 1)
                        if (m_propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) == LabelViewProperties.VALUE.OFF)
                            if (m_prevViewOrientation < 0)
                                m_propView.SetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION, LabelViewProperties.VALUE.Zero); //Вертикально - по умолчанию
                            else
                                //Восстановить значение "ориентация сплиттера"
                                m_propView.SetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION, m_prevViewOrientation);
                        else
                            ; //Оставить "как есть"
                    else
                        //Запомнить предыдущее стостояние "ориентация сплиттера"
                        savePrevViewOrientation ();
                } else {
                    //Запомнить предыдущее стостояние "ориентация сплиттера"
                    savePrevViewOrientation ();
                }
            }

            /// <summary>
            /// Запомнить предыдущее стостояние "ориентация сплиттера"
            /// </summary>
            private void savePrevViewOrientation () {
                m_prevViewOrientation = m_propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION);
                //Блокировать возможность выбора "ориентация сплиттера"
                m_propView.SetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION, LabelViewProperties.VALUE.OFF);
            }

            /// <summary>
            /// СОздать массив п. меню, управляющего содержанием элемента управления
            /// </summary>
            /// <returns></returns>
            private MenuItem[] createContentMenuItems()
            {
                LabelViewProperties.INDEX_PROPERTIES_VIEW indx = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                MenuItem[] arMenuItems = new MenuItem[(int)LabelViewProperties.INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW];

                for (indx = 0; indx < LabelViewProperties.INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW; indx++) {
                    arMenuItems [(int)indx] = new MenuItem (LabelViewProperties.GetText (indx), this.OnMenuItem_Content);
                    arMenuItems [(int)indx].Tag = indx;

                    if (indx == LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) {
                        arMenuItems [(int)indx] = new MenuItem (LabelViewProperties.GetText (indx), new MenuItem [] {
                            new MenuItem (@"Вертикально", this.OnMenuItem_Content)
                            , new MenuItem (@"Горизонтально", this.OnMenuItem_Content)
                        });

                        arMenuItems[0].Tag = SplitterPanel.or
                    } else
                        ;
                }
                

                return arMenuItems;
            }

            /// <summary>
            /// Изменить состояние меню
            /// </summary>
            private void ContentMenuStateChange () {
                Menu.MenuItemCollection arMenuItems = ContextMenu.MenuItems[ContextMenu.MenuItems.Count - (COUNT_FIXED_CONTEXT_MENUITEM - INDEX_START_CONTEXT_MENUITEM)].MenuItems;
                
                for (int i = (int)INDEX_PROPERTIES_VIEW.TABLE_MINS; i < (int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW; i ++) {
                    arMenuItems[i].Enabled = m_propView[i] < 0 ? false : true;

                    if (i == (int)INDEX_PROPERTIES_VIEW.ORIENTATION) {
                        if (arMenuItems[i].Enabled == true) {
                            for (int j = 0; j < 2; j ++) {
                                arMenuItems[i].MenuItems[j].RadioCheck = true;
                                bool bRadioChecked = false;
                                if (j == m_propView[i])
                                    bRadioChecked = true;
                                else
                                    ;

                                arMenuItems[i].MenuItems[j].Checked = bRadioChecked;
                            }
                        }
                        else
                            ;
                    }
                    else {
                        arMenuItems[i].Checked = m_propView[i] == 1;
                    }
                }
                
            }

            /// <summary>
            /// Добавить "постоянные" элементы в контекстное меню (Содержание, Очистить)
            /// </summary>
            /// <param name="indx">индекс 1-го из добавляемых пунктов (Содержание)</param>
            /// <param name="f">функция-обработчик выбора пункта очистить</param>
            public void AddContextMenuFixedMenuItems (int indx, EventHandler fClear) {
                this.ContextMenu.MenuItems.Add(@"-");
                this.ContextMenu.MenuItems.Add(@"Содержание", createContentMenuItems());
                this.ContextMenu.MenuItems[indx].Enabled = false;
                this.ContextMenu.MenuItems.Add(@"Очистить");
                this.ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].Click += fClear;

                ContentMenuStateChange ();
            }

            //TODO:
            public void onForeColorChanged(object obj, EventArgs ev)
            {
            //    if (Equals (ContextMenuStrip, null) == false)
            //        ContextMenu.ForeColor = (obj as Control).ForeColor;
            //    else
            //        ;
            }

            //TODO:
            public void onBackColorChanged(object obj, EventArgs ev)
            {
            //    if (Equals (ContextMenuStrip, null) == false)
            //        ContextMenu.BackColor = (obj as Control).BackColor;
            //    else
            //        ;
            }

            /// <summary>
            /// Состояние элемента управления
            ///  0/1 - нет/есть объекта отображения
            /// </summary>
            protected bool _state;
            /// <summary>
            /// Шрифт для подписи элемента управления
            /// </summary>
            private System.Drawing.Font _fontDefault
                , _fontActual;
            /// <summary>
            /// Цвет шрифта для подписи элемента управления
            /// </summary>
            Color _color { get { return _state == true ? Color.Red : Color.Black; } }

            /// <summary>
            /// Установить признак "Доступность" для п. меню
            /// </summary>
            /// <param name="indx">Индекс п. меню</param>
            /// <param name="bEnabled">Признпк "Доступность"</param>
            public void EnableContextMenuItem(int indx, bool bEnabled)
            {
                ContextMenu.MenuItems[indx].Enabled =
                _state =
                    bEnabled;
                // доработка по просьбе Заказчика 15.01.2016 г.
                //  увеличение читабельности (заметности) подписи
                if (bEnabled == true)
                {
                    FitFont();
                }
                else
                // только при наличии предыдущего шрифта
                    if (!(_fontDefault == null))
                    {
                        setFont(_fontDefault, _color);
                    }
                    else
                        ;
            }

            /// <summary>
            /// Установить шрифт и цвет шрифта для подписи
            /// </summary>
            /// <param name="font">Устанавливаемый шрифт</param>
            /// <param name="color">Устанавливаемый цвет шрифта</param>
            private void setFont(Font font, Color color)
            {
                this.Font = new System.Drawing.Font(
                    font.FontFamily
                    , font.Size
                    , font.Style
                    , font.Unit
                    , font.GdiCharSet
                );

                this.ForeColor = color;
            }

            /// <summary>
            /// Применить актуальный размер шрифта
            /// </summary>
            public void FitFont()
            {
                if (_state == true)
                {
                    _fontActual = HLabel.FitFont(this.CreateGraphics(), Text, ClientSize, new SizeF(0.95F, 0.95F), 0.05F);

                    setFont((!(_fontActual == null)) ? _fontActual : _fontDefault, _color);
                }
                else
                    ;
            }

            /// <summary>
            /// Возвратить идентификатор п. меню с установленным признаком "Использовать"
            /// </summary>
            /// <returns>Идентификатор п. меню</returns>
            private FormChangeMode.KeyDevice getKeyDeviceMenuItemChecked()
            {
                FormChangeMode.KeyDevice keyRes = FormChangeMode.KeyDevice.Empty;
                int indxMenuItem = -1;

                // найти индекс п. меню
                foreach (MenuItem mi in ContextMenu.MenuItems)
                {
                    indxMenuItem = ContextMenu.MenuItems.IndexOf(mi);
                    if (indxMenuItem < (ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM))
                        if (mi.Checked == true)
                            break;
                        else
                            ;
                    else
                        ;
                }

                if (indxMenuItem < (ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM))
                // присвоить значение идентификатора
                    keyRes = (FormChangeMode.KeyDevice)ContextMenu.MenuItems [indxMenuItem].Tag;

                else
                // идентификатор для этого п. меню нет
                    ;

                return keyRes;
            }

            /// <summary>
            /// Изменить содержимое ячейки для объекта отображения 
            /// </summary>
            /// <param name="arProp">Массив изменяемых парметров объекта отображения</param>
            public void LoadProfile(string []arProp)
            {
                MenuItem mItem;
                int indxItem = -1;

                //Очистить
                ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].PerformClick();
                //Установить параметры содержания отображения
                string[] arPropVal = arProp[2].Split(CHAR_DELIM_ARRAYITEM);
                if (arPropVal.Length == m_propView.Length)
                    for (int i = 0; i < m_propView.Length; i ++)
                        m_propView.SetValue((LabelViewProperties.INDEX_PROPERTIES_VIEW)i, (LabelViewProperties.VALUE)Int32.Parse (arPropVal [i]));
                else
                    ; //Ошибка ...

                //Назначить объект
                //int indx = m_listIdContextMenuItems.IndexOf(Int32.Parse(arProp[1]))
                mItem = ContextMenu.MenuItems.Cast<MenuItem>().First(mi => ((FormChangeMode.KeyDevice)mi.Tag).Id == Int32.Parse (arProp [1]));
                indxItem = ContextMenu.MenuItems.IndexOf (mItem);
                if ((!(indxItem < 0))
                    && (indxItem < ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM)) {
                    // инициировать операции по выбору п. меню
                    mItem.PerformClick();
                    // изменить состояние п. меню
                    ContentMenuStateChange();
                }
                else
                    ; //??? Ошибка: не найден
            }

            [Serializable]
            public struct LabelCustomTecViewProfile
            {
                public FormChangeMode.KeyDevice Key;

                public LabelViewProperties Properties;
            }

            [Serializable]
            public class LabelViewProperties
            {
                public enum VALUE { OFF = -1, Zero, ON }

                public enum INDEX_PROPERTIES_VIEW { UNKNOWN = -1, TABLE_MINS, TABLE_HOURS, GRAPH_MINS, GRAPH_HOURS, ORIENTATION, QUICK_PANEL, TABLE_AND_GRAPH, COUNT_PROPERTIES_VIEW };

                private List<VALUE> _values;

                private static List<VALUE> s_defaultValues = new List<VALUE>() {
                    VALUE.OFF // TABLE_MINS
                    , VALUE.OFF // TABLE_HOURS
                    , VALUE.OFF // GRAPH_MINS
                    , VALUE.OFF // GRAPH_HOURS
                    , VALUE.OFF // ORIENTATION
                    , VALUE.OFF // QUICK_PANEL
                    , VALUE.OFF // TABLE_AND_GRAPH
                };

                public LabelViewProperties ()
                {
                    _values = new List<VALUE> (s_defaultValues);
                }

                public LabelViewProperties (LabelViewProperties dest)
                    : this()
                {
                    
                }

                public static string GetText (INDEX_PROPERTIES_VIEW indx)
                {
                    string [] array = { @"Таблица(мин)", @"Таблица(час)", @"График(мин)", @"График(час)", @"Ориентация", @"Оперативные значения", @"Таблица+Гистограмма" };

                    return array [(int)indx];
                }

                public VALUE GetValue (INDEX_PROPERTIES_VIEW indx)
                {
                    return _values [(int)indx];
                }

                public void SetValue (INDEX_PROPERTIES_VIEW indx, VALUE value)
                {
                    _values [(int)indx] = value;
                }

                public void CopyTo (LabelViewProperties dest)
                {

                }

                public int Length
                {
                    get
                    {
                        return (int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW;
                    }
                }

                public int CountOn
                {
                    get
                    {
                        return _values.Count (v => v == VALUE.ON);
                    }
                }

                public int Item1;
            }

            /// <summary>
            /// Возвратить строку с закодированными настройками объекта отображения
            /// </summary>
            /// <returns>Строка для восстановления внешнего вида объекта при очередном вызове на отображение</returns>
            public string SaveProfile()
            {
                string strRes = string.Empty;
                FormChangeMode.KeyDevice key_device;

                key_device = getKeyDeviceMenuItemChecked();
                if (!(key_device.Id < 0))
                {
                    //Идентификатор объекта...
                    strRes += key_device.Id.ToString(); strRes += CHAR_DELIM_PROP;
                    //Параметры объекта...
                    foreach (int prop in m_propView)
                        strRes += prop.ToString() + CHAR_DELIM_ARRAYITEM;
                    //Обрезать лишний символ-разделитель 'CHAR_DELIM_ARRAYITEM'
                    strRes = strRes.Substring(0, strRes.Length - 1);
                }
                else
                    ;

                return strRes;
            }

            public LabelCustomTecViewProfile SaveProfile ()
            {
                LabelCustomTecViewProfile profileRes;
                FormChangeMode.KeyDevice key_device;

                key_device = getKeyDeviceMenuItemChecked ();
                if (!(key_device.Id < 0)) {
                    //Идентификатор объекта...
                    profileRes.Key = key_device;
                    //Параметры объекта...
                    foreach (int prop in m_propView)
                        strRes += prop.ToString () + CHAR_DELIM_ARRAYITEM;
                    //Обрезать лишний символ-разделитель 'CHAR_DELIM_ARRAYITEM'
                    strRes = strRes.Substring (0, strRes.Length - 1);
                } else
                    ;

                return profileRes;
            }
        }

        int m_indxContentMenuItem;
        /// <summary>
        /// Количество фиксированных п.п.  контекстного меню
        /// </summary>
        static int COUNT_FIXED_CONTEXT_MENUITEM = 3;
        static int INDEX_START_CONTEXT_MENUITEM = 1;
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
            MenuItem mItem;

            components = new System.ComponentModel.Container();

            PanelTecViewBase [] arPanelTecViewTable = new PanelTecViewBase [this.RowCount * this.ColumnCount];

            m_arLabelEmpty = new HLabelCustomTecView[this.RowCount * this.ColumnCount];
            //m_arControls = new Controls[this.RowCount * this.ColumnCount];

            m_indxContentMenuItem = m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items.Count + INDEX_START_CONTEXT_MENUITEM;

            for (int i = 0; i < arPanelTecViewTable.Length; i ++) {
                m_arLabelEmpty[i] = new HLabelCustomTecView();

                m_arLabelEmpty [i].ContextMenu = new ContextMenu ();
                //foreach (ToolStripItem tsi in m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items)
                foreach (FormChangeMode.ListBoxItem item in m_formChangeMode.m_listItems)
                {
                    if ((item.bVisibled == true)
                        && (item.key.Id < FormChangeMode.ID_ADMIN_TABS [(int)FormChangeMode.MANAGER.DISP]))
                    {
                        mItem = createMenuItem (item.name_shr);
                        m_arLabelEmpty [i].ContextMenu.MenuItems.Add(mItem);
                        mItem.Tag = item.key;
                        //m_arLabelEmpty[i].m_listIdContextMenuItems.Add(item.id);
                    }
                    else
                        ;
                }

                m_arLabelEmpty[i].AddContextMenuFixedMenuItems(m_indxContentMenuItem, MenuItem_OnClick);

                this.Controls.Add(m_arLabelEmpty [i], getAddress (i).Y, getAddress (i).X);

                //m_arControls [i] = m_arLabelEmpty [i];
            }

            m_formChangeMode.EventMenuItemsClear += new DelegateFunc(OnMenuItemsClear);
            m_formChangeMode.EventMenuItemAdd += new DelegateStringFunc (OnMenuItemAdd);

            initializeLayoutStyle ();

            this.Dock = DockStyle.Fill;

            this.SizeChanged += new EventHandler(onSizeChanged);
        }

        #endregion

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly ();
        }
    }

    public partial class PanelCustomTecView : PanelStatistic
    {
        public event DelegateFunc EventContentChanged;

        private HLabelCustomTecView[] m_arLabelEmpty;
        //private Control[] m_arControls;

        //public bool m_bIsActive;

        private FormChangeMode m_formChangeMode;
        DelegateStringFunc m_fErrorReport, m_fWarningReport, m_fActionReport;
        DelegateBoolFunc m_fReportClear;

        private Point getAddress (int indx) {
            Point ptRes = new Point(indx % this.RowCount, indx / this.ColumnCount);

            return ptRes;
        }

        public PanelCustomTecView(FormChangeMode formCM, Size sz/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fREpClr*/)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            m_formChangeMode = formCM;

            this.RowCount = sz.Height;
            this.ColumnCount = sz.Width;

            //SetDelegateReport (fErrRep, fWarRep, fActRep, fREpClr);

            InitializeComponent();
        }

        public PanelCustomTecView(IContainer container, FormChangeMode formCM, Size sz/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(formCM, sz/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {
            container.Add(this);
        }

        public override void Start()
        {
            base.Start ();
            
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Start(); else ;
            }
        }

        public override void Stop () {
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Stop(); else ;
            }

            base.Stop ();
        }

        public override bool Activate (bool active) {
            bool bRes = base.Activate(active);
            
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Activate(active); else ;
            }

            return bRes;
        }

        //protected override void initTableHourRows()
        //{
        //    //Ничего не делаем, т.к. составные элементы самостоятельно настраивают кол-во строк в таблицах
        //}

        protected void Clear () {
        }

        public override void SetDelegateReport (DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr) {
            m_fErrorReport = fErrRep;
            m_fWarningReport = fWarRep;
            m_fActionReport = fActRep;
            m_fReportClear = fRepClr;

            foreach (var child in Controls) {
                if (child is PanelTecViewBase) {
                    (child as PanelTecViewBase).m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
                } else {
                }
            }
        }

        /// <summary>
        /// Метод непосредственного применения параметров графического представления данных
        /// </summary>
        /// <param name="type">Тип изменившихся параметров</param>
        public override void UpdateGraphicsCurrent(int type)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecViewBase)
                {
                    ((PanelTecViewBase)ctrl).UpdateGraphicsCurrent (type);
                }
                else
                    ;
            }
        }

        private MenuItem createMenuItem (string nameItem) {
            MenuItem menuItemRes = new MenuItem (nameItem);
            menuItemRes.RadioCheck = true;
            menuItemRes.Click += new EventHandler(MenuItem_OnClick);

            return menuItemRes;
        }

        /// <summary>
        /// Обработчик события - изменение
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (панель)</param>
        /// <param name="ev">Аргумент события</param>
        private void onSizeChanged(object obj, EventArgs ev)
        {
            foreach (HLabelCustomTecView label in m_arLabelEmpty)
                label.FitFont();
        }

        private void OnMenuItemsClear  () {
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                while (le.ContextMenu.MenuItems.Count > COUNT_FIXED_CONTEXT_MENUITEM) {
                    le.ContextMenu.MenuItems.RemoveAt (0);
                }

                //le.m_listIdContextMenuItems.Clear();
            }

            m_indxContentMenuItem = INDEX_START_CONTEXT_MENUITEM;
        }
        /// <summary>
        /// Обработчик события - разместить на панели объект отображения
        /// </summary>
        /// <param name="item">Строка с параметрами (идентификатор, наименование) объекта отображения</param>
        private void OnMenuItemAdd (string item) {
            int indx = -1
                , id = Int32.Parse (item.Split (';')[0]);
            MenuItem mItem;
            string nameItem = item.Split(';')[1];
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                indx = le.ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM;
                if (indx < 0) indx = 0; else ;
                mItem = createMenuItem (nameItem);
                le.ContextMenu.MenuItems.Add(indx, mItem);
                //le.m_listIdContextMenuItems.Add(id);
                mItem.Tag = new FormChangeMode.KeyDevice { Id = id, Mode = TECComponent.GetMode (id) };
            }

            m_indxContentMenuItem ++;
        }
        /// <summary>
        /// Обработчик событияе - ывбор п. контекстного меню
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void MenuItem_OnClick(object obj, EventArgs ev)
        {
            int indxLabel = -1
                , indx = ((MenuItem)obj).Index;
            FormChangeMode.KeyDevice key_device = FormChangeMode.KeyDevice.Empty;
            TEC tec = null;
            TECComponent tec_comp = null;
            Point ptAddress;
            PanelTecViewBase panelTecView = null;

            foreach (HLabelCustomTecView le in m_arLabelEmpty)
                if (le.ContextMenu == ((ContextMenu)((MenuItem)obj).Parent)) {
                    indxLabel++;
                    break;
                } else
                    indxLabel++;

            if ((indxLabel < 0)
                || (!(indxLabel < m_arLabelEmpty.Length)))
                return;
            else
                ;

            if (indx == ((ContextMenu)((MenuItem)obj).Parent).MenuItems.Count - 1) {
                //Не устанавливать признак "выбран" для крайнего пункта
                ((MenuItem)obj).Checked = false;
                //Снять с отображения
                foreach (MenuItem mi in ((ContextMenu)((MenuItem)obj).Parent).MenuItems) {
                    if (mi.Checked == true) {
                        mi.Checked = false;
                    } else
                        ;
                }
                clearAddress (indxLabel);

                EnableLabelContextMenuItem (indxLabel, false);
            } else {
                if (((MenuItem)obj).Checked == true) {
                    //Снять с отображения
                    ((MenuItem)obj).Checked = false;
                    clearAddress (indxLabel);

                    EnableLabelContextMenuItem (indxLabel, false);  //.ContextMenu.MenuItems [m_indxContentMenuItem].Enabled = false;
                } else {
                    //Снять с отображения
                    foreach (MenuItem mi in ((ContextMenu)((MenuItem)obj).Parent).MenuItems) {
                        if ((mi.Checked == true) && (!(mi.Index == indx))) {
                            mi.Checked = false;
                        } else
                            ;
                    }
                    clearAddress (indxLabel);

                    //Вызвать на отображение
                    ((MenuItem)obj).Checked = true;
                    // отображаем вкладки ТЭЦ - аналог FormMain::сменитьРежим...
                    key_device = (FormChangeMode.KeyDevice)(obj as MenuItem).Tag;
                    switch (key_device.Mode) {
                        case FormChangeMode.MODE_TECCOMPONENT.TEC:
                            tec = m_formChangeMode.m_list_tec.Find (t => t.m_id == key_device.Id);
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            foreach (TEC t in m_formChangeMode.m_list_tec) {
                            }
                            tec = m_formChangeMode.m_list_tec.Find (t => {
                                return !(t.ListTECComponents.FindIndex(comp => comp.m_id == key_device.Id) < 0);
                            });
                            tec_comp = tec.ListTECComponents.Find (comp => comp.m_id == key_device.Id);
                            break;
                        default:
                            break;
                    }
                    ptAddress = getAddress (indxLabel);

                    panelTecView = null;
                    try {
                        if (tec.m_id > (int)TECComponent.ID.LK)
                            panelTecView = new PanelLKView (tec, key_device, m_arLabelEmpty [indxLabel]);
                        else
                            panelTecView = new PanelTecView (tec, key_device, m_arLabelEmpty [indxLabel]);
                        //        new PanelTecView(m_formChangeMode.m_list_tec[tec_index], tec_index, TECComponent_index, m_arLabelEmpty[indxLabel]/*, m_fErrorReport, m_fWarningReport, m_fActionReport, m_fReportClear*/);
                        panelTecView.SetDelegateReport (m_fErrorReport, m_fWarningReport, m_fActionReport, m_fReportClear);
                        this.Controls.Add (panelTecView, ptAddress.Y, ptAddress.X);
                        this.Controls.SetChildIndex (panelTecView, indxLabel);
                        indxLabel = this.Controls.GetChildIndex (panelTecView);
                        ((PanelTecViewBase)this.Controls [indxLabel]).Start ();
                        ((PanelTecViewBase)this.Controls [indxLabel]).Activate (true);
                    } catch (Exception e) {
                        ASUTP.Logging.Logg ().Exception (e, $"PanelCustomTecView::MenuItem_OnClick () - Индекс={indxLabel}, KeyDevice={key_device.ToString()}..."
                            , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    EnableLabelContextMenuItem (indxLabel, true);
                }
            }

            EventContentChanged ();
        }
        /// <summary>
        /// Включить/отключить п. контекстного меню
        /// </summary>
        /// <param name="indxLabel">Индекс панели</param>
        /// <param name="bEnabled">Признак включения/отключения</param>
        private void EnableLabelContextMenuItem(int indxLabel, bool bEnabled)
        {
            m_arLabelEmpty[indxLabel].EnableContextMenuItem(m_indxContentMenuItem, bEnabled);
        }
        /// <summary>
        /// Очистить панель (снять с отображения объект) по указанному индексу
        /// </summary>
        /// <param name="indx">Индекс панели</param>
        private void clearAddress (int indx) {
            PanelTecViewBase pnlTecView = null;
            // найти панель по индексу
            foreach (Control panel in this.Controls)
            {
                if ((panel is PanelTecViewBase) && (this.Controls.IndexOf (panel) == indx)) {
                    pnlTecView = (PanelTecViewBase)panel;

                    break;
                }
                else
                    ;
            }
            // остановить панель, удалить
            if (! (pnlTecView == null)) {
                pnlTecView.Activate(false);
                pnlTecView.Stop();

                this.Controls.Remove(pnlTecView);
                
                pnlTecView = null;
            }
            else
                ;
            // добавить пустую панель
            Point ptAddress = getAddress (indx);
            m_arLabelEmpty[indx].Text = HLabelCustomTecView.s_msg;
            this.Controls.Add (m_arLabelEmpty [indx], ptAddress.Y, ptAddress.X);
            this.Controls.SetChildIndex(m_arLabelEmpty[indx], indx);
        }
        /// <summary>
        /// Разбор строки с настройками всех панелей (отображаемые объекты, состав отображаемой информации)
        ///  для восстановления
        /// </summary>
        /// <param name="profile">Строка с настройками всех панелей</param>
        public void LoadProfile(string profile)
        {
            string[] arLabel = profile.Split(CHAR_DELIM_LABEL);
            foreach (string label in arLabel)
            {
                string[] arProp = label.Split(CHAR_DELIM_PROP);

                if (arProp.Length == 0)
                    ; //Ошибка...
                else
                    if (arProp.Length == 1)
                        ; //"Пустая"...
                    else
                        if (arProp.Length > 1)
                            if (! (arProp.Length == 3))
                                ; //Ошибка...
                            else
                                m_arLabelEmpty[Int32.Parse(arProp [0])].LoadProfile (arProp);
            }
        }
        /// <summary>
        /// Возвратить строку с настройками всех панелей (отображаемые объекты, состав отображаемой информации)
        ///  для их автоматического восстановления при очередном запуске на выполнение приложения
        /// </summary>
        /// <returns>Строка с настройками, подготовленная к записи в БД</returns>
        public string SaveProfile()
        {
            string strRes = string.Empty;
            int i = -1;

            for (i = 0; i < m_arLabelEmpty.Length; i ++)
            {
                //Координаты...
                strRes += i.ToString(); strRes += CHAR_DELIM_PROP;
                //Содержание
                strRes += m_arLabelEmpty[i].SaveProfile();
                //Разделитель
                strRes += CHAR_DELIM_LABEL;
            }

            //Обрезать лишний символ-разделитель 'CHAR_DELIM_LABEL'
            strRes = strRes.Substring(0, strRes.Length - 1);

            return strRes;
        }
    }
}
