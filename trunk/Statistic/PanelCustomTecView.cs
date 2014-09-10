using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using StatisticCommon;

namespace Statistic
{
    partial class PanelCustomTecView
    {
        public class HLabelCustomTecView : Label {
            public static string s_msg = @"Добавить выбором пункта контекстного меню...";
            public enum INDEX_PROPERTIES_VIEW { TABLE_MINS, TABLE_HOURS, GRAPH_MINS, GRAPH_HOURS, ORIENTATION, QUICK_PANEL, TABLE_AND_GRAPH, COUNT_PROPERTIES_VIEW };
            public int [] m_propView;
            private static string[] s_arContentMenuItems = { @"Таблица(мин)", @"Таблица(час)", @"График(мин)", @"График(час)", @"Ориентация", @"Оперативные значения", @"Таблица+Гистограмма" };

            public event DelegateFunc EventRestruct;

            private int m_prevViewOrientation;
            
            //public bool ContentEnabled {
            //    get {
            //        bool bRes = true;

            //        if (m_propView == null)
            //            bRes = false;
            //        else
            //            for (int i = 0; i < (int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW; i ++)
            //            {
            //                if (m_propView [i] < 0)
            //                {
            //                    bRes = false;
            //                    break;
            //                }
            //                else
            //                    ;
            //            }

            //        return bRes;
            //    }
            //}

