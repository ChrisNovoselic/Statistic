using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StatisticCommon
{
    public partial class FormParametersBase : Form
    {
        public enum TYPE_VALUE : int { CURRENT, PREVIOUS, COUNT_TYPE_VALUE };

        public System.Windows.Forms.Button btnOk;
        public System.Windows.Forms.Button btnReset;
        public System.Windows.Forms.Button btnCancel;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileString(String Section, String Key, String Value, String FilePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int GetPrivateProfileString(String Section, String Key, String Default, StringBuilder retVal, int Size, String FilePath);

        /*[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName,
           string lpKeyName, string lpString, string lpFileName);*/
        public string settingsFile = "setup.ini";

        public bool mayClose;
        //private DelegateFunc delegateParamsApply;

        public Int16 m_State;

        public FormParametersBase () {
            m_State = 0;
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

        public void Parameters_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!mayClose)
                e.Cancel = true;
            else
                mayClose = false;
        }

        virtual public void buttonCancel_Click(object sender, EventArgs e)
        {
            mayClose = true;
            Close();
        }
    }
}
