using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;

using ASUTP.Helper;
using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using System.Linq;

namespace StatisticCommon
{    
    public abstract class HAdmin : HHandlerDb
    {
        public static int SEASON_BASE = 5;
        public enum seasonJumpE
        {
            None,
            WinterToSummer,
            SummerToWinter,
        }

        /// <summary>
        /// ������� �������� ����������� ��� ��� ����������. �������� ���-��������
        ///  , ����� '_listTECComponentIndex'
        /// </summary>
        protected List<FormChangeMode.KeyDevice> _listTECComponentKey;

        public FormChangeMode.KeyDevice FirstTECComponentKey
        {
            get
            {
                return (Equals (_listTECComponentKey, null) == false)
                    && (_listTECComponentKey.Count > 0)
                        ? _listTECComponentKey [0]
                            : FormChangeMode.KeyDevice.Empty;
            }
        }

        /// <summary>
        /// ��������� ������ ����������� ��� � ����������� �� ���� ����������
        /// </summary>
        /// <param name="mode">����������� ���� �����������</param>
        /// <param name="bLimitLK">������� ����� ������ �� ��� ������������ ������</param>
        /// <returns>���������� ������ ������, �� ������� �������� ����� ����������</returns>
        public virtual List<FormChangeMode.KeyDevice> GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT mode, bool bLimitLK)
        {
            List<FormChangeMode.KeyDevice> listRes = new List<FormChangeMode.KeyDevice> ();

            int iLimitIdTec = bLimitLK == true ? (int)TECComponent.ID.LK : (int)TECComponent.ID.GTP;

            switch (mode) {
                case FormChangeMode.MODE_TECCOMPONENT.TEC:
                    foreach (TEC tec in m_list_tec) {
                        if (!(tec.m_id > iLimitIdTec))
                            listRes.Add (new FormChangeMode.KeyDevice () { Id = tec.m_id, Mode = mode });
                        else
                            ;
                    }
                    break;
                case FormChangeMode.MODE_TECCOMPONENT.GTP:
                case FormChangeMode.MODE_TECCOMPONENT.PC:
                case FormChangeMode.MODE_TECCOMPONENT.TG:
                    foreach (TECComponent comp in allTECComponents) {
                        if ((!(comp.tec.m_id > iLimitIdTec))
                            && (mode == comp.Mode))
                            listRes.Add (new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = mode });
                        else
                            ;
                    }
                    break;
                default:
                    break;
            }

