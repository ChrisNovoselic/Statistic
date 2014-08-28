using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using StatisticCommon;

namespace Statistic
{
    public class TecView : HAdmin
    {
        public enum TYPE_PANEL { VIEW, CUR_POWER, LAST_MINUTES, ADMIN_ALARM, COUNT_TYPE_PANEL };
        TYPE_PANEL m_typePanel;

        /* Из PanelTecViewBase....
        protected enum StatesMachine
        {
            Init,
            CurrentTime,
            CurrentHours_Fact,
            CurrentMins_Fact,
            Current_TM,
            LastMinutes_TM,
            RetroHours,
            RetroMins,
            PBRValues,
            AdminValues,
        }*/
        
        protected enum StatesMachine
        {
            InitSensors,
            CurrentTimeAdmin,
            CurrentTimeView,
            CurrentHours_Fact,
            CurrentMins_Fact,
            Current_TM_Gen,
            Current_TM_SN,
            LastMinutes_TM,
            RetroHours,
            RetroMins,
            AdminDates, //Получение списка сохранённых часовых значений
            AdminValues, //Получение административных данных
            PPBRDates,
            PPBRValues,
        }

        public DelegateFunc setDatetimeView;
        public DelegateIntIntFunc updateGUI_Fact;
        public DelegateFunc updateGUI_TM_Gen
                            , updateGUI_TM_SN
                            , updateGUI_LastMinutes;

        //private volatile string sensorsString_TM;
        //private List<TG> m_listSensorId2TG;

        public double m_dblTotalPower_TM_SN;
        public DateTime m_dtLastChangedAt_TM_Gen;
        public DateTime m_dtLastChangedAt_TM_SN;

        private AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected DataTable m_tablePPBRValuesResponse
                    //, m_tableRDGExcelValuesResponse
                    ;

        public enum seasonJumpE
        {
            None,
            WinterToSummer,
            SummerToWinter,
        }

        public abstract class values
        {
            public volatile double[] valuesLastMinutesTM;

            public volatile double[] valuesPBR;
            public volatile bool[] valuesForeignCommand;
            public volatile double[] valuesPmin;
            public volatile double[] valuesPmax;
            public volatile double[] valuesPBRe;
            public volatile double[] valuesUDGe;

            public volatile double[] valuesDiviation; //Значение в ед.изм.

            public volatile double[] valuesREC;

            public values(int sz)
            {
                valuesLastMinutesTM = new double[sz];

                valuesPBR = new double[sz];
                valuesForeignCommand = new bool[sz];
                valuesPmin = new double[sz];
                valuesPmax = new double[sz];
                valuesPBRe = new double[sz];
                valuesUDGe = new double[sz];
                valuesDiviation = new double[sz];

                valuesREC = new double[sz];
            }
        }

        public class valuesTECComponent : values
        {
            //public volatile double[] valuesREC;
            public volatile double[] valuesISPER; //Признак ед.изм. 'valuesDIV'
            public volatile double[] valuesDIV; //Значение из БД

            public valuesTECComponent(int sz)
                : base(sz)
            {
                //valuesREC = new double[sz];
                valuesISPER = new double[sz];
                valuesDIV = new double[sz];
            }
        }

        public class valuesTEC : values
        {
            public volatile double[] valuesFact;

            public double valuesFactAddon;
            public double valuesPBRAddon;
            public double valuesPBReAddon;
            public double valuesUDGeAddon;
            public double valuesDiviationAddon;
            public int hourAddon;
            public seasonJumpE season;
            public bool addonValues;

            public valuesTEC(int sz)
                : base(sz)
            {
                valuesFact = new double[sz];

                valuesFactAddon = 0.0;
                valuesPBRAddon = 0.0;
                valuesPBReAddon = 0.0;
                valuesUDGeAddon = 0.0;
                valuesDiviationAddon = 0.0;
                hourAddon = 0;
                season = seasonJumpE.None;
                addonValues = false;
            }
        }

        public object m_lockValue;
        
        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool currHour;
        public volatile int lastHour;
        public volatile int lastReceivedHour;
        public volatile int lastMin;
        public volatile bool lastMinError;
        public volatile bool lastHourError;
        public volatile bool lastHourHalfError;
        public volatile string lastLayout;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public valuesTEC m_valuesMins;
        public valuesTEC m_valuesHours;
        //public DateTime selectedTime;
        //public DateTime serverTime;

        public volatile int m_indx_TEC;
        //public volatile int m_indx_TECComponent;
        public List <TECComponentBase> m_localTECComponents;
        public Dictionary <int, TecView.valuesTECComponent> m_dictValuesTECComponent;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public double recomendation;

        //private bool started;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool adminValuesReceived;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool recalcAver;

        public StatisticCommon.TEC m_tec {
            get { return m_list_tec [0]; }
        }

        public List<TG> listTG
        {
            get
            {
                if (indxTECComponents < 0)
                    return m_tec.m_listTG;
                else
                    return m_tec.list_TECComponents[indxTECComponents].m_listTG;
            }
        }

        public int CountTG
        {
            get
            {
                if (indxTECComponents < 0)
                    return m_tec.m_listTG.Count;
                else
                    return allTECComponents[indxTECComponents].m_listTG.Count;
            }
        }

        protected override void Initialize () {
            base.Initialize ();

            currHour = true;
            lastHour = 0;
            lastMin = 0;

            recalcAver = true;

            m_lockValue = new object();

            m_valuesMins = new valuesTEC(21);
            m_valuesHours = new valuesTEC(24);

            m_localTECComponents = new List<TECComponentBase>();
            //tgsName = new List<System.Windows.Forms.Label>();

            m_dictValuesTECComponent = new Dictionary<int, TecView.valuesTECComponent>();

            ClearStates();
        }

        public void InitializeTECComponents () {
            //int positionXName = 515, positionXValue = 504, positionYName = 6, positionYValue = 19;
            //countTG = 0;
            List<int> tg_ids = new List<int>(); //Временный список идентификаторов ТГ

            if (indxTECComponents < 0) // значит этот view будет суммарным для всех ГТП
            {
                m_dictValuesTECComponent = new Dictionary<int,valuesTECComponent> ();
                
                foreach (TECComponent c in m_tec.list_TECComponents)
                {
                    if ((c.m_id > 100) && (c.m_id < 500)) {
                        m_localTECComponents.Add(c);

                        //m_dictValuesTECComponent.Add (c.m_id, new valuesTECComponent(25));
                    }
                    else
                        ;

                    //foreach (TG tg in g.m_listTG)
                    //{
                    //    //Проверка обработки текущего ТГ
                    //    if (tg_ids.IndexOf(tg.m_id) == -1)
                    //    {
                    //        tg_ids.Add(tg.m_id); //Запомнить, что ТГ обработан
                    //    
                    //        //positionYValue = 19;
                    //        m_pnlQuickData.addTGView(ref tg.name_shr);
                    //    }
                    //    else
                    //        ;
                    //}
                }
            }
            else
            {
                foreach (TG tg in m_tec.list_TECComponents[indxTECComponents].m_listTG)
                {
                    //tg_ids.Add(tg.m_id); //Добавить без проверки

                    ////positionYValue = 19;
                    ////addTGView(ref tg.name_shr, ref positionXName, ref positionYName, ref positionXValue, ref positionYValue);
                    //m_pnlQuickData.addTGView(ref tg.name_shr);

                    foreach (TECComponent c in m_tec.list_TECComponents)
                    {
                        if (tg.m_id == c.m_id)
                            m_localTECComponents.Add(c);
                        else
                            ;
                    }
                }
            }

            foreach (TECComponent c in m_localTECComponents)
            {
                m_dictValuesTECComponent.Add(c.m_id, new TecView.valuesTECComponent(24 + 1));
            }
        }

