using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace StatisticCommon
{
    public struct HReports
    {
        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;
    };

    public abstract class FormMainBaseWithStatusStrip : FormMainBase
    {
        protected System.Windows.Forms.StatusStrip m_statusStripMain;
        protected System.Windows.Forms.ToolStripStatusLabel m_lblMainState;
        protected System.Windows.Forms.ToolStripStatusLabel m_lblDescError;
        protected System.Windows.Forms.ToolStripStatusLabel m_lblDateError;

        public HReports m_report;

        protected FormMainBaseWithStatusStrip()
        {
            InitializeComponent();

            delegateEvent = new DelegateFunc(EventRaised);
        }

        private void InitializeComponent()
        {
            this.m_statusStripMain = new System.Windows.Forms.StatusStrip();
            this.m_lblMainState = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDateError = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_lblDescError = new System.Windows.Forms.ToolStripStatusLabel();

            this.m_statusStripMain.SuspendLayout();

            // 
            // m_statusStripMain
            // 
            this.m_statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_lblMainState,
            this.m_lblDateError,
            this.m_lblDescError});
            //this.m_statusStripMain.Location = new System.Drawing.Point(0, 762);
            this.m_statusStripMain.Name = "m_statusStripMain";
            //this.m_statusStripMain.Size = new System.Drawing.Size(982, 22);
            this.m_statusStripMain.TabIndex = 4;
            // 
            // m_lblMainState
            // 
            this.m_lblMainState.AutoSize = false;
            this.m_lblMainState.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.m_lblMainState.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.m_lblMainState.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.m_lblMainState.ForeColor = System.Drawing.Color.Red;
            this.m_lblMainState.Name = "m_lblMainState";
            //this.m_lblMainState.Size = new System.Drawing.Size(150, 17);
            // 
            // m_lblDateError
            // 
            this.m_lblDateError.AutoSize = false;
            this.m_lblDateError.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.m_lblDateError.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.m_lblDateError.Name = "m_lblDateError";
            //this.m_lblDateError.Size = new System.Drawing.Size(150, 17);
            // 
            // m_lblDescError
            // 
            this.m_lblDescError.AutoSize = false;
            this.m_lblDescError.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.m_lblDescError.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.m_lblDescError.Name = "m_lblDescError";
            //this.m_lblDescError.Size = new System.Drawing.Size(667, 17);
            this.m_lblDescError.Spring = true;

            this.Controls.Add(this.m_statusStripMain);

            this.m_statusStripMain.ResumeLayout(false);
            this.m_statusStripMain.PerformLayout();
        }

        protected void EventRaised()
        {
            lock (lockEvent)
            {
                UpdateStatusString();
                m_lblDescError.Invalidate();
                m_lblDateError.Invalidate();
            }
        }

        protected virtual void ErrorReport(string msg)
        {
            m_statusStripMain.BeginInvoke(delegateEvent);
        }

        protected virtual void ActionReport(string msg)
        {
            m_statusStripMain.BeginInvoke(delegateEvent);
        }

        protected abstract bool UpdateStatusString();
    }
}