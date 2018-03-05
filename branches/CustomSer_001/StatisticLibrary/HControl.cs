﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common; //DbConnection
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using ZedGraph;
using ASUTP.Core;



namespace StatisticCommon
{
    /// <summary>
    /// Базовый класс для графического представления значений
    ///  в н.вр. только для 'PanelSobstvNyzhd'
    /// </summary>
    public class HZedGraphControl : ZedGraph.ZedGraphControl
    {
        public enum INDEX_CONTEXTMENU_ITEM
        {
            SHOW_VALUES,
            SEPARATOR_1
                , COPY, SAVE, TO_EXCEL,
            SEPARATOR_2
                , SETTINGS_PRINT, PRINT,
            SEPARATOR_3
                , AISKUE_PLUS_SOTIASSO, AISKUE, SOTIASSO_3_MIN, SOTIASSO_1_MIN
                ,VISIBLE_TABLE
                , COUNT
        };
                
        // контекстные меню
        protected class HContextMenuStripZedGraph : System.Windows.Forms.ContextMenuStrip
        {
            public HContextMenuStripZedGraph()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                // 
                // contextMenuStrip
                // 
                this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    });
                this.Name = "contextMenuStripMins";
                this.Size = new System.Drawing.Size(198, 148);

                int indx = -1;
                // 
                // показыватьЗначенияToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES; ;
                this.Items[indx].Name = "показыватьЗначенияToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Показывать значения";
                ((ToolStripMenuItem)this.Items[indx]).Checked = true;

                // 
                // копироватьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.COPY;
                this.Items[indx].Name = "копироватьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Копировать";

                // 
                // сохранитьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SAVE;
                this.Items[indx].Name = "сохранитьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Сохранить график";

                // 
                // эксельToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL;
                this.Items[indx].Name = "эксельToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Сохранить в MS Excel";

                // 
                // параметрыПечатиToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT;
                this.Items[indx].Name = "параметрыПечатиToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Параметры печати";
                // 
                // распечататьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.PRINT;
                this.Items[indx].Name = "распечататьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Распечатать";

                // 
                // источникАИСКУЭиСОТИАССОToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                this.Items[indx].Name = "источникАИСКУЭиСОТИАССОToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"АИСКУЭ+СОТИАССО"; //"Источник: БД АИСКУЭ+СОТИАССО - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false; //HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO) == true;
                // 
                // источникАИСКУЭToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE;
                this.Items[indx].Name = "источникАИСКУЭToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                //Установлено в конструкторе "родителя"
                //this.источникАИСКУЭToolStripMenuItem.Text = "Источник: БД АИСКУЭ - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = true;
                this.Items[indx].Enabled = false;
                // 
                // источникСОТИАССО3минToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                this.Items[indx].Name = "источникСОТИАССО3минToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"СОТИАССО(3 мин)"; //"Источник: БД СОТИАССО - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false;
                // 
                // источникСОТИАССО1минToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                this.Items[indx].Name = "источникСОТИАССО1минToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"СОТИАССО(1 мин)"; //"Источник: БД СОТИАССО - 1 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false;
                // 
                // отобразитьВТаблицеToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.VISIBLE_TABLE;
                this.Items[indx].Name = "отобразитьВТаблицеToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"Отобразить в таблице"; //"Источник: БД СОТИАССО - 1 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = true;
            }
        }

        private object m_lockValue;

        public string SourceDataText
        {
            get
            {
                for (HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx = INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Checked == true)
                        return ((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Text;
                    else
                        ;

                return string.Empty;
            }
        }

        public HZedGraphControl(object lockVal, DelegateFunc fSetScale)
        {
            this.ContextMenuStrip = new HContextMenuStripZedGraph();

            InitializeComponent();

            m_lockValue = lockVal;

            delegateSetScale = fSetScale;
        }

        private void InitializeComponent()
        {
            // 
            // zedGraph
            // 
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.zedGraphMins].pt;
            this.Name = "zedGraph";
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

            this.BackColor = SystemColors.Window; // SystemColors.Window

            InitializeEventHandler ();

            this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
            this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
        }

        private void InitializeEventHandler()
        {
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES].Click += new System.EventHandler(показыватьЗначенияToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.COPY].Click += new System.EventHandler(копироватьToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SAVE].Click += new System.EventHandler(сохранитьToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT].Click += new System.EventHandler(параметрыПечатиToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.PRINT].Click += new System.EventHandler(распечататьToolStripMenuItem_Click);
        }

        public void InitializeEventHandler(EventHandler fToExcel, EventHandler fSourceData)
        {
            if (fToExcel != null)
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL].Click += new System.EventHandler(fToExcel);
            }
            if (fSourceData != null)
            {
                for (int i = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < this.ContextMenuStrip.Items.Count; i++)
                    ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[i].Click += new System.EventHandler(fSourceData);
            }
        }

        private void показыватьЗначенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            this.IsShowPointValues = ((ToolStripMenuItem)sender).Checked;
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.Copy(false);
            }
        }

        private void параметрыПечатиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = this.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void распечататьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.SaveAs();
            }
        }

        private string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
        {
            return curve[iPt].Y.ToString("f2");
        }

        private bool OnDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            //FormMain.formGraphicsSettings.SetScale();
            delegateSetScale();

            return true;
        }
        /// <summary>
        /// Делегат - изменение способа масштабированиягстограммы
        /// </summary>
        public DelegateFunc delegateSetScale;

        //public override Color BackColor
        //{
        //    get
        //    {
        //        return base.BackColor;
        //    }

        //    set
        //    {
        //        base.BackColor = value;
        //        //this.GraphPane.Fill = new Fill (value);
        //    }
        //}
    }

    public struct ComboBoxItem
    {
        public string Text;

        public FormChangeMode.KeyDevice Tag;

        public override string ToString ()
        {
            return Text;
        }

        public static bool operator == (ComboBoxItem item1, ComboBoxItem item2)
        {
            return item1.Tag == item2.Tag;
        }

        public static bool operator != (ComboBoxItem item1, ComboBoxItem item2)
        {
            return !(item1.Tag == item2.Tag);
        }

        public override bool Equals (object obj)
        {
            return (obj is ComboBoxItem) ? this == (ComboBoxItem)obj : false;
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }
    }
}
