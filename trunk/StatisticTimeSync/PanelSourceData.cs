using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms; //TableLayoutPanel
using System.Threading;

using HClassLibrary;

namespace StatisticTimeSync
{    
    public partial class PanelSourceData : TableLayoutPanel
    {
        public enum ID_ASKED_DATAHOST { CONN_SETT }
        
        System.Threading.Timer m_timerGetDate;
        public event DelegateObjectFunc EvtAskedData;

        private HGetDate m_getDate;

        public PanelSourceData()
        {
            InitializeComponent();

            //m_getDate = new HGetDate ();
        }

        public PanelSourceData(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private void checkBoxTurnOn_CheckedChanged (object obj, EventArgs ev) {
            this.m_comboBoxSourceData.Enabled = ! m_checkBoxTurnOn.Checked;

            if (m_checkBoxTurnOn.Checked == true) {
                //Start
                //Спросить параметры соединения
                IAsyncResult iar = BeginInvoke (new DelegateFunc (queryConnSett));
                //Установить соедиение, запустить поток
            } else {
                //Stop
                //Разорвать соедиенние

                //Остановить поток
                if (! (m_timerGetDate == null)) {
                    m_timerGetDate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    m_timerGetDate.Dispose ();
                    m_timerGetDate = null;
                }
                else
                    ;
            }
        }

        private void queryConnSett () {
            EvtAskedData(new EventArgsDataHost (0, this));
        }

        private void comboBoxSourceData_SelectedIndexChanged(object obj, EventArgs ev)
        {
            if (m_comboBoxSourceData.SelectedIndex > 0)
                m_checkBoxTurnOn.Enabled = true;
            else
                m_checkBoxTurnOn.Enabled = false;
        }

        private void fThreadGetDate (object obj) {
            m_timerGetDate.Change(1000, System.Threading.Timeout.Infinite);
        }

        public void AddSourceData (string desc) {
            m_comboBoxSourceData.Items.Add (desc);
        }

        public string GetSelectedSourceData()
        {
            return m_comboBoxSourceData.SelectedItem.ToString ();
        }

        public void OnEvtDataRecievedHost (EventArgsDataHost ev) {
            switch (ev.id) {
                case (int)ID_ASKED_DATAHOST.CONN_SETT:
                    //Установить соедиение
                    

                    //Запустить поток
                    m_timerGetDate = new System.Threading.Timer(fThreadGetDate);
                    m_timerGetDate.Change(0, System.Threading.Timeout.Infinite);
                    break;
                default:
                    break;
            }
        }
    }
}
