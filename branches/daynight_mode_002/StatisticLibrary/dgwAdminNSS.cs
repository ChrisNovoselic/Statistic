using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Drawing;

using HClassLibrary;

namespace StatisticCommon
{
    public class DataGridViewAdminNSS : DataGridViewAdmin
    {
        private enum ID_TYPE : ushort { ID, ID_OWNER, COUNT_ID_TYPE };

        private List <int []> m_listIds;

        DataGridViewCellStyle dgvCellStyleError,
                             dgvCellStyleGTP;

        protected override int INDEX_COLUMN_BUTTON_TO_ALL
        {
            get
            {
                return (int)ColumnCount - 1;
            }
        }

        public DataGridViewAdminNSS(Color []colors) : base(colors)
        {
            m_listIds = new List<int[]>();

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;

            dgvCellStyleGTP = new DataGridViewCellStyle();
            dgvCellStyleGTP.BackColor = Color.Yellow;

            //this.Anchor |= (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right);
            this.Dock = DockStyle.Fill;

            //this.CellValueChanged +=new DataGridViewCellEventHandler(DataGridViewAdminNSS_CellValueChanged);

            this.HorizontalScrollBar.Visible = true;
        }
        
        protected override void InitializeComponents () {
            base.InitializeComponents ();

            int col = -1;
            Columns.AddRange(new DataGridViewColumn[2] { new DataGridViewTextBoxColumn(), new DataGridViewButtonColumn() });
            col = 0;
            // 
            // DateHour
            // 
            Columns[col].Frozen = true;
            Columns[col].HeaderText = "����, ���";
            Columns[col].Name = "DateHour";
            Columns[col].ReadOnly = true;
            Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            col = Columns.Count -1;
            Columns[col].Frozen = false;
            Columns[col].HeaderText = "�����������";
            Columns[col].Name = "ToAll";
            Columns[col].ReadOnly = true;
            Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns [col].DefaultCellStyle.BackColor = SystemColors.Control;

            BackColor = SystemColors.Window;
        }

        public void DataGridViewAdminNSS_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if ((m_listIds.Count == Columns.Count - 2)
                && (Columns[e.ColumnIndex].ReadOnly == false)
                && (e.ColumnIndex > 0)
                && (e.ColumnIndex < Columns.Count - 1))
            {
                int id_gtp = m_listIds [e.ColumnIndex - 1][(int)ID_TYPE.ID_OWNER],
                    col_gtp = -1;
                List<int> list_col_tg = new List<int>();

                foreach (int [] ids in m_listIds) {
                    //����� ������ ������� ��� (������ ���� ���)
                    if ((col_gtp < 0)
                        && (id_gtp == ids[(int)ID_TYPE.ID]) && (ids[(int)ID_TYPE.ID_OWNER] < 0))
                        col_gtp = m_listIds.IndexOf(ids) + 1; // '+ 1' �� ��� ������� "����, �����"
                    else
                        ;

                    //��� ������� ��� ��� � id_gtp == ...
                    if (id_gtp == ids [(int)ID_TYPE.ID_OWNER])
                        list_col_tg.Add(m_listIds.IndexOf(ids) + 1); // '+ 1' �� ��� ������� "����, �����"
                    else
                        ;
                }

                if (list_col_tg.Count > 0) {
                    double plan_gtp = 0.0;

                    foreach (int col in list_col_tg) {
                        plan_gtp += Convert.ToDouble (Rows [e.RowIndex].Cells[col].Value);
                    }

                    if (Convert.ToDouble (Rows [e.RowIndex].Cells[col_gtp].Value).Equals (plan_gtp) == false) {
                        Rows[e.RowIndex].Cells[col_gtp].Style = dgvCellStyleError;
                    }
                    else
                    // �������� ����� ��� ��������� � ������ �������� �������� ��� ��
                        if (Rows[e.RowIndex].Cells[col_gtp].Style.BackColor == dgvCellStyleError.BackColor)
                        // ���� ����� ���� ����������� ������ - ��������� �� ���� ��� ��� "��� ������"
                            Rows[e.RowIndex].Cells[col_gtp].Style = dgvCellStyleGTP;
                        else
                        // ����� ��� ��� ���������� ����������� ����
                        //??? � �����, ����������� ������ � ������ ���������� �� ����� - ��������� ���������� ���. ��������
                            ;
                }
                else
                    ;
            }
            else
                ;
        }
        
        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            if ((e.ColumnIndex > 0) && (e.ColumnIndex < Columns.Count - 1))
            {
                valid = double.TryParse((string)Rows[e.RowIndex].Cells[e.ColumnIndex].Value, out value);
                if ((valid == false) || (value > DataGridViewAdmin.maxRecomendationValue))
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.ToString("F2");
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString("F2");
                }

