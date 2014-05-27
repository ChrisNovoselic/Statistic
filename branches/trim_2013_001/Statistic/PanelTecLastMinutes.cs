using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelTecLastMinutes : TableLayoutPanel
    {
        TEC m_tec;
        
        public PanelTecLastMinutes(TEC tec)
        {
            InitializeComponent();

            m_tec = tec;
            Initialize();
        }

        public PanelTecLastMinutes(IContainer container, TEC tec) : this (tec)
        {
            container.Add(this);
        }

        private void Initialize () {
        }
    }
}
