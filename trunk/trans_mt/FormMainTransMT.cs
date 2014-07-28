using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;

namespace trans_mt
{
    public partial class FormMainTransMT : FormMainTransDB
    {
        protected override void Start()
        {
            CreateFormConnectionSettingsConfigDB("connsett_mt.ini");

            base.Start();
        }
    }
}
