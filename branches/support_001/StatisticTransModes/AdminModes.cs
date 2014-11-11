﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;

namespace StatisticTransModes
{
    public abstract class AdminModes : HAdmin
    {
        protected List<string> m_listModesId;

        public AdminModes (HReports rep) : base (rep) {
            m_listModesId = new List<string>();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override bool Response(int idListener, out bool error, out DataTable table/*, bool isTec*/)
        {
            return DbMCSources.Sources ().Response(m_IdListenerCurrent, out error, out table);
        }

        protected override void GetPPBRDatesRequest(DateTime date) {
        }

        protected override bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        public override void getCurRDGValues(HAdmin source)
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = source.m_curRDGValues[i].pbr;
                m_curRDGValues[i].pmin = source.m_curRDGValues[i].pmin;
                m_curRDGValues[i].pmax = source.m_curRDGValues[i].pmax;

                m_curRDGValues[i].pbr_number = source.m_curRDGValues[i].pbr_number;
            }
        }

        public override void CopyCurToPrevRDGValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].pbr = m_curRDGValues[i].pbr;
                m_prevRDGValues[i].pmin = m_curRDGValues[i].pmin;
                m_prevRDGValues[i].pmax = m_curRDGValues[i].pmax;

                m_prevRDGValues[i].pbr_number = m_curRDGValues[i].pbr_number;
            }
        }

        public override void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = m_curRDGValues[i].pmin = m_curRDGValues[i].pbr = 0.0;
                m_curRDGValues[i].pbr_number = string.Empty;
            }

            //CopyCurToPrevRDGValues();
        }

        public override bool WasChanged()
        {
            int i = -1, j = -1;

            for (i = 0; i < 24; i++)
            {
                for (j = 0; j < 3 /*4 для SN???*/; j++)
                {
                    if (!(m_prevRDGValues[i].pbr == m_curRDGValues[i].pbr))
                        return true;
                    else
                        ;
                }
            }

            return false;
        }

        protected abstract bool InitDbInterfaces ();

        public override void Start()
        {
            InitDbInterfaces();

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            DbMCSources.Sources().UnRegister(m_IdListenerCurrent);
        }
    }
}