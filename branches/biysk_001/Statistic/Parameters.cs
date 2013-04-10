using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace Statistic
{
    public partial class Parameters : Form
    {
        private const int POLL_TIME = 30;
        private const int ERROR_DELAY = 60;
        private const int MAX_TRYES = 1;

        public int poll_time;
        public int error_delay;
        public int max_tryes;

        private bool mayClose;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(String Section, String Key, String Value, String FilePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int GetPrivateProfileString(String Section, String Key, String Default, StringBuilder retVal, int Size, String FilePath);

        /*[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName,
           string lpKeyName, string lpString, string lpFileName);*/
        private string settingsFile = "setup.ini";

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

        public String ReadString(String Section, String Key, String Default)
        {
            StringBuilder StrBu = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, Default, StrBu, 255, settingsFile);
            return StrBu.ToString();
        }

        public int ReadInt(String Section, String Key, int Default)
        {
            int value;
            string s;
            s = ReadString(Section, Key, "");
            if (s == "")
                value = Default;
            else
                if (!int.TryParse(s, out value))
                    value = Default;
            return value;
        }

        public void WriteString(String Section, String Key, String Value)
        {
            WritePrivateProfileString(Section, Key, Value, settingsFile);
        }

        public void WriteInt(String Section, String Key, int Value)
        {
            string s = Value.ToString();
            WriteString(Section, Key, s);
        }

        private void Parameters_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!mayClose)
                e.Cancel = true;
            else
                mayClose = false;
        }
    }
}