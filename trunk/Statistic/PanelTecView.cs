using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using StatisticCommon;

namespace Statistic
{
    public class PanelTecView : PanelTecViewBase
    {
        private enum INDEX_PROPERTIES_VIEW { TABLE_MINS, TABLE_HOURS, GRAPH_MINS, GRAPH_HOURS, QUICK_PANEL, ORIENTATION, TABLE_AND_GRAPH};
        HMark m_propView;

        PanelCustomTecView.HLabelEmpty m_label;

        public PanelTecView(StatisticCommon.TEC tec, int num_tec, int num_comp, PanelCustomTecView.HLabelEmpty label, DelegateFunc fErrRep, DelegateFunc fActRep)
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

                this.RowStyles.Add(new RowStyle(SizeType.Percent, 5));
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 71));
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 24));

                this.Controls.Add(m_label, 0, 0);

                //stctrView.Orientation = Orientation.Vertical;
                //stctrView.SplitterDistance = 76;

                //stctrView.Panel1.Controls.Add(m_dgwMins);
                //stctrView.Panel2.Controls.Add(m_dgwHours);
                //this.Controls.Add(this.stctrView, 0, 1);

                this.Controls.Add(m_ZedGraphHours, 0, 1);

                this.Controls.Add(m_pnlQuickData, 0, 2);
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

            this.m_ZedGraphMins.InitializeEventHandler(this.������ToolStripMenuItemMins_Click);
            this.m_ZedGraphHours.InitializeEventHandler(this.������ToolStripMenuItemHours_Click);
        }

        private void ������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = m_tecView.lastHour;
                if (hour == 24)
                    hour = 23;

                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "���� Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("����������� ������");
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

                    if (m_tecView.m_valuesHours.addonValues && hour == m_tecView.m_valuesHours.hourAddon)
                        ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + "* ��� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                    else
                        ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + " ��� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "������";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = "���";
                    ws.Cells[2, 3].Value = "����";
                    ws.Cells[2, 4].Value = "����";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < 21; i++)
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
                            MessageBox.Show(this, "�� ������� ��������� ����.\n�������� ��� �������, ���� ���� ����� ������ �����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "���� Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("������� ������");
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

                    ws.Cells[1, 0].Value = "�������� �� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "���";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = "���";
                    ws.Cells[2, 3].Value = "����";
                    ws.Cells[2, 4].Value = "����";
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
                            MessageBox.Show(this, "�� ������� ��������� ����.\n�������� ��� �������, ���� ���� ����� ������ �����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool zedGraphMins_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                //�����, ���� ������� �� �� "�����" ��.
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            //����� �������
            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                //�����, ���� ������ �� "����������" ����
                return true;
            
            if (m_tecView.lastMin <= index + 1)
                //�����, ���� ��������� ������ ��������� "� �������"
                return true;

            if (found == true)
            {
                //��������, ����������� ������ � ����������� ����������� � "���������" 3-� ��� ����������
                lock (m_tecView.m_lockValue)
                {
                    int prevLastMin = m_tecView.lastMin;
                    m_tecView.recalcAver = true;
                    m_tecView.lastMin = index + 2;
                    m_pnlQuickData.ShowFactValues();
                    //m_tecView.recalcAver = true;
                    m_tecView.lastMin = prevLastMin;
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

            if (found == true)
            {
                delegateStartWait();

                m_tecView.zedGraphHours_MouseUpEvent(index);

                delegateStopWait();
            }

            return true;
        }
    }
}
