using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing; //Color..
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public abstract class PanelTecViewStandard : PanelTecViewBase
    {
        protected class HZedGraphControlStandardHours : HZedGraphControl
        {
            public HZedGraphControlStandardHours(object obj) : base(obj, FormMain.formGraphicsSettings.SetScale) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"¿»— ”›";
            }
        }

        public PanelTecViewStandard(TEC tec, int indx_tec, int indx_comp)
            : base(TecView.TYPE_PANEL.VIEW, tec, indx_tec, indx_comp)
        {
            m_arPercRows = new int [] { 5, 71 };
        }

        protected override void createZedGraphControlHours(object objLock)
        {
            this.m_ZedGraphHours = new HZedGraphControlStandardHours(objLock);
        }

        protected override void createPanelQuickData()
        {
            this._pnlQuickData = new PanelQuickDataStandard();
        }

        protected PanelQuickDataStandard PanelQuickData { get { return _pnlQuickData as PanelQuickDataStandard; } }
    }
}
