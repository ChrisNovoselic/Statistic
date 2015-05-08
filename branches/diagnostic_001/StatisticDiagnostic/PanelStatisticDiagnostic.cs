using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms; //TableLayoutPanel

using System.Data; //DataTable
using System.Data.Common; //DbConnection

using HClassLibrary;
using StatisticCommon;

namespace StatisticDiagnostic
{
    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
        public PanelStatisticDiagnostic()
        {
            InitializeComponent();
        }

        public PanelStatisticDiagnostic(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }
    }
}
