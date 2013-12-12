using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.IO;
//using System.Windows.Forms;
//using System.Runtime.InteropServices;

namespace StatisticCommon
{
    public partial class FormParameters : FormParametersBase
    {
        public enum PARAMETR_SETUP { POLL_TIME, ERROR_DELAY, MAX_TRYES, COUNT_PARAMETR_SETUP };
        private string[] NAME_PARAMETR_SETUP = { "Polling period", "Error delay", "Max attempts count" };
        private int[] m_arParametrSetupDefault = { 30, 60, 1 };
        public int[] m_arParametrSetup = { 30, 60, 1 };

        private const string NAME_SECTION_MAIN = "Main settings (Statistic.exe)";

        public FormParameters(string nameFileINI)
            : base(nameFileINI)
        {
            InitializeComponent();

            this.btnCancel.Location = new System.Drawing.Point(8, 90);
            this.btnOk.Location = new System.Drawing.Point(89, 90);
            this.btnReset.Location = new System.Drawing.Point(170, 90);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            loadParam();
            mayClose = false;
        }

        public void loadParam()
        {
            m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] = m_FileINI.ReadInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.POLL_TIME], m_arParametrSetupDefault[(int)PARAMETR_SETUP.POLL_TIME]);
            if (m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] < nudnQueryPeriod.Minimum || m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] > nudnQueryPeriod.Maximum)
                m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.POLL_TIME];
            else
                ;
            m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] *= 1000;

            m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] = m_FileINI.ReadInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.ERROR_DELAY], m_arParametrSetupDefault[(int)PARAMETR_SETUP.ERROR_DELAY]);
            if (m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] < nudnDelayTime.Minimum || m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] > nudnDelayTime.Maximum)
                m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.ERROR_DELAY];
            else
                ;

            m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] = m_FileINI.ReadInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.MAX_TRYES], m_arParametrSetupDefault[(int)PARAMETR_SETUP.MAX_TRYES]);
            if (m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] < nudnRequeryCount.Minimum || m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] > nudnRequeryCount.Maximum)
                m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.MAX_TRYES];
            else
                ;
        }

        public void saveParam()
        {
            m_FileINI.WriteInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.POLL_TIME], m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] / 1000);
            m_FileINI.WriteInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.ERROR_DELAY], m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY]);
            m_FileINI.WriteInt(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)PARAMETR_SETUP.MAX_TRYES], m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES]);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] = (int)nudnQueryPeriod.Value * 1000;
            m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] = (int)nudnDelayTime.Value;
            m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] = (int)nudnRequeryCount.Value;

            saveParam();
            mayClose = true;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            m_arParametrSetup[(int)PARAMETR_SETUP.POLL_TIME] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.POLL_TIME] * 1000;
            m_arParametrSetup[(int)PARAMETR_SETUP.ERROR_DELAY] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.ERROR_DELAY];
            m_arParametrSetup[(int)PARAMETR_SETUP.MAX_TRYES] = m_arParametrSetupDefault[(int)PARAMETR_SETUP.MAX_TRYES];

            nudnQueryPeriod.Value = m_arParametrSetupDefault[(int)PARAMETR_SETUP.POLL_TIME];
            nudnDelayTime.Value = m_arParametrSetupDefault[(int)PARAMETR_SETUP.ERROR_DELAY];
            nudnRequeryCount.Value = m_arParametrSetupDefault[(int)PARAMETR_SETUP.MAX_TRYES];
        }
    }
}