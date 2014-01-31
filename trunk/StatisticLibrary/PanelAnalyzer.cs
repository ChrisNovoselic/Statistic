using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StatisticCommon
{
    public partial class PanelAnalyzer : Panel
    {
        public PanelAnalyzer()
        {
            InitializeComponent();
        }

        public PanelAnalyzer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
