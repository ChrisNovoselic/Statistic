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
        enum TYPE_VALUE : int { CURRENT, PREVIOUS, COUNT_TYPE_VALUE };

        private const int COUNT_TG = 8;
        private const char SEP_ID_TG = ',';
        private string[] NAME_TIME = { "min", "hour" };
        //private const string NAME_SECTION_TG_ID = "Main settings"; //"ID TG Biysk";

        private const int POLL_TIME = 30;
        private const int ERROR_DELAY = 60;
        private const int MAX_TRYES = 1;

        public int poll_time;
        public int error_delay;
        public int max_tryes;

        private System.Windows.Forms.TextBox[,] m_array_tbxTG;
        private System.Windows.Forms.Label[,] m_array_lblTG;
        //Только для особенной ТЭЦ (Бийск)
        private int[,] m_tg_id_default = { { 9223, 9222, 9431, 9430, 9433, 9435, 9434, 9432 }, { 8436, 8470, 8878, 8674, 8980, 9150, 6974, 8266 } };
        private int[,] m_tg_id;

        private bool mayClose;
        private DelegateFunc delegateParamsApply;

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

        public Parameters(DelegateFunc delParApp)
        {
            InitializeComponent();
            settingsFile = MainForm.logPath + "\\" + settingsFile;

            delegateParamsApply = delParApp;

            if ((m_tg_id_default.Length / m_tg_id_default.Rank == COUNT_TG) && (m_tg_id_default.Rank == (int)TG.ID_TIME.COUNT_ID_TIME)) ;
            else ;

            m_tg_id = new int[(int)TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];
            for (int i = (int)TG.ID_TIME.MINUTES; i < (int)TG.ID_TIME.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    m_tg_id[i, j] = m_tg_id_default[i, j];
                }
            }
            m_array_tbxTG = new System.Windows.Forms.TextBox[(int)TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];
            m_array_lblTG = new System.Windows.Forms.Label[(int)TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];

            //m_array_lblTG[(int)TG.ID_TIME.MINUTES, 0] = new Label();
            //m_array_tbxTG[(int)TG.ID_TIME.MINUTES, 0] = tbxTG1Mins;

            for (int i = (int)TG.ID_TIME.MINUTES; i < (int)TG.ID_TIME.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    m_array_lblTG[i, j] = new System.Windows.Forms.Label();
                    m_array_lblTG[i, j].AutoSize = true;
                    m_array_lblTG[i, j].Location = new System.Drawing.Point(12 + i * 164, 101 + j * 27);
                    //m_array_lblTG[i, j].Name = "lblTG1Mins";
                    //m_array_lblTG[i, 0].Size = new System.Drawing.Size(79, 13);
                    m_array_lblTG[i, j].TabIndex = (i * COUNT_TG) + j + 6;
                    m_array_lblTG[i, j].Text = "ТГ" + (j + 1).ToString() + " ";
                    if (i % 2 == 0)
                        m_array_lblTG[i, j].Text += "минутный";
                    else
                        m_array_lblTG[i, j].Text += "получасовой";
                    this.Controls.Add(m_array_lblTG[i, j]);

                    m_array_tbxTG[i, j] = new System.Windows.Forms.TextBox();
                    m_array_tbxTG[i, j].Location = new System.Drawing.Point(97 + i * 178, 98 + j * 27);
                    //m_array_tbxTG[i, j].Name = "tbxTG1Hours";
                    m_array_tbxTG[i, j].Size = new System.Drawing.Size(70, 20);
                    m_array_tbxTG[i, j].TabIndex = (i * COUNT_TG) + j + 7;
                    this.Controls.Add(m_array_tbxTG[i, j]);
                }
            }

            //this.Height = 466;

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

        public int ParamsGetTgId(int sensor, bool mins)
        {
            if (mins)
                return m_tg_id[(int)TG.ID_TIME.MINUTES, sensor];
            else
                return m_tg_id[(int)TG.ID_TIME.HOURS, sensor];
        }
    }
}