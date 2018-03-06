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

//using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelTecView : PanelTecViewStandard
    {
        public PanelTecView(StatisticCommon.TEC tec, FormChangeMode.KeyDevice key, PanelCustomTecView.HLabelCustomTecView label/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : base(tec, key, new ASUTP.Core.HMark (new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO }))
        {
            m_label = label;
            
            InitializeComponent ();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }

        protected override void InitializeComponent () {
            base.InitializeComponent ();

            if (!(m_label == null))
                m_label.PerformRestruct();
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

                this.Controls.Add(PanelQuickData, 0, 1);
            }

            this.m_ZedGraphMins.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphMins_MouseUpEvent);
            // ��� zedGraphHours ���������� �������� � ������� ������

            this.m_ZedGraphMins.InitializeContextMenuItemAddingEventHandler(this.������ToolStripMenuItemMins_Click, this.sourceDataMins_Click);
            this.m_ZedGraphHours.InitializeContextMenuItemAddingEventHandler(this.������ToolStripMenuItemHours_Click, this.sourceDataHours_Click);
        }

        protected override void createTecView(FormChangeMode.KeyDevice key)
        {
            m_tecView = new TecViewStandard(key);
        }

        protected override void createDataGridViewHours()
        {
            this.m_dgwHours = new DataGridViewStandardHours();
        }

        private void ������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = m_tecView.lastHour;
                if (hour == 24)
                    hour = 23;
                else
                    ;

                sf.CheckPathExists = true;
                sf.ValidateNames = true;
                sf.DereferenceLinks = false; // Will return .lnk in shortcuts.
                sf.DefaultExt = ".xls";
                sf.Filter = s_DialogMSExcelBrowseFilter;
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string strSheetName = "��������_����";
                    //int indxItemMenuStrip = -1;
                    //if (m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_ASKUE)
                    //    indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 2;
                    //else
                    //    if (m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
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
                    if (Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    {
                        if (m_tecView.m_tec.ListTECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            //foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                            //    ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.ListTECComponents.Find(comp => comp.m_id == m_tecView.CurrentKey.Id).name_shr;
                    }

                    //if (m_tecView.m_valuesHours.addonValues && hour == m_tecView.m_valuesHours.hourAddon)
                    //    ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + "* ��� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                    //else
                        ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + " ��� " + _pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "������";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = HAdmin.PBR_PREFIX;
                    ws.Cells[2, 3].Value = $"{HAdmin.PBR_PREFIX}�";
                    ws.Cells[2, 4].Value = "����";
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
                sf.ValidateNames = true;
                sf.DereferenceLinks = false; // Will return .lnk in shortcuts.
                sf.DefaultExt = ".xls";
                sf.Filter = s_DialogMSExcelBrowseFilter;
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    string strSheetName = "�������_����";
                    //int indxItemMenuStrip = -1;
                    //if (m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_ASKUE)
                    //    indxItemMenuStrip = m_ZedGraphHours.ContextMenuStrip.Items.Count - 2;
                    //else
                    //    if (m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
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
                    if (Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    {
                        if (m_tecView.m_tec.ListTECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            //foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                //ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.ListTECComponents.Find(comp => comp.m_id == m_tecView.CurrentKey.Id).name_shr;
                    }

                    ws.Cells[1, 0].Value = "�������� �� " + _pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "���";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = HAdmin.PBR_PREFIX;
                    ws.Cells[2, 3].Value = $"{HAdmin.PBR_PREFIX}�";
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

            if (m_tecView.currHour == true)
                if (!(m_tecView.lastMin > index + 1))
                    //�����, ���� ��������� ������ ��������� "� �������"
                    return true;
                else
                    ;
            else
                ;

            if (found == true)
            {
                //��������, ����������� ������ � ����������� ����������� � "���������" 3-� ��� ����������
                lock (m_tecView.m_lockValue)
                {
                    int prevLastMin = m_tecView.lastMin;
                    m_tecView.recalcAver = true;
                    m_tecView.lastMin = index + 2;
                    m_tecView.GetRetroMinTMGen ();
                    _pnlQuickData.ShowFactValues();
                    //m_tecView.recalcAver = true;
                    //???��������� ������� ���������� ��� ��������������� ��������...
                    //m_tecView.lastMin = prevLastMin;

                    if (m_tecView.currHour == false)
                        setRetroTickTime(m_tecView.lastHour, (index + 1) * m_tecView.GetIntervalOfTypeSourceData(ASUTP.Core.HDateTime.INTERVAL.MINUTES));
                    else
                        ;
                }
            }
            else
                ;

            return true;
        }
    }
}
