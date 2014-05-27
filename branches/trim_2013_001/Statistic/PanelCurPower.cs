using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Drawing;
//using System.Threading;
//using System.Data;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelCurPower : TableLayoutPanel
    {       
        private List<PanelTecCurPower> m_listTECCurrentPower;

        enum StatesMachine : int {Init_TM, Current_TM};

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        FormParameters m_parameters;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;        

        private StatusStrip m_stsStrip;

        public PanelCurPower(List<TEC> listTec, StatusStrip stsStrip, FormParameters par)
        {
            InitializeComponent();

            m_stsStrip = stsStrip;
            m_parameters = par;

            this.RowStyles.Add(new RowStyle (SizeType.AutoSize));

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            m_listTECCurrentPower = new List<PanelTecCurPower> ();
            PanelTecCurPower ptcp;

            this.ColumnCount = listTec.Count;
            for (int i = 0; i < listTec.Count; i++)
            {
                ptcp = new PanelTecCurPower(listTec[i]);
                this.Controls.Add(ptcp, i, 0);
                
                m_listTECCurrentPower.Add(ptcp);
                //ptcp.Location = new Point (1 + i * widthTec, 1);
                //ptcp.Size = new Size(widthTec, this.Height);

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / listTec.Count));                
            }
        }

        public PanelCurPower(IContainer container, List<TEC> listTec, StatusStrip stsStrip, FormParameters par)
            : this(listTec, stsStrip, par)
        {
            container.Add(this);
        }

        public void SetDelegate(DelegateFunc dStatus)
        {
            this.delegateEventUpdate = dStatus;
        }
    }
}
