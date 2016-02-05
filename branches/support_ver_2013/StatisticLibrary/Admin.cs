using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using HClassLibrary;

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

        protected int GetSeasonValue (int h) {
            int iRes = SEASON_BASE;

            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == true) {

            } else {
                if (m_curDate.Date.CompareTo (HAdmin.SeasonDateTime.Date) < 0) {
                    //ДО перехода
                    iRes += (int)seasonJumpE.SummerToWinter;
                } else {
                    //ПОСЛЕ перехода
                    iRes += (int)seasonJumpE.WinterToSummer;
                }
            }

            return iRes;
        }

        /// <summary>
        /// структура дл яхранения данных элемента (час) расписания диспетчерского графика (РДГ)
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
        }

        public volatile RDGStruct[] m_prevRDGValues;
        public RDGStruct[] m_curRDGValues;

        protected DelegateFunc saveComplete = null;
        protected DelegateDateFunc readyData = null;
        protected DelegateFunc errorData = null;

        protected DelegateDateFunc setDatetime;

        /// <summary>
        /// Список объектов 'TEC'
        /// </summary>
        public volatile StatisticCommon.InitTECBase.ListTEC m_list_tec;
        /// <summary>
        /// Список 
        /// </summary>
        public volatile List<TECComponent> allTECComponents;
        /// <summary>
        /// Текущий индекс компонента из списка 'allTECComponents' (для сохранения между вызовами функций)
        /// </summary>
        public int indxTECComponents;
        /// <summary>
        /// Хранения значений дыты/времени
        /// </summary>
        public DateTime m_prevDate
            , serverTime
            , m_curDate;

        //Обрабатывать ли данные?
        public HMark m_markQueries; //CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE

        protected volatile bool using_date;

        public bool m_ignore_date;
        /// <summary>
        /// Массив меток дат/времени, имеющихся в БД
        /// элементу, соответствующему часу, устанавливается значение идентификатора записи в таблице БД
        /// 1-я размерность - тип значений (ПБР, АДМИН), 2-я - идентификаторы записей
        /// </summary>
        protected int[,] m_arHaveDates;
        /// <summary>
        /// Номер ПБР в БД
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

        public HAdmin()
        {
            m_iHavePBR_Number = -1;

            Initialize ();
        }

        protected override void Initialize () {
            base.Initialize ();

            using_date = false;
            m_ignore_date = false;
            //m_ignore_connsett_data = false;

            m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, 24];

            allTECComponents = new List<TECComponent>();

            m_curRDGValues = new RDGStruct[24];
            m_prevRDGValues = new RDGStruct[24];

            //for (int i = 0; i < 24; i++)
            //{
            //    m_curRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //    m_prevRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //}
        }

        /// <summary>
        /// Удалить ТЭЦ из списка по идентификатору
        /// </summary>
        /// <param name="id_tec">идентификатор ТЭЦ</param>
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

        private void initQueries(HMark markQueries)
        {
            if (m_markQueries == null)
                m_markQueries = markQueries;
            else
                m_markQueries.Add(markQueries);
        }
        
        public virtual void InitTEC(List <StatisticCommon.TEC> listTEC, HMark markQueries)
        {
            this.m_list_tec = new InitTECBase.ListTEC ();
            foreach (TEC t in listTEC)
            {
                //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id))
                    this.m_list_tec.Add (t);
                //else ;
            }

            initQueries(markQueries);
            initTEC();
        }

        public void InitTEC(int idListener, FormChangeMode.MODE_TECCOMPONENT mode, HMark markQueries)
        {
            //Logging.Logg().Debug("Admin::InitTEC () - вход...");

            //m_ignore_connsett_data = ! bUseData;

            if (!(idListener < 0))
                if (mode == FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                    this.m_list_tec = new InitTEC(idListener, false).tec;                    
                else
                    this.m_list_tec = new InitTEC(idListener, (short)mode, false).tec;
                    
            else
                this.m_list_tec = new InitTECBase.ListTEC ();

            initQueries(markQueries);
            initTEC();
        }

        private void initTEC()
        {
            //comboBoxTecComponent.Items.Clear ();
            allTECComponents.Clear();

            foreach (StatisticCommon.TEC t in this.m_list_tec)
            {
                //Logging.Logg().Debug("Admin::InitTEC () - формирование компонентов для ТЭЦ:" + t.name);

                if (t.list_TECComponents.Count > 0)
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        //comboBoxTecComponent.Items.Add(t.name + " - " + g.name);
                        allTECComponents.Add(g);
                    }
                else
                {
                    //comboBoxTecComponent.Items.Add(t.name);
                    allTECComponents.Add(t.list_TECComponents[0]);
                }
            }

            /*if (! (fillTECComponent == null))
                fillTECComponent ();
            else
                ;*/
        }

        protected static bool CheckNameFieldsOfTable(DataTable tbl, string[] nameFields)
        {
            bool bRes = true;

            foreach (string nameField in nameFields)
            {
                if (tbl.Columns.IndexOf(nameField) < 0)
                {
                    bRes = false;

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

                        if ((bInitSensorsStrings == true) && (t.m_bSensorsStrings == false))
                            t.InitSensorsTEC();
                        else
                            ;
                    }
                    else
                        //Вообще нельзя что-либо инициализировать
                        Logging.Logg().Error(@"HAdmin::StartDbInterfaces () - connSetts == null ...", Logging.INDEX_MESSAGE.NOT_SET);
                } //foreach...
            }
            else
                //Вообще нельзя что-либо инициализировать
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
        //        //Вообще нельзя что-либо инициализировать
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

        public void SetDelegateSaveComplete(DelegateFunc f) 
        {            
            saveComplete = f;

            //Logging.Logg().Debug(@"HAdmin::SetDelegateSaveComplete () - saveComplete is set=" + (saveComplete == null ? false.ToString() : true.ToString()) + @" - вЫход", Logging.INDEX_MESSAGE.NOT_SET);
        }

        public void SetDelegateData(DelegateDateFunc s, DelegateFunc e) { readyData = s; errorData = e; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunc f) { setDatetime = f; }
 
        public override void ClearValues()
        {
            int cntHours = 24;

            //if (cnt < 0) {
                //Проверка признака "обычного" размера массива
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

        public abstract void GetRDGValues(int indx, DateTime date);

        protected abstract void GetPPBRDatesRequest(DateTime date);

        protected abstract int GetPPBRDatesResponse(DataTable table, DateTime date);

        protected abstract void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date);

        protected abstract int GetPPBRValuesResponse(DataTable table, DateTime date);        

        protected virtual void ClearDates(CONN_SETT_TYPE type)
        {
            int i = 1
                , cntHours = 24
                , length = m_arHaveDates.Length / m_arHaveDates.Rank;

            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
                if (length > 24)
                    m_arHaveDates = null;
                else
                    ;
            else
                if (length < 25)
                {
                    m_arHaveDates = null;
                    cntHours = 25;
                }
                else
                    ;

            if (m_arHaveDates == null)
                m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, cntHours];
            else
                ;

            for (i = 0; i < cntHours; i++)
            {
                m_arHaveDates[(int)type, i] = 0; //false;
            }
        }

        protected void ClearPPBRDates()
        {
            ClearDates(CONN_SETT_TYPE.PBR);
        }

        public TECComponent FindTECComponent(int id)
        {
            foreach (TECComponent tc in allTECComponents)
            {
                if (tc.m_id == id)
                    return tc;
                else ;
            }

            return null;
        }

        protected string getNamePBRNumber (int hour = -1) {
            return @"ПБР" + getPBRNumber (hour);
        }

        public int GetPBRNumber(int indx = -1)
        {
            int iRes = -1
                , iIndx = indx;

            if (iIndx < 0)
                iIndx = m_curRDGValues.Length - 1;
            else
                ;

            if (m_curDate.Date.CompareTo(serverTime.Date) == 0)
                if ((!(m_curRDGValues == null))
                    && (!(m_curRDGValues[iIndx].pbr_number == null))
                    && (m_curRDGValues[iIndx].pbr_number.Length > @"ПБР".Length))
                    if (Int32.TryParse(m_curRDGValues[iIndx].pbr_number.Substring(@"ПБР".Length), out iRes) == false)
                        iRes = getPBRNumber();
                    else
                        ;
                else
                    iRes = getPBRNumber();
            else
                if (m_curDate.Date.CompareTo(serverTime.Date) > 0)
                    if ((!(m_curRDGValues == null))
                        && (!(m_curRDGValues[iIndx].pbr_number == null))
                        && (m_curRDGValues[iIndx].pbr_number.Length > @"ПБР".Length))
                        if (Int32.TryParse(m_curRDGValues[iIndx].pbr_number.Substring(@"ПБР".Length), out iRes) == false)
                            iRes = 0; //Предварительный ПБР
                        else
                            ;
                    else
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
                    iHour = HDateTime.ToMoscowTimeZone (DateTime.Now).Hour;
                else
                    iHour = serverTime.Hour;
            }
            else
                ;

            if ((iHour % 2) > 0)
                iRes = iHour;
            else
                iRes = iHour + 1;

            return iRes;
        }

        public override void ClearStates()
        {
            //lock (m_lockState)
            //{
                base.ClearStates();
            //}
        }

        public FormChangeMode.MODE_TECCOMPONENT modeTECComponent(int indx)
        {
            FormChangeMode.MODE_TECCOMPONENT modeRes = FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;
            ////Вариант №1
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
            //Вариант №2
            modeRes = TECComponent.Mode(allTECComponents[indx].m_id);

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
                serverTime = (DateTime)table.Rows[0][0];
            }
            else
            {
                DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
                int timezone_offset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc;
                if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight) == true)
                    timezone_offset++;
                else
                    ;

                //serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);
                //serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(timezone_offset);
                serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(allTECComponents[indxTECComponents].tec.m_timezone_offset_msc);

                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
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

        protected bool IsCanUseTECComponents()
        {
            //bool bRes = false;
            return (!(indxTECComponents < 0)) && (indxTECComponents < allTECComponents.Count);
            //return bRes;
        }

        //public virtual void AbortThreadRDGValues(INDEX_WAITHANDLE_REASON reason)
        //{
        //    abortThreadGetValues(reason);
        //}

        protected bool IsHaveDates(CONN_SETT_TYPE type, int indx)
        {
            return m_arHaveDates[(int)type, indx] > 0 ? true : false;
        }

        public static int GetSeasonHourOffset(DateTime dt, int h)
        {
            int iRes = 0;

            if (dt.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
            {
                //if (! (h < HAdmin.SeasonDateTime.Hour))
                if (h > HAdmin.SeasonDateTime.Hour)
                    iRes = 1;
                else
                    ;
            }
            else
            {
            }

            return iRes;
            //return HourSeason < 0 ? 0 : !(h < HourSeason) ? 1 : 0;
        }

        public int GetSeasonHourOffset(int h)
        {
            return GetSeasonHourOffset(m_curDate, h);
        }

        protected void GetSeasonHourIndex(int ssn, ref int h) //Это ссылки на ИНДЕКСЫ, НЕ на ЧАСЫ
        {
            //Проверка перехода сезонов
            if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
                if (h == HAdmin.SeasonDateTime.Hour)
                    //Проверить сезон
                    if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.WinterToSummer)
                        h++;
                    else
                        if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.SummerToWinter)
                        {
                        }
                        else
                            if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.None)
                                ;
                            else
                                ;
                else
                    if (h > HAdmin.SeasonDateTime.Hour)
                        h++;
                    else
                        ;
            else
                ;
        }

        //protected void GetSeasonHours(ref int prev_h, ref int h) //Это ссылки на ИНДЕКСЫ, НЕ на ЧАСЫ
        //{
        //    int offset = 0;
            
        //    //Проверка перехода сезонов
        //    if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
        //    {
        //        //Необходимо искать одинаковые часы
        //        if (prev_h < 0)
        //            ; //Не было ни одного предыдущего часа                                
        //        else
        //        {
        //            if (prev_h == h)
        //            {
        //                //Найден одинаковый
        //                offset++;
        //            }
        //            else
        //            {
        //                if (prev_h < h)
        //                    //Норма
        //                    //if (HAdmin.SeasonDateTime.Hour < h)
        //                    if (! (HAdmin.SeasonDateTime.Hour > h))
        //                        offset ++;
        //                    else
        //                        ;
        //                else
        //                    ; //Ошибка ???
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //Оставить "как есть"
        //    }

        //    prev_h = h; //Запомнить текущий
        //    h += offset;
        //}

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
