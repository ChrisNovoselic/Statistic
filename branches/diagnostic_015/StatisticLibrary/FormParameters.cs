using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Forms; //Application.ProductVersion

using HClassLibrary;

namespace StatisticCommon
{
    public abstract partial class FormParameters : FormParametersBase
    {
        public enum PARAMETR_SETUP
        {
            UNKNOWN = -1
            , POLL_TIME, ERROR_DELAY, MAX_ATTEMPT, WAITING_TIME, WAITING_COUNT,
            MAIN_DATASOURCE, MAIN_PRIORITY,
            /*ALARM_USE, */
            ALARM_TIMER_UPDATE, ALARM_EVENT_RETRY, ALARM_TIMER_BEEP,
            ALARM_SYSTEMMEDIA_TIMERBEEP
                , USERS_DOMAIN_NAME, USERS_ID_TEC,
            USERS_ID_ROLE
                , SEASON_DATETIME,
            SEASON_ACTION
                //, GRINVICH_OFFSET_DATETIME
                , APP_VERSION,
            APP_VERSION_QUERY_INTERVAL
                ,
            KOMDISP_FOLDER_CSV
                //������������
                ,
            MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE
                , MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER,
            MAINFORMBASE_SETPBRQUERY_LOGQUERY
                , TECVIEW_LOGRECOMENDATIONVAL,
            TECVIEW_GETCURRENTTMGEN_LOGWARNING
                ,
            PANELQUICKDATA_LOGDEVIATIONEVAL
                //����������� ����������...
                ,
            VALIDATE_TM_VALUE
                ,
            VALIDATE_ASKUE_VALUE
                 ,
            DIAGNOSTIC_TIMER_UPDATE
                ////??? � ��� �� ���������������
                //, ID_SOURCE_SOTIASSO_BTEC, ID_SOURCE_SOTIASSO_TEC2, ID_SOURCE_SOTIASSO_TEC3, ID_SOURCE_SOTIASSO_TEC4, ID_SOURCE_SOTIASSO_TEC5, ID_SOURCE_SOTIASSO_BiTEC
                , COUNT_PARAMETR_SETUP
        };
        protected static string[] NAME_PARAMETR_SETUP = { "Polling period", "Error delay", "Max attempts count", @"Waiting time", @"Waiting count"
                                                            , @"Main DataSource", @"Main Priority"
                                                            /*@"Alarm Use", */, @"Alarm Timer Update" , @"Alarm Event Retry", @"Alarm Timer Beep", @"Alarm SytemMedia FileNam"
                                                            , @"Users DomainName", @"Users ID_TEC", @"Users ID_ROLE"
                                                            , @"Season DateTime", @"Season Action"
                                                            //, @"Grinvich OffsetDateTime"
                                                            , @"App Version", @"App Version Query Interval"
                                                            , @"KomDisp Folder CSV"
                                                            //������������
                                                            , @"ControlHandle LogErrorCreate"
                                                            , @"SetPBRQuery LogPBRNumber", @"SetPBRQuery LogQuery"
                                                            , @"TecView LogRecomendation", @"GetCurrentTMGenResponse LogWarning"
                                                            , @"ShowFactValues LogDevEVal"
                                                            //����������� ����������...
                                                            , @"Validate TM Value"
                                                            ,@"Validate ASKUE Value"
                                                             ,@"Diagnostic Timer Update" 
                                                            ////�������������� �������������� �������� ���������� ��������
                                                            //, @"ID_SOURCE_SOTIASSO_BTEC", @"ID_SOURCE_SOTIASSO_TEC2", @"ID_SOURCE_SOTIASSO_TEC3", @"ID_SOURCE_SOTIASSO_TEC4", @"ID_SOURCE_SOTIASSO_TEC5", @"ID_SOURCE_SOTIASSO_BiTEC"
                                                    };
        protected static string[] NAMESI_PARAMETR_SETUP = { "���", "���", "��.", @"����", @"����",
                                                            @"���", @"���",
                                                            /*@"���", */"���", "���", "���", @"���"
                                                            , @"���", @"���", @"���"
                                                            , @"����/�����", @"���"
                                                            //, "���"
                                                            , @"���", @"����"
                                                            , @"���"
                                                            //������������
                                                            , @"���-���"
                                                            , @"���-���", @"���-���"
                                                            , @"���-���", @"���-���"
                                                            , @"���-���"
                                                            //����������� ����������...
                                                            , @"���"
                                                            ,@"���"
                                                             ,@"���"
                                                            //�������������� �������������� �������� ���������� ��������
                                                            //, @"���", @"���", @"���", @"���", @"���", @"���"
                                                    };
        protected Dictionary<int, string> m_arParametrSetupDefault;
        public Dictionary<int, string> m_arParametrSetup;

