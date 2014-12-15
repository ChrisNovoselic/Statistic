using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace StatisticCommon
{
    public abstract partial class FormParameters : FormParametersBase
    {
        public enum PARAMETR_SETUP { POLL_TIME, ERROR_DELAY, MAX_ATTEMPT, WAITING_TIME, WAITING_COUNT, MAIN_DATASOURCE,
                                    /*ALARM_USE, */ALARM_TIMER_UPDATE, ALARM_EVENT_RETRY,
                                    USERS_DOMAIN_NAME, USERS_ID_TEC, USERS_ID_ROLE                                    
                                    , SEASON_DATETIME, SEASON_ACTION
                                    //, GRINVICH_OFFSET_DATETIME
                                    , COUNT_PARAMETR_SETUP };
        protected string[] NAME_PARAMETR_SETUP = { "Polling period", "Error delay", "Max attempts count", @"Waiting time", @"Waiting count", @"Main DataSource",
                                                    /*@"Alarm Use", */@"Alarm Timer Update" , @"Alarm Event Retry",
                                                    @"Users DomainName", @"Users ID_TEC", @"Users ID_ROLE"
                                                    , @"Season DateTime", @"Season Action"
                                                    //, @"Grinvich OffsetDateTime"
                                                    };
        protected string[] NAMESI_PARAMETR_SETUP = { "сек", "сек", "ед.", @"мсек", @"мсек", @"ном",
                                                    /*@"лог", */"сек", "сек",
                                                    @"стр", @"ном", @"ном"
                                                    , @"дата/время", @"ном"
                                                    //, "час"
                                                    };
        protected Dictionary<int, string> m_arParametrSetupDefault;
        public Dictionary<int, string> m_arParametrSetup;

        public FormParameters() : base()
        {
            InitializeComponent();

            m_arParametrSetup = new Dictionary<int,string> ();
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.POLL_TIME, @"30");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.ERROR_DELAY, @"60");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.MAX_ATTEMPT, @"3");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_TIME, @"106");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_COUNT, @"39");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_DATASOURCE, @"671");

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_USE, @"True");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_TIMER_UPDATE, @"300");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_EVENT_RETRY, @"900");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_DOMAIN_NAME, @"");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_TEC, @"-1");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_ROLE, @"-1");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_DATETIME, @"26.10.2014 02:00");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_ACTION, @"-1");

            ////Из БД не считывается пока 30.10.2014
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.GRINVICH_OFFSET_DATETIME, @"3"); 

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_APP, ((int)ProgramBase.ID_APP.STATISTIC).ToString ());

            m_arParametrSetupDefault = new Dictionary<int, string>(m_arParametrSetup);

            this.btnCancel.Location = new System.Drawing.Point(8, 90);
            this.btnOk.Location = new System.Drawing.Point(89, 90);
            this.btnReset.Location = new System.Drawing.Point(170, 90);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            mayClose = false;
        }

        //protected override void btnOk_Click(object sender, EventArgs e)
        protected void btnOk_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i ++) {
                m_arParametrSetup[(int)i] = m_dgvData.Rows [(int)i + 0].Cells [1].Value.ToString ();
            }

            saveParam();
            mayClose = true;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
            }
        }
    }

    public partial class FormParameters_FIleINI : FormParameters
    {
        private static string NAME_SECTION_MAIN = "Main settings (" + ProgramBase.AppName + ")";

        private FileINI m_FileINI;

        public FormParameters_FIleINI(string nameSetupFileINI) : base ()
        {
            m_FileINI = new FileINI(nameSetupFileINI);
            //ProgramBase.s_iAppID = (int)ProgramBase.ID_APP.STATISTIC;
            //ProgramBase.s_iAppID = Int32.Parse(m_arParametrSetup[(int)PARAMETR_SETUP.ID_APP]);
            //ProgramBase.s_iAppID = Properties.s

            loadParam();
        }

        public override void loadParam()
        {
            string strDefault = string.Empty;

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_FileINI.ReadString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], strDefault);
                if (m_arParametrSetup[(int)i].Equals(strDefault) == true)
                {
                    m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
                    m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
                }
                else
                    ;
            }

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_dgvData.Rows.Insert((int)i, new object[] { NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], NAMESI_PARAMETR_SETUP[(int)i] });

                m_dgvData.Rows[(int)i].Height = 19;
                m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
            }
        }

        public override void saveParam()
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
        }
    }

    public partial class FormParameters_DB : FormParameters
    {
        private ConnectionSettings m_connSett;
        private DbConnection m_dbConn;

        //public FormParameters_DB(int idListener)
        public FormParameters_DB(ConnectionSettings connSett) : base ()
        {
            m_connSett = connSett;

            int err = -1;
            int idListener = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");
            m_dbConn = DbSources.Sources().GetConnection(idListener, out err);

            if (err == 0)
                loadParam();
            else
                ;

            DbSources.Sources().UnRegister(idListener);
        }

        public override void loadParam()
        {
            string strDefault = string.Empty;

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = readString(NAME_PARAMETR_SETUP[(int)i], strDefault);
                if (m_arParametrSetup[(int)i].Equals(strDefault) == true)
                {
                    m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
                    writeString(NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
                }
                else
                    ;
            }

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_dgvData.Rows.Insert((int)i, new object[] { NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], NAMESI_PARAMETR_SETUP[(int)i] });

                m_dgvData.Rows[(int)i].Height = 19;
                m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
            }
        }

        public override void saveParam()
        {
            int err = -1;
            int idListener = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");
            m_dbConn = DbSources.Sources().GetConnection(idListener, out err);
            
            if (err == 0)
                for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                    writeString(NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
            else
                ;

            DbSources.Sources().UnRegister(idListener);
        }

        private string readString (string key, string valDef) {
            string strRes = valDef;
            int err = -1;
            DataTable table = null;            

            string query = string.Empty;
            //query = @"SELECT * FROM [dbo].[setup] WHERE [KEY]='" + key + @"'";
            query = string.Format (@"SELECT * FROM setup WHERE [KEY]='{0}'", key);
            table = DbTSQLInterface.Select (ref m_dbConn, query, null, null, out err);
            if (table.Rows.Count == 1)
                strRes = table.Rows [0][@"Value"].ToString ().Trim ();
            else
                ;

            return strRes;
        }

        private void writeString(string key, string val)
        {
            int err = -1;
            string query = string.Empty;
            //query = @"UPDATE [dbo].[setup] SET [VALUE] = '" + val + @"' WHERE [KEY]='" + key + @"'";
            query = string.Format(@"UPDATE setup SET [VALUE] = '{0}' WHERE [KEY]='{1}'", key, val);
            DbTSQLInterface.ExecNonQuery (ref m_dbConn, query, null, null, out err);
        }
    }
}