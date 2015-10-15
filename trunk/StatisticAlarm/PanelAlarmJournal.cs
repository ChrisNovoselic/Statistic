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
        private event DelegateIntIntFunc EventConfirm;

        ConnectionSettings m_connSett;
        List<TEC> m_list_tec;
        AdminAlarm m_adminAlarm;

        public PanelAlarmJournal(ConnectionSettings connSett, MODE mode)
        {
            initialize(connSett);
        }

        public PanelAlarmJournal(IContainer container, ConnectionSettings connSett, MODE mode)
        {
            container.Add(this);

            initialize(connSett);
        }

        private void initialize(ConnectionSettings connSett)
        {
            InitializeComponent();

            m_connSett = connSett;

            int err = -1
                , iListenerId = DbSources.Sources().Register(m_connSett, false, @"CONFIG_DB");

            m_list_tec = new InitTEC_200(iListenerId, true, false).tec;

            DbSources.Sources().UnRegister(iListenerId);

            //if (this.m_cbxAlarm.Checked == true)
            //{
            //    initAdminAlarm();

            //    m_adminAlarm.Start();
            //} else ;
        }

        public override void Start()
        {
            base.Start ();

            if (m_adminAlarm == null) initAdminAlarm(); else ;

            if (m_adminAlarm.IsStarted == false)
                m_adminAlarm.Start();
            else ;
        }

        public override void Stop()
        {
            if (m_adminAlarm.IsStarted == true)
            {
                m_adminAlarm.Activate (false);
                m_adminAlarm.Stop();
            }
            else ;

            base.Stop ();
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate (activate);
            
            if (bRes == true)
            {
                if ((Find (INDEEX_CONTROL.CBX_WORK) as CheckBox).Checked == true)
                {
                    m_adminAlarm.Activate(activate);
                }
                else
                    ;
            }
            else
                ;

            return bRes;
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }

        private void initAdminAlarm()
        {
            m_adminAlarm = new AdminAlarm();
            m_adminAlarm.InitTEC(m_list_tec);

            m_adminAlarm.EventAdd += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventRetry);

            this.EventConfirm += new DelegateIntIntFunc(m_adminAlarm.OnEventConfirm);
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

        private Control Find (INDEEX_CONTROL indx)
        {
            return Controls.Find (indx.ToString (), true) [0];
        }
    }

    partial class PanelAlarmJournal
    {
        private enum INDEEX_CONTROL { DTP_CURRENT, CBX_WORK };
        
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

            Control ctrl = null;

            this.SuspendLayout();
            
            ctrl = new DateTimePicker ();
            ctrl.Name = INDEEX_CONTROL.DTP_CURRENT.ToString();
            this.Controls.Add(ctrl);
            
            ctrl = new CheckBox ();
            ctrl.Name = INDEEX_CONTROL.CBX_WORK.ToString ();
            ctrl.Text = @"Включить";
            this.Controls.Add (ctrl);

            this.ResumeLayout(false);
            this.PerformLayout ();
        }

        #endregion
    }
}
