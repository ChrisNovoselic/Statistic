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
    public abstract partial class FormMain : /*Form //FormMainBase//:*/ FormMainBaseWithStatusStrip
    {

        public FormMain(int idListener, List<StatisticCommon.TEC> tec)
        {
            //Создать объект - чтение зашифрованного файла с параметрами соединения
            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            //Добавить элемент с параметрами соединения из объекта 'FIleConnSett' 
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);

            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            if (this is FormMain_TCPIP)
                m_panel = new PanelAnalyzer_TCPIP(tec);
            else
                if (this is FormMain_DB)
                    m_panel = new PanelAnalyzer_DB(tec);
                else
                    ;

            if (! (m_panel == null))
            {
                //m_panel.EvtClose += new EventHandler(this.FormMainAnalyzer_OnEvtPanelClose);

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

    public class FormMain_TCPIP : FormMain
    {
        public FormMain_TCPIP(int idListener, List<StatisticCommon.TEC> tec)
            : base(idListener, tec)
        {
        }

        protected override void timer_Start()
        {
            throw new NotImplementedException();
        }

        protected override int UpdateStatusString()
        {
            throw new NotImplementedException();
        }

        protected override void HideGraphicsSettings()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateActiveGui(int type)
        {
            throw new NotImplementedException();
        }

    }

    public class FormMain_DB : FormMain
    {
        public FormMain_DB(int idListener, List<StatisticCommon.TEC> tec)
            : base(idListener, tec)
        {
        }

        protected override void timer_Start()
        {
            throw new NotImplementedException();
        }

        protected override int UpdateStatusString()
        {
            throw new NotImplementedException();
        }

        protected override void HideGraphicsSettings()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateActiveGui(int type)
        {
            throw new NotImplementedException();
        }

    }
}