                DataGridViewAdminNSS_CellValueChanged (sender, e);
            }
            else
                ;
        }

        protected override void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == (Columns.Count - 1)) && (!(e.RowIndex < 0))) // ������ ���������� ��� ����
            {
                DataGridViewCellEventArgs ev;
                
                for (int j = 1; j < Columns.Count - 1; j++) 
                {
                    if (Columns [j].ReadOnly == false)
                        for (int i = e.RowIndex + 1; i < 24; i++)
                        {
                            Rows[i].Cells[j].Value = Rows[e.RowIndex].Cells[j].Value;

                            ev = new DataGridViewCellEventArgs (j, i);
                            DataGridViewAdminNSS_CellValueChanged (null, ev);
                        }
                    else
                        ;
                }
            }
            else
                ;
        }

        public void addTextBoxColumn(string name, int id, int id_owner, DateTime date)
        {
            DataGridViewTextBoxColumn insColumn = new DataGridViewTextBoxColumn ();
            insColumn.Frozen = false;
            insColumn.Width = 66;
            insColumn.HeaderText = name;
            insColumn.Name = "column" + (Columns.Count - 1);
            insColumn.ReadOnly = false;
            insColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            try { Columns.Insert(Columns.Count - 1, insColumn); }
            catch (Exception e) {
                Logging.Logg().Exception(e, "dgwAdminNSS - addTextBoxColumn () - Columns.Insert", Logging.INDEX_MESSAGE.NOT_SET);
            }

            m_listIds.Add (new int [(int)ID_TYPE.COUNT_ID_TYPE] {id, id_owner});

            if (id_owner < 0) {
                Columns[Columns.Count - 1 - 1].Frozen = true;
                Columns [Columns.Count - 1 - 1].ReadOnly = true;
                Columns [Columns.Count - 1 - 1].DefaultCellStyle = dgvCellStyleGTP;
            }
            else
                ;
        }

        public override void ClearTables () {
            int i = -1;

            m_listIds.Clear ();
            
            while (Columns.Count > 2)
            {
                Columns.RemoveAt(Columns.Count - 1 - 1);
            }            
            
            for (i = 0; i < 24; i++)
            {
                Rows[i].Cells[0].Value = string.Empty;
            }
        }

        public int GetIdGTPOwner(int i)
        {
            int iRes = -1;

            if ((i < m_listIds.Count) && ((int)ID_TYPE.ID_OWNER < m_listIds[i].Length))
                iRes = m_listIds [i][(int)ID_TYPE.ID_OWNER];
            else
                ;

            return iRes;
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

                if ((INDEX_COLUMN_BUTTON_TO_ALL > 0)
                    && (RowCount > 0))
                    for (int col = 0; col < (int)INDEX_COLUMN_BUTTON_TO_ALL; col++)
                        for (int i = 0; i < 24; i++) {
                            if ((Rows [i].Cells [col].Style.BackColor.Equals (dgvCellStyleError.BackColor) == false)
                                && (Rows [i].Cells [col].Style.BackColor.Equals (dgvCellStyleError.BackColor) == false))
                            // ������� ����������� ��� ���������� �������� ����� ��� �����
                                Rows [i].Cells [col].Style.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                            else
                                ;
                        }
                else
                // ��� ��������/����� - ��� �������� �� ��������� ����� ���� �����
                    ;
            }
        }
    }
}
