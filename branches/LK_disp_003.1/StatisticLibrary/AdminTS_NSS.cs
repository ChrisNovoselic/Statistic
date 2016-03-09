using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;

namespace StatisticCommon
{
    public class AdminTS_NSS : AdminTS_TG
    {
        public AdminTS_NSS(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues)
        {
        }
    }
}