            public HLabelCustomTecView()
            {
                this.Dock = DockStyle.Fill;
                this.Text = s_msg;
                this.BorderStyle = BorderStyle.Fixed3D;
                this.TextAlign = ContentAlignment.MiddleCenter;

                m_propView = new int[(int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW] {0, 0, 0, 1, -1, 0, -1};

                m_prevViewOrientation = -1;
            }

            private void OnMenuItem_Content(object obj, EventArgs ev)
            {
                int indx = ((MenuItem)obj).Index;

                if ((((MenuItem)obj).Parent is MenuItem) && (! (((MenuItem)obj).Parent.MenuItems.Count == 2)))
                    setProperty(((MenuItem)obj).Index, ((MenuItem)obj).Checked == true ? 0 : 1);
                else
                    setProperty((int)INDEX_PROPERTIES_VIEW.ORIENTATION, indx);

                EventRestruct ();

                ContentMenuStateChange ();
            }

            private void setProperty (int indx, int newVal) {
                m_propView [indx] = newVal;

                int i = -1
                    , cnt = 0;
                for (i = (int)INDEX_PROPERTIES_VIEW.TABLE_MINS; ! (i > (int)INDEX_PROPERTIES_VIEW.GRAPH_HOURS); i ++)
                    if (m_propView [i] == 1) cnt ++; else ;

                if (cnt > 1) {
                    if (cnt > 2) {
                        //if (cnt > 3) {
                            int iStart = -1
                                , iEnd = -1;
                            if (indx < (int)INDEX_PROPERTIES_VIEW.GRAPH_MINS)
                            { //3-й установленный признак - таблица: снять с отображения графики
                                iStart = (int)INDEX_PROPERTIES_VIEW.GRAPH_MINS;
                                iEnd = (int)INDEX_PROPERTIES_VIEW.GRAPH_HOURS;
                            }
                            else
                            { //3-й установленный признак - график: снять с отображения таблицы
                                iStart = (int)INDEX_PROPERTIES_VIEW.TABLE_MINS;
                                iEnd = (int)INDEX_PROPERTIES_VIEW.TABLE_HOURS;
                            }

                            for (i = iStart; ! (i > iEnd); i ++)
                                m_propView [i] = 0; //Снять с отображения

                            cnt = 1;
                        //} else ;
                    }
                    else
                        ;

                    if (cnt > 1)
                        if (m_propView[(int)INDEX_PROPERTIES_VIEW.ORIENTATION] < 0)
                            if (m_prevViewOrientation < 0)
                                m_propView[(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = 0; //Вертикально - по умолчанию
                            else
                                //Восстановить значение "ориентация сплиттера"
                                m_propView[(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = m_prevViewOrientation;
                        else
                            ; //Оставить "как есть"
                    else
                        //Запомнить предыдущее стостояние "ориентация сплиттера"
                        savePrevViewOrientation ();
                }
                else {
                    //Запомнить предыдущее стостояние "ориентация сплиттера"
                    savePrevViewOrientation ();
                }
            }

            /// <summary>
            /// Запомнить предыдущее стостояние "ориентация сплиттера"
            /// </summary>
            private void savePrevViewOrientation () {
                m_prevViewOrientation = m_propView[(int)INDEX_PROPERTIES_VIEW.ORIENTATION];
                //Блокировать возможность выбора "ориентация сплиттера"
                m_propView[(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = -1;
            }

            private MenuItem[] createContentMenuItems()
            {
                int indx = -1;
                MenuItem[] arMenuItems = new MenuItem[(int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW];

                indx = (int)INDEX_PROPERTIES_VIEW.TABLE_MINS;
                arMenuItems [indx] = new MenuItem (s_arContentMenuItems[indx], this.OnMenuItem_Content);

                indx = (int)INDEX_PROPERTIES_VIEW.TABLE_HOURS;
                arMenuItems [indx] = new MenuItem (s_arContentMenuItems[indx], this.OnMenuItem_Content);

                indx = (int)INDEX_PROPERTIES_VIEW.GRAPH_MINS;
                arMenuItems [indx] = new MenuItem (s_arContentMenuItems[indx], this.OnMenuItem_Content);

                indx = (int)INDEX_PROPERTIES_VIEW.GRAPH_HOURS;
                arMenuItems[indx] = new MenuItem(s_arContentMenuItems[indx], this.OnMenuItem_Content);

                indx = (int)INDEX_PROPERTIES_VIEW.ORIENTATION;
                arMenuItems [indx] = new MenuItem (s_arContentMenuItems[indx], new MenuItem[] { new MenuItem (@"Вертикально", this.OnMenuItem_Content), new MenuItem (@"Горизонтально", this.OnMenuItem_Content) });                

                indx = (int)INDEX_PROPERTIES_VIEW.QUICK_PANEL;
                arMenuItems [indx] = new MenuItem (s_arContentMenuItems[indx], this.OnMenuItem_Content);

                indx = (int)INDEX_PROPERTIES_VIEW.TABLE_AND_GRAPH;
                arMenuItems[indx] = new MenuItem(s_arContentMenuItems[indx], this.OnMenuItem_Content);

                return arMenuItems;
            }

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

            public void AddContextMenuFixedMenuItems (int indx, EventHandler f) {
                this.ContextMenu.MenuItems.Add(@"-");
                this.ContextMenu.MenuItems.Add(@"Содержание", createContentMenuItems());
                this.ContextMenu.MenuItems[indx].Enabled = false;
                this.ContextMenu.MenuItems.Add(@"Очистить");
                this.ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].Click += f;

                ContentMenuStateChange ();
            }
        }

        int m_indxContentMenuItem;
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
            components = new System.ComponentModel.Container();

            PanelTecView [] arPanelTecViewTable = new PanelTecView [this.RowCount * this.ColumnCount];

            m_arLabelEmpty = new HLabelCustomTecView[this.RowCount * this.ColumnCount];
            //m_arControls = new Controls[this.RowCount * this.ColumnCount];

            m_indxContentMenuItem = m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items.Count + INDEX_START_CONTEXT_MENUITEM;

            for (int i = 0; i < arPanelTecViewTable.Length; i ++) {
                m_arLabelEmpty[i] = new HLabelCustomTecView();

                m_arLabelEmpty [i].ContextMenu = new ContextMenu ();
                foreach (ToolStripItem tsi in m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items)
                {
                    m_arLabelEmpty[i].ContextMenu.MenuItems.Add(createMenuItem (tsi.Text));
                    //m_arLabelEmpty[i].ContextMenu.MenuItems [m_arLabelEmpty[i].ContextMenu.MenuItems.Count - 1].Click += MenuItem_OnClick;
                }

                m_arLabelEmpty[i].AddContextMenuFixedMenuItems(m_indxContentMenuItem, MenuItem_OnClick);

                this.Controls.Add(m_arLabelEmpty [i], getAddress (i).Y, getAddress (i).X);

                //m_arControls [i] = m_arLabelEmpty [i];
            }

            m_formChangeMode.OnMenuItemsClear += new DelegateFunc(OnMenuItemsClear);
            m_formChangeMode.OnMenuItemAdd += new DelegateStringFunc(OnMenuItemAdd);

            for (int i = 0; i < RowCount; i++)
            {
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float) Math.Floor (100F / RowCount)));
            }

            for (int i = 0; i < ColumnCount; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)Math.Floor(100F / ColumnCount)));
            }

            this.Dock = DockStyle.Fill;
        }

        #endregion
    }
    
    public partial class PanelCustomTecView : PanelStatistic
    {
        private HLabelCustomTecView[] m_arLabelEmpty;
        //private Control[] m_arControls;

        public bool m_bIsActive;

        private FormChangeMode m_formChangeMode;
        DelegateFunc m_fErrorReport, m_fActionReport;

        private Point getAddress (int indx) {
            Point ptRes = new Point(indx % this.RowCount, indx / this.ColumnCount);

            return ptRes;
        }

        public PanelCustomTecView(FormChangeMode formCM, Size sz, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            m_formChangeMode = formCM;

            this.RowCount = sz.Height;
            this.ColumnCount = sz.Width;

            m_fErrorReport = fErrRep;
            m_fActionReport = fActRep;

            InitializeComponent();
        }

