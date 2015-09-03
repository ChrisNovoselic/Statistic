using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelSOTIASSO : PanelStatistic
    {
        private TecView m_tecView;

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new System.NotImplementedException();
        }

        public PanelSOTIASSO ()
        {
            m_tecView = new TecView (TecView.TYPE_PANEL.SOTIASSO, -1, -1);
        }

        public override void Start()
        {
            base.Start ();
        }

        public override void Stop()
        {
            base.Stop ();
        }

        public override bool Activate(bool active)
        {
            bool bRes = false;

            bRes = base.Activate(active);

            return bRes;
        }
    }
}