﻿using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace StatisticCommon
{
    public class FIleINI
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileString(String Section, String Key, String Value, String FilePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int GetPrivateProfileString(String Section, String Key, String Default, StringBuilder retVal, int Size, String FilePath);

        public string m_NameFileINI = string.Empty;

        public FIleINI(string nameFile)
        {
            m_NameFileINI = System.Environment.CurrentDirectory + "\\" + nameFile;
            if (!File.Exists(m_NameFileINI))
            {
                File.Create(m_NameFileINI);
                //throw new Exception ("Не удалось найти файл инициализации (полный путь: " + m_NameFileINI + ")");
            }
            else
                ;
        }

        public String ReadString(String Section, String Key, String Default)
        {
            StringBuilder StrBu = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, Default, StrBu, 255, m_NameFileINI);
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
            WritePrivateProfileString(Section, Key, Value, m_NameFileINI);
        }

        public void WriteInt(String Section, String Key, int Value)
        {
            string s = Value.ToString();
            WriteString(Section, Key, s);
        }
    }
}