            return listRes;
        }

        public virtual FormChangeMode.KeyDevice PrepareActionRDGValues ()
        {
            return FirstTECComponentKey;
        }

        public virtual void TECComponentComplete (int state, bool bResult)
        {
            //??? � ���� ����������� �� save - ������������ � ����������� ������
            if (_listTECComponentKey.Count > 0)
                _listTECComponentKey.RemoveAt (0);
            else
                ;
        }

        protected int GetSeasonValue (int h) {
            int iRes = SEASON_BASE;

            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == true) {

            } else {
                if (m_curDate.Date.CompareTo (HAdmin.SeasonDateTime.Date) < 0) {
                    //�� ��������
                    iRes += (int)seasonJumpE.SummerToWinter;
                } else {
                    //����� ��������
                    iRes += (int)seasonJumpE.WinterToSummer;
                }
            }

            return iRes;
        }

        /// <summary>
        /// ��������� �� ��������� ������ �������� (���) ���������� �������������� ������� (���)
        /// </summary>
        public struct /*class*/ RDGStruct
        {
            //public double [] ppbr;
            public double pbr;
            public double pmin;
            public double pmax;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
            public bool fc;

            public string pbr_number;
            public DateTime dtRecUpdate;

            public void From(RDGStruct src, bool bPBRNumberEmptyChecked = false)
            {
                pbr = src.pbr;
                pmin = src.pmin;
                pmax = src.pmax;
                recomendation = src.recomendation;
                deviationPercent = src.deviationPercent;
                deviation = src.deviation;
                fc = src.fc;

                if (bPBRNumberEmptyChecked == true)
                    if (src.pbr_number.Equals(string.Empty) == false)
                        pbr_number = src.pbr_number;
                    else
                        ;
                else
                    pbr_number = src.pbr_number;
                dtRecUpdate = src.dtRecUpdate;
            }

            public RDGStruct Copy(bool bPBRNumberEmptyChecked = false)
            {
                RDGStruct oRes = new RDGStruct();

                oRes.From(this, bPBRNumberEmptyChecked);

                return oRes;
            }

            public static bool operator == (RDGStruct s1, RDGStruct s2)
            {
                bool bRes = false;

                bRes = (s1.pbr == s2.pbr)
                    && (s1.recomendation == s2.recomendation)
                    && (s1.fc == s2.fc)
                    && ((s1.deviation == s2.deviation)
                        && (s1.deviationPercent == s2.deviationPercent));

                return bRes;
            }

            public static bool operator != (RDGStruct s1, RDGStruct s2)
            {
                bool bRes = true;

                bRes = !(s1.pbr == s2.pbr)
                    || !(s1.recomendation == s2.recomendation)
                    || !(s1.fc == s2.fc)
                    || (!(s1.deviation == s2.deviation)
                        || !(s1.deviationPercent == s2.deviationPercent));

                return bRes;
            }

            public override bool Equals (object obj)
            {
                return (obj is RDGStruct) ? this == (RDGStruct)obj : false;
            }

            public override int GetHashCode ()
            {
                return base.GetHashCode();
            }
        }

        protected TimeSpan _tsOffsetToMoscow;
        /// <summary>
        /// �������� �� �������� �����
        /// </summary>
        public TimeSpan m_tsOffsetToMoscow { get { return _tsOffsetToMoscow; } }

        public volatile RDGStruct[] m_prevRDGValues;
        public RDGStruct[] m_curRDGValues;

        protected DelegateIntFunc saveComplete = null;
        protected Action<DateTime, bool> readyData = null;
        protected DelegateIntFunc errorData = null;

        protected DelegateDateFunc setDatetime;

        /// <summary>
        /// ��� �������� ������� (��� ������������ �������-, �����)
        /// </summary>
        protected TECComponentBase.TYPE _type;
        ///// <summary>
        ///// ??? ��� �������� ������� ������ ���������� �� ��� ���, ���� �������� ���������� �� ����� �������� � ����� ������ 'allTECComponents'
        /////  , ������� ��������� ���� ��� �������� �������; ����������, ���������� ����������, ���� ������ - ��� � �����
        ///// </summary>
        //public TECComponentBase.TYPE Type { get { return _type; } }
        /// <summary>
        /// ������ �������� 'TEC'
        /// </summary>
        public volatile StatisticCommon.DbTSQLConfigDatabase.ListTEC m_list_tec;
        /// <summary>
        /// ������ 
        /// </summary>
        protected volatile List<TECComponent> allTECComponents;

        //private int _indxTECComponents;
        ///// <summary>
        ///// ������� ������ ���������� �� ������ 'allTECComponents' (��� ���������� ����� �������� �������)
        ///// </summary>
        //public int indxTECComponents
        //{
        //    get
        //    {
        //        return _indxTECComponents;
        //    }

        //    set
        //    {
        //        _indxTECComponents = value;
        //    }
        //}
        private FormChangeMode.KeyDevice _currentKey;
        public FormChangeMode.KeyDevice CurrentKey
        {
            get
            {
                return _currentKey;
            }

            set
            {
                _currentKey = value;
            }
        }

        public IDevice CurrentDevice
        {
            get
            {
                return FindTECComponent (CurrentKey);
            }
        }
        /// <summary>
        /// �������� �������� ����/�������
        /// </summary>
        public DateTime m_prevDate
            , serverTime
            , m_curDate;

        //������������ �� ������?
        public HMark m_markQueries; //CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE

        protected volatile bool using_date;

        public bool m_ignore_date;
        /// <summary>
        /// ������ ����� ���/�������, ��������� � ��
        /// ��������, ���������������� ����, ��������������� �������� �������������� ������ � ������� ��
        /// 1-� ����������� - ��� �������� (���, �����), 2-� - �������������� �������
        /// </summary>
        protected int[,] m_arHaveDates;
        //protected Dictionary<CONN_SETT_TYPE, List<HAVE_DATES>> m_dictHaveDates;
        /// <summary>
        /// ����� ��� � ��
        /// </summary>
        protected int m_iHavePBR_Number;

        private static int m_iSeasonAction;

        public static int SeasonAction 
        {
            get { return m_iSeasonAction; } 

            set { m_iSeasonAction = value; }
        }

        private static DateTime m_dtSeason;

        public static DateTime SeasonDateTime {
            get {
                return m_dtSeason;
            }

            set {
                m_dtSeason = value;
            }
        }

        public HAdmin(TECComponentBase.TYPE type)
        {
            _type = type;

            m_iHavePBR_Number = -1;

            Initialize ();
        }

        protected override void Initialize () {
            base.Initialize ();

            using_date = false;
            m_ignore_date = false;
            //m_ignore_connsett_data = false;

            m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, 24];
            //m_dictHaveDates = new Dictionary<CONN_SETT_TYPE, List<HAVE_DATES>>() {
            //    { CONN_SETT_TYPE.ADMIN, new List<HAVE_DATES>(24) }
            //    , { CONN_SETT_TYPE.PBR, new List<HAVE_DATES>(24) }
            //};

            allTECComponents = new List<TECComponent>();

            m_curRDGValues = new RDGStruct[24];
            m_prevRDGValues = new RDGStruct[24];

            //for (int i = 0; i < 24; i++)
            //{
            //    m_curRDGValues[i].ppbr = new double[3 /*4 ��� SN???*/];
            //    m_prevRDGValues[i].ppbr = new double[3 /*4 ��� SN???*/];
            //}
        }

        /// <summary>
        /// ������� ��� �� ������ �� ��������������
        /// </summary>
        /// <param name="id_tec">������������� ���</param>
        public void RemoveTEC(int id_tec)
        {
            foreach (TEC t in this.m_list_tec) {
                if (t.m_id == id_tec) {
                    this.m_list_tec.Remove (t);
                    break;
                }
                else
                    ;
            }

            for (int i = 0; i < allTECComponents.Count; i ++) {
                if (allTECComponents [i].tec.m_id == id_tec)
                {
                    allTECComponents.RemoveAt (i);

                    i --;
                }
                else
                    ;
            }
        }

        private void initQueries(ASUTP.Core.HMark markQueries)
        {
            if (m_markQueries == null)
                m_markQueries = markQueries;
            else
                m_markQueries.Add(markQueries);
        }

        public virtual void InitTEC(List <StatisticCommon.TEC> listTEC, ASUTP.Core.HMark markQueries)
        {
            this.m_list_tec = new DbTSQLConfigDatabase.ListTEC ();
            ////������� �1
            //this.m_list_tec.AddRange(listTEC);
            ////������� �2
            //listTEC.ForEach(t => this.m_list_tec.Add(t));
            //������� �3 - ��������� ��������� ��� ������������� �������� � ������������ � ������������� ��������
            foreach (TEC t in listTEC)
                //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id))
                    this.m_list_tec.Add (t);
                //else ;

            initQueries(markQueries);
            initTECComponents();
        }

        public void InitTEC(FormChangeMode.MODE_TECCOMPONENT mode, /*TYPE_DATABASE_CFG typeCfg, */HMark markQueries, bool bIgnoreTECInUse, int[] arTECLimit, bool bUseData = false)
        {
            //Logging.Logg().Debug("Admin::InitTEC () - ����...");

            if ((mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                || (mode == FormChangeMode.MODE_TECCOMPONENT.ANY)) //??? ����� '.ANY'
                this.m_list_tec = DbTSQLConfigDatabase.DbConfig().InitTEC(bIgnoreTECInUse, arTECLimit, bUseData) as DbTSQLConfigDatabase.ListTEC;
            else
                this.m_list_tec = DbTSQLConfigDatabase.DbConfig ().InitTEC(mode, bIgnoreTECInUse, arTECLimit, bUseData) as DbTSQLConfigDatabase.ListTEC;

            initQueries(markQueries);
            initTECComponents();

            try {
                if ((mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    || (mode == FormChangeMode.MODE_TECCOMPONENT.ANY)) //??? ����� '.ANY'
                    CurrentKey = new FormChangeMode.KeyDevice () { Id = this.m_list_tec[0].m_id, Mode = mode };
                else
                    CurrentKey = new FormChangeMode.KeyDevice () { Id = allTECComponents.First (comp => comp.Mode == mode).m_id, Mode = mode };
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"HADmin::InitTEC (mode={mode}) - �� ������ 1-�� ������� ��� ������������� ������", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
        /// <summary>
        /// ������������� ������ �� ����� ������������ ���
        /// </summary>
        protected virtual void initTECComponents()
        {
            allTECComponents.Clear();

            foreach (StatisticCommon.TEC t in this.m_list_tec)
            {
                //Logging.Logg().Debug("Admin::InitTEC () - ������������ ����������� ��� ���:" + t.name);

                //if (t.list_TECComponents.Count > 0)
                    foreach (TECComponent g in t.ListTECComponents)
                        if (g.Type == _type)
                            allTECComponents.Add(g);
                        else
                            ;
                //else
                //    //??? ���������� - ������������ ������ ��� ����������� ���������
                //    allTECComponents.Add(t.list_TECComponents[0]);
            }
        }

        protected static bool CheckNameFieldsOfTable(DataTable tbl, string[] nameFields)
        {
            bool bRes = true;

            foreach (string nameField in nameFields)
            {
                if (tbl.Columns.IndexOf(nameField) < 0)
                {
                    bRes = false;

                    Logging.Logg ().Error ($"HAdmin::CheckNameFieldsOfTable (����� ��������={tbl.Columns.Count}) - �� ������ �������: {nameField}", Logging.INDEX_MESSAGE.NOT_SET);

                    break;
                }
                else
                    ;
            }

            return bRes;
        }

        public abstract bool WasChanged();

        private void register(int id, ConnectionSettings connSett, string name, CONN_SETT_TYPE type)
        {
            register(id, (int)type, connSett, name);
        }

        public override void StartDbInterfaces()
        {
            if (!(m_list_tec == null))
            {
                bool bInitSensorsStrings = (m_markQueries.IsMarked((int)CONN_SETT_TYPE.DATA_AISKUE) == true)
                                            || (m_markQueries.IsMarked((int)CONN_SETT_TYPE.DATA_SOTIASSO) == true)
                                            || (m_markQueries.IsMarked((int)CONN_SETT_TYPE.MTERM) == true);
                foreach (TEC t in m_list_tec)
                {
                    if (!(t.connSetts == null))
                    {
                        CONN_SETT_TYPE i = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;

                        if (m_dictIdListeners.ContainsKey(t.m_id) == false)
                        {
                            m_dictIdListeners.Add(t.m_id, new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE]);

                            for (i = CONN_SETT_TYPE.ADMIN; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                                m_dictIdListeners[t.m_id][(int)i] = -1;
                        }
                        else
                            ;

                        for (i = CONN_SETT_TYPE.ADMIN; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                        {
                            if ((!(t.connSetts[(int)i] == null)) && (m_markQueries.IsMarked((int)i) == true))
                            {
                                if (m_dictIdListeners[t.m_id][(int)i] < 0)
                                    ;
                                else
                                    DbSources.Sources().UnRegister(m_dictIdListeners[t.m_id][(int)i]);

                                register(t.m_id, t.connSetts[(int)i], t.name_shr, i);
                            }
                            else
                                ;
                        }

                        if ((bInitSensorsStrings == true)
                            && (t.GetReadySensorsStrings (_type) == false))
                            t.InitSensorsTEC();
                        else
                            ;
                    }
                    else
                        //������ ������ ���-���� ����������������
                        Logging.Logg().Error(@"HAdmin::StartDbInterfaces () - connSetts == null ...", Logging.INDEX_MESSAGE.NOT_SET);
                } //foreach...
            }
            else
                //������ ������ ���-���� ����������������
                Logging.Logg().Error(@"HAdmin::StartDbInterfaces () - m_list_tec == null ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        //private void stopDbInterfaces()
        //{
        //    if (!(m_list_tec == null))
        //        foreach (TEC t in m_list_tec)
        //            for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
        //            {
        //                if ((m_dictIdListeners.ContainsKey (t.m_id) == true) && (!(m_dictIdListeners[t.m_id][i] < 0)))
        //                {
        //                    DbSources.Sources().UnRegister(m_dictIdListeners[t.m_id][i]);
        //                    m_dictIdListeners[t.m_id][i] = -1;
        //                }
        //                else
        //                    ;
        //            }
        //    else
        //        //������ ������ ���-���� ����������������
        //        Logging.Logg().Error(@"HAdmin::stopDbInterfaces () - m_list_tec == null ...");
        //}

        //public void RefreshConnectionSettings()
        //{
        //    if (threadIsWorking > 0)
        //    {
        //        foreach (TEC t in m_list_tec) {
        //            for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
        //            {
        //                if (!(m_dictIdListeners [t.m_id][i] < 0))
        //                    DbSources.Sources().SetConnectionSettings(m_dictIdListeners[t.m_id][i], t.connSetts[i], true);
        //                else
        //                    ;
        //            }
        //        }
        //    }
        //    else
        //        ;
        //}

        public void SetDelegateSaveComplete(DelegateIntFunc f) 
        {            
            saveComplete = f;

            //Logging.Logg().Debug(@"HAdmin::SetDelegateSaveComplete () - saveComplete is set=" + (saveComplete == null ? false.ToString() : true.ToString()) + @" - �����", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public void SetDelegateData(Action<DateTime, bool> s, DelegateIntFunc e) { readyData = s; errorData = e; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunc f) { setDatetime = f; }

        //protected void actualizeTimezone(ref DataTable table, int indxCol)
        //{
        //    foreach (DataRow r in table.Rows)
        //        r[indxCol] = Convert.ToDateTime(r[indxCol].ToString()).Add(m_tsOffsetToMoscow);
        //}

        public override void ClearValues()
        {
            int cntHours = 24;

            //if (cnt < 0) {
                //�������� �������� "��������" ������� �������
                if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
                {
                    if (m_curRDGValues.Length > 24)
                    {
                        m_curRDGValues = null;
                    }
                    else
                    {
                    }
                }
                else
                {
                    if (m_curRDGValues.Length < 25)
                    {
                        m_curRDGValues = null;
                        cntHours = 25;
                    }
                    else
                    {
                    }
                }
            //} else {
            //    m_curRDGValues = null;
            //    cntHours = cnt;
            //}

            if (m_curRDGValues == null)
                m_curRDGValues = new RDGStruct[cntHours];
            else
                ;
        }

        public abstract void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date);

        protected abstract void getPPBRDatesRequest(DateTime date);

        protected abstract int getPPBRDatesResponse(DataTable table, DateTime date);

        protected abstract void getPPBRValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/);

        protected abstract int getPPBRValuesResponse(DataTable table, DateTime date);

        protected virtual void clearDates(CONN_SETT_TYPE type)
        {
            int cntHours = -1
                , length = -1
                    ;

            cntHours = m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == false
                ? 24
                    : 25;
            length =
                m_arHaveDates.Length / m_arHaveDates.Rank
                //m_dictHaveDates[type].Count
                ;

            if (!(length == cntHours)) {
                if (length < cntHours)
                    m_arHaveDates = new int[2, cntHours];
                    //while (m_dictHaveDates[type].Count < cntHours)
                    //    m_dictHaveDates[type].Add(new HAVE_DATES());
                else if (length > cntHours)
                    m_arHaveDates = new int[2, cntHours];
                    //while (m_dictHaveDates[type].Count > cntHours)
                    //    m_dictHaveDates[type].RemoveAt(0);
                else
                    // ������������ ���
                    ;
            } else
                ;

            for (int i = 0; i < cntHours; i ++)
                m_arHaveDates[(int)type, i] = 0;
            //m_dictHaveDates[type].ForEach(date => date.Reset());
        }

        protected void clearPPBRDates()
        {
            clearDates(CONN_SETT_TYPE.PBR);
        }

        public TECComponent FindTECComponent(int id)
        {
            return allTECComponents.FirstOrDefault (tc => tc.m_id == id);
        }

        public IDevice FindTECComponent (FormChangeMode.KeyDevice key)
        {
            IDevice dev;

            if ((key.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                || (key.Mode == FormChangeMode.MODE_TECCOMPONENT.ANY))
                dev = m_list_tec.FirstOrDefault(tec => tec.m_id == key.Id);
            else
                dev = FindTECComponent (key.Id);

            return dev;
        }

        public const string PBR_PREFIX = @"���";

        /// <summary>
        /// ���������� ������������ ������ ��� �� ������ ����
        /// </summary>
        /// <param name="hour">����� ����</param>
        /// <returns>������������ ���</returns>

        protected string getNamePBRNumber (int hour = -1) {
            return string.Format("{0}{1}", PBR_PREFIX, getPBRNumber (hour));
        }

        /// <summary>
        /// ���������� ����� ��� �� ������������
        /// </summary>
        /// <param name="pbr">������������ ���</param>
        /// <param name="err">������� ������ ��� ���������� ������ ���</param>
        /// <param name="bLogging">������� ������� �������������� ������</param>
        /// <returns>����� ���</returns>
        public static int GetPBRNumber (string pbr, out int err, bool bLogging = true) {
            int iRes = -1;

            err = pbr.Length > PBR_PREFIX.Length ? 0 : -1;

            if (err == 0) {
                err = int.TryParse (pbr.Substring (PBR_PREFIX.Length), out iRes) == true
                    ? err = 0
                        : err = -1;

                if (err < 0)
                    if (pbr.Equals (string.Format ("{0}{1}", "�", PBR_PREFIX)) == true) {
                        err = 1;
                        iRes = 0;
                    } else
                        ;
                else
                    ;
            } else
                if (bLogging == true)
                    Logging.Logg().Error($"HAdmin::GetPBRNumber (����={pbr}) - ������ ������� �{PBR_PREFIX}...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    ;

            return iRes;
        }

        /// <summary>
        /// ���������� ����� ��� �� �������� ����
        /// </summary>
        /// <param name="err">������� ���������� ���������� ������</param>
        /// <returns>����� ���</returns>
        public int GetPBRNumber(out int err)
        {
            return GetPBRNumber(-1, out err);
        }

        public int GetPBRNumber(int indx, out int err)
        {
            err = 0;

            int iRes = -1
                , iIndx = indx;

            if (iIndx < 0)
                iIndx = m_curRDGValues.Length - 1;
            else
                ;

            if (m_curDate.Date.CompareTo(serverTime.Date) == 0)
                if ((!(m_curRDGValues == null))
                    && (!(m_curRDGValues[iIndx].pbr_number == null))
                    && (m_curRDGValues[iIndx].pbr_number.Length > PBR_PREFIX.Length)) {
                    iRes = GetPBRNumber (m_curRDGValues[iIndx].pbr_number, out err);
                    if (err < 0) {
                        iRes = getPBRNumber();
                    } else
                        ;
                } else {
                    err = -2; //��� �� ��������

                    iRes = getPBRNumber();
                }
            else
                if (m_curDate.Date.CompareTo(serverTime.Date) > 0)
                    if ((!(m_curRDGValues == null))
                        && (!(m_curRDGValues[iIndx].pbr_number == null))
                        && (m_curRDGValues[iIndx].pbr_number.Length > PBR_PREFIX.Length)) {
                        iRes = GetPBRNumber (m_curRDGValues[iIndx].pbr_number, out err);
                    } else
                        iRes = getPBRNumber();
                else
                    ;

            return iRes;
        }

        protected int getPBRNumber(int hour = -1)
        {
            int iRes = -1
                , iHour = hour;

            if (iHour < 0)
            {
                if (m_ignore_date == true)
                    iHour = HDateTime.ToMoscowTimeZone ().Hour;
                else
                    iHour = serverTime.Hour;
            }
            else
                ;

            //// ��� ��������� ���������� ��� = 2 �
            //if ((iHour % 2) > 0)
            //    iRes = iHour;
            //else
                iRes = iHour + 1;

            return iRes;
        }

        public FormChangeMode.MODE_TECCOMPONENT modeTECComponent(int indx)
        {
            FormChangeMode.MODE_TECCOMPONENT modeRes = FormChangeMode.MODE_TECCOMPONENT.ANY;
            ////������� �1
            //if ((allTECComponents[indx].m_id > 0) && (allTECComponents[indx].m_id < 100))
            //    //???
            //    modeRes = FormChangeMode.MODE_TECCOMPONENT.TEC;
            //else
            //    if (allTECComponents[indx].IsGTP == true)
            //        modeRes = FormChangeMode.MODE_TECCOMPONENT.GTP;
            //    else
            //        if (allTECComponents[indx].IsPC == true)
            //            modeRes = FormChangeMode.MODE_TECCOMPONENT.PC;
            //        else
            //            if (allTECComponents[indx].IsTG == true)
            //                modeRes = FormChangeMode.MODE_TECCOMPONENT.TG;
            //            else
            //                ;
            //������� �2
            modeRes = TECComponent.GetMode(allTECComponents[indx].m_id);

            return modeRes;
        }

        public virtual void CopyCurToPrevRDGValues() 
        {
            if (!(m_prevRDGValues.Length == m_curRDGValues.Length))
            {
                m_prevRDGValues = null;
                m_prevRDGValues = new RDGStruct[m_curRDGValues.Length];
            }
            else
            {
            }
        }

        public virtual void getCurRDGValues (HAdmin source) 
        {
            if (!(m_curRDGValues.Length == source.m_curRDGValues.Length))
            {
                m_prevRDGValues = null;
                m_prevRDGValues = new RDGStruct[source.m_curRDGValues.Length];
            }
            else
            {
            }
        }

        protected virtual int GetCurrentTimeResponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                serverTime = ((DateTime)table.Rows[0][0]).Add(m_tsOffsetToMoscow);
            }
            else
            {
                //DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
                //int timezone_offset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc;
                //if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight) == true)
                //    timezone_offset++;
                //else
                //    ;

                ////serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);
                ////serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(timezone_offset);
                //serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(allTECComponents[indxTECComponents].tec.m_timezone_offset_msc);
                serverTime = HDateTime.ToMoscowTimeZone();

                ErrorReport("������ ��������� �������� ������� �������. ������������ ��������� �����.");
            }

            return 0;
        }

        public virtual void ResetRDGExcelValues()
        {
            if (m_waitHandleState.Length > 1)
                ((ManualResetEvent)m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.ERROR]).Reset();
            else
                ;
        }

        protected virtual bool IsCanUseTECComponents
        {
            get
            {
                return CurrentKey.Id > 0;
            }
        }

        /// <summary>
        /// ���������� ������� ������� �������� � �� (PPBR, ADMIN) �� ������� ����
        /// </summary>
        /// <param name="type">��� �������� (PPBR, ADMIN)</param>
        /// <param name="indx">������ ����</param>
        /// <returns>������� �=������� ��������</returns>
        protected bool IsHaveDates(CONN_SETT_TYPE type, int indx)
        {
            return
                m_arHaveDates[(int)type, indx] > 0 ? true : false
                //m_dictHaveDates[type].Exists(date => { return (date.date_time.Hour - 1) == indx; });
                ;
        }

        /// <summary>
        /// ���������� �������� ������� ���� � ~ �� (��)���������� ����������� ����-�������-���� � ����/������� �������� �������/������� ����������
        /// </summary>
        /// <param name="dt">����������� ����</param>
        /// <param name="h">??? ������ ���� ����� ���������� ���, ���� ��� ����� ����������� � ����/�������</param>
        /// <returns>�������� ������������ �������������</returns>
        public static int GetSeasonHourOffset(DateTime dt, int h)
        {
            int iRes = 0;

            iRes = dt.Date.Equals(HAdmin.SeasonDateTime.Date) == true
                ? h > HAdmin.SeasonDateTime.Hour
                    ? 1
                        : 0
                : 0;

            return iRes;
        }

        /// <summary>
        /// ���������� �������� ������� ���� � ~ �� (��)���������� �����������-������� ����-�������-���� � ����/������� �������� �������/������� ����������
        /// </summary>
        /// <param name="h">??? ������ ���� ����� ���������� ���, ���� ��� ����� ����������� � ������� ����/�������</param>
        /// <returns>�������� ������������ �������������</returns>
        public int GetSeasonHourOffset(int h)
        {
            return GetSeasonHourOffset(m_curDate, h);
        }

        /// <summary>
        /// �������� ������ ���� � ~ �� (��)���������� ������������� ����-�������-���� � ����/������� �������� �������/������� ����������
        /// </summary>
        /// <param name="ssn">������� �������� ����� �������� ��� ����������� ������� ����</param>
        /// <param name="h">������ �� ���������� ������ ����</param>
        protected void GetSeasonHourIndex(int ssn, ref int h) //��� ������ �� �������, �� �� ����
        {
            //�������� ���� �������� �������
            if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
                //??? �������� ������� ���� �������� �������
                if (h == HAdmin.SeasonDateTime.Hour)
                    //��������� �����
                    if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.WinterToSummer)
                        h++;
                    else if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.SummerToWinter)
                    //??? ������ �� ������
                        ;
                    else if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.None)
                    //??? ������ �� ������
                        ;
                    else
                    //??? ������������ ���
                        ;
                else
                    if (h > HAdmin.SeasonDateTime.Hour)
                        h++;
                    else
                        ;
            else
                ;
        }

        public string GetFmtDatetime(int h)
        {
            string strRes = @"dd-MM-yyyy HH";

            if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
            {
                if ((h) == (HAdmin.SeasonDateTime.Hour))
                    strRes += @"*";
                else
                    ;
            }
            else
                ;

            strRes += @":00";

            return strRes;
        }

        public static int CountHoursOfDate (DateTime dtReq) {
            int iRes = -1;

            if (dtReq.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                iRes = 25;
            else
                iRes = 24;

            return iRes;
        }
    }
}
