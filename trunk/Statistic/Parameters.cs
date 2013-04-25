using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.IO;
//using System.Windows.Forms;
//using System.Runtime.InteropServices;


namespace Statistic
{
    public partial class Parameters : ParametrsBase
    {
        private const int POLL_TIME = 30;
        private const int ERROR_DELAY = 60;
        private const int MAX_TRYES = 1;

        public int poll_time;
        public int error_delay;
        public int max_tryes;

        public Parameters()
        {
            InitializeComponent();
            settingsFile = MainForm.logPath + "\\" + settingsFile;
            
            loadParam();
            mayClose = false;
        }

        public void loadParam()
        {
            poll_time = ReadInt("Main settings", "Polling period", POLL_TIME);
            if (poll_time < nudnQueryPeriod.Minimum || poll_time > nudnQueryPeriod.Maximum)
                poll_time = POLL_TIME;
            poll_time *= 1000;

            error_delay = ReadInt("Main settings", "Error delay", ERROR_DELAY);
            if (error_delay < nudnDelayTime.Minimum || error_delay > nudnDelayTime.Maximum)
                error_delay = ERROR_DELAY;

            max_tryes = ReadInt("Main settings", "Max attempts count", MAX_TRYES);
            if (max_tryes < nudnRequeryCount.Minimum || max_tryes > nudnRequeryCount.Maximum)
                max_tryes = ERROR_DELAY;
        }

        public void saveParam()
        {
            WriteInt("Main settings", "Polling period", poll_time / 1000);
            WriteInt("Main settings", "Error delay", error_delay);
            WriteInt("Main settings", "Max attempts count", max_tryes);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            poll_time = (int)nudnQueryPeriod.Value * 1000;
            error_delay = (int)nudnDelayTime.Value;
            max_tryes = (int)nudnRequeryCount.Value;
            
            saveParam();
            mayClose = true;
            Close();
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            poll_time = POLL_TIME * 1000;
            error_delay = ERROR_DELAY;
            max_tryes = MAX_TRYES;

            nudnQueryPeriod.Value = POLL_TIME;
            nudnDelayTime.Value = ERROR_DELAY;
            nudnRequeryCount.Value = MAX_TRYES;
        }
    }
}