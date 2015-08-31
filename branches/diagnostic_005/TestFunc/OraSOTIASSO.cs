using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection

using System.Data.OracleClient;
using System.Data.OleDb;

using HClassLibrary;
using StatisticCommon;

namespace TestFunc
{    
    public class OraSOTIASSO
    {
        HDbOracle m_conn;

        public OraSOTIASSO ()
        {
            m_conn.Open ();
        }
    }

    public abstract class HDbOracle
    {
        protected struct SIGNAL
        {
            public int id;
            public string table;
            public SIGNAL (int id, string table)
            {
                this.id = id;
                this.table = table;
            }
        }
        protected SIGNAL [] arSignals;

        private DateTime dtStart;
        public DbConnection conn;
        public DbCommand cmd;
        public DbDataAdapter adapter;
        public DataTable results;
        public string host = @"10.220.2.5", port = @"1521"
            , dataSource = @"ORCL"
            , uid = @"arch_viewer"
            , pswd = @"1"
            , query = string.Empty;

        public HDbOracle ()
        {
            dtStart = DateTime.Now;

            //Инициализировать массив сигналов
            arSignals = new SIGNAL[] { new SIGNAL(20001, @"TAG_000046")
                                        , new SIGNAL(20002, @"TAG_000047")
                                        , new SIGNAL(20003, @"TAG_000048")
                                        , new SIGNAL(20004, @"TAG_000049")
                                    };

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = DateTime.Now.AddMinutes(-1).ToString(@"yyyyMMdd HHmm")
                , strEnd = DateTime.Now.ToString(@"yyyyMMdd HHmm");
            //Формировать зпрос
            foreach (SIGNAL s in arSignals)
            {
                query += @"SELECT " + s.id + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.table + @" WHERE DATETIME BETWEEN"
                + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24mi')" + @" AND"
                + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24mi')"
                + strUnion
                ;
            }

            //Удалить "лишний" UNION
            query = query.Substring (0, query.Length - strUnion.Length);
            //Установить сортировку
            query += @" ORDER BY DATETIME DESC";
        }

        public abstract int Open();
        public int Query()
        {
            int iRes = 0;
            
            if (conn.State == ConnectionState.Open)
            {
                results = new DataTable();
                adapter.Fill(results);
            }
            else
                ;

            return iRes;
        }
        
        protected int open ()
        {
            int iRes = 0;
                
            conn.Open();

            Console.WriteLine("State: {0}", conn.State);
            Console.WriteLine("ConnectionString: {0}", conn.ConnectionString);
            Console.WriteLine("Query: {0}", query);

            return iRes;
        }

        public void Close ()
        {
            conn.Close();
        }

        public void OutResult ()
        {
            Console.WriteLine("Time (msec): {0}", (DateTime.Now - dtStart).Milliseconds);
            Console.WriteLine("Last changed at: {0}", results.Rows[0][@"DATETIME"]);
            Console.WriteLine("Rows result count: {0}", results.Rows.Count);
        }
    }

    public class HOleDbOracleConnection : HDbOracle
    {
        public HOleDbOracleConnection() : base ()
        {
        }

        public override int Open()
        {
            conn = new OleDbConnection();
            conn.ConnectionString = @"Provider=OraOLEDB.Oracle;host=" + host + @":" + port + @";Data Source=" + dataSource + @";User Id=" + uid + @";Password=" + pswd + @"; OLEDB.NET=True;";

            cmd = (conn as OleDbConnection).CreateCommand();
            adapter = new OleDbDataAdapter(cmd as OleDbCommand);
            
            return open ();
        }
    }

    public class HOracleConnection : HDbOracle
    {            
        public HOracleConnection() : base ()
        {
        }

        public override int Open()
        {
            OracleConnectionStringBuilder csb = new OracleConnectionStringBuilder();
            csb.DataSource = dataSource;
            csb.UserID = uid;
            csb.Password = pswd;

            conn = new OracleConnection(csb.ConnectionString);

            cmd = new OracleCommand(query, conn as OracleConnection);
            adapter = new OracleDataAdapter(cmd as OracleCommand);

            return open();
        }
    }
}
