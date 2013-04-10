using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void FillMinTable(DataTable table)
        {
            dgwMT.Rows.Clear();
            if (table.Rows.Count <= 0)
                return;
            dgwMT.Rows.Add(table.Rows.Count);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                dgwMT.Rows[i].Cells[0].Value = table.Rows[i][0].ToString();
                dgwMT.Rows[i].Cells[1].Value = table.Rows[i][1].ToString();
                dgwMT.Rows[i].Cells[2].Value = table.Rows[i][2].ToString();
                dgwMT.Rows[i].Cells[3].Value = table.Rows[i][3].ToString();
                dgwMT.Rows[i].Cells[4].Value = table.Rows[i][4].ToString();
                dgwMT.Rows[i].Cells[5].Value = table.Rows[i][5].ToString();
                dgwMT.Rows[i].Cells[6].Value = table.Rows[i][6].ToString();
                dgwMT.Rows[i].Cells[7].Value = table.Rows[i][7].ToString();
            }
        }

        public void FillMinValues(int lastMin, DateTime selectedTime, double[] valuesFact)
        {
            dgwM.Rows.Clear();
            dgwM.Rows.Add(2 + valuesFact.Length);
            dgwM.Rows[0].Cells[0].Value = "lastMin";
            dgwM.Rows[0].Cells[1].Value = lastMin.ToString();
            dgwM.Rows[1].Cells[0].Value = "selectedTime";
            dgwM.Rows[1].Cells[1].Value = selectedTime.ToString();
            for (int i = 0; i < valuesFact.Length; i++)
            {
                dgwM.Rows[2 + i].Cells[0].Value = "valuesFact[" + i.ToString() + "]";
                dgwM.Rows[2 + i].Cells[1].Value = valuesFact[i].ToString();
            }
        }

        public void FillHourTable(DataTable table)
        {
            dgwHT.Rows.Clear();
            if (table.Rows.Count <= 0)
                return;
            dgwHT.Rows.Add(table.Rows.Count);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                dgwHT.Rows[i].Cells[0].Value = table.Rows[i][0].ToString();
                dgwHT.Rows[i].Cells[1].Value = table.Rows[i][1].ToString();
                dgwHT.Rows[i].Cells[2].Value = table.Rows[i][2].ToString();
                dgwHT.Rows[i].Cells[3].Value = table.Rows[i][3].ToString();
                dgwHT.Rows[i].Cells[4].Value = table.Rows[i][4].ToString();
                dgwHT.Rows[i].Cells[5].Value = table.Rows[i][5].ToString();
                dgwHT.Rows[i].Cells[6].Value = table.Rows[i][6].ToString();
                dgwHT.Rows[i].Cells[7].Value = table.Rows[i][7].ToString();
            }
        }

        public void FillHourValues(int lastHour, DateTime selectedTime, double[] valuesFact)
        {
            dgwH.Rows.Clear();
            dgwH.Rows.Add(2 + valuesFact.Length);
            dgwH.Rows[0].Cells[0].Value = "lastHour";
            dgwH.Rows[0].Cells[1].Value = lastHour.ToString();
            dgwH.Rows[1].Cells[0].Value = "selectedTime";
            dgwH.Rows[1].Cells[1].Value = selectedTime.ToString();
            for (int i = 0; i < valuesFact.Length; i++)
            {
                dgwH.Rows[2 + i].Cells[0].Value = "valuesFact[" + i.ToString() + "]";
                dgwH.Rows[2 + i].Cells[1].Value = valuesFact[i].ToString();
            }
        }
    }
}