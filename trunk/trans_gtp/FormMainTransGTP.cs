﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticTrans;

namespace trans_gtp
{
    public partial class FormMainTransGTP : FormMainTransDB
    {
        protected override void Start()
        {
            CreateFormConnectionSettingsConfigDB("connsett_gtp.ini");

            base.Start ();
        }
    }
}
