using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System..SqlClient;
using System.Drawing;
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelTecView : PanelTecViewBase
    {
        PanelCustomTecView.HLabelCustomTecView m_label;

        public PanelTecView(StatisticCommon.TEC tec, int num_tec, int num_comp, PanelCustomTecView.HLabelCustomTecView label, DelegateFunc fErrRep, DelegateFunc fActRep)
            : base(tec, num_tec, num_comp, fErrRep, fActRep)
        {
            m_label = label;
            
            InitializeComponent ();
        }

        protected override void InitializeComponent () {
            base.InitializeComponent ();

            if (! (m_label == null))
            {
                m_label.Text = m_tecView.m_tec.name_shr;
                if (!(indx_TECComponent < 0))
                    m_label.Text += @" - " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                else
                    ;

                m_label.EventRestruct += new DelegateFunc(OnEventRestruct);

                OnEventRestruct ();
            }
            else
            {
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 84));
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 16));

                this.Controls.Add(this.stctrView, 0, 0);

                this.stctrView.Orientation = System.Windows.Forms.Orientation.Horizontal;

                this.stctrViewPanel1.Orientation = Orientation.Vertical;
                this.stctrViewPanel1.Panel1.Controls.Add(this.m_dgwMins);
                this.stctrViewPanel1.Panel2.Controls.Add(this.m_ZedGraphMins);
                this.stctrView.Panel1.Controls.Add(this.stctrViewPanel1);

                this.stctrViewPanel2.Orientation = Orientation.Vertical;
                this.stctrViewPanel2.Panel1.Controls.Add(this.m_dgwHours);
                this.stctrViewPanel2.Panel2.Controls.Add(this.m_ZedGraphHours);
                this.stctrView.Panel2.Controls.Add(this.stctrViewPanel2);

                this.Controls.Add(m_pnlQuickData, 0, 1);
            }

            this.m_ZedGraphMins.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphMins_MouseUpEvent);
            this.m_ZedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_MouseUpEvent);

            this.m_ZedGraphMins.InitializeEventHandler(this.эксельToolStripMenuItemMins_Click, this.sourceDataMins_Click);
            this.m_ZedGraphHours.InitializeEventHandler(this.эксельToolStripMenuItemHours_Click, this.sourceDataHours_Click);
        }

        private void OnEventRestruct () {
            this.Controls.Clear ();
            this.RowStyles.Clear ();
            stctrView.Panel1.Controls.Clear ();
            stctrView.Panel2.Controls.Clear();
            this.stctrViewPanel1.Panel1.Controls.Clear ();
            this.stctrViewPanel2.Panel1.Controls.Clear();

            int iRow = 0;
            int iPercTotal = 100;
            int[] arPercRows = { 5, 71 };

            this.Controls.Add(m_label, 0, iRow);
            iPercTotal -= arPercRows[iRow];
            this.RowStyles.Add(new RowStyle(SizeType.Percent, arPercRows[iRow++]));

            if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.ORIENTATION] < 0) {
                //Отобразить ТОЛЬКО один элемент
                bool bVisible = true;
                if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1)
                    this.Controls.Add(m_dgwMins, 0, iRow);
                else
                    if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1)
                        this.Controls.Add(m_dgwHours, 0, iRow);
                    else
                        if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1)
                            this.Controls.Add(m_ZedGraphMins, 0, iRow);
                        else
                            if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1)
                                this.Controls.Add(m_ZedGraphHours, 0, iRow);
                            else
                                bVisible = false;

                if (bVisible == true) {
                    iPercTotal -= arPercRows [iRow];
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, arPercRows[iRow ++]));
                }
                else
                    ;
            }
            else
            { //Отобразить ДВА или ЧЕТЫРЕ элемента
                if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                    (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                { //Отобразить 4 элемента (таблица(мин) + таблица(час) + график(мин) + график(час))
                }
                else
                { //Отобразить ДВА элемента
                    if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.ORIENTATION] == 0) {
                        stctrView.Orientation = Orientation.Vertical;

                        stctrView.SplitterDistance = stctrView.Width / 2;
                    }
                    else {
                        stctrView.Orientation = Orientation.Horizontal;

                        stctrView.SplitterDistance = stctrView.Height / 2;
                    }

                    if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                        (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                    { //Отобразить 2 элемента (таблица(мин) + таблица(час))
                        stctrView.Panel1.Controls.Add(m_dgwMins);
                        stctrView.Panel2.Controls.Add(m_dgwHours);                            
                    }
                    else {
                        if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                            (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                        { //Отобразить 2 элемента (график(мин) + график(час))
                            stctrView.Panel1.Controls.Add(m_ZedGraphMins);
                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                        }
                        else
                        {
                            if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                                (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                            { //Отобразить 2 элемента (таблица(мин) + график(мин))
                                stctrView.Panel1.Controls.Add(m_dgwMins);
                                stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                            }
                            else
                            {
                                if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                                    (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                                { //Отобразить 2 элемента (таблица(час) + график(час))
                                    stctrView.Panel1.Controls.Add(m_dgwHours);
                                    stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                }
                                else
                                {
                                    if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                                        (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                                    { //Отобразить 2 элемента (таблица(час) + график(час))
                                        stctrView.Panel1.Controls.Add(m_dgwHours);
                                        stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                                    }
                                    else
                                    {
                                        if ((m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                                            (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                                        { //Отобразить 2 элемента (таблица(час) + график(час))
                                            stctrView.Panel1.Controls.Add(m_dgwMins);
                                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }

                    this.Controls.Add(this.stctrView, 0, iRow);
                    iPercTotal -= arPercRows[iRow];
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, arPercRows[iRow ++]));                        
                }

                switch (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_AND_GRAPH])
                {
                    case -1: //Таблица и график с аналогичными интервалами НЕ МОГУТ быть размещены в одном 'SplitContainer'
                        break;
                    case 0:
                        break;
                    case 1:
                        break;
                    default:
                        break;
                }
            }

            if (m_label.m_propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.QUICK_PANEL] == 1) {                    
                this.Controls.Add(m_pnlQuickData, 0, iRow);
                m_pnlQuickData.ShowFactValues();
                m_pnlQuickData.ShowTMValues ();
            }
            else {
            }

            this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercTotal));
        }

        private void эксельToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = m_tecView.lastHour;
                if (hour == 24)
                    hour = 23;

                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "Файл Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string strSheetName = "Минутные_знач";
                    //int indxItemMenuStrip = -1;
                    //if (m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_ASKUE)
                    //    indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 2;
                    //else
                    //    if (m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                    //        indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 1;
                    //    else
                    //        ;

                    //if (! (indxItemMenuStrip < 0))
                    //    strSheetName += @"(" + m_ZedGraphHours.ContextMenuStrip.Items[indxItemMenuStrip].Text + @")";
                    //else
                    //    ;
                    
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add(strSheetName);
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (m_tecView.m_tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    //if (m_tecView.m_valuesHours.addonValues && hour == m_tecView.m_valuesHours.hourAddon)
                    //    ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + "* час " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                    //else
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + " час " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "Минута";
                    ws.Cells[2, 1].Value = "Факт";
                    ws.Cells[2, 2].Value = "ПБР";
                    ws.Cells[2, 3].Value = "ПБРэ";
                    ws.Cells[2, 4].Value = "УДГэ";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < m_dgwMins.Rows.Count; i++)
                    {
                        valid = int.TryParse((string)m_dgwMins.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = m_dgwMins.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = m_dgwMins.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = m_dgwMins.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = m_dgwMins.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = m_dgwMins.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = m_dgwMins.Rows[i].Cells[5].Value;
                    }

                    int tryes = 5;
                    while (tryes > 0)
                    {
                        try
                        {
                            ef.SaveXls(sf.FileName);
                            break;
                        }
                        catch
                        {
                            FileInfo fi = new FileInfo(sf.FileName);
                            sf.FileName = fi.DirectoryName + "\\Copy " + fi.Name;
                        }
                        tryes--;
                        if (tryes == 0)
                            MessageBox.Show(this, "Не удалось сохранить файл.\nВозможно нет доступа, либо файл занят другим приложением.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void эксельToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "Файл Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string strSheetName = "Часовые_знач";
                    //int indxItemMenuStrip = -1;
                    //if (m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_ASKUE)
                    //    indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 2;
                    //else
                    //    if (m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
                    //        indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 1;
                    //    else
                    //        ;

                    //if (! (indxItemMenuStrip < 0))
                    //    strSheetName += @" (" + m_ZedGraphHours.ContextMenuStrip.Items[indxItemMenuStrip].Text + @")";
                    //else
                    //    ;

                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add(strSheetName);
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (m_tecView.m_tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    ws.Cells[1, 0].Value = "Мощность на " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "Час";
                    ws.Cells[2, 1].Value = "Факт";
                    ws.Cells[2, 2].Value = "ПБР";
                    ws.Cells[2, 3].Value = "ПБРэ";
                    ws.Cells[2, 4].Value = "УДГэ";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < m_dgwHours.Rows.Count; i++)
                    {
                        valid = int.TryParse((string)m_dgwHours.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = m_dgwHours.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = m_dgwHours.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = m_dgwHours.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = m_dgwHours.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = m_dgwHours.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = m_dgwHours.Rows[i].Cells[5].Value;
                    }

                    int tryes = 5;
                    while (tryes > 0)
                    {
                        try
                        {
                            ef.SaveXls(sf.FileName);
                            break;
                        }
                        catch
                        {
                            FileInfo fi = new FileInfo(sf.FileName);
                            sf.FileName = fi.DirectoryName + "\\Copy " + fi.Name;
                        }
                        tryes--;
                        if (tryes == 0)
                            MessageBox.Show(this, "Не удалось сохранить файл.\nВозможно нет доступа, либо файл занят другим приложением.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool zedGraphMins_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                //Выход, если событие не от "Левой" кн.
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            //Поиск объекта
            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                //Выход, если объект не "требуемого" типа
                return true;
            
            if (m_tecView.lastMin <= index + 1)
                //Выход, если выбранный объект находится "в будущем"
                return true;

            if (found == true)
            {
                //Пересчет, перерисовка панели с оперативной информацией с "выбранным" 3-х мин интервалом
                lock (m_tecView.m_lockValue)
                {
                    int prevLastMin = m_tecView.lastMin;
                    m_tecView.recalcAver = true;
                    m_tecView.lastMin = index + 2;
                    m_pnlQuickData.ShowFactValues();
                    //m_tecView.recalcAver = true;
                    m_tecView.lastMin = prevLastMin;

                    if (m_tecView.currHour == false)
                        setRetroTickTime(m_tecView.lastHour, (index + 1) * 3);
                    else
                        ;
                }
            }
            else
                ;

            return true;
        }

        private bool zedGraphHours_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                return true;

            if ((! (m_tecView == null)) && (found == true))
            {
                if (!(delegateStartWait == null)) delegateStartWait(); else ;

                bool bRetroHour = m_tecView.zedGraphHours_MouseUpEvent(index);

                if (bRetroHour == true)
                    setRetroTickTime(m_tecView.lastHour, 60);
                else {
                    ////Вариань №1
                    //setNowDate(false);

                    //Вариань №2
                    m_tecView.currHour = true;
                    NewDateRefresh();
                }

                if (!(delegateStopWait == null)) delegateStopWait(); else ;
            }

            return true;
        }
    }
}
