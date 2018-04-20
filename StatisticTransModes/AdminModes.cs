﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;

namespace StatisticTransModes
{
    public enum INDEX_PLAN_FACTOR { Unknown = -1, PBR, Pmin, Pmax, COUNT }

    public abstract class AdminModes : HAdmin
    {
        protected List<string> m_listModesId;

        public AdminModes()
            : base(TECComponentBase.TYPE.ELECTRO)
        {
            m_listModesId = new List<string>();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override int response(int idListener, out bool error, out object outobj/*, bool isTec*/)
        {
            int iRes = -1;
            DataTable table = null;
            iRes = ASUTP.Database.DbSources.Sources().Response(idListener, out error, out table);
            outobj = table as DataTable;

            return iRes;
        }

        public override void getCurRDGValues(HAdmin source)
        {
            base.getCurRDGValues (source);

            setCurrentRDGValues (source.m_curRDGValues);
        }

        public void setCurrentRDGValues (RDGStruct [] values)
        {
            setRDGValues (m_curRDGValues, values);
        }

        public void setRDGValues (RDGStruct [] valuesDest, RDGStruct [] valuesSource)
        {
            for (int i = 0; i < valuesSource.Length; i++)
                setRDGValue (ref valuesDest [i], valuesSource [i]);
        }

        public void setCurrentRDGValue (int iHour, RDGStruct valueSource)
        {
            setRDGValue(ref m_curRDGValues[iHour], valueSource);
        }

        private void setRDGValue (ref RDGStruct valueDest, RDGStruct valueSource)
        {
            valueDest.pbr = valueSource.pbr;
            valueDest.pmin = valueSource.pmin;
            valueDest.pmax = valueSource.pmax;

            valueDest.pbr_number = valueSource.pbr_number;
            valueDest.dtRecUpdate = valueSource.dtRecUpdate;
        }

        public override void CopyCurToPrevRDGValues()
        {
            base.CopyCurToPrevRDGValues ();

            setRDGValues (m_prevRDGValues, m_curRDGValues);
        }

        //public override void ClearValues(int cnt = -1)
        public override void ClearValues()
        {
            //base.ClearValues(cnt);
            base.ClearValues();

            for (int i = 0; i < m_curRDGValues.Length; i++)
            {
                m_curRDGValues[i].pbr = m_curRDGValues[i].pmin = m_curRDGValues[i].pmax = 0.0;
                m_curRDGValues[i].pbr_number = string.Empty;
                m_curRDGValues[i].dtRecUpdate = DateTime.MinValue;
            }

            //CopyCurToPrevRDGValues();
        }

        public override bool WasChanged()
        {
            int i = -1, j = -1;

            for (i = 0; i < m_curRDGValues.Length; i++)
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

            ASUTP.Database.DbSources.Sources().UnRegister(m_IdListenerCurrent);
        }
    }
}
