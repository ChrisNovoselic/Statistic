using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_rdg
{
    public partial class FormMain : FormMainBase
    {
        private enum CONN_SETT_TYPE {SOURCE, DEST, COUNT_CONN_SETT_TYPE};
        private enum INDX_UICONTROL_DB { SERVER_IP, PORT, NAME_DATABASE, USER_ID, PASS, COUNT_INDX_UICONTROL_DB };

        Admin [] m_arAdmin;
        FormConnectionSettings m_formConnectionSettings;
        GroupBox [] m_arGroupBox;
        System.Windows.Forms.Control [,] m_arUIControlDB;

        private Int16 indxDB
        {
            get {
                CONN_SETT_TYPE i;
                for (i = CONN_SETT_TYPE.SOURCE; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    if (m_arGroupBox [(Int16)i].BackColor == SystemColors.Info)
                        break;
                    else
                        ;
                }

                return (Int16)i;
            }

            set {
            }
        }

        public FormMain()
        {
            InitializeComponent();

            m_arGroupBox = new GroupBox[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { groupBoxSource, groupBoxDest };

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { tbxSourceServerIP, nudnSourcePort, tbxSourceNameDatabase, tbxSourceUserId, mtbxSourcePass },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            delegateEvent = new DelegateFunc(EventRaised);

            m_formConnectionSettings = new FormConnectionSettings();

            if (m_formConnectionSettings.Protected == false) {
                m_formConnectionSettings.addConnSett (new ConnectionSettings ());

                //formConnectionSettings.setConnSett();
            }
            else
                ;

            m_arAdmin = new Admin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                m_arAdmin[i] = new Admin(m_formConnectionSettings.getConnSett(i));

                for (int j = 0; j < (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB; j ++) {
                    switch (j) {
                        case (Int16)INDX_UICONTROL_DB.SERVER_IP:
                            ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.server;
                            break;
                        case (Int16)INDX_UICONTROL_DB.PORT:
                            if (m_arUIControlDB[i, j].Enabled)
                                ((NumericUpDown)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.port.ToString ();
                            else
                                ;
                            break;
                        case (Int16)INDX_UICONTROL_DB.NAME_DATABASE:
                            ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.dbName;
                            break;
                        case (Int16)INDX_UICONTROL_DB.USER_ID:
                            ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.userName;
                            break;
                        case (Int16)INDX_UICONTROL_DB.PASS:
                            ((MaskedTextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.password;
                            break;
                        default:
                            break;
                    }
                }

                m_arAdmin [i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin [i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin [i].StartDbInterface();
            }

            //panelMain.Visible = false;

            timerMain.Start ();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                m_arAdmin[i].StopDbInterface();
            }

            Close ();
        }

        private void groupBoxFocus (GroupBox groupBox) {
            GroupBox groupBoxOther = null;
            bool bBackColorChange = false;
            if (! (groupBox.BackColor == SystemColors.Info)) {
                groupBox.BackColor = SystemColors.Info;

                UpdateStatusString ();

                bBackColorChange = true;
            }
            else
                ;

            switch (groupBox.Name) {
                case "groupBoxSource":
                    groupBoxOther = groupBoxDest;
                    break;
                case "groupBoxDest":
                    groupBoxOther = groupBoxSource;
                    break;
                default:
                    break;
            }

            if (bBackColorChange)
                groupBoxOther.BackColor = SystemColors.Control;
            else
                ;
        }

        private void groupBox_MouseClick(object sender, MouseEventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void groupBox_Enter(object sender, EventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClose_Click (null, null);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new FormAbout ();
            formAbout.ShowDialog();
        }

        private void ErrorReport (string msg) {
            statusStripMain.BeginInvoke(delegateEvent);
        }

        private void ActionReport(string msg)
        {
            statusStripMain.BeginInvoke(delegateEvent);
        }

        public bool UpdateStatusString()
        {
            bool have_eror = false;

            if (m_arAdmin[indxDB].actioned_state && m_arAdmin[indxDB].threadIsWorking)
            {
                lblDateError.Text = m_arAdmin[indxDB].last_action;
                lblDescError.Text = m_arAdmin[indxDB].last_time_action.ToString();
            }
            else
                ;

            if (m_arAdmin[indxDB].errored_state)
            {
                have_eror = true;
                lblDescError.Text = m_arAdmin[indxDB].last_error;
                lblDateError.Text = m_arAdmin[indxDB].last_time_error.ToString();
            }
            else
                ;

            return have_eror;
        }

        protected override void EventRaised()
        {
            lock (lockEvent)
            {
                UpdateStatusString();
                lblDescError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //if (m_arAdmin[indxDB].threadIsWorking == false)
            //{
            //    m_arAdmin[indxDB].StartDbInterface();
            //}
            //else
            //    ;

            lock (lockEvent)
            {
                bool have_eror = UpdateStatusString();

                if (have_eror)
                    lblMainState.Text = "ОШИБКА";
                else
                    ;

                if (!have_eror || !show_error_alert)
                    lblMainState.Text = "";
                else
                    ;

                show_error_alert = !show_error_alert;
                lblDescError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            m_arAdmin[indxDB].GetCurrentTime ();
        }
    }
}