        public FormParameters()
            : base()
        {
            InitializeComponent();

            m_arParametrSetup = new Dictionary<int, string>();
            m_arParametrSetup.Add((int)PARAMETR_SETUP.POLL_TIME, @"30");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ERROR_DELAY, @"60");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAX_ATTEMPT, @"3");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_TIME, @"106");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_COUNT, @"39");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_DATASOURCE, @"671");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_PRIORITY, @"��������");

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_USE, @"True");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_TIMER_UPDATE, @"300");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_EVENT_RETRY, @"900");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_TIMER_BEEP, @"16");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_SYSTEMMEDIA_TIMERBEEP, @"16");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_DOMAIN_NAME, @"");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_TEC, @"-1");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_ROLE, @"-1");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_DATETIME, @"26.10.2014 02:00");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_ACTION, @"-1");

            ////�� �� �� ����������� ���� 30.10.2014
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.GRINVICH_OFFSET_DATETIME, @"3"); 

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_APP, ((int)ProgramBase.ID_APP.STATISTIC).ToString ());

            m_arParametrSetup.Add((int)PARAMETR_SETUP.APP_VERSION, Application.ProductVersion/*StatisticCommon.Properties.Resources.TradeMarkVersion*/);
            m_arParametrSetup.Add((int)PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL, @"66666");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.KOMDISP_FOLDER_CSV, @"\\ne2844\2.X.X\���-csv");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE, @"False");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER, @"False");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY, @"False");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.TECVIEW_LOGRECOMENDATIONVAL, @"False");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.TECVIEW_GETCURRENTTMGEN_LOGWARNING, @"False");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.PANELQUICKDATA_LOGDEVIATIONEVAL, @"False");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.VALIDATE_TM_VALUE, @"86");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.VALIDATE_ASKUE_VALUE, @"86");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.DIAGNOSTIC_TIMER_UPDATE, @"30000");

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_BTEC, @"12");
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_TEC2, @"22");
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_TEC3, @"32");
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_TEC4, @"42");
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_TEC5, @"52");
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_SOURCE_SOTIASSO_BiTEC, @"63");

            m_arParametrSetupDefault = new Dictionary<int, string>(m_arParametrSetup);

            this.btnCancel.Location = new System.Drawing.Point(8, 290);
            this.btnOk.Location = new System.Drawing.Point(89, 290);
            this.btnReset.Location = new System.Drawing.Point(170, 290);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            mayClose = false;
        }

        protected void setDataGUI(bool bInit)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                if (bInit == true)
                {
                    m_dgvData.Rows.Insert((int)i, new object[] { NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], NAMESI_PARAMETR_SETUP[(int)i] });

                    m_dgvData.Rows[(int)i].Height = 19;
                    m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                    m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
                }
                else
                    m_dgvData.Rows[(int)i].Cells[1].Value = m_arParametrSetup[(int)i];
            }
        }

        //protected override void btnOk_Click(object sender, EventArgs e)
        protected void btnOk_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_dgvData.Rows[(int)i + 0].Cells[1].Value.ToString();
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

        public FormParameters_FIleINI(string nameSetupFileINI)
            : base()
        {
            m_FileINI = new FileINI(nameSetupFileINI, false);
            //ProgramBase.s_iAppID = (int)ProgramBase.ID_APP.STATISTIC;
            //ProgramBase.s_iAppID = Int32.Parse(m_arParametrSetup[(int)PARAMETR_SETUP.ID_APP]);
            //ProgramBase.s_iAppID = Properties.s

            loadParam(true);
        }

        public override void Update(out int err) { err = -1; }

        protected override void loadParam(bool bInit)
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

            setDataGUI(bInit);
        }

        protected override void saveParam()
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
        public FormParameters_DB(ConnectionSettings connSett)
            : base()
        {
            m_connSett = connSett;

            int err = -1;
            int idListener = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");
            m_dbConn = DbSources.Sources().GetConnection(idListener, out err);

            if (err == 0)
                loadParam(true);
            else
                ;

            DbSources.Sources().UnRegister(idListener);
        }

        public override void Update(out int err)
        {
            err = -1;
            int idListener = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");

            Update(idListener, out idListener);

            DbSources.Sources().UnRegister(idListener);
        }

        public void Update(int idListener, out int err)
        {
            m_dbConn = DbSources.Sources().GetConnection(idListener, out err);

            if (err == 0)
            {
                loadParam(false);

                //???��������� ����������� ��������� ��� ���

                //???��������� ����������� ��������� ...

                //???���� ��������� ����������� ���������
            }
            else
                ;
        }

        protected override void loadParam(bool bInit)
        {
            int err = -1;

            string query = string.Empty;
            //query = @"SELECT * FROM [dbo].[setup] WHERE [KEY]='" + key + @"'";
            query = string.Format(@"SELECT * FROM setup");
            DataTable table = DbTSQLInterface.Select(ref m_dbConn, query, null, null, out err);
            DataRow[] rowRes;
            if (err == (int)DbTSQLInterface.Error.NO_ERROR)
                if (!(table == null))
                {
                    query = string.Empty;

                    for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                    {
                        //strRead = readString(NAME_PARAMETR_SETUP[(int)i], strDefault, out err);
                        rowRes = table.Select(@"KEY='" + NAME_PARAMETR_SETUP[(int)i].ToString() + @"'");
                        switch (rowRes.Length)
                        {
                            case 1:
                                m_arParametrSetup[(int)i] =
                                m_arParametrSetupDefault[(int)i] =
                                    rowRes[0][@"VALUE"].ToString().Trim();
                                break;
                            case 0:
                                m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
                                query += getWriteStringRequest(NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], true) + @";";
                                break;
                            default:
                                break;
                        }
                    }

                    if (query.Equals(string.Empty) == false)
                        DbTSQLInterface.ExecNonQuery(ref m_dbConn, query, null, null, out err);
                    else
                        ;
                }
                else
                    err = (int)DbTSQLInterface.Error.TABLE_NULL;
            else
                ;

            setDataGUI(bInit);
        }

        protected override void saveParam()
        {
            int err = -1;
            int idListener = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");
            string query = string.Empty;
            m_dbConn = DbSources.Sources().GetConnection(idListener, out err);

            if (err == 0)
                for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                    query += getWriteStringRequest(NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], false) + @";";
            else
                ;

            if (query.Equals(string.Empty) == false)
                DbTSQLInterface.ExecNonQuery(ref m_dbConn, query, null, null, out err);
            else
                ;

            DbSources.Sources().UnRegister(idListener);
        }

        private string readString(string key, string valDef, out int err)
        {
            return ReadString(ref m_dbConn, key, valDef, out err);
        }

        public static string ReadString(ref DbConnection dbConn, int key, string valDef, out int err)
        {
            return ReadString(ref dbConn, NAME_PARAMETR_SETUP[key], valDef, out err);
        }

        public static string ReadString(ref DbConnection dbConn, string key, string valDef, out int err)
        {
            string strRes = valDef;
            err = -1;
            DataTable table = null;

            string query = string.Empty;
            //query = @"SELECT * FROM [dbo].[setup] WHERE [KEY]='" + key + @"'";
            query = string.Format(@"SELECT * FROM setup WHERE [KEY]='{0}'", key);
            table = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);
            if (err == (int)DbTSQLInterface.Error.NO_ERROR)
                if (!(table == null))
                    if (table.Rows.Count == 1)
                        strRes = table.Rows[0][@"Value"].ToString().Trim();
                    else
                        err = (int)DbTSQLInterface.Error.TABLE_ROWS_0;
                else
                    err = (int)DbTSQLInterface.Error.TABLE_NULL;
            else
                ;

            return strRes;
        }

        private string getWriteStringRequest(string key, string val, bool bInsert)
        {
            int err = -1;
            string strRes = string.Empty;
            if (bInsert == false)
                //query = @"UPDATE [dbo].[setup] SET [VALUE] = '" + val + @"' WHERE [KEY]='" + key + @"'";
                strRes = string.Format(@"UPDATE setup SET [VALUE]='{0}', [LAST_UPDATE]=GETDATE() WHERE [KEY]='{1}'", val, key);
            else
                strRes = string.Format(@"INSERT INTO [setup] ([VALUE],[KEY],[LAST_UPADTE],[ID_UNIT]) VALUES ('{0}','{1}',GETDATE(),{2})", val, key, -1);

            return strRes;
        }
    }
}