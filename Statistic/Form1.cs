using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Statistic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string connectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=H:\Work\ÍîâîñèáèðñêÝíåðãî\Piramida2000.MDF;Integrated Security=True;Connect Timeout=30;User Instance=True";
            string queryStringJoin = @"SELECT DEVICES.NAME, DATA.OBJECT, SENSORS.NAME AS Expr1, " +
                                    @"DATA.ITEM, DATA.PARNUMBER, DATA.VALUE0, DATA.DATA_DATE " +
                                    @"FROM DEVICES " +
                                    @"INNER JOIN SENSORS ON " +
                                    @"DEVICES.ID = SENSORS.STATIONID " +
                                    @"INNER JOIN DATA ON " +
                                    @"DEVICES.CODE = DATA.OBJECT AND " +
                                    @"SENSORS.CODE = DATA.ITEM AND " +
                                    @"DATA.DATA_DATE > '2009/10/16 1:00:00' AND " +
                                    @"DATA.DATA_DATE <= '2009/10/16 2:00:00' " +
                                    @"WHERE DATA.PARNUMBER = 2 AND SENSORS.NAME LIKE 'ÒÃ%P +' " +
                                    @"ORDER BY DATA.DATA_DATE";


            dataGridView1.AutoGenerateColumns = true;


            bindingSource1.DataSource = GetData(connectionString, queryStringJoin);
            dataGridView1.DataSource = bindingSource1;

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        }

        private static DataTable GetData(string connectionString, string sqlCommand)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(sqlCommand, connection);
            command.CommandType = CommandType.Text;
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = command;

            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            adapter.Fill(table);

            return table;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ïîêàçûâàòüÇíà÷åíèÿToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}