        public TecView(bool[] arMarkSavePPBRValues, TYPE_PANEL type, int indx_tec, int indx_comp)
            : base()
        {
            m_typePanel = type;

            m_indx_TEC = indx_tec;
            indxTECComponents = indx_comp;
        }

        public event DelegateIntIntFunc EventAlarmCurPower, EventAlarmTGTurnOnOff;
        
        public override void GetRDGValues(int mode, int indx, DateTime date)
        {
            m_prevDate = m_curDate;
            m_curDate = date.Date;

            newState = true;
            states.Clear();

            if ((m_tec.m_bSensorsStrings == false))
            {
                states.Add((int)StatesMachine.InitSensors);
            }
            else ;

            states.Add((int)StatesMachine.Current_TM_Gen);
            states.Add((int)StatesMachine.CurrentHours_Fact);
            states.Add((int)StatesMachine.CurrentMins_Fact);
            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
        }

        private void getRDGValues () {
            GetRDGValues((int)s_typeFields, indxTECComponents, DateTime.Now);

            try
            {
                semaState.Release(1);
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, @"TecView::ChangeState () - semaState.Release (1) - ...");
            }
        }

        public void SuccessThreadRDGValues(int curHour, int curMinute)
        {
            double power_TM = 0.0;

            Console.WriteLine (@"curHour=" + curHour.ToString () + @"; curMinute=" + curMinute.ToString ());

            foreach (TG tg in allTECComponents[indxTECComponents].m_listTG)
            {
                Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + tg.power_TM);

                if (tg.power_TM < 1) {
                    EventAlarmTGTurnOnOff (indxTECComponents, tg.m_id);
                }
                else
                    power_TM += tg.power_TM;

                if (allTECComponents[indxTECComponents].m_listTG.IndexOf(tg) < allTECComponents[indxTECComponents].m_listTG.Count)
                    Console.Write(@", ");
                else
                    ;
            }

            if (Math.Abs(power_TM - m_valuesHours.valuesUDGe[curHour]) > m_valuesHours.valuesUDGe[curHour] * ((double)allTECComponents[indxTECComponents].m_dcKoeffAlarmPcur / 100))
                EventAlarmCurPower (indxTECComponents, -1);
            else
                ;

            Console.WriteLine ();