        public PanelCustomTecView(IContainer container, FormChangeMode formCM, Size sz, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(formCM, sz, fErrRep, fActRep)
        {
            container.Add(this);
        }

        public override void Start()
        {
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecView) ((PanelTecView)panel).Start(); else ;
            }
        }

        public override void Stop () {
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecView) ((PanelTecView)panel).Stop(); else ;
            }
        }

        public override void Activate (bool active) {
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecView) ((PanelTecView)panel).Activate(active); else ;
            }
        }

        protected void Clear () {
        }

        public void UpdateGraphicsCurrent()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecView)
                {
                    ((PanelTecView)ctrl).UpdateGraphicsCurrent ();
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

        private void OnMenuItemsClear  () {
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                while (le.ContextMenu.MenuItems.Count > COUNT_FIXED_CONTEXT_MENUITEM) {
                    le.ContextMenu.MenuItems.RemoveAt (0);
                }
            }

            m_indxContentMenuItem = INDEX_START_CONTEXT_MENUITEM;
        }

        private void OnMenuItemAdd (string nameItem) {
            int indx = -1;
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                indx = le.ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM;
                if (indx < 0) indx = 0; else ;
                le.ContextMenu.MenuItems.Add(indx, createMenuItem(nameItem));
            }

            m_indxContentMenuItem ++;
        }

        private void MenuItem_OnClick(object obj, EventArgs ev)
        {
            int indxLabel = -1
                , indx = ((MenuItem)obj).Index;

            foreach (HLabelCustomTecView le in m_arLabelEmpty)
                if (le.ContextMenu == ((ContextMenu)((MenuItem)obj).Parent)) {
                    indxLabel++;
                    break;
                }
                else
                    indxLabel ++;

            if ((indxLabel < 0) || (! (indxLabel < m_arLabelEmpty.Length)))
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
                    }
                    else
                        ;
                }
                clearAddress(indxLabel);

                m_arLabelEmpty[indxLabel].ContextMenu.MenuItems [m_indxContentMenuItem].Enabled = false;
            }
            else {
                if (((MenuItem)obj).Checked == true)
                {
                    //Снять с отображения
                    ((MenuItem)obj).Checked = false;
                    clearAddress(indxLabel);

                    m_arLabelEmpty[indxLabel].ContextMenu.MenuItems [m_indxContentMenuItem].Enabled = false;
                }
                else
                {
                    //Снять с отображения
                    foreach (MenuItem mi in ((ContextMenu)((MenuItem)obj).Parent).MenuItems) {
                        if ((mi.Checked == true) && (! (mi.Index == indx))) {
                            mi.Checked = false;
                        }
                        else
                            ;
                    }
                    clearAddress(indxLabel);

                    //Вызвать на отображение
                    ((MenuItem)obj).Checked = true;
                    // отображаем вкладки ТЭЦ - аналог FormMain::сменитьРежим...
                    int tec_index = m_formChangeMode.m_list_tec_index[indx],
                        TECComponent_index = m_formChangeMode.m_list_TECComponent_index[indx];
                    Point ptAddress = getAddress(indxLabel);

                    PanelTecView panelTecView = new PanelTecView(m_formChangeMode.m_list_tec[tec_index], tec_index, TECComponent_index, m_arLabelEmpty[indxLabel], m_fErrorReport, m_fActionReport);
                    this.Controls.Add (panelTecView, ptAddress.Y, ptAddress.X);
                    this.Controls.SetChildIndex(panelTecView, indxLabel);
                    indxLabel = this.Controls.GetChildIndex(panelTecView);
                    ((PanelTecView)this.Controls [indxLabel]).Start ();
                    ((PanelTecView)this.Controls[indxLabel]).Activate(true);

                    m_arLabelEmpty[indxLabel].ContextMenu.MenuItems [m_indxContentMenuItem].Enabled = true;
                }
            }
        }

        private void clearAddress (int indx) {
            PanelTecView pnlTecView = null;
            foreach (Control panel in this.Controls)
            {
                if ((panel is PanelTecView) && (this.Controls.IndexOf (panel) == indx)) {
                    pnlTecView = (PanelTecView)panel;

                    break;
                }
                else
                    ;
            }

            if (! (pnlTecView == null)) {
                pnlTecView.Activate(false);
                pnlTecView.Stop();

                this.Controls.Remove(pnlTecView);
                
                pnlTecView = null;
            }
            else
                ;

            Point ptAddress = getAddress (indx);
            m_arLabelEmpty[indx].Text = HLabelCustomTecView.s_msg;
            this.Controls.Add (m_arLabelEmpty [indx], ptAddress.Y, ptAddress.X);
            this.Controls.SetChildIndex(m_arLabelEmpty[indx], indx);
        }
    }
}
