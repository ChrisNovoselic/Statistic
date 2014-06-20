using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using StatisticCommon;

namespace Statistic
{
    public class PanelTecViewTable : PanelTecViewBase
    {
        public PanelTecViewTable (TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep) : base (tec, num_tec, num_comp, sts, gs, par, rep) {
        }
    }

    public class PanelTecViewGraph : PanelTecViewBase
    {
        public PanelTecViewGraph(TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep)
            : base(tec, num_tec, num_comp, sts, gs, par, rep)
        {
        }
    }
}
