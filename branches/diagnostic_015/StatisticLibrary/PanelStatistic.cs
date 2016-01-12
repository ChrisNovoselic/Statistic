﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Drawing; //Color..

using HClassLibrary;

namespace StatisticCommon
{
    public abstract class PanelStatistic : HPanelCommon
    {
        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        public static DataGridViewCellStyle dgvCellStyleError, dgvCellStyleWarning
            , dgvCellStyleCommon;

        public PanelStatistic(int cCols = -1, int cRows = -1)
            : base(cCols, cRows)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleWarning = new DataGridViewCellStyle();
            dgvCellStyleWarning.BackColor = Color.Yellow;
            dgvCellStyleCommon = new DataGridViewCellStyle();
        }        

        public static volatile int POOL_TIME = -1
            , ERROR_DELAY = -1;

        public virtual void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public abstract void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr);

        public virtual bool MayToClose()
        {
            return true;
        }
    }

    public abstract class PanelStatisticWithTableHourRows : PanelStatistic
    {
        protected abstract void initTableHourRows();
    }
}
