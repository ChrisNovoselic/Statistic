using HClassLibrary;
using StatisticCommon;
using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Data.Common; //DbConnection
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;



namespace StatisticAnalyzer
{
    public abstract partial class FormMainAnalyzer : Form //FormMainBase//: FormMainBaseWithStatusStrip
    {

        public FormMainAnalyzer(int idListener, List<StatisticCommon.TEC> tec)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            if (this is FormMainAnalyzer_TCPIP)
                m_panel = new PanelAnalyzer_TCPIP(idListener, tec);
            else
                if (this is FormMainAnalyzer_DB)
                    m_panel = new PanelAnalyzer_DB(idListener, tec);
                else
                    ;

            if (! (m_panel == null))
            {
                m_panel.EvtClose += new EventHandler(this.FormMainAnalyzer_OnEvtPanelClose);

                InitializeComponent();
                /*
                //При наследовании от 'FormMainBaseWithStatusStrip'
                // m_statusStripMain
                this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
                this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
                // m_lblMainState
                this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
                // m_lblDateError
                this.m_lblDateError.Size = new System.Drawing.Size(166, 17);
                // m_lblDescError
                this.m_lblDescError.Size = new System.Drawing.Size(463, 17);
                */
            }
            else
                ; //???Исключение
        }

        private void FormMainAnalyzer_OnEvtPanelClose(object sender, EventArgs e)
        {
            Close ();
        }

        private void FormMainAnalyzer_FormClosed(object sender, FormClosingEventArgs e)
        {            
            m_panel.Stop ();
        }

        /*
        //При наследовании от 'FormMainBaseWithStatusStrip'
        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            return have_eror;
        }
        */
    }

    public class FormMainAnalyzer_TCPIP : FormMainAnalyzer
    {
        public FormMainAnalyzer_TCPIP(int idListener, List<StatisticCommon.TEC> tec)
            : base(idListener, tec)
        {
        }
    }

    public class FormMainAnalyzer_DB : FormMainAnalyzer
    {
        public FormMainAnalyzer_DB(int idListener, List<StatisticCommon.TEC> tec)
            : base(idListener, tec)
        {
        }
    }
}