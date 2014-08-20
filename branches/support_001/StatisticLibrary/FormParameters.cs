using System;
using System.Collections.Generic;

namespace StatisticCommon
{
    public abstract partial class FormParameters : FormParametersBase
    {
        public enum PARAMETR_SETUP { POLL_TIME, ERROR_DELAY, MAX_ATTEMPT, WAITING_TIME, WAITING_COUNT, MAIN_DATASOURCE, COUNT_PARAMETR_SETUP };
        protected string[] NAME_PARAMETR_SETUP = { "Polling period", "Error delay", "Max attempts count", @"Waiting time", @"Waiting count", @"Main DataSource" };
        protected string[] NAMESI_PARAMETR_SETUP = { "мсек", "сек", "ед.", @"мсек", @"мсек", @"ном" };
        protected Dictionary<int, string> m_arParametrSetupDefault;
        public Dictionary<int, string> m_arParametrSetup;

        public FormParameters() : base()
        {
            InitializeComponent();

            m_arParametrSetup = new Dictionary<int,string> ();
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.POLL_TIME, @"30000");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.ERROR_DELAY, @"60");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.MAX_ATTEMPT, @"2");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_TIME, @"66");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_COUNT, @"13");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_DATASOURCE, @"671");

            m_arParametrSetupDefault = new Dictionary<int, string>(m_arParametrSetup);

            this.btnCancel.Location = new System.Drawing.Point(8, 90);
            this.btnOk.Location = new System.Drawing.Point(89, 90);
            this.btnReset.Location = new System.Drawing.Point(170, 90);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            mayClose = false;
        }
        
        private void btnOk_Click(object sender, EventArgs e)
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

        public FormParameters_FIleINI(string nameSetupFileINI)
        {
            m_FileINI = new FileINI(nameSetupFileINI);

            loadParam();
        }

        public override void loadParam()
        {
            string strDefault = string.Empty;

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i ++) {
                m_arParametrSetup[(int)i] = m_FileINI.ReadString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], strDefault);
                if (m_arParametrSetup[(int)i].Equals (strDefault) == true)
                {
                    m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
                    m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
                }
                else
                    ;
            }

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_dgvData.Rows.Insert((int)i, new object [] {NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], NAMESI_PARAMETR_SETUP[(int)i]});

                m_dgvData.Rows[(int)i].Height = 19;
                m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
            }
        }

        public override void saveParam()
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i ++)
                m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
        }
    }
}