            //for (int i = 0; i < m_valuesHours.valuesFact.Length; i ++)
            //    Console.WriteLine(@"valuesFact[" + i.ToString() + @"]=" + m_valuesHours.valuesFact[i]);
        }

        private void threadGetRDGValues(object synch)
        {
            int indxEv = -1;

            foreach (TECComponent tc in allTECComponents) {
                if ((tc.m_id > 100) && (tc.m_id < 500)) {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0) {
                        indxTECComponents = allTECComponents.IndexOf (tc);
                        
                        getRDGValues();
                    }
                    else
                        break;
                }
                else
                    ; //Это не ГТП
            }
        }

        public void ChangeState_AdminAlarm () {
            new Thread(new ParameterizedThreadStart(threadGetRDGValues)).Start();
        }

        public override void Activate(bool active)
        {
            base.Activate(active);
        }

        public override void Start()
        {
            ClearValues();

            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StartDbInterfaces(CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
                }
            else
                Logging.Logg().LogErrorToFile(@"TecView::Start () - m_list_tec == null");
            
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StopDbInterfaces();
                }
            else
                Logging.Logg().LogErrorToFile(@"TecView::Stop () - m_list_tec == null");
        }

        public override void InitTEC(List<StatisticCommon.TEC> listTEC)
        {
            base.InitTEC(listTEC);

            InitializeTECComponents ();
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            return bRes;
        }

        public override void  ClearValues()
        {
            ClearValuesMins();
            ClearValuesHours();
        }

        public void ClearValuesLastMinutesTM()
        {
            for (int i = 0; i < 24; i++) {
                m_valuesHours.valuesLastMinutesTM [i] = 0.0;
                
                if ((i + 1) < 24)
                    foreach (TECComponent g in m_localTECComponents)
                        m_dictValuesTECComponent[g.m_id].valuesLastMinutesTM[i] = 0.0;
                else
                    foreach (TECComponent g in m_localTECComponents)
                        m_dictValuesTECComponent[g.m_id].valuesLastMinutesTM[i + 1] = 0.0;
            }
        }

        public override void CopyCurToPrevRDGValues()
        {
        }

        public override void getCurRDGValues(HAdmin source)
        {
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents() == true)
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request (m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetPBRDatesQuery(date, s_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        protected override void GetPPBRValuesRequest(StatisticCommon.TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
        }

        protected override bool GetPPBRDatesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool GetPPBRValuesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        private bool GetSensorsTEC()
        {
            if (m_tec.m_bSensorsStrings == false) {
                m_tec.InitSensorsTEC ();
            }
            else
                ;

            return m_tec.m_bSensorsStrings;
        }

        private void GetCurrentTMGenRequest()
        {
            //Request(allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], allTECComponents[indxTECComponents].tec.currentTMRequest(sensorsString_TM));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMRequest(m_tec.GetSensorsString (-1, CONN_SETT_TYPE.DATA_SOTIASSO)));
        }

        private bool GetCurrentTMGenResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1,
                id = -1;
            double value = -1;
            DateTime dtLastChangedAt =
                m_dtLastChangedAt_TM_Gen = DateTime.Now;
            TG tgTmp;

            foreach (TECComponent g in m_tec.list_TECComponents)
            {
                foreach (TG t in g.m_listTG)
                {
                    t.power_TM = 0;
                }
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                    return false;
                else
                    ;

                tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                if (tgTmp == null)
                    return false;
                else
                    ;

                if (!(table.Rows[i]["value"] is DBNull))
                    if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                        return false;
                    else
                        ;
                else
                    value = 0.0;

                if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                    return false;
                else
                    ;

                if (m_dtLastChangedAt_TM_Gen > dtLastChangedAt)
                    m_dtLastChangedAt_TM_Gen = dtLastChangedAt;
                else
                    ;

                switch (m_tec.type())
                {
                    case StatisticCommon.TEC.TEC_TYPE.COMMON:
                        break;
                    case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                        //value *= 20;
                        break;
                    default:
                        break;
                }

                tgTmp.power_TM = value;
            }

            try { m_dtLastChangedAt_TM_Gen = HAdmin.ToCurrentTimeZone(m_dtLastChangedAt_TM_Gen); }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, @"TecView::GetCurrentTMGenResponse () - HAdmin.ToCurrentTimeZone () - ...");
            }

            return bRes;
        }

        private void GetCurrentTMSNRequest()
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
            Request(m_tec.m_arIdListeners [(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMSNRequest());
        }

        private bool GetCurrentTMSNResponse(DataTable table)
        {
            bool bRes = true;
            int id = -1;

            m_dtLastChangedAt_TM_SN = DateTime.Now;

            if (table.Rows.Count == 1)
            {
                if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
                    return false;
                else
                    ;

                if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
                    if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_dblTotalPower_TM_SN) == false)
                        return false;
                    else
                        ;
                else
                    m_dblTotalPower_TM_SN = 0.0;

                if ((!(m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_dtLastChangedAt_TM_SN) == false))
                    return false;
                else
                    ;
            }
            else
            {
                bRes = false;
            }

            return bRes;
        }
            
        protected override bool StateCheckResponse(int state, out bool error, out System.Data.DataTable table)
        {
            bool bRes = true;
            
            error = false;
            table = null;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    //Ответ без запроса к БД в 'StateResponse'
                    //Запрос не требуется, т.к. сведеения получены при вызове 'InitTEC'
                    break;
                case (int)StatesMachine.CurrentTimeAdmin:
                case (int)StatesMachine.CurrentTimeView:
                case (int)StatesMachine.CurrentHours_Fact:
                case (int)StatesMachine.CurrentMins_Fact:
                case (int)StatesMachine.Current_TM_Gen:
                case (int)StatesMachine.Current_TM_SN:
                case (int)StatesMachine.LastMinutes_TM:
                case (int)StatesMachine.RetroHours:
                case (int)StatesMachine.RetroMins:
                case (int)StatesMachine.PPBRDates:
                case (int)StatesMachine.PPBRValues:
                case (int)StatesMachine.AdminDates:
                case (int)StatesMachine.AdminValues:
                    //bRes = Response(m_IdListenerCurrent, out error, out table);
                    bRes = Response(out error, out table);
                    break;
                default:
                    bRes = false;
                    break;
            }

            error = !bRes;

            return bRes;
        }

        protected override void StateErrors(int state, bool response)
        {
            string reason = string.Empty,
                    waiting = string.Empty,
                    msg = string.Empty;
            
            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    reason = @"получения идентификаторов датчиков";
                    waiting = @"Переход в ожидание";
                    AbortThreadRDGValues ();
                    break;
                case (int)StatesMachine.CurrentTimeAdmin:
                case (int)StatesMachine.CurrentTimeView:
                    if (response == true)
                    {
                        reason = @"разбора";
                    }
                    else
                    {
                        reason = @"получения";
                    }

                    reason += @" текущего времени сервера";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.CurrentHours_Fact:
                    reason = @"получасовых значений";
                    waiting = @"Ожидание " + FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    reason = @"3-х минутных значений";
                    waiting = @"Ожидание " + FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    reason = @"текущих значений (генерация)";
                    waiting = @"Ожидание " + FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    AbortThreadRDGValues();
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    reason = @"текущих значений (собств. нужды)";
                    waiting = @"Ожидание " + FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    reason = @"текущих значений 59 мин.";
                    waiting = @"Ожидание " + FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case (int)StatesMachine.RetroHours:
                    reason = @"получасовых значений";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.RetroMins:
                    reason = @"3-х минутных значений";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (response == true)
                    {
                        reason = @"разбора";
                    }
                    else
                    {
                        reason = @"получения";
                    }

                    reason += @" сохранённых часовых значений (PPBR)";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.PPBRValues:
                    reason = @"данных плана";
                    AbortThreadRDGValues();
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    reason = @"административных значений";
                    AbortThreadRDGValues();
                    break;
                default:
                    msg = @"Неизвестная команда...";
                    break;
            }

            if (response)
                reason = @"разбора " + reason;
            else
                reason = @"получения " + reason;

            msg = m_tec.name_shr;

            if (waiting.Equals(string.Empty) == true)
                msg += ". Ошибка " + reason + ". " + waiting + ".";
            else
                msg += ". Ошибка " + reason + ".";

            ErrorReport(msg);

            Logging.Logg().LogErrorToFile(@"TecView::StateErrors () - ошибка " + reason + @". " + waiting + @". ");
        }

        protected override bool StateRequest(int state)
        {
            bool bRes = true;

            string msg = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    msg = @"идентификаторов датчиков";
                    //Запрос не требуется...
                    break;
                case (int)StatesMachine.CurrentTimeAdmin:
                case (int)StatesMachine.CurrentTimeView:
                    msg = @"текущего времени сервера";
                    GetCurrentTimeRequest();
                    break;
                case (int)StatesMachine.CurrentHours_Fact:
                    msg = @"получасовых значений";
                    adminValuesReceived = false;
                    GetHoursRequest(m_curDate.Date);
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    msg = @"трёхминутных значений";
                    adminValuesReceived = false;
                    GetMinsRequest(lastHour);
                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    msg = @"текущих значений (генерация)";
                    GetCurrentTMGenRequest ();
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    msg = @"текущих значений (собств. нужды)";
                    GetCurrentTMSNRequest();
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    msg = @"текущих значений 59 мин";
                    GetLastMinutesTMRequest(m_curDate);
                    break;
                case (int)StatesMachine.RetroHours:
                    msg = @"получасовых значений";
                    adminValuesReceived = false;
                    GetHoursRequest(m_curDate.Date);
                    break;
                case (int)StatesMachine.RetroMins:
                    msg = @"трёхминутных значений";
                    adminValuesReceived = false;
                    GetMinsRequest(lastHour);
                    break;
                case (int)StatesMachine.PPBRDates:
                    msg = @"списка сохранённых часовых значений";
                    GetPPBRDatesRequest(m_curDate);
                    break;
                case (int)StatesMachine.PPBRValues:
                    msg = @"данных плана";
                    GetPPBRValuesRequest();
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    msg = @"административных данных";
                    adminValuesReceived = false;
                    GetAdminValuesRequest(s_typeFields);
                    break;
                default:
                    bRes = false;
                    break;
            }

            ActionReport (@"Получение " + msg + @".");

            return bRes;
        }

        protected override bool StateResponse(int state, System.Data.DataTable table)
        {
            bool result = true;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    switch (m_tec.type())
                        {
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                                result = GetSensorsTEC();
                                break;
                            default:
                                break;
                        }
                        if (result == true)
                        {
                        }
                        else
                            ;
                    break;
                case (int)StatesMachine.CurrentTimeAdmin:
                    result = GetCurrentTimeAdminResponse(table);
                    if (result == true)
                    {
                        if (using_date == true) {
                            m_prevDate = serverTime.Date;
                            m_curDate = m_prevDate;

                            if (!(setDatetime == null)) setDatetime(m_curDate); else;
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.CurrentTimeView:
                    result = GetCurrentTimeViewResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        m_curDate = m_curDate.AddSeconds(-1 * Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]));
                        //this.BeginInvoke(delegateSetNowDate, true);
                        setDatetimeView ();
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.CurrentHours_Fact:
                    ClearValues();
                    //GenerateHoursTable(seasonJumpE.SummerToWinter, 3, table);
                    result = GetHoursResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    result = GetMinsResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Current_TM_Gen:
                    result = GetCurrentTMGenResponse(table);
                    if (result == true)
                    {
                        if (!(updateGUI_TM_Gen == null)) updateGUI_TM_Gen(); else ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Current_TM_SN:
                    result = GetCurrentTMSNResponse(table);
                    if (result == true)
                    {
                        updateGUI_TM_SN ();
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    ClearValuesLastMinutesTM ();
                    result = GetLastMinutesTMResponse(table, m_curDate);
                    if (result == true)
                    {
                        if (! (updateGUI_LastMinutes == null))
                            updateGUI_LastMinutes();
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.RetroHours:
                    ClearValues();
                    result = GetHoursResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.RetroMins:
                    result = GetMinsResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                        updateGUI_Fact(lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, m_curDate);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    ClearPBRValues();
                    result = GetPPBRValuesResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    ClearAdminValues();
                    result = GetAdminValuesResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.AdminValues");
                        ComputeRecomendation(lastHour);
                        adminValuesReceived = true;
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                        if (! (updateGUI_Fact == null)) updateGUI_Fact(lastHour, lastMin); else ;
                    }
                    else
                        ;
                    break;
                default:
                    result = false;
                    break;
            }

            if (result == true)
                FormMainBaseWithStatusStrip.m_report.ClearStates ();
            else
                ;

            Logging.Logg().LogDebugToFile(@"TecView::StateResponse () - TECname=" + m_tec.name_shr + @", state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...");

            return result;
        }

        private void ChangeState_CurPower () {
            lock (m_lockObj) {
                newState = true;
                states.Clear();

                if (m_tec.m_bSensorsStrings == false)
                    states.Add((int)StatesMachine.InitSensors);
                else ;

                states.Add((int)TecView.StatesMachine.Current_TM_Gen);
                states.Add((int)TecView.StatesMachine.Current_TM_SN);                
            }
        }

        private void ChangeState_LastMinutes () {
            newState = true;
            states.Clear();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
            states.Add((int)StatesMachine.LastMinutes_TM);
        }

        private void ChangeState_View () {
            newState = true;
            states.Clear();

            if (m_tec.m_bSensorsStrings == true)
            {
                if (currHour == true)
                {
                    states.Add((int)StatesMachine.CurrentTimeView);
                }
                else
                {
                    //selectedTime = m_pnlQuickData.dtprDate.Value.Date;
                }
            }
            else
            {
                states.Add((int)StatesMachine.InitSensors);
                states.Add((int)StatesMachine.CurrentTimeView);
            }

            states.Add((int)StatesMachine.CurrentHours_Fact);
            states.Add((int)StatesMachine.CurrentMins_Fact);
            states.Add((int)StatesMachine.Current_TM_Gen);
            states.Add((int)StatesMachine.LastMinutes_TM);
            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
        }

        private void ChangeState_TMSNPower () {
            newState = true;
            states.Clear();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            states.Add((int)StatesMachine.Current_TM_Gen);
            states.Add((int)StatesMachine.Current_TM_SN);
        }

        public void ChangeState()
        {
            lock (m_lockValue)
            {
                switch (m_typePanel)
                {
                    case TecView.TYPE_PANEL.VIEW:
                        ChangeState_View ();
                        break;
                    case TecView.TYPE_PANEL.CUR_POWER:
                        ChangeState_CurPower ();
                        break;
                    case TecView.TYPE_PANEL.LAST_MINUTES:
                        ChangeState_LastMinutes ();
                        break;
                    case TecView.TYPE_PANEL.ADMIN_ALARM:
                        ChangeState_AdminAlarm();
                        break;
                    default:
                        break;
                }

                if (! (m_typePanel == TecView.TYPE_PANEL.ADMIN_ALARM))
                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, @"TecView::ChangeState () - semaState.Release(1)...");
                    }
                else
                    ;
            }
        }

        protected void GetCurrentTimeRequest()
        {
            if (IsCanUseTECComponents())
            {
                GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(allTECComponents[indxTECComponents].tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port),
                                    allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN]);
            }
            else
                GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port),
                                    m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN]);
        }

        private bool GetCurrentTimeAdminResponse(DataTable table)
        {
            return GetCurrentTimeResponse(table);
        }

        private bool GetCurrentTimeViewResponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                try
                {
                    m_curDate = (DateTime)table.Rows[0][0];
                    serverTime = m_curDate;
                }
                catch (Exception excpt)
                {
                    Logging.Logg().LogExceptionToFile(excpt, "TecView::GetCurrentTimeViewReponse () - (DateTime)table.Rows[0][0]");

                    return false;
                }
            }
            else
            {
                //selectedTime = System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
                //ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
                return false;
            }

            return true;
        }

        public void zedGraphHours_MouseUpEvent (int indx) {
            lock (m_lockValue)
            {
                currHour = false;
                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (indx <= m_valuesHours.hourAddon)
                    {
                        lastHour = indx;
                        m_valuesHours.addonValues = false;
                    }
                    else
                    {
                        if (indx == m_valuesHours.hourAddon + 1)
                        {
                            lastHour = indx - 1;
                            m_valuesHours.addonValues = true;
                        }
                        else
                        {
                            lastHour = indx - 1;
                            m_valuesHours.addonValues = false;
                        }
                    }
                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (indx < m_valuesHours.hourAddon)
                            lastHour = indx;
                        else
                            lastHour = indx + 1;
                    }
                    else
                        lastHour = indx;
                ClearValuesMins();

                newState = true;
                states.Clear();
                states.Add((int)StatesMachine.RetroMins);
                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - zedGraphHours_MouseUpEvent () - sem.Release(1)"); }

            }
        }

        protected void ClearValuesMins()
        {
            for (int i = 0; i < 21; i++)
                m_valuesMins.valuesFact[i] =
                m_valuesMins.valuesDiviation[i] =
                m_valuesMins.valuesPBR[i] =
                m_valuesMins.valuesPBRe[i] =
                m_valuesMins.valuesUDGe[i] = 0;
        }

        protected void ClearValuesHours()
        {
            for (int i = 0; i < 24; i++)
            {
                m_valuesHours.valuesFact[i] =
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPmin[i] =
                m_valuesHours.valuesPmax[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;

                m_valuesHours.valuesForeignCommand[i] = true;
            }

            m_valuesHours.valuesFactAddon =
            m_valuesHours.valuesDiviationAddon =
            m_valuesHours.valuesPBRAddon =
            m_valuesHours.valuesPBReAddon =
            m_valuesHours.valuesUDGeAddon = 0;
            m_valuesHours.season = seasonJumpE.None;
            m_valuesHours.hourAddon = 0;
            m_valuesHours.addonValues = false;
        }

        private void ClearPBRValues()
        {
        }

        private void ClearAdminValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPmin[i] =
                m_valuesHours.valuesPmax[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;

                m_valuesHours.valuesForeignCommand[i] = true;

                if (i + 1 < 24) {
                    for (int j = 0; j < m_localTECComponents.Count; j ++) {
                        m_dictValuesTECComponent [m_localTECComponents [j].m_id].valuesDiviation [i] = 
                        m_dictValuesTECComponent [m_localTECComponents [j].m_id].valuesPBR [i] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[i] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[i] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBRe[i] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesUDGe[i] = 0.0;

                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesForeignCommand[i] = true;
                    }
                }
                else {
                    for (int j = 0; j < m_localTECComponents.Count; j++)
                    {
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesDiviation[i + 1] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[i + 1] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[i + 1] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[i + 1] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBRe[i + 1] =
                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesUDGe[i + 1] = 0.0;

                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesForeignCommand[i + 1] = true;
                    }
                }
            }

            m_valuesHours.valuesDiviationAddon =
            m_valuesHours.valuesPBRAddon =
            m_valuesHours.valuesPBReAddon =
            m_valuesHours.valuesUDGeAddon = 0;

            for (int i = 0; i < 21; i++)
                m_valuesMins.valuesDiviation[i] =
                m_valuesMins.valuesPBR[i] =
                m_valuesMins.valuesPBRe[i] =
                m_valuesMins.valuesUDGe[i] = 0;
        }

        private bool GetAdminValuesResponse(DataTable table_in)
        {
            DateTime date = m_curDate //m_pnlQuickData.dtprDate.Value.Date
                    , dtPBR;
            int hour;

            double currPBRe;
            int offsetPrev = -1
                //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePPBRValuesResponse.Rows.Count
                , i = -1, j = -1,
                offsetUDG, offsetPlan, offsetLayout;

            lastLayout = "---";

            //switch (tec.type ()) {
            //    case TEC.TEC_TYPE.COMMON:
            //        offsetPrev = -1;

            if ((indxTECComponents < 0) || ((!(indxTECComponents < 0)) && (m_tec.list_TECComponents[indxTECComponents].m_id > 500)))
            {
                //double[,] valuesPBR = new double[/*tec.list_TECComponents.Count*/m_localTECComponents.Count, 25];
                //double[,] valuesPmin = new double[m_localTECComponents.Count, 25];
                //double[,] valuesPmax = new double[m_localTECComponents.Count, 25];
                //double[,] valuesREC = new double[m_localTECComponents.Count, 25];
                //int[,] valuesISPER = new int[m_localTECComponents.Count, 25];
                //double[,] valuesDIV = new double[m_localTECComponents.Count, 25];

                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                m_tablePPBRValuesResponse = restruct_table_pbrValues(m_tablePPBRValuesResponse, m_tec.list_TECComponents, indxTECComponents);
                offsetLayout = (!(m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_localTECComponents.Count * 3) : m_tablePPBRValuesResponse.Columns.Count;

                table_in = restruct_table_adminValues(table_in, m_tec.list_TECComponents, indxTECComponents);

                //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
                //    try { table_in.Columns.Remove("ID_COMPONENT"); }
                //    catch (Exception excpt)
                //    {
                //        /*
                //        Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecViewBase.GetAdminValuesResponse () - ...");
                //        */
                //    }
                //else
                //    ;

                // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if ((hour == 0) && (dtPBR.Day == date.Day))
                            {
                                offsetPrev = i;
                                //foreach (TECComponent g in tec.list_TECComponents)
                                for (j = 0; j < m_localTECComponents.Count; j++)
                                {
                                    if ((offsetPlan + j * 3) < m_tablePPBRValuesResponse.Columns.Count)
                                    {
                                        //valuesPBR[j, 24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3];
                                        m_dictValuesTECComponent [m_localTECComponents[j].m_id].valuesPBR[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3];
                                        //m_dictValuesTECComponent.valuesPmin[j, 24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 1];
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 1];
                                        //valuesPmax[j, 24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 2];
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 2];
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[24] = 0.0;
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[24] = 0.0;
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[24] = 0.0;
                                    }
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecViewBase.GetAdminValuesResponse () - ..."); }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                            {
                                offsetPrev = i;
                            }
                            else
                                ;
                        }
                        catch
                        {
                        }
                    }
                }

                // разбор остальных значений
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;
                    else
                        ;

                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if (hour == 0 && ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_localTECComponents.Count; j++)
                            {
                                try
                                {
                                    if ((offsetPlan + (j * 3) < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                    {
                                        //valuesPBR[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        //valuesPmin[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        //valuesPmax[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[hour - 1] = 0.0;
                                        //m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[hour - 1] = 0.0;
                                        //m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[hour - 1] = 0.0;
                                    }

                                    DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    //if (i < table_in.Rows.Count)
                                    if (row_in.Length > 0)
                                    {
                                        if (row_in.Length > 1)
                                            ; //Ошибка....
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            //valuesREC[j, hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                        else
                                            //valuesREC[j, hour - 1] = 0;
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = 0.0;

                                        if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesISPER[hour - 1] = (int)row_in[0][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesDIV[hour - 1] = (double)row_in[0][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = 0.0;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
                                }
                                //j++;
                            }

                            string tmp = "";
                            //if ((m_tablePPBRValuesResponse.Columns.Contains ("PBR_NUMBER")) && !(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull))
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmp = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (LayoutIsBiggerByName(lastLayout, tmp))
                                lastLayout = tmp;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_localTECComponents.Count; j++)
                            {
                                try
                                {
                                    m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[hour - 1] = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = (double)table_in.Rows[i][offsetUDG + j * 3];
                                        else
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesISPER[hour - 1] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
                                            m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesDIV[hour - 1] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[hour - 1] = 0.0;
                                    }
                                }
                                catch
                                {
                                }
                                //j++;
                            }

                            string tmp = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmp = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (LayoutIsBiggerByName(lastLayout, tmp))
                                lastLayout = tmp;
                            else
                                ;
                        }
                        catch
                        {
                        }
                    }
                }

                //for (int ii = 1; ii < 24 + 1; ii++)
                for (i = 0; i < 24; i++)
                {
                    //i = ii - 1;
                    for (j = 0; j < m_localTECComponents.Count; j++)
                    {
                        m_valuesHours.valuesPBR[i] += m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[i];
                        m_valuesHours.valuesPmin[i] += m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmin[i];
                        m_valuesHours.valuesPmax[i] += m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPmax[i];
                        if (i == 0)
                        {
                            currPBRe = (m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[i] + m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[24]) / 2;
                            m_valuesHours.valuesPBRe[i] += currPBRe;
                        }
                        else
                        {
                            currPBRe = (m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[i] + m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesPBR[i - 1]) / 2;
                            m_valuesHours.valuesPBRe[i] += currPBRe;
                        }

                        m_valuesHours.valuesREC[i] += m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[i];

                        m_valuesHours.valuesUDGe[i] += currPBRe + m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[i];

                        if (m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesISPER[i] == 1)
                            m_valuesHours.valuesDiviation[i] += (currPBRe + m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesREC[i]) * m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesDIV[i] / 100;
                        else
                            m_valuesHours.valuesDiviation[i] += m_dictValuesTECComponent[m_localTECComponents[j].m_id].valuesDIV[i];
                    }
                    /*m_valuesHours.valuesPBR[i] = 0.20;
                    m_valuesHours.valuesPBRe[i] = 0.20;
                    m_valuesHours.valuesUDGe[i] = 0.20;
                    m_valuesHours.valuesDiviation[i] = 0.05;*/
                }

                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                }
            }
            else
            {
                double[] valuesPBR = new double[25];
                double[] valuesPmin = new double[25];
                double[] valuesPmax = new double[25];
                double[] valuesREC = new double[25];
                int[] valuesISPER = new int[25];
                double[] valuesDIV = new double[25];

                offsetUDG = 1;
                offsetPlan = 1;
                offsetLayout = (!(m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? offsetPlan + 3 : m_tablePPBRValuesResponse.Columns.Count;

                // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if (hour == 0 && ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                            {
                                offsetPrev = i;
                                valuesPBR[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[24] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
                            }
                            else
                                ;
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                            {
                                offsetPrev = i;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                // разбор остальных значений
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;
                    else
                        ;

                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if ((hour == 0) && (!(dtPBR.Day == date.Day)))
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            if ((offsetPlan < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan] is System.DBNull)))
                            {
                                valuesPBR[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
                            }
                            else
                                ;

                            DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                            //if (i < table_in.Rows.Count)
                            if (row_in.Length > 0)
                            {
                                if (row_in.Length > 1)
                                    ; //Ошибка....
                                else
                                    ;

                                if (!(row_in[0][offsetUDG] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG] is System.DBNull)))
                                    valuesREC[hour - 1] = (double)row_in[0][offsetUDG + 0];
                                else
                                    valuesREC[hour - 1] = 0;

                                if (!(row_in[0][offsetUDG + 1] is System.DBNull))
                                    valuesISPER[hour - 1] = (int)row_in[0][offsetUDG + 1];
                                else
                                    ;

                                if (!(row_in[0][offsetUDG + 2] is System.DBNull))
                                    valuesDIV[hour - 1] = (double)row_in[0][offsetUDG + 2];
                                else
                                    ;
                            }
                            else
                            {
                                valuesREC[hour - 1] = 0;
                                //valuesISPER[hour - 1] = 0;
                                //valuesDIV[hour - 1] = 0;
                            }

                            string tmp = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmp = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (LayoutIsBiggerByName(lastLayout, tmp))
                                lastLayout = tmp;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, "PanelTecViewBase::GetAdminValueResponse ()...");
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            valuesPBR[hour - 1] = 0;

                            if (i < table_in.Rows.Count)
                            {
                                if (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                    valuesREC[hour - 1] = (double)table_in.Rows[i][offsetUDG + 0];
                                else
                                    valuesREC[hour - 1] = 0;

                                if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
                                    valuesISPER[hour - 1] = (int)table_in.Rows[i][offsetUDG + 1];
                                else
                                    ;

                                if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
                                    valuesDIV[hour - 1] = (double)table_in.Rows[i][offsetUDG + 2];
                                else
                                    ;
                            }
                            else
                            {
                                valuesREC[hour - 1] = 0;
                                //valuesISPER[hour - 1] = 0;
                                //valuesDIV[hour - 1] = 0;
                            }

                            string tmp = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmp = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (LayoutIsBiggerByName(lastLayout, tmp))
                                lastLayout = tmp;
                            else
                                ;
                        }
                        catch
                        {
                        }
                    }
                }

                for (i = 0; i < 24; i++)
                {

                    m_valuesHours.valuesPBR[i] = valuesPBR[i];
                    m_valuesHours.valuesPmin[i] = valuesPmin[i];
                    m_valuesHours.valuesPmax[i] = valuesPmax[i];

                    if (i == 0)
                    {
                        currPBRe = (valuesPBR[i] + valuesPBR[24]) / 2;
                        m_valuesHours.valuesPBRe[i] = currPBRe;
                    }
                    else
                    {
                        currPBRe = (valuesPBR[i] + valuesPBR[i - 1]) / 2;
                        m_valuesHours.valuesPBRe[i] = currPBRe;
                    }

                    m_valuesHours.valuesUDGe[i] = currPBRe + valuesREC[i];

                    if (valuesISPER[i] == 1)
                        m_valuesHours.valuesDiviation[i] = (currPBRe + valuesREC[i]) * valuesDIV[i] / 100;
                    else
                        m_valuesHours.valuesDiviation[i] = valuesDIV[i];
                }

                if (m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                {
                    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                }
                else
                    ;
            }

            hour = lastHour;
            if (hour == 24)
                hour = 23;

            for (i = 0; i < 21; i++)
            {
                m_valuesMins.valuesPBR[i] = m_valuesHours.valuesPBR[hour];
                m_valuesMins.valuesPBRe[i] = m_valuesHours.valuesPBRe[hour];
                m_valuesMins.valuesUDGe[i] = m_valuesHours.valuesUDGe[hour];
                m_valuesMins.valuesDiviation[i] = m_valuesHours.valuesDiviation[hour];
            }

            return true;
        }

        private void ComputeRecomendation(int hour)
        {
            if (hour == 24)
                hour = 23;

            if (m_valuesHours.valuesUDGe[hour] == 0)
            {
                recomendation = 0;
                return;
            }

            if (!currHour)
            {
                recomendation = m_valuesHours.valuesUDGe[hour];
                return;
            }

            if (lastMin < 2)
            {
                recomendation = m_valuesHours.valuesUDGe[hour];
                return;
            }

            double factSum = 0;
            for (int i = 1; i < lastMin; i++)
                factSum += m_valuesMins.valuesFact[i];

            if (lastMin == 21)
                recomendation = 0;
            else
                recomendation = (m_valuesHours.valuesUDGe[hour] * 20 - factSum) / (20 - (lastMin - 1));

            if (recomendation < 0)
                recomendation = 0;
        }

        public static DataTable restruct_table_pbrValues(DataTable table_in, List<TECComponent> listTECComp, int num_comp)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_PBR"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME]

            for (i = 0; i < table_in.Columns.Count; i++)
            {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //Преобразование таблицы
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                List<TG> list_TG = null;
                List<TECComponent> list_TECComponents = null;
                int count_comp = -1;

                if (num_comp < 0)
                {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((listTECComp[i].m_id > 100) && (listTECComp[i].m_id < 500))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = listTECComp[num_comp].m_listTG;

                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                        //&& (!(table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true))
                    )
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                            //|| (table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)
                            || (table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true)
                        )
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;
                    }
                }

                //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                //table_in_restruct.Columns[tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                table_in_restruct.Columns[@"PBR_NUMBER"].SetOrdinal(table_in_restruct.Columns.Count - 1);
                //else
                //    ;

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_comp < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    else
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TG[i].m_id);

                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows[i].Length; j++)
                    {
                        for (k = 0; k < table_in_restruct.Rows.Count; k++)
                        {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (!(k < table_in_restruct.Rows.Count))
                        {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //Заполнение DATE_ADMIN (постоянные столбцы)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                            //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                            //table_in_restruct.Rows[indx_row][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                            table_in_restruct.Rows[indx_row][@"PBR_NUMBER"] = listDataRows[i][j][@"PBR_NUMBER"];
                            //else
                            //    ;
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            if (num_comp < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            else
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TG[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            return table_in_restruct;
        }

        public static DataTable restruct_table_adminValues(DataTable table_in, List<TECComponent> listTECComp, int num_comp)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_ADMIN"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

            for (i = 0; i < table_in.Columns.Count; i++)
            {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //Преобразование таблицы
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                List<TG> list_TG = null;
                List<TECComponent> list_TECComponents = null;
                int count_comp = -1;

                if (num_comp < 0)
                {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((listTECComp[i].m_id > 100) && (listTECComp[i].m_id < 500))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = listTECComp[num_comp].m_listTG;

                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName == "ID_COMPONENT")) && (!(table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)))
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if (table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;
                    }
                }

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_comp < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    else
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TG[i].m_id);
                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows[i].Length; j++)
                    {
                        for (k = 0; k < table_in_restruct.Rows.Count; k++)
                        {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (!(k < table_in_restruct.Rows.Count))
                        {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //Заполнение DATE_ADMIN (постоянные столбцы)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            if (num_comp < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            else
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TG[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            return table_in_restruct;
        }

        private bool GetHoursResponse(DataTable table)
        {
            int i, j, half, hour = 0, halfAddon;
            double hourVal = 0, halfVal = 0, value, hourValAddon = 0;
            double[] oldValuesTG = new double[CountTG];
            int[] oldIdTG = new int[CountTG];
            int id;
            TG tgTmp;
            bool end = false;
            DateTime dt, dtNeeded;
            int season = 0, prev_season = 0;
            bool jump_forward = false, jump_backward = false;

            lastHour = lastReceivedHour = 0;
            half = 0;
            halfAddon = 0;

            /*Form2 f2 = new Form2();
            f2.FillHourTable(table);*/

            lastHourHalfError = lastHourError = false;

            foreach (TECComponent g in m_tec.list_TECComponents)
            {
                foreach (TG t in g.m_listTG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.receivedHourHalf1[i] = t.receivedHourHalf2[i] = false;
                    }
                }
            }

            if (table.Rows.Count > 0)
            {
                try
                {
                    //if (!DateTime.TryParse(table.Rows[0][6].ToString(), out dt))
                    if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                        return false;

                    //if (!int.TryParse(table.Rows[0][8].ToString(), out season))
                    if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                        return false;
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetHoursResponse () - ...");

                    dt = DateTime.Now.Date;
                }

                GetSeason(dt, season, out season);
                prev_season = season;
                hour = dt.Hour;
                dtNeeded = dt;

                if (dt.Minute == 0)
                    half++;
                else
                    ;
            }
            else
            {
                if (currHour)
                {
                    if (m_curDate.Hour != 0)
                    {
                        lastHour = lastReceivedHour = m_curDate.Hour;
                        lastHourError = true;
                    }
                }
                /*f2.FillHourValues(lastHour, selectedTime, m_tecView.m_valuesHours.valuesFact);
                f2.ShowDialog();*/
                return true;
            }

            for (i = 0; hour < 24 && !end; )
            {
                if (half == 2 || halfAddon == 2) // прошёл один час
                {
                    if (!jump_backward)
                    {
                        if (jump_forward)
                            m_valuesHours.hourAddon = hour; // уточнить
                        m_valuesHours.valuesFact[hour] = hourVal / 2000;
                        hour++;
                        half = 0;
                        hourVal = 0;
                    }
                    else
                    {
                        m_valuesHours.valuesFactAddon = hourValAddon / 2000;
                        m_valuesHours.hourAddon = hour - 1;
                        hourValAddon = 0;
                        prev_season = season;
                        halfAddon++;
                    }
                    lastHour = lastReceivedHour = hour;
                }

                halfVal = 0;

                jump_forward = false;
                jump_backward = false;

                for (j = 0; j < CountTG; j++, i++)
                {
                    if (i >= table.Rows.Count)
                    {
                        end = true;
                        break;
                    }

                    try
                    {
                        if (DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt) == false)
                            return false;

                        if (int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season) == false)
                            return false;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetHoursResponse () - ...");

                        dt = DateTime.Now.Date;
                    }

                    if (dt.CompareTo(dtNeeded) != 0)
                    {
                        GetSeason(dt, season, out season);
                        if (dt.CompareTo(dtNeeded.AddMinutes(-30)) == 0 && prev_season == 1 && season == 2)
                        {
                            dtNeeded = dtNeeded.AddMinutes(-30);
                            jump_backward = true;
                        }
                        else
                            if (dt.CompareTo(dtNeeded.AddMinutes(30)) == 0 && prev_season == 0 && season == 1)
                            {
                                jump_forward = true;
                                break;
                            }
                            else
                                break;
                    }

                    //if (!int.TryParse(table.Rows[i][7].ToString(), out id))
                    if (table.Columns.Contains(@"ID") == true)
                        if (int.TryParse(table.Rows[i][@"ID"].ToString(), out id) == false)
                            return false;
                        else
                            ;
                    else
                        return false;

                    tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.FACT, TG.ID_TIME.HOURS);
                    if (tgTmp == null)
                        return false;
                    else
                        ;

                    //if (!double.TryParse(table.Rows[i][5].ToString(), out value))
                    if (table.Columns.Contains(@"VALUE0") == true)
                        if (double.TryParse(table.Rows[i][@"VALUE0"].ToString(), out value) == false)
                            return false;
                        else
                            ;
                    else
                        return false;

                    switch (m_tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            value *= 2;
                            break;
                        default:
                            break;
                    }

                    halfVal += value;
                    if (!jump_backward)
                    {
                        if (half == 0)
                            tgTmp.receivedHourHalf1[hour] = true;
                        else
                            tgTmp.receivedHourHalf2[hour] = true;
                    }
                    else
                    {
                        if (halfAddon == 0)
                            tgTmp.receivedHourHalf1Addon = true;
                        else
                            tgTmp.receivedHourHalf2Addon = true;
                    }
                }

                dtNeeded = dtNeeded.AddMinutes(30);

                if (!jump_backward)
                {
                    if (jump_forward)
                        m_valuesHours.season = seasonJumpE.WinterToSummer;

                    if (!end)
                        half++;

                    hourVal += halfVal;
                }
                else
                {
                    m_valuesHours.season = seasonJumpE.SummerToWinter;
                    m_valuesHours.addonValues = true;

                    if (!end)
                        halfAddon++;

                    hourValAddon += halfVal;
                }
            }

            /*f2.FillHourValues(lastHour, selectedTime, m_valuesHours.valuesFact);
            f2.ShowDialog();*/

            if (currHour)
            {
                if (lastHour < m_curDate.Hour)
                {
                    lastHourError = true;
                    lastHour = m_curDate.Hour;
                }
                else
                {
                    if ((m_curDate.Hour == 0) && (! (lastHour == 24)) && (! (dtNeeded.Date == m_curDate.Date)))
                    {
                        lastHourError = true;
                        lastHour = 24;
                    }
                    else
                    {
                        if (lastHour != 0)
                        {
                            for (i = 0; i < listTG.Count; i++)
                            {
                                if ((half & 1) == 1)
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h1 " + sensorId2TG[i].receivedHourHalf1[lastHour - 1].ToString());
                                    if (!listTG[i].receivedHourHalf1[lastHour - 1])
                                    {
                                        lastHourHalfError = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h2 " + sensorId2TG[i].receivedHourHalf2[lastHour - 1].ToString());
                                    if (!listTG[i].receivedHourHalf2[lastHour - 1])
                                    {
                                        lastHourHalfError = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            lastReceivedHour = lastHour;

            return true;
        }

        //private bool GetCurrentTMResponse(DataTable table)
        //{
        //    bool bRes = true;
        //    int i = -1,
        //        id = -1;
        //    double value = -1;
        //    TG tgTmp;

        //    foreach (TECComponent g in m_tec.list_TECComponents)
        //    {
        //        foreach (TG t in g.m_listTG)
        //        {
        //            for (i = 0; i < t.power.Length; i++)
        //            {
        //                t.power_TM = 0;
        //            }
        //        }
        //    }

        //    for (i = 0; i < table.Rows.Count; i++)
        //    {
        //        if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
        //            return false;
        //        else
        //            ;

        //        tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

        //        if (tgTmp == null)
        //            return false;
        //        else
        //            ;

        //        if (!(table.Rows[i]["value"] is DBNull))
        //            if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
        //                return false;
        //            else
        //                ;
        //        else
        //            value = 0.0;

        //        switch (m_tec.type())
        //        {
        //            case TEC.TEC_TYPE.COMMON:
        //                break;
        //            case TEC.TEC_TYPE.BIYSK:
        //                //value *= 20;
        //                break;
        //            default:
        //                break;
        //        }

        //        tgTmp.power_TM = value;
        //    }

        //    return bRes;
        //}

        private bool GetLastMinutesTMResponse(DataTable table_in, DateTime dtReq)
        {
            bool bRes = true;
            int i = -1,
                hour = -1,
                offsetUTC = (int)HAdmin.GetUTCOffsetOfCurrentTimeZone().TotalHours;
            double value = -1;
            DateTime dtVal = DateTime.Now;
            DataRow[] tgRows = null;

            if (indxTECComponents < 0)
            {
                foreach (TECComponent g in m_localTECComponents)
                {
                    foreach (TG tg in g.m_listTG)
                    {
                        for (i = 0; i < tg.power_LastMinutesTM.Length; i++)
                        {
                            tg.power_LastMinutesTM[i] = 0;
                        }

                        tgRows = table_in.Select(@"[ID]=" + tg.id_tm);

                        for (i = 0; i < tgRows.Length; i++)
                        {
                            if (!(tgRows[i]["value"] is DBNull))
                                if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                    return false;
                                else
                                    ;
                            else
                                value = 0.0;

                            if ((!(value < 1)) && (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false))
                                return false;
                            else
                                ;

                            hour = dtVal.Hour + offsetUTC + 1; //Т.к. мин.59 из прошедшего часа
                            if (!(hour < 24)) hour -= 24; else ;

                            tg.power_LastMinutesTM[hour] = value;

                            //Запрос с учетом значения перехода через сутки
                            if (hour > 0 && value > 1) {
                                m_valuesHours.valuesLastMinutesTM[hour - 1] += value;
                                m_dictValuesTECComponent[tg.m_id_owner_gtp].valuesLastMinutesTM [hour - 1] += value;
                            }
                            else
                                ;
                        }
                    }
                }
            }
            else
            {
                foreach (TECComponent comp in m_localTECComponents)
                {
                    for (i = 0; i < comp.m_listTG [0].power_LastMinutesTM.Length; i++)
                    {
                        comp.m_listTG[0].power_LastMinutesTM[i] = 0;
                    }

                    tgRows = table_in.Select(@"[ID]=" + comp.m_listTG[0].id_tm);

                    for (i = 0; i < tgRows.Length; i++)
                    {
                        if (tgRows[i] == null)
                            continue;
                        else
                            ;

                        try
                        {
                            if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                return false;
                            else
                                ;

                            if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                return false;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                            dtVal = DateTime.Now.Date;
                        }

                        hour = dtVal.Hour + offsetUTC + 1;
                        //if (!(hour < 24))
                        if (hour > 24)
                            hour -= 24;
                        else ;

                        //if (dtReq.Date.Equals (dtVal.Date) == true) {
                        comp.m_listTG[0].power_LastMinutesTM[hour] = value;

                        if (hour > 0 && value > 1)
                            m_valuesHours.valuesLastMinutesTM[hour - 1] += value;
                        else
                            ;
                        //} else ;
                    }
                }
            }

            return bRes;
        }

        private bool GetMinsResponse(DataTable table)
        {
            int i, j = 0, min = 0;
            double minVal = 0, value;
            TG tgTmp;
            int id;
            bool end = false;
            DateTime dt, dtNeeded;
            int season = 0, need_season = 0, max_season = 0;
            bool jump = false;

            lastMinError = false;

            /*Form2 f2 = new Form2();
            f2.FillMinTable(table);*/

            foreach (TECComponent g in m_tec.list_TECComponents)
            {
                foreach (TG t in g.m_listTG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.power[i] = 0;
                        t.receivedMin[i] = false;
                    }
                }
            }

            lastMin = 0;

            if (table.Rows.Count > 0)
            {
                if (table.Columns.Contains(@"DATA_DATE") == true)
                    if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                        return false;
                    else
                        ;
                else
                    return false;

                if (table.Columns.Contains(@"SEASON") == true)
                    if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                        return false;
                    else
                        ;
                else
                    return false;

                need_season = max_season = season;
                min = (int)(dt.Minute / 3);
                dtNeeded = dt;
            }
            else
            {
                if (currHour)
                {
                    if ((m_curDate.Minute / 3) != 0)
                    {
                        lastMinError = true;
                        lastMin = ((m_curDate.Minute) / 3) + 1;
                    }
                }
                /*f2.FillMinValues(lastMin, selectedTime, m_tecView.m_valuesMins.valuesFact);
                f2.ShowDialog();*/
                return true;
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
                    return false;
                if (season > max_season)
                    max_season = season;
            }

            if (currHour)
            {
                if (need_season != max_season)
                {
                    m_valuesHours.addonValues = true;
                    m_valuesHours.hourAddon = lastHour - 1;
                    need_season = max_season;
                }
            }
            else
            {
                if (m_valuesHours.addonValues)
                {
                    need_season = max_season;
                }
            }

            for (i = 0; !end && min < 21; min++)
            {
                if (jump)
                {
                    min--;
                }
                else
                {
                    m_valuesMins.valuesFact[min] = 0;
                    minVal = 0;
                }

                /*MessageBox.Show("min " + min.ToString() + ", lastMin " + lastMin.ToString() + ", i " + i.ToString() +
                                 ", table.Rows.Count " + table.Rows.Count.ToString());*/
                jump = false;
                for (j = 0; j < CountTG; j++, i++)
                {
                    if (i >= table.Rows.Count)
                    {
                        end = true;
                        break;
                    }

                    try
                    {
                        if (!DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt))
                            return false;
                        if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
                            return false;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                        dt = DateTime.Now.Date;
                    }

                    if (season != need_season)
                    {
                        jump = true;
                        i++;
                        break;
                    }

                    if (dt.CompareTo(dtNeeded) != 0)
                    {
                        break;
                    }

                    if (!int.TryParse(table.Rows[i][@"ID"].ToString(), out id))
                        return false;

                    tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.FACT, (int)TG.ID_TIME.MINUTES);

                    if (tgTmp == null)
                        return false;

                    if (!double.TryParse(table.Rows[i][@"VALUE0"].ToString(), out value))
                        return false;
                    else
                        ;

                    switch (m_tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            value *= 20;
                            break;
                        default:
                            break;
                    }

                    minVal += value;
                    tgTmp.power[min] = value / 1000;
                    tgTmp.receivedMin[min] = true;
                }

                if (!jump)
                {
                    dtNeeded = dtNeeded.AddMinutes(3);

                    //MessageBox.Show("end " + end.ToString() + ", minVal " + (minVal / 1000).ToString());

                    if (!end)
                    {
                        m_valuesMins.valuesFact[min] = minVal / 1000;
                        lastMin = min + 1;
                    }
                }
            }

            /*f2.FillMinValues(lastMin, selectedTime, m_tecView.m_valuesMins.valuesFact);
            f2.ShowDialog();*/

            if (lastMin <= ((m_curDate.Minute - 1) / 3))
            {
                lastMinError = true;
                //lastMin = ((selectedTime.Minute - 1) / 3) + 1;
            }

            return true;
        }

        private int LayotByName(string l)
        {
            int iRes = -1;

            if (l.Length > 3)
                switch (l)
                {
                    case "ППБР": iRes = 0; break;
                    default:
                        {
                            if (l.Substring(0, 3) != "ПБР" || int.TryParse(l.Substring(3), out iRes) == false || iRes <= 0 || iRes > 24)
                                ;
                            else
                                ;
                        }
                        break;
                }
            else
                ;

            return iRes;
        }

        private void GetSeason(DateTime date, int db_season, out int season)
        {
            season = db_season - date.Year - date.Year;
            if (season < 0)
                season = 0;
            else
                if (season > 2)
                    season = 2;
                else
                    ;
        }

        private bool LayoutIsBiggerByName(string l1, string l2)
        {
            bool bRes = false;

            int num1 = LayotByName(l1),
                num2 = LayotByName(l2);

            if (num2 > num1)
                bRes = true;
            else
                ;

            return bRes;
        }

        private bool GetPPBRValuesResponse(DataTable table)
        {
            bool bRes = true;

            if (!(table == null))
                m_tablePPBRValuesResponse = table.Copy();
            else
                ;

            return bRes;
        }

        /*private void GetCurrentTimeRequest()
        {
            string query = string.Empty;
            DbInterface.DB_TSQL_INTERFACE_TYPE typeDB = DbTSQLInterface.getTypeDB(m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_ASKUE].port);

            switch (typeDB)
            {
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL:
                    query = @"SELECT now()";
                    break;
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL:
                    query = @"SELECT GETDATE()";
                    break;
                default:
                    break;
            }

            if (query.Equals(string.Empty) == false)
                m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, query);
            else
                ;
        }*/

        //private void GetSensorsFactRequest()
        //{
        //    tec.Request(CONN_SETT_TYPE.CONFIG_DB, tec.sensorsFactRequest());
        //}

        //private void GetSensorsTMRequest()
        //{
        //    tec.Request(CONN_SETT_TYPE.CONFIG_DB, tec.sensorsTMRequest());
        //}

        private void GetHoursRequest(DateTime date)
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.HOURS)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.HOURS)));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_ASKUE], m_tec.hoursRequest(date, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.HOURS)));
        }

        private void GetMinsRequest(int hour)
        {
            //tec.Request(CONN_SETT_TYPE.DATA_ASKUE, tec.minsRequest(selectedTime, hour, sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES]));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.MINUTES)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.MINUTES)));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_ASKUE], m_tec.minsRequest(m_curDate, hour, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.MINUTES)));
        }

        //private void GetCurrentTMRequest()
        //{
        //    //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMRequest(m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_SOTIASSO)));
        //    //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMRequest(m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_SOTIASSO)));
        //    Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMRequest(m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_SOTIASSO)));
        //}

        private void GetLastMinutesTMRequest(DateTime dtReq)
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_SOTIASSO)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_SOTIASSO)));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO)));
        }

        private void GetPPBRValuesRequest()
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], tec.GetPBRValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, m_admin.m_typeFields));
            //m_tec.Request(CONN_SETT_TYPE.PBR, m_tec.GetPBRValueQuery(m_indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, s_typeFields));
            //m_tec.Request(CONN_SETT_TYPE.PBR, m_tec.GetPBRValueQuery(m_indx_TECComponent, selectedTime.Date, s_typeFields));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], m_tec.GetPBRValueQuery(indxTECComponents, m_curDate.Date, s_typeFields));
        }

        private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], tec.GetAdminValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
            //m_tec.Request(CONN_SETT_TYPE.ADMIN, m_tec.GetAdminValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
            //m_tec.Request(CONN_SETT_TYPE.ADMIN, m_tec.GetAdminValueQuery(m_indx_TECComponent, selectedTime.Date, mode));
            Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(indxTECComponents, m_curDate.Date, mode));
        }

        protected override void InitializeSyncState()
        {
            m_waitHandleState = new WaitHandle[2] { new AutoResetEvent(true), new ManualResetEvent(false) };
        }
    }
}
