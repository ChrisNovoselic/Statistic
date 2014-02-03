using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using System.IO;

using StatisticCommon;

namespace StatisticAnalyzer
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        FileInfo m_fi;
        StreamReader m_sr;
        
        public FormMain()
        {
            InitializeComponent();

            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(463, 17);

            TCPSender sender = new TCPSender ();
            sender.Close ();

            string fileName = "W:\\Статистика\\Statistic_NE1150_log.txt";

            try
            {
                FileInfo f = new FileInfo(fileName);
                FileStream fs = f.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                m_sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));
                m_fi = new FileInfo(fileName);
            }
            catch (Exception e)
            {
            }
        }

        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            return have_eror;
        }
    }
}
