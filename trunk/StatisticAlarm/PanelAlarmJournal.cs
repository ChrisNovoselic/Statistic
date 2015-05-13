using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private event DelegateIntIntFunc EventConfirm;
        
        List<TEC> m_list_tec;
        AdminAlarm m_adminAlarm;

        public PanelAlarmJournal(List <TEC> list_tec)
        {
            panelAlarmJournal(list_tec);
        }

        public PanelAlarmJournal(IContainer container, List<TEC> list_tec)
        {
            container.Add(this);

            panelAlarmJournal(list_tec);
        }

        private void panelAlarmJournal(List<TEC> list_tec)
        {
            InitializeComponent();

            m_list_tec = list_tec;

            if (this.m_cbxAlarm.Checked == true)
            {
                initAdminAlarm();

                m_adminAlarm.Start();
            } else ;
        }

        public override void Start()
        {
            base.Start ();
        }

        public override void Stop()
        {
            base.Stop ();
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate (activate);
            
            if (m_adminAlarm == null) initAdminAlarm(); else ;

            if ((activate == true)
                && (m_adminAlarm.IsStarted == false))
                m_adminAlarm.Start();
            else ;

            m_adminAlarm.Activate(activate);

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
    }

    partial class PanelAlarmJournal
    {
        CheckBox m_cbxAlarm;
        
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
        }

        #endregion
    }
}
