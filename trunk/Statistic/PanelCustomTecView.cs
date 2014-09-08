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
        public class HLabelEmpty : Label {
            public static string s_msg = @"Добавить выбором пункта контекстного меню...";

            public HLabelEmpty () {
                this.Dock = DockStyle.Fill;
                this.Text = s_msg;
                this.BorderStyle = BorderStyle.Fixed3D;
                this.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

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

            this.RowCount = 2;
            this.ColumnCount = 2;

            PanelTecView [] arPanelTecViewTable = new PanelTecView [this.RowCount * this.ColumnCount];

            m_arLabelEmpty = new HLabelEmpty[this.RowCount * this.ColumnCount];
            //m_arControls = new Controls[this.RowCount * this.ColumnCount];

            for (int i = 0; i < arPanelTecViewTable.Length; i ++) {
                m_arLabelEmpty[i] = new HLabelEmpty ();

                m_arLabelEmpty [i].ContextMenu = new ContextMenu ();
                foreach (ToolStripItem tsi in m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items)
                {
                    m_arLabelEmpty[i].ContextMenu.MenuItems.Add(createMenuItem (tsi.Text));
                    //m_arLabelEmpty[i].ContextMenu.MenuItems [m_arLabelEmpty[i].ContextMenu.MenuItems.Count - 1].Click += MenuItem_OnClick;
                }

                m_arLabelEmpty[i].ContextMenu.MenuItems.Add(@"-");
                m_arLabelEmpty[i].ContextMenu.MenuItems.Add(@"Вид");
                m_arLabelEmpty[i].ContextMenu.MenuItems[m_arLabelEmpty[i].ContextMenu.MenuItems.Count - 1].MenuItems.AddRange (new MenuItem [] {new MenuItem (@"Таблица(час)", MenuItem_TableHour)});
                m_arLabelEmpty [i].ContextMenu.MenuItems.Add (@"Очистить");
                m_arLabelEmpty[i].ContextMenu.MenuItems[m_arLabelEmpty[i].ContextMenu.MenuItems.Count - 1].Click += MenuItem_OnClick;

                this.Controls.Add(m_arLabelEmpty [i], getAddress (i).Y, getAddress (i).X);

                //m_arControls [i] = m_arLabelEmpty [i];
            }

            m_formChangeMode.OnMenuItemsClear += new DelegateFunc(OnMenuItemsClear);
            m_formChangeMode.OnMenuItemAdd += new DelegateStringFunc(OnMenuItemAdd);

            //this.RowStyles.Add (new RowStyle (SizeType.AutoSize));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            //this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            this.Dock = DockStyle.Fill;
        }

        #endregion
    }
    
    public partial class PanelCustomTecView : PanelStatistic
    {
        private HLabelEmpty [] m_arLabelEmpty;
        //private Control[] m_arControls;

        public bool m_bIsActive;

        private FormChangeMode m_formChangeMode;
        DelegateFunc m_fErrorReport, m_fActionReport;

        private Point getAddress (int indx) {
            Point ptRes = new Point(indx % this.RowCount, indx / this.ColumnCount);

            return ptRes;
        }

        public PanelCustomTecView(FormChangeMode formCM, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            m_formChangeMode = formCM;

            m_fErrorReport = fErrRep;
            m_fActionReport = fActRep;

            InitializeComponent();
        }

        public PanelCustomTecView(IContainer container, FormChangeMode formCM, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(formCM, fErrRep, fActRep)
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

        private MenuItem createMenuItem (string nameItem) {
            MenuItem menuItemRes = new MenuItem (nameItem);
            menuItemRes.RadioCheck = true;
            menuItemRes.Click += new EventHandler(MenuItem_OnClick);

            return menuItemRes;
        }

        private void OnMenuItemsClear  () {
            foreach (HLabelEmpty le in m_arLabelEmpty) {
                while (le.ContextMenu.MenuItems.Count > 2) {
                    le.ContextMenu.MenuItems.RemoveAt (0);
                }
            }
        }

        private void OnMenuItemAdd (string nameItem) {
            int indx = -1;
            foreach (HLabelEmpty le in m_arLabelEmpty) {
                indx = le.ContextMenu.MenuItems.Count - 2;
                if (indx < 0) indx = 0; else ;
                le.ContextMenu.MenuItems.Add(indx, createMenuItem(nameItem));
            }
        }

        private void MenuItem_TableHour(object obj, EventArgs ev)
        {
        }

        private void MenuItem_OnClick(object obj, EventArgs ev)
        {
            int indxLabel = -1
                , indx = ((MenuItem)obj).Index;

            foreach (HLabelEmpty le in m_arLabelEmpty)
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
            }
            else {
                if (((MenuItem)obj).Checked == true)
                {
                    //Снять с отображения
                    ((MenuItem)obj).Checked = false;
                    clearAddress(indxLabel);
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
            m_arLabelEmpty[indx].Text = HLabelEmpty.s_msg;
            this.Controls.Add (m_arLabelEmpty [indx], ptAddress.Y, ptAddress.X);
            this.Controls.SetChildIndex(m_arLabelEmpty[indx], indx);
        }
    }
}
