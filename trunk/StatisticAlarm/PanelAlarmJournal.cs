using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms; //CheckBox

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    public partial class PanelAlarmJournal : PanelStatistic
    {
        public enum MODE { SERVICE, ADMIN, VIEW };
        private MODE mode;
        private event DelegateIntIntFunc EventConfirm;

        private event DelegateDateFunc EventDateChanged;

        private List<TEC> m_list_tec;
        private List <int> m_listIdTECComponents;
        private AdminAlarm m_adminAlarm;
        private ViewAlarm m_viewAlarm;
        private System.Windows.Forms.Timer m_timerView;

        public PanelAlarmJournal(MODE mode)
        {
            initialize(mode);
        }

        public PanelAlarmJournal(IContainer container, MODE mode)
        {
            container.Add(this);

            initialize(mode);
        }

        private void initialize(MODE mode)
        {
            InitializeComponent();

            this.mode = mode;            
        }

        public override void Start()
        {            
            base.Start ();

            Control ctrl = null;
            int err = -1
                , iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB")
                , indx = -1;

            m_list_tec = new InitTEC_200(iListenerId, true, false).tec;
            //m_adminAlarm = new AdminAlarm();
            m_viewAlarm = new ViewAlarm(new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, iListenerId, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], 0));
            startViewAlarm();
            m_timerView = new Timer();            
            m_timerView.Tick += new EventHandler(fTimerView_Tick);

            DbSources.Sources().UnRegister(iListenerId);

            m_listIdTECComponents = new List<int>();

            ctrl = Find(INDEEX_CONTROL.CLB_TECCOMPONENT);
            (ctrl as CheckedListBox).Items.Add(@"Все компоненты", true);
            foreach (TEC tec in m_list_tec)
                foreach (TECComponent comp in tec.list_TECComponents)
                    if (comp.IsGTP == true)
                    {
                        indx = (ctrl as CheckedListBox).Items.Add(tec.name_shr + @" - " + comp.name_shr, true);
                        m_listIdTECComponents.Add(comp.m_id);
                    }
                    else
                        ;
            (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(fTECComponent_OnItemCheck);
            (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(fTECComponent_OnSelectedIndexChanged);
            (ctrl as CheckedListBox).SelectedIndex = 0;
            (ctrl as CheckedListBox).Enabled = false;

            ctrl = Find(INDEEX_CONTROL.CBX_WORK) as CheckBox;
            if ((ctrl as CheckBox).Checked == true)
            {
                startAdminAlarm();

                m_timerView.Interval = 1;
                m_timerView.Start();
            }
            else
                ;

            (ctrl as CheckBox).CheckedChanged += new EventHandler(cbxWork_OnCheckedChanged);

            (Find (INDEEX_CONTROL.MCLDR_CURRENT) as MonthCalendar).DateChanged += new DateRangeEventHandler(mcldrCurrent_onDateChanged);
        }

        private void startAdminAlarm()
        {
            if (m_adminAlarm == null) initAdminAlarm(); else ;

            if (m_adminAlarm.IsStarted == false)
            {
                m_adminAlarm.Start();                
                m_adminAlarm.Activate(true);
            }
            else ;
        }

        private void startViewAlarm()
        {
            if (m_adminAlarm == null) initViewAlarm(); else ;

            if (m_adminAlarm.IsStarted == false)
            {
                m_adminAlarm.Start();
                m_adminAlarm.Activate(true);
            }
            else ;
        }

        private void fTimerView_Tick(object obj, EventArgs ev)
        {
            m_timerView.Interval = PanelStatistic.POOL_TIME * 1000;
        }

        private void mcldrCurrent_onDateChanged(object obj, DateRangeEventArgs ev)
        {
            EventDateChanged(ev.Start.Date);
        }

        public override void Stop() 
        {
            //Остановить объект "обзор событий"
            if (! (m_viewAlarm == null))
                if (m_viewAlarm.IsStarted == true)
                {
                    m_viewAlarm.Activate(false);
                    m_viewAlarm.Stop();
                }
                else ;
            else
                ;
            //Остановить объект "регистрация событий"
            if (! (m_adminAlarm == null))
                if (m_adminAlarm.IsStarted == true)
                {
                    m_adminAlarm.Activate (false);
                    m_adminAlarm.Stop();
                }
                else ;
            else
                ;

            if (!(m_timerView == null))
            {
                m_timerView.Stop();
                m_timerView.Dispose();
                m_timerView = null;
            }

            base.Stop ();
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate (activate);

            if (bRes == true)
            {
                if ((Find (INDEEX_CONTROL.CBX_WORK) as CheckBox).Checked == true)
                {
                    switch (mode)
                    {
                        case MODE.SERVICE:                            
                            m_viewAlarm.Activate(activate);
                            break;
                        case MODE.ADMIN:
                            m_viewAlarm.Activate(activate);
                            break;
                        case MODE.VIEW:
                            m_viewAlarm.Activate(activate);
                            break;
                        default:
                            break;
                    }
                }
                else
                    ;
            }
            else
                ;

            return bRes;
        }

        private int _widthPanelManagement = 166;

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            this.RowCount = rows;

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _widthPanelManagement));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        private void initAdminAlarm()
        {
            m_adminAlarm = new AdminAlarm();
            m_adminAlarm.InitTEC(m_list_tec);

            m_adminAlarm.EventAdd += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventRetry);

            this.EventConfirm += new DelegateIntIntFunc(m_adminAlarm.OnEventConfirm);
        }

        private void initViewAlarm()
        {
            this.EventDateChanged += new DelegateDateFunc(m_viewAlarm.OnEventDateChanged);
        }

        private void OnAdminAlarm_EventAdd(TecView.EventRegEventArgs ev)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {//...для this.BeginInvoke
            }
            else
                Logging.Logg().Error(@"PanelAlarm::OnAdminAlarm_EventAdd () - ... BeginInvoke (...) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        private void OnAdminAlarm_EventRetry(TecView.EventRegEventArgs ev)
        {
        }

        private void fTECComponent_OnItemCheck (object obj, ItemCheckEventArgs ev)
        {
            CheckedListBox ctrl = obj as CheckedListBox;
            
            if (ev.Index == 0)
            {
                ctrl.ItemCheck -= new ItemCheckEventHandler(fTECComponent_OnItemCheck);

                for (int i = 1; i < ctrl.Items.Count; i ++)
                    ctrl.SetItemCheckState (i, ev.NewValue);

                ctrl.ItemCheck += new ItemCheckEventHandler(fTECComponent_OnItemCheck);
            }
            else
                ;
        }

        private void cbxWork_OnCheckedChanged (object obj, EventArgs ev)
        {
            CheckBox ctrl = obj as CheckBox;

            m_adminAlarm.Activate(ctrl.Checked);

            if (ctrl.Checked == true)
            {
                m_timerView.Interval = 1;
                m_timerView.Start();
            }
            else
                m_timerView.Stop();
        }

        private TECComponent findTECComponentOfID (int id)
        {
            foreach  (TEC tec in m_list_tec)
                foreach (TECComponent comp in tec.list_TECComponents)
                    if ((comp.IsGTP == true) && (comp.m_id == id))
                    {
                        return comp;
                    }
                    else
                        ;

            return null;
        }

        private void fTECComponent_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
            NumericUpDown ctrl = Find(INDEEX_CONTROL.NUD_KOEF) as NumericUpDown;
            if ((Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex > 0)
            {
                if (ctrl.Enabled == false) ctrl.Enabled = true; else ;
                setNudnKoeffAlarmCurPowerValue ();
            }
            else
            {
                if (ctrl.Enabled == true) ctrl.Enabled = false; else ;
                setNudnKoeffAlarmCurPowerValue(-1);
            }
        }

        private void setNudnKoeffAlarmCurPowerValue()
        {
            TECComponent comp = findTECComponentOfID (m_listIdTECComponents[(Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            setNudnKoeffAlarmCurPowerValue (comp.m_dcKoeffAlarmPcur);
        }

        private void setNudnKoeffAlarmCurPowerValue(decimal value)
        {
            NumericUpDown ctrl = Find(INDEEX_CONTROL.NUD_KOEF) as NumericUpDown;
            ctrl.ValueChanged -= new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
            if (value > 0)
            {
                ctrl.Minimum = 2M;
                ctrl.Maximum = 90M;
            }
            else
            {
                ctrl.Minimum =
                ctrl.Maximum =
                    -1;
            }
            ctrl.Value = value;
            ctrl.ValueChanged += new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
        }

        private void NudnKoeffAlarmCurPower_ValueChanged(object obj, EventArgs ev)
        {
            TECComponent comp = findTECComponentOfID(m_listIdTECComponents[(Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            comp.m_dcKoeffAlarmPcur = (obj as NumericUpDown).Value;

            int err = -1
                , idListenerConfigDB = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            System.Data.Common.DbConnection dbConn = DbSources.Sources().GetConnection(idListenerConfigDB, out err);
            //DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur] = " + comp.m_dcKoeffAlarmPcur + @" WHERE [ID] = " + comp.m_id, null, null, out err);
            DbSources.Sources().UnRegister(idListenerConfigDB);
        }

        private Control Find (INDEEX_CONTROL indx)
        {
            return Controls.Find (indx.ToString (), true) [0];
        }

        private class DataGridViewAlarmJournal : DataGridView
        {
            public enum iINDEX_COLUMN
            {
                TECCOMPONENT_NAMESHR, TYPE_ALARM, VALUE, DATETIME_REGISTRED, DATETIME_FIXED, DATETIME_CONFIRM,
                BTN_CONFIRM
                    , COUNT_INDEX_COLUMN
            }

            public DataGridViewAlarmJournal()
                : base()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                DataGridViewColumn column = null;

                string [] arHeaderText = { @"Компонент", @"Тип сигнализации", @"Значение", @"Время регистрации", @"Время фиксации", @"Время подтверждения", @"Подтверждение" };

                for (int i = 0; i < (int)iINDEX_COLUMN.COUNT_INDEX_COLUMN; i++)
                {
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.TECCOMPONENT_NAMESHR:
                        case iINDEX_COLUMN.TYPE_ALARM:
                        case iINDEX_COLUMN.VALUE:
                        case iINDEX_COLUMN.DATETIME_REGISTRED:
                        case iINDEX_COLUMN.DATETIME_FIXED:
                        case iINDEX_COLUMN.DATETIME_CONFIRM:
                            column = new DataGridViewTextBoxColumn();
                            break;
                        case iINDEX_COLUMN.BTN_CONFIRM:
                            column = new DataGridViewButtonColumn();
                            break;
                        default:
                            break;
                    }

                    column.HeaderText = arHeaderText[i];
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    
                    this.Columns.Add(column);
                }

                this.RowHeadersVisible = false;
                this.ReadOnly = true;
            }
        }        
    }

    partial class PanelAlarmJournal
    {
        private enum INDEEX_CONTROL { MCLDR_CURRENT, NUD_HOUR_BEGIN, NUD_HOUR_END, CLB_TECCOMPONENT, NUD_KOEF
            , CBX_WORK
            , DGV_EVENTS };
        
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            _widthPanelManagement += 2 * Margin.Horizontal;

            Control ctrl = null
                , ctrlRel = null;
            int posX = -1
                , posY = -1
                , cols = -1
                , widthRel = -1;

            initializeLayoutStyle (2, 1);

            this.SuspendLayout();

            Panel panelManagement = new Panel();
            panelManagement.Dock = DockStyle.Fill;
            this.Controls.Add(panelManagement, 0, 0);

            posX = Margin.Horizontal;
            posY = Margin.Vertical;
            cols = 4;
            widthRel = (_widthPanelManagement - 2 * Margin.Horizontal) / 4;
            
            ctrl = new System.Windows.Forms.MonthCalendar();
            ctrl.Name = INDEEX_CONTROL.MCLDR_CURRENT.ToString();
            ctrl.Location = new System.Drawing.Point(posX, posY);
            ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
            (ctrl as MonthCalendar).MaxSelectionCount = 1;
            //(ctrl as MonthCalendar).ShowToday = false;
            //(ctrl as MonthCalendar).ShowTodayCircle = false;
            panelManagement.Controls.Add(ctrl);

            ctrlRel = Find (INDEEX_CONTROL.MCLDR_CURRENT);            

            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point (posX, posY += 172);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"c";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            ctrl = new NumericUpDown();
            ctrl.Name = INDEEX_CONTROL.NUD_HOUR_BEGIN.ToString();
            ctrl.Location = new System.Drawing.Point(posX += widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).ReadOnly = true;
            panelManagement.Controls.Add(ctrl);

            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX += widthRel + widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"до";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            panelManagement.Controls.Add(ctrl);

            ctrl = new NumericUpDown();
            ctrl.Name = INDEEX_CONTROL.NUD_HOUR_END.ToString();
            ctrl.Location = new System.Drawing.Point(posX += widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).ReadOnly = true;
            panelManagement.Controls.Add(ctrl);

            ctrlRel = Find(INDEEX_CONTROL.NUD_HOUR_BEGIN);
            ctrl = new CheckedListBox ();
            ctrl.Name = INDEEX_CONTROL.CLB_TECCOMPONENT.ToString();
            ctrl.Location = new System.Drawing.Point(0, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);
            ctrl.Size = new System.Drawing.Size(_widthPanelManagement - Margin.Horizontal, _widthPanelManagement / 1);
            //(ctrl as CheckedListBox).CheckOnClick = true;
            panelManagement.Controls.Add(ctrl);

            ctrlRel = Find(INDEEX_CONTROL.CLB_TECCOMPONENT);
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height);
            ctrl.Size = new System.Drawing.Size(5 * widthRel / 2, ctrl.Height);
            ctrl.Text = @"Коэффициент";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            ctrl = new NumericUpDown();
            ctrl.Name = INDEEX_CONTROL.NUD_KOEF.ToString();
            ctrl.Location = new System.Drawing.Point(posX += 5 * widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).ReadOnly = true;
            (ctrl as NumericUpDown).TextAlign = HorizontalAlignment.Right;
            (ctrl as NumericUpDown).Increment = 2M;
            panelManagement.Controls.Add(ctrl);

            ctrlRel = Find(INDEEX_CONTROL.NUD_KOEF);
            ctrl = new CheckBox();
            ctrl.Name = INDEEX_CONTROL.CBX_WORK.ToString();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);            
            ctrl.Text = @"Включено";
            panelManagement.Controls.Add(ctrl);

            ctrl = new DataGridViewAlarmJournal();
            ctrl.Name = INDEEX_CONTROL.DGV_EVENTS.ToString();
            ctrl.Dock = DockStyle.Fill;
            this.Controls.Add(ctrl, 1, 0);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
