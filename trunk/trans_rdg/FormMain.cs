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

        Admin[] m_arAdmin;
        FormConnectionSettings m_formConnectionSettings;
        GroupBox [] m_arGroupBox;
        System.Windows.Forms.Control [,] m_arUIControlDB;

        DataGridViewAdmin m_dgwAdminTable;

        private Int16 m_IndexDB
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

            comboBoxTECComponent.SelectedIndexChanged += new EventHandler(comboBoxTECComponent_SelectedIndexChanged);

            m_arGroupBox = new GroupBox[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { groupBoxSource, groupBoxDest };

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { tbxSourceServerIP, nudnSourcePort, tbxSourceNameDatabase, tbxSourceUserId, mtbxSourcePass },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            delegateEvent = new DelegateFunc(EventRaised);

            m_formConnectionSettings = new FormConnectionSettings("connsett.ini");

            if (m_formConnectionSettings.Protected == false || m_formConnectionSettings.Count < 2)
            {
                m_formConnectionSettings.addConnSett (new ConnectionSettings ());

                //formConnectionSettings.setConnSett();
            }
            else
                ;

            m_arAdmin = new Admin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            //Получатель
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST] = new Admin();
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].SetDelegateTECComponent(FillComboBoxTECComponent);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].InitTEC (m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), FormChangeMode.MODE_TECCOMPONENT.GTP);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST);

            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new Admin();
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].InitTEC(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), FormChangeMode.MODE_TECCOMPONENT.GTP);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.SOURCE);

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                setUIControlConnectionSettings (i);

                m_arAdmin [i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin [i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin [i].StartDbInterface();
            }

            //panelMain.Visible = false;

            timerMain.Start ();

            //m_arAdmin [m_IndexDB].GetRDGValues ();
        }

        private void setUIControlConnectionSettings (int i) {
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
        }

        private void FillComboBoxTECComponent () {
            for (int i = 0; i < m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents.Count; i ++)
                comboBoxTECComponent.Items.Add(m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[i].tec.name + " - " + m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[i].name);

            if (comboBoxTECComponent.Items.Count > 0)
                comboBoxTECComponent.SelectedIndex = 0;
            else
                ;
        }

        private void CopyCurAdminValues()
        {
        }

        private void setDataGridViewAdmin(DateTime date)
        {
            int indxDB = m_IndexDB;
            
            for (int i = 0; i < 24; i++)
            {
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value = m_arAdmin [indxDB].m_curRDGValues[i].plan.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value = m_arAdmin [indxDB].m_curRDGValues[i].recomendation.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value = m_arAdmin [indxDB].m_curRDGValues[i].deviationPercent.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value = m_arAdmin [indxDB].m_curRDGValues[i].deviation.ToString("F2");
            }

            CopyCurAdminValues();
        }

        private void setDatetimePickerMain (DateTime date) {
            dateTimePickerMain.Value = date;
        }

        private void setDatetimePicker (DateTime date) {
            this.BeginInvoke(new DelegateDateFunction(setDatetimePickerMain), date);
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

            if (bBackColorChange) {
                groupBoxOther.BackColor = SystemColors.Control;

                m_formConnectionSettings.SelectedIndex = m_IndexDB;
            }
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

            if (m_arAdmin[m_IndexDB].actioned_state && m_arAdmin[m_IndexDB].threadIsWorking)
            {
                lblDescError.Text = m_arAdmin[m_IndexDB].last_action;
                lblDateError.Text = m_arAdmin[m_IndexDB].last_time_action.ToString();
            }
            else
                ;

            if (m_arAdmin[m_IndexDB].errored_state)
            {
                have_eror = true;
                lblDescError.Text = m_arAdmin[m_IndexDB].last_error;
                lblDateError.Text = m_arAdmin[m_IndexDB].last_time_error.ToString();
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
            if (timerMain.Interval == 666) {
                //Первый запуск
                timerMain.Interval = 1000;

                m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].GetRDGValues(Admin.TYPE_FIELDS.STATIC, comboBoxTECComponent.SelectedIndex);
            }
            else
                ;

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

        //private void FormMain_Activated(object sender, EventArgs e)
        //{
        //    m_arAdmin[m_IndexDB].GetCurrentTime ();
        //}

        private void buttonSave_Click(object sender, EventArgs e)
        {
            //m_formConnectionSettings.SaveSettingsFile ();
            m_formConnectionSettings.btnOk_Click (null, null);
        }

        private void component_Changed(object sender, EventArgs e)
        {
            uint indxDB = (uint)m_IndexDB;
            ConnectionSettings connSett = new ConnectionSettings ();

            connSett.server = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.SERVER_IP].Text;
            connSett.port = (Int32)((NumericUpDown)m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PORT]).Value;
            connSett.dbName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.NAME_DATABASE].Text;
            connSett.userName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.USER_ID].Text;
            connSett.password = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PASS].Text;
            connSett.ignore = false;

            m_formConnectionSettings.ConnectionSettingsEdit = connSett;
        }

        private void comboBoxTECComponent_SelectedIndexChanged (object sender, EventArgs e) {
            if (! (m_arAdmin[(int)CONN_SETT_TYPE.SOURCE] == null)) {
                ClearTables ();

                m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].GetRDGValues(Admin.TYPE_FIELDS.STATIC, comboBoxTECComponent.SelectedIndex);
            }
            else
                ;
        }

        public void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DATE_HOUR].Value = "";
                m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value = "";
                m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value = "";
                m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value = "false";
                m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value = "";
            }
        }

        private void getDataGridViewAdmin()
        {
            int indxDB = m_IndexDB;
            
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdmin.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdmin.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value, out value);
                            m_arAdmin [indxDB].m_curRDGValues[i].plan = value;
                            break;
                        case (int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value, out value);
                                m_arAdmin[indxDB].m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE:
                            {
                                m_arAdmin[indxDB].m_curRDGValues[i].deviationPercent = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdmin.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value, out value);
                                m_arAdmin[indxDB].m_curRDGValues[i].deviation = value;

                                break;
                            }
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurRDGValues ();
        }

        private void buttonSourceExport_Click(object sender, EventArgs e)
        {
            //Взять значения "с окна" в таблицу
            getDataGridViewAdmin();

            //ClearTables();

            m_arAdmin[m_IndexDB].SaveRDGValues(dateTimePickerMain.Value);
        }
    }
}
