using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;

using StatisticCommon;

namespace Statistic
{
    public class PanelAdminKomDisp : PanelAdmin
    {
        public PanelAdminKomDisp(Admin a)
            : base(a)
        {
        }
    }
}
