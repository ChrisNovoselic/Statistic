using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class PanelLastMinutes : Panel
    {
        public PanelLastMinutes()
        {
            InitializeComponent();
        }

        public PanelLastMinutes(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
