using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    class PanelTecViewLK : PanelTecViewBase
    {
        public PanelTecViewLK(StatisticCommon.TEC tec, int num_tec, int num_comp)
            : base(tec, num_tec, num_comp/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {            
            InitializeComponent ();
        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new NotImplementedException();
        }
    }
}
