using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color

using HClassLibrary;

namespace StatisticCommon
{
    public class TecView : HAdmin
    {
        public enum TYPE_PANEL { VIEW, CUR_POWER, LAST_MINUTES, ADMIN_ALARM, SOBSTV_NYZHDY, COUNT_TYPE_PANEL };
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
            InitSensors, //Инициализация строк с идентификаторами ГТП (ТГ) для дальнейшего использования в запросах
            CurrentTimeAdmin, //время сервера, источник данных: сервер с административными значениями
            CurrentTimeView, //время сервера, источник данных: ...
            Hour_TM, //текущий час, СОТИАССО
            Hours_Fact, //указанные сутки, АСКУЭ
            CurrentMin_TM, //текущий интервальный отрезок (3 или 1мин), СОТИАССО
            CurrentMins_Fact, //текущие сутки/час, АСКУЭ
            Hours_TM, //указанные сутки, СОТИАССО
            CurrentMins_TM, //текущие сутки/час, СОТИАССО
            CurrentHours_TM_SN_PSUM, //текущие сутки для Собственные Нужды, СОТИАССО
            LastValue_TM_Gen, //крайние значения для ГЕНЕРАЦИЯ, СОТИАССО
            LastValue_TM_SN, //крайние значения для Собственные Нужды, СОТИАССО
            LastMinutes_TM, //значения крайних минут часа за указанные сутки, СОТИАССО
            //RetroHours,
            RetroMins_Fact, //указанные сутки/час, АСКУЭ
            RetroMins_TM, //указанные сутки/час, СОТИАССО
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            AdminValues, //Получение административных/ПБР значений
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
        public double [] m_arValueCurrentTM_Gen;

        private AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected DataTable m_tablePPBRValuesResponse
                    //, m_tableRDGExcelValuesResponse
                    ;

        public abstract class values
        {
            public double valuesLastMinutesTM;

            public double valuesPBR;
            public bool valuesForeignCommand;
            public double valuesPmin;
            public double valuesPmax;
            public double valuesPBRe;
            public double valuesUDGe;

            public double valuesDiviation; //Значение в ед.изм.

            public double valuesREC;
        }

        public class valuesTG : Object {
            public double[] m_powerMinutes; //для мин./значений в течении часа
            public bool m_bPowerMinutesRecieved; //для мин./значений в течении часа
            public double m_powerCurrent_TM; //для текущего значения ТМ
            public double [] m_power_LastMinutesTM; //для 59-х мин каждого часа
        }

        public class valuesTECComponent : values
        {
            //public volatile double[] valuesREC;
            public double valuesISPER; //Признак ед.изм. 'valuesDIV'
            public double valuesDIV; //Значение из БД
        }

        public class valuesTEC : values
        {
            public double valuesFact;
            public double valuesTMSNPsum;
        }

        public object m_lockValue;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool currHour;
        //private bool m_bCurrHour; public volatile bool CurrHour { }

        public volatile int lastHour;
        public volatile int lastReceivedHour;
        public volatile int lastMin;

        public volatile bool lastMinError;
        public volatile bool lastHourError;
        public volatile bool lastHourHalfError;
        public volatile bool currentMinuteTM_GenError;

        public volatile string lastLayout;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile valuesTEC[] m_valuesMins;
        public volatile valuesTEC[] m_valuesHours;
        //public DateTime selectedTime;
        //public DateTime serverTime;

        public volatile int m_indx_TEC;
        //public volatile int m_indx_TECComponent;
        public List <TECComponentBase> m_localTECComponents;
        public int m_ID { get { return indxTECComponents < 0 ? m_tec.m_id : m_tec.list_TECComponents[indxTECComponents].m_id; } }
        public volatile Dictionary<int, TecView.valuesTECComponent> [] m_dictValuesTECComponent;
        public volatile Dictionary<int, TecView.valuesTG>m_dictValuesTG;

        public CONN_SETT_TYPE[] m_arTypeSourceData;
        public int[] m_arIdListeners; //Идентификаторы номеров клиентов подключенных к

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public double recomendation;

        //private bool started;
        private int m_pool_time
            , m_error_delay
            ;

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

            m_valuesMins = new valuesTEC [21];
            m_valuesHours = new valuesTEC [24];

            m_dictValuesTG = new Dictionary<int,valuesTG> ();

            m_localTECComponents = new List<TECComponentBase>();
            //tgsName = new List<System.Windows.Forms.Label>();

            m_arTypeSourceData = new CONN_SETT_TYPE [(int)TG.ID_TIME.COUNT_ID_TIME];

            m_arValueCurrentTM_Gen = new double[(int)TG.ID_TIME.COUNT_ID_TIME] { -1F, -1F };

            ClearStates();
        }

        public void InitializeTECComponents () {
            //int positionXName = 515, positionXValue = 504, positionYName = 6, positionYValue = 19;
            //countTG = 0;
            List<int> tg_ids = new List<int>(); //Временный список идентификаторов ТГ

            if (indxTECComponents < 0) // значит этот view будет суммарным для всех ГТП
            {                
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

                    initDictValuesTG (c);
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
                        if (tg.m_id == c.m_id) {
                            m_localTECComponents.Add(c);

                            initDictValuesTG(c);
                        }
                        else
                            ;
                    }
                }
            }

            initDictValuesTECComponent (24 + 1);
        }

        private void initDictValuesTECComponent(int cnt)
        {
            m_dictValuesTECComponent = new Dictionary<int, valuesTECComponent>[cnt];
            
            foreach (TECComponent c in m_localTECComponents)
                for (int i = 0; i < m_dictValuesTECComponent.Length; i ++) {
                    if (m_dictValuesTECComponent[i] == null) m_dictValuesTECComponent[i] = new Dictionary<int,valuesTECComponent> (); else ;
                    m_dictValuesTECComponent[i].Add(c.m_id, new TecView.valuesTECComponent ());
                }
        }

        private void initDictValuesTG(TECComponent comp)
        {
            foreach (TG tg in comp.m_listTG)
                if (m_dictValuesTG.ContainsKey(tg.m_id) == false)
                    m_dictValuesTG.Add(tg.m_id, new valuesTG());
                else
                    ;
        }

        //public TecView(bool[] arMarkSavePPBRValues, TYPE_PANEL type, int indx_tec, int indx_comp)
        public TecView(TYPE_PANEL type, int indx_tec, int indx_comp, int pool_time, int error_delay)
            : base()
        {            
            m_typePanel = type;

            m_indx_TEC = indx_tec;
            indxTECComponents = indx_comp;

            m_pool_time = pool_time;
            m_error_delay = m_error_delay;

            m_arIdListeners = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arIdListeners[i] = -1;
            }
        }

        public class EventRegEventArgs : EventArgs
        {
            public int m_id_gtp;
            public int m_id_tg;
            public int m_situation;

            //public EventRegEventArgs() : base ()
            //{
            //    m_id_gtp = -1;
            //    m_id_tg = -1;
            //    m_situation = 0;
            //}

            public EventRegEventArgs(int id_gtp, int id_tg, int s) : base ()
            {
                m_id_gtp = id_gtp;
                m_id_tg = id_tg;
                m_situation = s;
            }
        }

        public delegate void DelegateOnEventReg (TecView sender, EventRegEventArgs e);
        public event DelegateOnEventReg EventReg;

        public override void GetRDGValues(int mode, int indx, DateTime date)
        {
            m_prevDate = m_curDate;
            m_curDate = date.Date;

            ClearStates();

            adminValuesReceived = false;

            if ((m_tec.m_bSensorsStrings == false))
            {
                states.Add((int)StatesMachine.InitSensors);
            }
            else ;

            if (currHour == true)
                states.Add((int)StatesMachine.CurrentTimeView);
            else
                ;
            states.Add((int)StatesMachine.LastValue_TM_Gen);

            //??? а где AISKUE+SOTIASSO
            if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                states.Add((int)StatesMachine.Hours_Fact);
            else
                if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
                    states.Add((int)StatesMachine.Hours_TM);
                else
                    ;

            if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                states.Add((int)StatesMachine.CurrentMins_Fact);
            else
                if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                    states.Add((int)StatesMachine.CurrentMins_TM);
                else
                    ;

            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
        }

        public void OnEventConfirm(int id_tg)
        {
            foreach (TECComponent tc in allTECComponents)
            {
                if (tc.m_id == id_tg)
                {
                    if (tc.m_listTG[0].m_TurnOnOff == TG.INDEX_TURNOnOff.ON)
                        tc.m_listTG[0].m_TurnOnOff = TG.INDEX_TURNOnOff.OFF;
                    else
                        if (tc.m_listTG[0].m_TurnOnOff == TG.INDEX_TURNOnOff.OFF)
                            tc.m_listTG[0].m_TurnOnOff = TG.INDEX_TURNOnOff.ON;
                        else
                            ;

                    break;
                }
                else
                    ;
            }
        }

        private void getRDGValues () {
            GetRDGValues((int)s_typeFields, indxTECComponents, DateTime.Now);

            try
            {
                semaState.Release(1);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"TecView::ChangeState () - semaState.Release (1) - ...");
            }
        }

        public void SuccessThreadRDGValues(int curHour, int curMinute)
        {
            double TGTURNONOFF_VALUE = -1F, NOT_VALUE = -2F
                , power_TM = NOT_VALUE;
            TG.INDEX_TURNOnOff curTurnOnOff = TG.INDEX_TURNOnOff.UNKNOWN;

            Console.WriteLine (@"curHour=" + curHour.ToString () + @"; curMinute=" + curMinute.ToString ());

            //if (((lastHour == 24) || (lastHourError == true)) || ((lastMin == 0) || (lastMinError == true)))
            if (((curHour == 24) || (lastHourError == true)) || ((curMinute == 0) || (lastMinError == true)))
            {
                Logging.Logg().Error(@"TecView::SuccessThreadRDGValues () - curHour=" + curHour + @"; curMinute=" + curMinute);
            }
            else {            
                foreach (TG tg in allTECComponents[indxTECComponents].m_listTG)
                {
                    curTurnOnOff = TG.INDEX_TURNOnOff.UNKNOWN;

                    Console.Write(tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + m_dictValuesTG[tg.m_id].m_powerCurrent_TM);

                    if (m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 1)
                        if (!(m_dictValuesTG[tg.m_id].m_powerCurrent_TM < 0))
                            curTurnOnOff = TG.INDEX_TURNOnOff.OFF;
                        else
                            ;
                    else
                    {//Больше ИЛИ равно 1.0
                        curTurnOnOff = TG.INDEX_TURNOnOff.ON;

                        if (power_TM == NOT_VALUE) power_TM = 0F; else ;
                        power_TM += m_dictValuesTG[tg.m_id].m_powerCurrent_TM;
                    }

                    ////Отладка - изменяем состояние
                    //if (!(tg.m_TurnOnOff == TG.INDEX_TURNOnOff.UNKNOWN))
                    //{
                    //    if (curTurnOnOff == TG.INDEX_TURNOnOff.ON)
                    //    {
                    //        power_TM -= tg.power_TM;

                    //        tg.power_TM = 0.666;

                    //        curTurnOnOff = TG.INDEX_TURNOnOff.OFF;
                    //    }
                    //    else
                    //        if (curTurnOnOff == TG.INDEX_TURNOnOff.OFF)
                    //        {
                    //            tg.power_TM = 66.6;

                    //            curTurnOnOff = TG.INDEX_TURNOnOff.ON;
                    //        }
                    //        else
                    //            ;

                    //    Console.Write(Environment.NewLine + @"Отладка:: " + tg.m_id_owner_gtp + @":" + tg.m_id + @"=" + tg.power_TM + Environment.NewLine);
                    //}
                    //else
                    //    ;

                    if (tg.m_TurnOnOff == TG.INDEX_TURNOnOff.UNKNOWN)
                    {
                        tg.m_TurnOnOff = curTurnOnOff;
                    }
                    else
                    {
                        if (!(tg.m_TurnOnOff == curTurnOnOff))
                        {
                            EventReg(this, new EventRegEventArgs(allTECComponents[indxTECComponents].m_id, tg.m_id, (int)curTurnOnOff));

                            //Прекращаем текущий цикл...
                            //Признак досрочного прерывания цикла для сигн. "Текущая P"
                            power_TM = TGTURNONOFF_VALUE;

                            break;
                        }
                        else
                            ; //EventUnReg...
                    }

                    if ((allTECComponents[indxTECComponents].m_listTG.IndexOf(tg) + 1) < allTECComponents[indxTECComponents].m_listTG.Count)
                        Console.Write(@", ");
                    else
                        ;
                }

                //Для отладки
                //EventReg(this, new EventRegEventArgs(allTECComponents[indxTECComponents].m_id, -1, -1)); //Меньше

                if (!(power_TM == TGTURNONOFF_VALUE))
                    if ((!(power_TM == NOT_VALUE)) && (!(power_TM < 1)))
                        if (Math.Abs(power_TM - m_valuesHours[curHour].valuesUDGe) > m_valuesHours[curHour].valuesUDGe * ((double)allTECComponents[indxTECComponents].m_dcKoeffAlarmPcur / 100))
                            //EventReg(allTECComponents[indxTECComponents].m_id, -1);
                            if (power_TM < m_valuesHours[curHour].valuesUDGe)
                                EventReg(this, new EventRegEventArgs(allTECComponents[indxTECComponents].m_id, -1, -1)); //Меньше
                            else
                                EventReg(this, new EventRegEventArgs(allTECComponents[indxTECComponents].m_id, -1, 1)); //Больше
                        else
                            ; //EventUnReg...
                    else
                        ; //Нет значений ИЛИ значения ограничены 1 МВт
                else
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.BREAK);

                Console.WriteLine ();

                //for (int i = 0; i < m_valuesHours.valuesFact.Length; i ++)
                //    Console.WriteLine(@"valuesFact[" + i.ToString() + @"]=" + m_valuesHours.valuesFact[i]);
            }
        }

        private void threadGetRDGValues(object synch)
        {
            int indxEv = -1;

            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i ++)
                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

            foreach (TECComponent tc in allTECComponents) {
                if ((tc.m_id > 100) && (tc.m_id < 500)) {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == (int)INDEX_WAITHANDLE_REASON.SUCCESS)
                    {
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

        public void ChangeState_SobstvNyzhdy () {
            ClearStates();

            ClearValues();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            //states.Add((int)TecView.StatesMachine.CurrentHours_Fact); //Только для определения сезона ???            
            states.Add((int)TecView.StatesMachine.CurrentHours_TM_SN_PSUM);
            states.Add((int)TecView.StatesMachine.LastValue_TM_SN);
        }

        private void ChangeState_AdminAlarm () {
            new Thread(new ParameterizedThreadStart(threadGetRDGValues)).Start();
        }

        public override void Activate(bool active)
        {
            base.Activate(active);

            ClearStates();
        }

        public override void Start()
        {
            ClearValues();

            StartDbInterfaces();
            
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            StopDbInterfaces();
        }

        public override void InitTEC(List<StatisticCommon.TEC> listTEC, HMark markQueries)
        {
            base.InitTEC(listTEC, markQueries);

            InitializeTECComponents ();
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            return bRes;
        }

        //public override void  ClearValues(int cnt = -1)
        public override void  ClearValues()
        {
            ClearValuesMins();
            //ClearValuesHours(cnt);
            ClearValuesHours();

            ClearValuesLastMinutesTM();
        }

        public void ClearValuesLastMinutesTM()
        {
            if (!(m_dictValuesTECComponent.Length - m_valuesHours.Length == 1))
            {
                m_dictValuesTECComponent = null;
                m_dictValuesTECComponent = new Dictionary<int, valuesTECComponent>[m_valuesHours.Length + 1];
            }
            else
                ;

            for (int i = 0; i < (m_valuesHours.Length + 1); i++)
            {
                if (i < m_valuesHours.Length)
                    m_valuesHours[i].valuesLastMinutesTM = 0.0;
                else
                    ;

                foreach (TECComponent g in m_localTECComponents)
                {
                    foreach (TG tg in g.m_listTG)
                    {
                        if (m_dictValuesTG[tg.m_id].m_power_LastMinutesTM == null)
                            m_dictValuesTG[tg.m_id].m_power_LastMinutesTM = new double[m_dictValuesTECComponent.Length + 1];
                        else {
                            if (!(m_dictValuesTG[tg.m_id].m_power_LastMinutesTM.Length == m_dictValuesTECComponent.Length))
                            {
                                m_dictValuesTG[tg.m_id].m_power_LastMinutesTM = null;

                                m_dictValuesTG[tg.m_id].m_power_LastMinutesTM = new double[m_dictValuesTECComponent.Length + 1];
                            } else
                                ;
                        }

                        m_dictValuesTG[tg.m_id].m_power_LastMinutesTM[i] = 0F;
                    }
                    
                    if (m_dictValuesTECComponent[i] == null) m_dictValuesTECComponent[i] = new Dictionary<int, valuesTECComponent>(); else ;

                    if (m_dictValuesTECComponent[i].ContainsKey(g.m_id) == false)
                        m_dictValuesTECComponent[i].Add(g.m_id, new valuesTECComponent());
                    else
                        ;

                    if (m_dictValuesTECComponent[i][g.m_id] == null) m_dictValuesTECComponent[g.m_id][i] = new valuesTECComponent(); else ;

                    m_dictValuesTECComponent[i][g.m_id].valuesLastMinutesTM = 0.0;
                }
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
                Request (m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN], m_tec.GetPBRDatesQuery(date, s_typeFields, allTECComponents[indxTECComponents]));
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
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMRequest(m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO)));
        }

        private static bool CheckNameFieldsOfTable (DataTable tbl, string [] nameFields) {
            bool bRes = true;

            foreach (string nameField in nameFields) {
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

        private bool GetCurrentTMGenResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1,
                id = -1;
            double value = -1;
            DateTime dtLastChangedAt =
                m_dtLastChangedAt_TM_Gen = DateTime.UtcNow
                , dtServer = serverTime.Add(-HAdmin.GetUTCOffsetOfMoscowTimeZone());
            TG tgTmp;

            currentMinuteTM_GenError = false;

            foreach (TECComponent g in m_localTECComponents)
            {
                foreach (TG tg in g.m_listTG)
                {
                    m_dictValuesTG[tg.m_id].m_powerCurrent_TM = -1F;
                }
            }

            bRes = CheckNameFieldsOfTable(table, new string[] { @"ID", @"value", @"last_changed_at" });
            if (bRes == true)
            {
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
                        value = -1F;

                    //Опрделить дата/время для "нормальных" (>= 1) значений
                    if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                        //Нельзя определить дата/время для "нормальных" (>= 1) значений
                        return false;
                    else
                        ;

                    if (m_dtLastChangedAt_TM_Gen > dtLastChangedAt)
                        m_dtLastChangedAt_TM_Gen = dtLastChangedAt;
                    else
                        ;

                    if ((!(value < 1)) && ((dtServer - m_dtLastChangedAt_TM_Gen).TotalMinutes > 3) && (currentMinuteTM_GenError == false))
                    {
                        currentMinuteTM_GenError = true;

                        Logging.Logg().Warning(@"TecView::GetCurrentTMGenResponse () - currentMinuteTM_GenError = true");

                        //return true;
                        break; //bRes по-прежнему == true ???
                    }
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

                    if (!(m_dictValuesTG[tgTmp.m_id].m_powerCurrent_TM == value)) m_dictValuesTG[tgTmp.m_id].m_powerCurrent_TM = value; else ;
                }

                //Преобразование из UTC в МСК ??? С 26.10.2014 г. в БД записи по МСК !!! Нет оставили "как есть"
                try { m_dtLastChangedAt_TM_Gen = HAdmin.ToMoscowTimeZone(m_dtLastChangedAt_TM_Gen); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"TecView::GetCurrentTMGenResponse () - HAdmin.ToCurrentTimeZone () - ...");
                }
            }
            else
                ;

            return bRes;
        }

        private void GetCurrentTMSNRequest()
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMSNRequest());
        }

        private bool GetCurrentTMSNResponse(DataTable table)
        {
            bool bRes = true;
            int id = -1;

            m_dtLastChangedAt_TM_SN = DateTime.Now;

            bRes = CheckNameFieldsOfTable(table, new string[] { @"ID_TEC", @"SUM_P_SN", @"LAST_UPDATE" });
            if ((bRes == true) && (table.Rows.Count == 1))
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
                case (int)StatesMachine.Hours_Fact:
                case (int)StatesMachine.Hour_TM:
                case (int)StatesMachine.Hours_TM:
                case (int)StatesMachine.CurrentMins_Fact:
                case (int)StatesMachine.CurrentMin_TM:
                case (int)StatesMachine.CurrentMins_TM:
                case (int)StatesMachine.CurrentHours_TM_SN_PSUM:
                case (int)StatesMachine.LastValue_TM_Gen:
                case (int)StatesMachine.LastValue_TM_SN:
                case (int)StatesMachine.LastMinutes_TM:
                //case (int)StatesMachine.RetroHours:
                case (int)StatesMachine.RetroMins_Fact:
                case (int)StatesMachine.RetroMins_TM:
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
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
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
                case (int)StatesMachine.Hours_Fact:
                    reason = @"получасовых значений";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.Hour_TM:
                    reason = @"усредн. за час телемеханики";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    break;
                case (int)StatesMachine.Hours_TM:
                    reason = @"часовых значений";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    reason = @"3-х минутных значений";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.CurrentMin_TM:
                    reason = @"усредн. за интервал телемеханики";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    break;
                case (int)StatesMachine.CurrentMins_TM:
                    reason = @"1-минутных значений";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.CurrentHours_TM_SN_PSUM:
                    reason = @"часовых значений (собств. нужды)";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    break;
                case (int)StatesMachine.LastValue_TM_Gen:
                    reason = @"текущих значений (генерация)";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.LastValue_TM_SN:
                    reason = @"текущих значений (собств. нужды)";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    reason = @"текущих значений 59 мин.";
                    waiting = @"Ожидание " + m_pool_time.ToString() + " секунд";
                    break;
                //case (int)StatesMachine.RetroHours:
                //    reason = @"получасовых значений";
                //    waiting = @"Переход в ожидание";
                //    break;
                case (int)StatesMachine.RetroMins_Fact:
                    reason = @"3-х минутных значений";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.RetroMins_TM:
                    reason = @"1-минутных значений";
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
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    reason = @"административных значений";
                    AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
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

            if (!(m_typePanel == TYPE_PANEL.ADMIN_ALARM))
                ErrorReport(msg);
            else
                ;

            Logging.Logg().Error(@"TecView::StateErrors () - ошибка " + reason + @". " + waiting + @". ");
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
                case (int)StatesMachine.Hours_Fact:
                    msg = @"получасовых значений";
                    GetHoursFactRequest(m_curDate.Date);
                    break;
                case (int)StatesMachine.Hour_TM:
                    msg = @"усредн. за час телемеханики";
                    GetHourTMRequest(m_curDate.Date, lastHour);
                    break;
                case (int)StatesMachine.Hours_TM:
                    msg = @"часовых значений";
                    GetHoursTMRequest(m_curDate.Date);
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    msg = @"трёхминутных значений";
                    GetMinsFactRequest(lastHour);
                    break;
                case (int)StatesMachine.CurrentMins_TM:
                    msg = @"1-минутных значений";
                    GetMinsTMRequest(lastHour);
                    break;
                case (int)StatesMachine.CurrentMin_TM:
                    msg = @"усредн. за интервал телемеханики";
                    GetMinTMRequest(m_curDate.Date, lastHour, lastMin);
                    break;
                case (int)StatesMachine.CurrentHours_TM_SN_PSUM:
                    msg = @"часовых значений (собств. нужды)";
                    GetHoursTMSNPsumRequest();
                    break;
                case (int)StatesMachine.LastValue_TM_Gen:
                    msg = @"текущих значений (генерация)";
                    GetCurrentTMGenRequest ();
                    break;
                case (int)StatesMachine.LastValue_TM_SN:
                    msg = @"текущих значений (собств. нужды)";
                    GetCurrentTMSNRequest();
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    msg = @"текущих значений 59 мин";
                    GetLastMinutesTMRequest();
                    break;
                //case (int)StatesMachine.RetroHours:
                //    msg = @"получасовых значений";
                //    adminValuesReceived = false;
                //    GetHoursRequest(m_curDate.Date);
                //    break;
                case (int)StatesMachine.RetroMins_Fact:
                    msg = @"трёхминутных значений";
                    GetMinsFactRequest(lastHour);
                    break;
                case (int)StatesMachine.RetroMins_TM:
                    msg = @"1-минутных значений";
                    GetMinsTMRequest(lastHour);
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
                    GetAdminValuesRequest(s_typeFields);
                    break;
                default:
                    bRes = false;
                    break;
            }

            if (!(m_typePanel == TYPE_PANEL.ADMIN_ALARM))
                ActionReport (@"Получение " + msg + @".");
            else
                ;

            //Logging.Logg().Debug(@"TecView::StateRequest () - TECname=" + m_tec.name_shr + @", state=" + state.ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

            return bRes;
        }

        protected override bool StateResponse(int state, System.Data.DataTable table)
        {
            bool bRes = true;

            switch (state)
            {
                case (int)StatesMachine.InitSensors:
                    switch (m_tec.type())
                        {
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                                bRes = GetSensorsTEC();
                                break;
                            default:
                                break;
                        }
                        if (bRes == true)
                        {
                        }
                        else
                            ;
                    break;
                case (int)StatesMachine.CurrentTimeAdmin:
                    bRes = GetCurrentTimeAdminResponse(table);
                    if (bRes == true)
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
                    bRes = GetCurrentTimeViewResponse(table);
                    if (bRes == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        m_curDate = m_curDate.AddSeconds(-1 * m_error_delay);
                        //this.BeginInvoke(delegateSetNowDate, true);
                        if (!(setDatetimeView == null)) setDatetimeView(); else ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Hours_Fact:
                    ClearValuesHours ();
                    //GenerateHoursTable(seasonJumpE.SummerToWinter, 3, table);
                    bRes = GetHoursFactResponse(table);
                    if (bRes == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.Hour_TM:
                    bRes = GetHourTMResponse(table);
                    break;
                case (int)StatesMachine.Hours_TM:
                    ClearValuesHours();
                    bRes = GetHoursTMResponse(table);
                    break;
                case (int)StatesMachine.CurrentMins_Fact:
                    ClearValuesMins();
                    bRes = GetMinsFactResponse(table);
                    if (bRes == true)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.CurrentMin_TM:
                    bRes = GetMinTMResponse(table);
                    break;
                case (int)StatesMachine.CurrentMins_TM:
                    ClearValuesMins();
                    bRes = GetMinsTMResponse(table);
                    break;
                case (int)StatesMachine.CurrentHours_TM_SN_PSUM:
                    bRes = GetHoursTMSNPsumResponse(table);
                    if (bRes == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastValue_TM_Gen:
                    bRes = GetCurrentTMGenResponse(table);
                    if (bRes == true)
                    {
                        if (!(updateGUI_TM_Gen == null)) updateGUI_TM_Gen(); else ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastValue_TM_SN:
                    bRes = GetCurrentTMSNResponse(table);
                    if (bRes == true)
                    {
                        updateGUI_TM_SN ();
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.LastMinutes_TM:
                    ClearValuesLastMinutesTM ();
                    bRes = GetLastMinutesTMResponse(table, m_curDate);
                    if (bRes == true)
                    {
                        if (! (updateGUI_LastMinutes == null))
                            updateGUI_LastMinutes();
                        else
                            ;
                    }
                    else
                        ;
                    break;
                //case (int)StatesMachine.RetroHours:
                //    ClearValues();
                //    bRes = GetHoursResponse(table);
                //    if (bRes == true)
                //    {
                //    }
                //    else
                //        ;
                //    break;
                case (int)StatesMachine.RetroMins_Fact:
                case (int)StatesMachine.RetroMins_TM:
                    ClearValuesMins();
                    if ((m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                        || (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        bRes = GetMinsFactResponse(table);
                    else
                        if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                            bRes = GetMinsTMResponse(table);
                        else
                            ;
                    if (bRes == true)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                        updateGUI_Fact(lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    bRes = GetPPBRDatesResponse(table, m_curDate);
                    if (bRes == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    ClearPBRValues();
                    bRes = GetPPBRValuesResponse(table);
                    if (bRes == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminDates:
                    break;
                case (int)StatesMachine.AdminValues:
                    ClearAdminValues();
                    bRes = GetAdminValuesResponse(table);
                    if (bRes == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.AdminValues");
                        ComputeRecomendation(lastHour - 0);
                        adminValuesReceived = true;
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                        if (! (updateGUI_Fact == null)) updateGUI_Fact(lastHour, lastMin); else ;
                    }
                    else
                        ;
                    break;
                default:
                    bRes = false;
                    break;
            }

            if ((bRes == true) && (! (m_typePanel == TYPE_PANEL.ADMIN_ALARM)))
                FormMainBaseWithStatusStrip.m_report.ClearStates ();
            else
                ;

            //Logging.Logg().Debug(@"TecView::StateResponse () - TECname=" + m_tec.name_shr + @", state=" + state.ToString() + @", bRes=" + bRes.ToString() + @" - вЫход...");

            return bRes;
        }

        private void ChangeState_CurPower () {
            ClearStates ();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            states.Add((int)TecView.StatesMachine.CurrentTimeView);
            states.Add((int)TecView.StatesMachine.LastValue_TM_Gen);
            states.Add((int)TecView.StatesMachine.LastValue_TM_SN);
        }

        private void ChangeState_LastMinutes () {
            ClearStates ();

            ClearValues();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            adminValuesReceived = false;
            
            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
            //states.Add((int)StatesMachine.CurrentTimeView);
            states.Add((int)StatesMachine.LastMinutes_TM);
        }

        private void ChangeState_View () {
            ClearStates ();

            adminValuesReceived = false;

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

            //Часы...
            if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
            {
                states.Add((int)StatesMachine.Hours_Fact);
                if (currHour == true)
                    states.Add((int)StatesMachine.Hour_TM);
                else
                    ;
            }
            else
                if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                    states.Add((int)StatesMachine.Hours_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.Hours_TM);
                    else
                        ;
            //Минуты...
            if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
            {
                states.Add((int)StatesMachine.CurrentMins_Fact);
                if (currHour == true)
                    states.Add((int)StatesMachine.CurrentMin_TM);
                else
                    ;
            }
            else
                if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                    states.Add((int)StatesMachine.CurrentMins_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.CurrentMins_TM);
                    else
                        ;
            states.Add((int)StatesMachine.LastValue_TM_Gen);
            states.Add((int)StatesMachine.LastMinutes_TM);
            states.Add((int)StatesMachine.PPBRValues);
            states.Add((int)StatesMachine.AdminValues);
        }

        private void ChangeState_TMSNPower () {
            ClearStates ();

            if (m_tec.m_bSensorsStrings == false)
                states.Add((int)StatesMachine.InitSensors);
            else ;

            states.Add((int)StatesMachine.LastValue_TM_Gen);
            states.Add((int)StatesMachine.LastValue_TM_SN);
        }

        public void ChangeState()
        {
            lock (m_lockState)
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
                    case TecView.TYPE_PANEL.SOBSTV_NYZHDY:
                        ChangeState_SobstvNyzhdy();
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
                        Logging.Logg().Exception(e, @"TecView::ChangeState () - semaState.Release(1)...");
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
                                    m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)CONN_SETT_TYPE.ADMIN]);
            }
            else
                GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port),
                                    m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN]);
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
                    Logging.Logg().Exception(excpt, "TecView::GetCurrentTimeViewReponse () - (DateTime)table.Rows[0][0]");

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

        public void GetRetroHours()
        {
            lock (m_lockValue)
            {
                ClearValuesHours();

                ClearStates();

                adminValuesReceived = false;

                if ((m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    states.Add((int)StatesMachine.Hours_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.Hours_TM);
                    else
                        ;

                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().Exception(excpt, "catch - TecView::GetRetroHours () - sem.Release(1)"); }
            }
        }

        public void GetRetroValues()
        {
            lock (m_lockValue)
            {
                ClearValues();

                ClearStates();

                adminValuesReceived = false;

                if ((m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    states.Add((int)StatesMachine.RetroMins_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.RetroMins_TM);
                    else
                        ;

                if ((m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    states.Add((int)StatesMachine.Hours_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.Hours_TM);
                    else
                        ;

                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().Exception(excpt, "catch - TecView::GetRetroHours () - sem.Release(1)"); }
            }
        }

        private void getRetroMins(int indxHour)
        {
            lock (m_lockValue)
            {
                currHour = false;

                //Отладка ???
                if (indxHour < 0)
                {
                    string strMes = @"TecView::getRetroMins (indxHour = " + indxHour + @") - ...";
                    //Logging.Logg().Error(strMes);
                    //throw new Exception(strMes);
                }
                else ;
                lastHour = indxHour;

                ClearValuesMins();

                ClearStates();

                adminValuesReceived = false;

                if ((m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    states.Add((int)StatesMachine.RetroMins_Fact);
                else
                    if (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO)
                        states.Add((int)StatesMachine.RetroMins_TM);
                    else
                        ;
                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().Exception(excpt, "catch - TecView::getRetroMins () - sem.Release(1)"); }
            }
        }

        public void GetRetroMins()
        {
            getRetroMins(lastHour);
        }

        public bool zedGraphHours_MouseUpEvent (int indx) {
            bool bRes = true;

            if ((indx == serverTime.Hour) && (m_curDate.Date.Equals (serverTime.Date) == true) && (serverTime.Minute > 2))
                bRes = false;
            else
                getRetroMins(indx);

            return bRes;
        }

        private void initValuesMinLength()
        {
            if ((m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE) && (! (m_valuesMins.Length == 21))
                || (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO) && (! (m_valuesMins.Length == 21))) {
                m_valuesMins = null;
            }
            else
                if ((m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO) && (! (m_valuesMins.Length == 61))) {
                    m_valuesMins = null;
                }
                else
                    ;

            if (m_valuesMins == null) {
                int cnt = -1;
                
                switch (m_arTypeSourceData[(int)TG.ID_TIME.MINUTES]) {
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        cnt = 61;
                        break;
                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    case CONN_SETT_TYPE.DATA_AISKUE:
                    default:
                        cnt = 21;
                        break;
                }

                m_valuesMins = new valuesTEC[cnt];

                //Следовательно и для ТГ требуется изменить размер массива
                foreach (TECComponent g in m_localTECComponents)
                {
                    foreach (TG tg in g.m_listTG)
                    {
                        this.m_dictValuesTG[tg.m_id].m_powerMinutes = null;
                        this.m_dictValuesTG[tg.m_id].m_powerMinutes = new double[cnt];
                    }
                }
            }
            else
                ;
        }

        protected void ClearValuesMins()
        {
            initValuesMinLength ();

            for (int i = 0; i < m_valuesMins.Length; i++)
            {
                if (m_valuesMins[i] == null) m_valuesMins[i] = new valuesTEC(); else ;

                m_valuesMins[i].valuesFact =
                m_valuesMins[i].valuesDiviation =
                m_valuesMins[i].valuesPBR =
                m_valuesMins[i].valuesPBRe =
                m_valuesMins[i].valuesUDGe = 0;
            }

            //foreach (TECComponent g in m_tec.list_TECComponents)
            foreach (TECComponent comp in m_localTECComponents)
            {
                foreach (TG tg in comp.m_listTG)
                {
                    if ((! (m_dictValuesTG[tg.m_id].m_powerMinutes == null)) && (!(m_dictValuesTG[tg.m_id].m_powerMinutes.Length == m_valuesMins.Length)))
                        m_dictValuesTG[tg.m_id].m_powerMinutes = null;
                    else
                        ;

                    if (m_dictValuesTG[tg.m_id].m_powerMinutes == null)
                        m_dictValuesTG[tg.m_id].m_powerMinutes = new double[m_valuesMins.Length];
                    else
                        ;

                    for (int i = 0; i < m_dictValuesTG[tg.m_id].m_powerMinutes.Length; i++)
                    {
                        m_dictValuesTG[tg.m_id].m_powerMinutes[i] = -1; //Признак НЕполучения данных
                    }
                }
            }

            m_arValueCurrentTM_Gen [(int)TG.ID_TIME.MINUTES] = -1F;
        }

        //protected void ClearValuesHours(int cnt = -1)
        protected void ClearValuesHours()
        {
            int cntHours = -1;
            
            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
            {
                if (m_valuesHours.Length > 24)
                {
                    m_valuesHours = null;
                    cntHours = 24;
                }
                else
                {
                }
            }
            else
            {
                if (m_valuesHours.Length < 25)
                {
                    m_valuesHours = null;
                    cntHours = 25;
                }
                else
                {
                }
            }

            if (m_valuesHours == null)
                m_valuesHours = new valuesTEC [cntHours];
            else
                ;

            for (int i = 0; i < m_valuesHours.Length; i++)
            {
                if (m_valuesHours[i] == null) m_valuesHours[i] = new valuesTEC(); else ;
                
                m_valuesHours[i].valuesFact =
                m_valuesHours[i].valuesTMSNPsum =
                m_valuesHours[i].valuesDiviation =
                m_valuesHours[i].valuesPBR =
                m_valuesHours[i].valuesPmin =
                m_valuesHours[i].valuesPmax =
                m_valuesHours[i].valuesPBRe =
                m_valuesHours[i].valuesUDGe = 0;

                m_valuesHours[i].valuesForeignCommand = false;
            }

            //m_valuesHours.valuesFactAddon =
            //m_valuesHours.valuesDiviationAddon =
            //m_valuesHours.valuesPBRAddon =
            //m_valuesHours.valuesPBReAddon =
            //m_valuesHours.valuesUDGeAddon = 0;
            //m_valuesHours.season = seasonJumpE.None;
            //m_valuesHours.hourAddon = 0;
            //m_valuesHours.addonValues = false;

            m_arValueCurrentTM_Gen[(int)TG.ID_TIME.HOURS] = -1F;
        }

        private void ClearPBRValues()
        {
        }

        private void ClearAdminTECComponentValues(int indx)
        {
            int id = -1;

            for (int j = 0; j < m_localTECComponents.Count; j ++) {
                id = m_localTECComponents[j].m_id;
                
                if (m_dictValuesTECComponent[indx][id] == null) m_dictValuesTECComponent[indx][id] = new valuesTECComponent(); else ;

                m_dictValuesTECComponent[indx][id].valuesDiviation =
                m_dictValuesTECComponent[indx][id].valuesPBR =
                m_dictValuesTECComponent[indx][id].valuesPmin =
                m_dictValuesTECComponent[indx][id].valuesPmax =
                m_dictValuesTECComponent[indx][id].valuesPBRe =
                m_dictValuesTECComponent[indx][id].valuesUDGe = 0.0;

                m_dictValuesTECComponent[indx][id].valuesForeignCommand = false;
            }
        }

        private void ClearAdminValues()
        {
            int i = -1;

            if (! ((m_dictValuesTECComponent.Length - m_valuesHours.Length) == 1)) {
                m_dictValuesTECComponent = null;
                initDictValuesTECComponent(m_valuesHours.Length + 1);
            } else { }

            for (i = 0; i < m_valuesHours.Length; i++)
            {
                m_valuesHours[i].valuesDiviation =
                m_valuesHours[i].valuesPBR =
                m_valuesHours[i].valuesPmin =
                m_valuesHours[i].valuesPmax =
                m_valuesHours[i].valuesPBRe =
                m_valuesHours[i].valuesUDGe = 0;

                m_valuesHours[i].valuesForeignCommand = false;

                ClearAdminTECComponentValues(i);
            }

            ClearAdminTECComponentValues(i);

            initValuesMinLength ();

            for (i = 0; i < m_valuesMins.Length; i++)
                m_valuesMins[i].valuesDiviation =
                m_valuesMins[i].valuesPBR =
                m_valuesMins[i].valuesPBRe =
                m_valuesMins[i].valuesUDGe = 0;
        }

        private bool GetAdminValuesResponse(DataTable table_in)
        {
            DateTime date = m_curDate //m_pnlQuickData.dtprDate.Value.Date
                    , dtPBR;
            int hour;
            bool bSeason = false;

            double currPBRe;
            int offsetPrev = -1
                //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePPBRValuesResponse.Rows.Count
                , i = -1, j = -1,
                offsetUDG, offsetPlan, offsetLayout;

            lastLayout = "---";

            //Определить признак даты переходы сезонов (заранее, не при итерации в цикле)
            if (HAdmin.SeasonDateTime.Date.CompareTo (m_curDate.Date) == 0)
                bSeason = true;
            else
                ;

            //switch (tec.type ()) {
            //    case TEC.TEC_TYPE.COMMON:
            //        offsetPrev = -1;

            if ((indxTECComponents < 0) || //ТЭЦ
                ((!(indxTECComponents < 0)) && (m_tec.list_TECComponents[indxTECComponents].m_id > 500))) //компоненты ЩУ, ТГ
            {//Для ТЭЦ, и комопнентов ЩУ, ТГ
                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                m_tablePPBRValuesResponse = restruct_table_pbrValues(m_tablePPBRValuesResponse, m_tec.list_TECComponents, indxTECComponents);
                offsetLayout = (!(m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_localTECComponents.Count * 3) : m_tablePPBRValuesResponse.Columns.Count;

                DataTable tableAdminValuesResponse = null;
                if (bSeason == true)
                    //Сохранить таблицу с админ./знач.
                    tableAdminValuesResponse = table_in.Copy();
                else
                    ;
                table_in = restruct_table_adminValues(table_in, m_tec.list_TECComponents, indxTECComponents);

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
                                    int id = m_localTECComponents[j].m_id;

                                    if ((offsetPlan + j * 3) < m_tablePPBRValuesResponse.Columns.Count)
                                    {
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 0].ToString(), out m_dictValuesTECComponent[0][id].valuesPBR) == false)
                                            m_dictValuesTECComponent[0][id].valuesPBR = 0F;
                                        else
                                            ;
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 1].ToString(), out m_dictValuesTECComponent[0][id].valuesPmin) == false)
                                            m_dictValuesTECComponent[0][id].valuesPmin = 0F;
                                        else
                                            ;
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 2].ToString(), out m_dictValuesTECComponent[0][id].valuesPmax) == false)
                                            m_dictValuesTECComponent[0][id].valuesPmax = 0F;
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[0][id].valuesPBR = 0.0;
                                        m_dictValuesTECComponent[0][id].valuesPmin = 0.0;
                                        m_dictValuesTECComponent[0][id].valuesPmax = 0.0;
                                    }
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) { Logging.Logg().Exception(excpt, "catch - PanelTecViewBase.GetAdminValuesResponse () - ..."); }
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
                    {//значение ПБР даты/времени в этой записи ЕСТЬ
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

                            //GetSeasonHours(ref prev_hour, ref hour);
                            hour += GetSeasonHourOffset(hour);

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_localTECComponents.Count; j++)
                            {
                                int id = -1;
                                
                                try
                                {
                                    id = m_localTECComponents[j].m_id;

                                    if ((offsetPlan + (j * 3) < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                    {
                                        //valuesPBR[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        m_dictValuesTECComponent[hour - 0][id].valuesPBR = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        //valuesPmin[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        m_dictValuesTECComponent[hour - 0][id].valuesPmin = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        //valuesPmax[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                        m_dictValuesTECComponent[hour - 0][id].valuesPmax = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[hour - 0][id].valuesPBR = 0.0;
                                        //m_dictValuesTECComponent[id].valuesPmin[hour - 1] = 0.0;
                                        //m_dictValuesTECComponent[id].valuesPmax[hour - 1] = 0.0;
                                    }

                                    DataRow[] row_in;
                                    //Копия снизу по разбору ТЭЦ в целом + копии 'AdminTS'
                                    if (bSeason == true) {
                                        if ((hour - 1) == (HAdmin.SeasonDateTime.Hour + 1)) {
                                            m_dictValuesTECComponent[hour - 1][id].valuesPBR = m_dictValuesTECComponent[hour - 2][id].valuesPBR;
                                            m_dictValuesTECComponent[hour - 1][id].valuesPmin = m_dictValuesTECComponent[hour - 2][id].valuesPmin;
                                            m_dictValuesTECComponent[hour - 1][id].valuesPmax = m_dictValuesTECComponent[hour - 2][id].valuesPmax;
                                        } else {
                                        }

                                        if (hour == HAdmin.SeasonDateTime.Hour)
                                        {
                                            //row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString(@"yyyy-MM-dd HH:mm:ss") + @"'");
                                            row_in = tableAdminValuesResponse.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ID_COMPONENT = " + id.ToString ());
                                            //Копия в 'AdminRS'
                                            if (row_in.Length > 0)
                                            {
                                                int h = -1;
                                                foreach (DataRow r in row_in)
                                                {
                                                    h = dtPBR.Hour;
                                                    //GetSeasonHourIndex(Int32.Parse(r[@"SEASON_" + id.ToString ()].ToString()), ref h);
                                                    GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString()), ref h);

                                                    m_dictValuesTECComponent[h - 0][id].valuesForeignCommand = (byte)r[@"FC"] == 1;

                                                    //m_dictValuesTECComponent[h - 0][id].valuesREC = (double)r[@"REC_" + id.ToString()];
                                                    m_dictValuesTECComponent[h - 0][id].valuesREC = (double)r[@"REC"];
                                                    //m_dictValuesTECComponent[h - 0][id].valuesISPER = (int)r[@"IS_PER_" + id.ToString()];
                                                    m_dictValuesTECComponent[h - 0][id].valuesISPER = (int)r[@"IS_PER"];
                                                    //m_dictValuesTECComponent[h - 0][id].valuesDIV = (double)r[@"DIVIAT_" + id.ToString()];
                                                    m_dictValuesTECComponent[h - 0][id].valuesDIV = (double)r[@"DIVIAT"];
                                                }
                                            }
                                            else
                                            {//Ошибка ... ???
                                                Logging.Logg().Error(@"TecView::GetAdminValueResponse () - ... нет ни одной записи для [HAdmin.SeasonDateTime.Hour] = " + hour);
                                            }
                                        }
                                        else
                                        {
                                        }
                                    } else {
                                    }

                                    if (((!(hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)) ||
                                        (bSeason == false))
                                    {                                    
                                        row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                        //if (i < table_in.Rows.Count)
                                        if (row_in.Length > 0)
                                        {
                                            if (row_in.Length > 1)
                                                ; //Ошибка....
                                            else
                                                ;

                                            if (!(row_in[0]["FC_" + id.ToString()] is System.DBNull))
                                                m_dictValuesTECComponent[hour - 0][id].valuesForeignCommand = (byte)row_in[0]["FC_" + id.ToString()] == 1;
                                            else
                                                m_dictValuesTECComponent[hour - 0][id].valuesForeignCommand = false;

                                            //if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                            if (!(row_in[0]["REC_" + id.ToString ()] is System.DBNull))
                                                //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                                //valuesREC[j, hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                                //m_dictValuesTECComponent[hour - 0][id].valuesREC = (double)row_in[0][offsetUDG + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id].valuesREC = (double)row_in[0]["REC_" + id.ToString()];
                                            else
                                                //valuesREC[j, hour - 1] = 0;
                                                m_dictValuesTECComponent[hour - 0][id].valuesREC = 0.0;

                                            //if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                            if (!(row_in[0]["IS_PER_" + id.ToString ()] is System.DBNull))
                                                //m_dictValuesTECComponent[hour - 0][id].valuesISPER = (int)row_in[0][offsetUDG + 1 + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id].valuesISPER = (int)row_in[0]["IS_PER_" + id.ToString()];
                                            else
                                                ;

                                            //if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                            if (!(row_in[0]["DIVIAT_" + id.ToString ()] is System.DBNull))
                                                //m_dictValuesTECComponent[hour - 0][id].valuesDIV = (double)row_in[0][offsetUDG + 2 + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id].valuesDIV = (double)row_in[0]["DIVIAT_" + id.ToString()];
                                            else
                                                ;
                                        }
                                        else
                                        {
                                            m_dictValuesTECComponent[hour - 0][id].valuesForeignCommand = false;

                                            m_dictValuesTECComponent[hour - 0][id].valuesREC = 0.0;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
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
                            Logging.Logg().Exception(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
                        }
                    }
                    else
                    {//значение ПБР даты/времени в этой записи НЕТ - использовать админ. дату/время
                        int cntFields = 5; //REC, IS_PER, DIV, SEASON, FC

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
                                int id = -1;

                                try
                                {
                                    id = m_localTECComponents[j].m_id;

                                    m_dictValuesTECComponent[hour - 0][id].valuesPBR = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + 4 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id].valuesForeignCommand = (byte)table_in.Rows[i][offsetUDG + 4 + j * cntFields] == 1;
                                        else
                                            m_dictValuesTECComponent[hour - 0][id].valuesForeignCommand = false;
                                        
                                        if (!(table_in.Rows[i][offsetUDG + j * cntFields] is System.DBNull))
                                            //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * cntFields] is System.DBNull)))
                                            m_dictValuesTECComponent[hour - 0][id].valuesREC = (double)table_in.Rows[i][offsetUDG + j * cntFields];
                                        else
                                            m_dictValuesTECComponent[hour - 0][id].valuesREC = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id].valuesISPER = (int)table_in.Rows[i][offsetUDG + 1 + j * cntFields];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id].valuesDIV = (double)table_in.Rows[i][offsetUDG + 2 + j * cntFields];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[hour - 0][id].valuesREC = 0.0;
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
                //for (i = 0; i < 24; i++)
                for (i = 0; i < m_valuesHours.Length; i++) //??? m_valuesHours.Length == m_dictValuesTECComponent.Length + 1
                {
                    //i = ii - 1;
                    for (j = 0; j < m_localTECComponents.Count; j++)
                    {
                        int id = m_localTECComponents [j].m_id;
                        
                        m_valuesHours[i].valuesPBR += m_dictValuesTECComponent[i + 1][id].valuesPBR;
                        m_valuesHours[i].valuesPmin += m_dictValuesTECComponent[i + 1][id].valuesPmin;
                        m_valuesHours[i].valuesPmax += m_dictValuesTECComponent[i + 1][id].valuesPmax;
                        if (i == 0)
                        {
                            currPBRe = (m_dictValuesTECComponent[i + 1][id].valuesPBR + m_dictValuesTECComponent[0][id].valuesPBR) / 2;
                        }
                        else
                        {
                            currPBRe = (m_dictValuesTECComponent[i + 1][id].valuesPBR + m_dictValuesTECComponent[i][id].valuesPBR) / 2;                            
                        }

                        m_dictValuesTECComponent[i + 1][id].valuesPBRe = currPBRe;
                        m_valuesHours[i].valuesPBRe += currPBRe;

                        m_valuesHours[i].valuesForeignCommand |= m_dictValuesTECComponent[i + 1][id].valuesForeignCommand;

                        m_valuesHours[i].valuesREC += m_dictValuesTECComponent[i + 1][id].valuesREC;

                        m_dictValuesTECComponent[i + 1][id].valuesUDGe = currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC;
                        m_valuesHours[i].valuesUDGe += currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC;

                        if (m_dictValuesTECComponent[i + 1][id].valuesISPER == 1)
                        {
                            m_dictValuesTECComponent[i + 1][id].valuesDiviation = (currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC) * m_dictValuesTECComponent[i + 1][id].valuesDIV / 100;
                        }
                        else {
                            m_dictValuesTECComponent[i + 1][id].valuesDiviation = m_dictValuesTECComponent[i + 1][id].valuesDIV;
                        }
                        m_valuesHours[i].valuesDiviation += m_dictValuesTECComponent[i + 1][id].valuesDiviation;
                    }
                    /*m_valuesHours[i].valuesPBR = 0.20;
                    m_valuesHours[i].valuesPBRe = 0.20;
                    m_valuesHours[i].valuesUDGe = 0.20;
                    m_valuesHours[i].valuesDiviation = 0.05;*/
                }

                //if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                //{
                //    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                //}
                //else
                //    ;
            }
            else
            {//Для ГТП
                int lValues = m_valuesHours.Length + 1;

                double[] valuesPBR = new double[lValues];
                double[] valuesPmin = new double[lValues];
                double[] valuesPmax = new double[lValues];
                bool[] valuesForeignCmd = new bool[lValues];
                double[] valuesREC = new double[lValues];                
                int[] valuesISPER = new int[lValues];
                double[] valuesDIV = new double[lValues];

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
                                valuesPBR[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
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

                            //GetSeasonHours (ref prev_hour, ref hour);
                            hour += GetSeasonHourOffset(hour);

                            if ((offsetPlan < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan] is System.DBNull)))
                            {
                                valuesPBR[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
                            }
                            else
                                ;

                            DataRow[] row_in;
                            //Копия сверху по разбору компонента ТЭЦ + копии 'AdminTS'
                            if (bSeason == true)
                            {
                                if ((hour - 1) == (HAdmin.SeasonDateTime.Hour + 1))
                                {
                                    valuesPBR[hour - 1] = valuesPBR[hour - 2];
                                    valuesPmin[hour - 1] = valuesPmin[hour - 2];
                                    valuesPmax[hour - 1] = valuesPmax[hour - 2];
                                }
                                else
                                {
                                }

                                if (hour == HAdmin.SeasonDateTime.Hour)
                                {
                                    row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    //Копия в 'AdminRS'
                                    if (row_in.Length > 0)
                                    {
                                        foreach (DataRow r in row_in)
                                        {
                                            hour = dtPBR.Hour;
                                            GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString()), ref hour);

                                            valuesForeignCmd[hour - 1] = (byte)r[@"FC"] == 1;

                                            valuesREC [hour - 1] = (double)r[@"REC"];
                                            valuesISPER [hour - 1] = (int)r[@"IS_PER"];
                                            valuesDIV[hour - 1] = (double)r[@"DIVIAT"];
                                        }
                                    }
                                    else
                                    {//Ошибка ... ???
                                        Logging.Logg().Error(@"TecView::GetAdminValueResponse () - ... нет ни одной записи для [HAdmin.SeasonDateTime.Hour] = " + hour);
                                    }
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            if (((!(hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)) ||
                                        (bSeason == false))
                            {
                                row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                //if (i < table_in.Rows.Count)
                                if (row_in.Length > 0)
                                {
                                    if (row_in.Length > 1)
                                        ; //Ошибка....
                                    else
                                        ;

                                    if (!(row_in[0][offsetUDG + 5] is System.DBNull))
                                        valuesForeignCmd[hour - 0] = (byte)row_in[0][offsetUDG + 5] == 1;
                                    else
                                        valuesForeignCmd[hour - 0] = false;

                                    if (!(row_in[0][offsetUDG] is System.DBNull))
                                        //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG] is System.DBNull)))
                                        valuesREC[hour - 0] = (double)row_in[0][offsetUDG + 0];
                                    else
                                        valuesREC[hour - 0] = 0;

                                    if (!(row_in[0][offsetUDG + 1] is System.DBNull))
                                        valuesISPER[hour - 0] = (int)row_in[0][offsetUDG + 1];
                                    else
                                        ;

                                    if (!(row_in[0][offsetUDG + 2] is System.DBNull))
                                        valuesDIV[hour - 0] = (double)row_in[0][offsetUDG + 2];
                                    else
                                        ;
                                }
                                else
                                {
                                    valuesForeignCmd[hour - 0] = false;

                                    valuesREC[hour - 0] = 0;
                                    //valuesISPER[hour - 1] = 0;
                                    //valuesDIV[hour - 1] = 0;
                                }
                            }
                            else
                            {
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
                            Logging.Logg().Exception(e, "PanelTecViewBase::GetAdminValueResponse ()...");
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

                            //GetSeasonHours (ref prev_hour, ref hour);
                            //hour += GetSeasonHourOffset(hour);
                            GetSeasonHourIndex(Int32.Parse(table_in.Rows[i]["SEASON"].ToString()), ref hour);

                            valuesPBR[hour - 0] = 0;

                            if (i < table_in.Rows.Count)
                            {
                                if (!(table_in.Rows[i][@"FC"] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                    valuesForeignCmd[hour - 0] = (byte)table_in.Rows[i][@"FC"] == 1;
                                else
                                    valuesREC[hour - 0] = 0;

                                if (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                    valuesREC[hour - 0] = (double)table_in.Rows[i][offsetUDG + 0];
                                else
                                    valuesREC[hour - 0] = 0;

                                if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
                                    valuesISPER[hour - 0] = (int)table_in.Rows[i][offsetUDG + 1];
                                else
                                    ;

                                if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
                                    valuesDIV[hour - 0] = (double)table_in.Rows[i][offsetUDG + 2];
                                else
                                    ;
                            }
                            else
                            {
                                valuesForeignCmd [hour - 0] = false;

                                valuesREC[hour - 0] = 0;
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

                for (i = 0; i < m_valuesHours.Length; i++)
                {
                    m_valuesHours[i].valuesPBR = valuesPBR[i + 1];
                    m_valuesHours[i].valuesPmin = valuesPmin[i + 1];
                    m_valuesHours[i].valuesPmax = valuesPmax[i + 1];

                    if (i == 0)
                    {
                        currPBRe = (valuesPBR[i + 1] + valuesPBR[0]) / 2;
                        m_valuesHours[i].valuesPBRe = currPBRe;
                    }
                    else
                    {
                        currPBRe = (valuesPBR[i + 1] + valuesPBR[i - 0]) / 2;
                        m_valuesHours[i].valuesPBRe = currPBRe;
                    }

                    m_valuesHours[i].valuesForeignCommand = valuesForeignCmd [i + 1];

                    m_valuesHours[i].valuesREC = valuesREC[i + 1];

                    m_valuesHours[i].valuesUDGe = currPBRe + valuesREC[i + 1];

                    if (valuesISPER[i + 1] == 1)
                        m_valuesHours[i].valuesDiviation = (currPBRe + valuesREC[i + 1]) * valuesDIV[i + 1] / 100;
                    else
                        m_valuesHours[i].valuesDiviation = valuesDIV[i + 1];
                }

                //if (m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                //{
                //    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                //}
                //else
                //    ;
            }

            hour = lastHour;

            //Отладка ???
            if (hour == 0)
                ; //hour = 1;
            else
                ;

            if (hour == m_valuesHours.Length)
                hour = m_valuesHours.Length - 1;
            else
                ;

            for (i = 0; i < m_valuesMins.Length; i++)
            {
                //??? [hour - 1] vs [hour - 0] 26.10.2014 с учетом того, что перенесена запись '00:00' из [24] -> [0]
                m_valuesMins[i].valuesPBR = m_valuesHours[hour - 0].valuesPBR;
                m_valuesMins[i].valuesPBRe = m_valuesHours[hour - 0].valuesPBRe;
                m_valuesMins[i].valuesUDGe = m_valuesHours[hour - 0].valuesUDGe;
                m_valuesMins[i].valuesDiviation = m_valuesHours[hour - 0].valuesDiviation;
            }

            return true;
        }

        private void ComputeRecomendation(int hour)
        {
            if (! (hour < m_valuesHours.Length))
                //???
                hour = m_valuesHours.Length - 1;
            else
                ;

            if (m_valuesHours[hour].valuesUDGe == 0)
            {
                recomendation = 0;
                return;
            }

            if (currHour == false)
            {
                recomendation = m_valuesHours[hour].valuesUDGe;
                return;
            } else {
            }

            if (lastMin < 2)
            {
                recomendation = m_valuesHours[hour].valuesUDGe;
                return;
            }
            else {
            }

            double factSum = 0;
            for (int i = 1; i < lastMin; i++)
                factSum += m_valuesMins[i].valuesFact;

            if (lastMin == m_valuesMins.Length)
                recomendation = 0;
            else
                recomendation = (m_valuesHours[hour].valuesUDGe * (m_valuesMins.Length - 1) - factSum) / ((m_valuesMins.Length - 1) - (lastMin - 1));

            if (recomendation < 0)
                recomendation = 0;
            else
                ;

            Logging.Logg().Debug(@"recomendation=" + recomendation.ToString(@"F3")
                                + @" (factSum=" + factSum.ToString(@"F3")
                                + @"; valuesUDGe=" + m_valuesHours[hour].valuesUDGe.ToString(@"F3")
                                + @") [" + hour + @", " + lastMin + @"]");
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

        private bool GetHoursFactResponse(DataTable table)
        {
            int i, j
                , half = 0 //Индекс получаса
                , hour = 0
                , prev_season = 0, season = 0, offset_season = 0;
            double hourVal = 0, halfVal = 0, value; //Значения за период времени
            DateTime dt , dtNeeded, dtServer;
            dt = dtNeeded = DateTime.Now;

            dtServer = serverTime;
            if ((currHour == true) && (dtServer.Minute < 2))
                dtServer = dtServer.AddMinutes(-1 * (dtServer.Minute + 1));
            else
                ;

            double[, ,] powerHourHalf = new double[listTG.Count, 2, m_valuesHours.Length];

            if (currHour == true)
                lastHour = lastReceivedHour = 0;
            else
                ;

            /*Form2 f2 = new Form2();
            f2.FillHourTable(table);*/

            //Предполагаем, что ошибок нет ???
            lastHourHalfError = lastHourError = false;

            //Предполагаем, что не получено ни одного значения ни за один получасовой интервал ???
            //foreach (TECComponent g in m_tec.list_TECComponents)
            //{
                i = 0;
                foreach (TG t in listTG)
                {
                    //t.power.Length == t.receivedHourHalf1.Length == t.receivedHourHalf2.Length
                    for (j = 0; j < m_valuesHours.Length; j++)
                    {
                        powerHourHalf[i, 0, j] = powerHourHalf[i, 1, j] = -1F;
                    }

                    i ++;
                }
            //}

            //Проверка наличия в таблице необходимых полей
            if (CheckNameFieldsOfTable(table, new string[] { @"ID", @"DATA_DATE", @"SEASON", @"VALUE0" }) == false)
                return false;
            else
                ;

            //Проверка наличия в таблице строк
            if (! (table.Rows.Count > 0))
            {//Ошибка - завершаем выполнение функции
                if (currHour == true)
                {//Отображается текущий час
                    if (! (dtServer.Hour == 0))
                    {//Не начало суток
                        lastHour = lastReceivedHour = dtServer.Hour;
                        //Признак частичной ошибки
                        lastHourError = true;
                    }
                    else
                        ;
                }
                else
                    ;

                //Завершаем выполнение функции, но возращаем признак успеха ???
                return true;
            }
            else
                ;

            i = 0;
            DataRow [] tgRows = null;
            //Цикл по ТГ 
            foreach (TG tg in listTG)
            {                
                tgRows = table.Select(@"ID=" + tg.ids_fact[(int)TG.ID_TIME.HOURS], @"DATA_DATE");

                hour = -1;
                offset_season = 0;
                foreach (DataRow r in tgRows) {
                    try
                    {
                        if (DateTime.TryParse(r[@"DATA_DATE"].ToString(), out dt) == false)
                            return false;
                        else
                            ;

                        if (int.TryParse(r[@"SEASON"].ToString(), out season) == false)
                            return false;
                        else
                            ;

                        if (hour < 0)
                        {
                            if (season > DateTime.Now.Year)
                                GetSeason(dt, season, out season);
                            else
                                ;
                            prev_season = season;
                            hour = 0;
                            dtNeeded = dt;
                        }
                        else
                            ;

                        ////Отладка ???
                        ////if ((dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0) && (! (dt.Hour < HAdmin.SeasonDateTime.Hour)))
                        //if (((dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0) && ((dt.Hour + dt.Minute / 30) > HAdmin.SeasonDateTime.Hour)) ||
                        //    ((dt.Hour == 0) && (dt.Minute == 0) && (dt.AddDays(-1).CompareTo(HAdmin.SeasonDateTime.Date) == 0)))
                        //{
                        //    if (HAdmin.SeasonAction < 0)
                        //        season = (int)HAdmin.seasonJumpE.SummerToWinter;
                        //    else
                        //        if (HAdmin.SeasonAction > 0)
                        //            season = (int)HAdmin.seasonJumpE.WinterToSummer;
                        //        else
                        //            season = (int)HAdmin.seasonJumpE.None;

                        //    season += DateTime.Now.Year * 2;
                        //}
                        //else
                        //{
                        //}
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"PanelTecViewBase::GetHoursResponse () - ...");
                    }

                    if (double.TryParse(r[@"VALUE0"].ToString(), out value) == false)
                        return false;
                    else
                        ;

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

                    //??? якобы для перехода через границу суток
                    dtNeeded = dt;

                    hour = (dt.Hour + dt.Minute / 30);
                    if (hour == 0)
                        if (!(dt.Date == dtServer.Date))
                            //hour = m_valuesHours.Length;
                            hour = 24;
                        else
                            ;
                    else
                        ;

                    //if (!(prev_season == (int)HAdmin.seasonJumpE.None))
                    //{
                        //Отладка ???
                        if (season > DateTime.Now.Year)
                            GetSeason(dt, season, out season);
                        else
                            ;

                        if ((! (season == (int)HAdmin.seasonJumpE.None)) && (! (prev_season == season)))
                        {
                            if (offset_season == 1)
                            {//Ошибка ??? 2 перехода за сутки

                            }
                            else
                            {
                                if (prev_season == (int)HAdmin.seasonJumpE.None)
                                {
                                    if (season == (int)HAdmin.seasonJumpE.SummerToWinter)
                                        offset_season = 1;
                                    else
                                        //prev_season == (int)HAdmin.seasonJumpE.WinterToSummer
                                        ; // offset_season = -1; ??? 26.10.2014 нет перехода зима-лето
                                }
                                else
                                    if (prev_season == (int)HAdmin.seasonJumpE.WinterToSummer)
                                        offset_season = 1;
                                    else
                                        //prev_season == (int)HAdmin.seasonJumpE.SummerToWinter
                                        ; // offset_season = -1; ??? 26.10.2014 нет перехода зима-лето
                            }
                        }
                        else
                            ;
                    //} else ;

                    //Отладка ???
                    if (season > DateTime.Now.Year)
                        GetSeason(dt, season, out season);
                    else
                        ;

                    prev_season = season;

                    powerHourHalf[i, ((dt.Minute / 30) == 0) ? 1 : 0, hour - 1/* + offset_season*/] = (value / 2000);
                }

                //??? якобы для перехода через границу суток
                dtNeeded = dtNeeded.AddMinutes(30);

                i ++;
            }

            i = 0;
            for (hour = 0; hour < m_valuesHours.Length; hour ++) {
                hourVal = -1F;

                for (j = 0; j < 2; j ++) {
                    halfVal = -1F;
                    i = 0; //Индекс ТГ
                    foreach (TG tg in listTG)
                    {
                        if (powerHourHalf [i, j, hour] < 0) {
                            //Нет данных для ТГ
                            //break;
                        } else {
                            if (halfVal < 0) halfVal = 0F; else ;
                            halfVal += powerHourHalf [i, j, hour];
                        }
                        i ++;
                    }

                    if (i < listTG.Count) {
                        //Нет данных для одного из ТГ
                        //break;
                    } else {
                        //Для всех ТГ есть данные
                        if (! (halfVal < 0))
                        {
                            if (hourVal < 0) hourVal = 0F; else ;
                            hourVal += halfVal;
                        } else {
                            //Нет данных за получас ни для одного ТГ
                            if (currHour == true)
                                break;
                            else
                                ;
                        }
                    }
                }

                //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - hour=" + hour + @"; indxHalf=" + j + @"; halfVal=" + halfVal + @"; hourVal=" + hourVal);

                if (j < 2)
                {//Нет данных за один из получасов
                    if (! (hour > serverTime.Hour)) {
                        break;
                    }
                    else
                    {//hour > m_curDate.Hour
                        if (j == 0)
                            //1-ый получас
                            ; //break;
                        else
                            //2-ой получас
                            ;
                    }
                }
                else
                {
                    if (! (hourVal < 0))
                        m_valuesHours [hour].valuesFact += hourVal;
                    else
                        ; //Нет данных за час
                }
            }

            if (hour == m_valuesHours.Length)
                hour = 24;
            else
                if (hour == 0)
                    ; //hour = 1;
                else
                    ;

            //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - hour=" + hour);

            if (currHour == true)
            {//Отображение тек./часа
                if (hour < 0)
                {
                    string strMes = @"TecView::GetHoursFactResponse (hour = " + hour + @") - ...";
                    //Logging.Logg().Error(strMes);
                    //throw new Exception(strMes);
                }
                else ;

                lastHour = hour;

                if (lastHour < dtServer.Hour)
                {//Ошибка получения часовых значений
                    lastHourError = true;
                    lastHour = dtServer.Hour;
                }
                else
                {
                    if ((dtServer.Hour == 0) && (!(lastHour == 24)) && (!(dtNeeded.Date == dtServer.Date)))
                    {//Переход через границу суток
                        lastHourError = true;
                        lastHour = 24;
                    }
                    else
                    {
                        if (! (lastHour == 0))
                        {//Не начало суток
                            for (i = 0; i < listTG.Count; i++)
                            {
                                if ((half & 1) == 1)
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h1 " + sensorId2TG[i].receivedHourHalf1[lastHour - 1].ToString());
                                    if (powerHourHalf[i, 0, lastHour - 1] < 0)
                                    {//Ошибка получения 1-ого получасового значения
                                        lastHourHalfError = true;
                                        break;
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h2 " + sensorId2TG[i].receivedHourHalf2[lastHour - 1].ToString());
                                    if (powerHourHalf[i, 1, lastHour - 1] < 0)
                                    {
                                        //Ошибка получения 2-ого получасового значения
                                        lastHourHalfError = true;
                                        break;
                                    }
                                    else
                                        ;
                                }
                            }
                        }
                        else
                            ; //Начало суток
                    }
                }
            }
            else
                ; //Отображение ретроспективы

            //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - lastHour=" + lastHour);

            lastReceivedHour = lastHour;

            return true;
        }

        private bool GetHourTMResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetHoursTMResponse (lastHour=" + lastHour + @") - Rows.Count=" + table.Rows.Count);

            int iRes = -1
                , hour = -1;

            string [] checkFields = null;

            switch (TEC.s_SourceSOTIASSO) {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"ID", @"VALUE", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { @"VALUE", @"HOUR" };
                    break;
                default:
                    break;
            }

            iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            if (iRes == -1)
                ;
            else {
                if (table.Rows.Count == 0)
                    if (serverTime.Minute < 3)
                    {
                        //...в начале часа значений может не быть ???
                        iRes = 1;
                    }
                    else
                        iRes = -1;
                else ;

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO)
                    {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            hour = lastHour;

                            List<string> listSensors = new List<string>(m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES).Split(','));
                            double[] valHours = new double[listSensors.Count + 1];
                            //60 мин * 60 сек = 1 час
                            valHours = avgInterval(table
                                                    , m_curDate.Date.AddHours(hour)
                                                    , 60 * 60
                                                    , listSensors
                                                    , out iRes);
                            //if (iRes == 0)
                                m_valuesHours[hour].valuesFact = valHours[listSensors.Count];
                            //else ;
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                            double val = -1F;

                            foreach (DataRow r in table.Rows)
                            {
                                if (Int32.TryParse(r[@"HOUR"].ToString(), out hour) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                if (double.TryParse(r[@"VALUE"].ToString(), out val) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                m_valuesHours[hour].valuesFact += val;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ; //Нет строк во вХодной таблице для обработки
            }

            switch (iRes)
            {
                case -12:
                    iRes = 0;
                    break;
                case -2:
                    if (! (hour < serverTime.Hour))
                        iRes = 0;
                    else
                        ;
                    break;
                default:
                    break;
            }

            if (iRes < 0)
                ;
            else
                if (currHour == true)
                    if (hour < 0)
                    {
                        string strMes = @"TecView::GetHourTMResponse () - hour = " + hour + @" ...";
                        //Logging.Logg().Error(strMes);
                        //throw new Exception(strMes);
                    }
                    else ;
                else
                    ;

            return
                true
                ;
        }

        private bool GetHoursTMResponse(DataTable table, bool bErrorCritical = true)
        {
            int iRes = -1
                , hour = -1;
            double val = -1F;

            string[] checkFields = null;

            switch (TEC.s_SourceSOTIASSO)
            {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"ID", @"VALUE", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { @"VALUE", @"HOUR" };
                    break;
                default:
                    break;
            }

            iRes = !(checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            if (iRes == 0) {
                //Logging.Logg().Debug(@"TecView::GetHoursTMResponse (lastHour=" + lastHour + @") - Rows.Count=" + table.Rows.Count);

                if (table.Rows.Count == 0)
                    if (serverTime.Minute < 3)
                    {
                        //...в начале часа значений может не быть ???
                        iRes = 1;
                    }
                    else
                        iRes = -1;
                else ;

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO)
                    {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            List <string> listSensors = new List <string> (m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES).Split (','));
                            double[] valHours = new double[listSensors.Count];
                            for (hour = 0; (hour < (m_valuesHours.Length + 0)) && (iRes == 0); hour ++)
                            {
                                valHours = avgInterval (table
                                                        , m_curDate.Date.AddHours(hour)
                                                        , 60 * 60
                                                        , listSensors
                                                        , out iRes);
                                if (iRes == 0)
                                    m_valuesHours[hour].valuesFact = valHours[listSensors.Count];
                                else
                                    ;

                                //Console.WriteLine (@"TecView::GetHoursTMResponse () - hour=" + hour + @", iRes=" + iRes);
                            }
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                            foreach (DataRow r in table.Rows)
                            {
                                if (Int32.TryParse(r[@"HOUR"].ToString(), out hour) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                if (double.TryParse(r[@"VALUE"].ToString(), out val) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                m_valuesHours[hour].valuesFact += val;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ;
            }
            else
                ;

            switch (iRes)
            {
                case -12:
                    iRes = 0;
                    break;
                case -2:
                    if (! (hour < serverTime.Hour))
                        iRes = 0;
                    else
                        ;
                    break;
                default:
                    break;
            }

            if (iRes < 0)
            {
                if (bErrorCritical == true)
                    lastHour = serverTime.Hour; //24
                else
                    ;
            }
            else
            {
                if (bErrorCritical == true)
                    lastHourError =
                    lastHourHalfError = false;
                else
                    ;

                if (currHour == true)
                {
                    if (hour < 0)
                    {
                        string strMes = @"TecView::GetHoursTMResponse () - hour = " + hour + @" ...";
                        //Logging.Logg().Error(strMes);
                        //throw new Exception(strMes);
                    }
                    else ;

                    if (bErrorCritical == true)
                        if (iRes == 0)
                            switch (TEC.s_SourceSOTIASSO)
                            {
                                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                                    switch (hour)
                                    {
                                        case 0:
                                            lastHour = 0;
                                            break;
                                        case 1:
                                            lastHour = 0;
                                            m_valuesHours[lastHour + 1].valuesFact = 0F;
                                            break;
                                        default:                                    
                                            lastHour = hour - 2;
                                            m_valuesHours[lastHour + 1].valuesFact =
                                            m_valuesHours[lastHour + 2].valuesFact = 0F;
                                            break;
                                    }
                                    break;
                                case TEC.SOURCE_SOTIASSO.AVERAGE:
                                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                                    break;
                                default:
                                    break;
                            }
                        else
                            if (iRes == 1)
                                lastHour = serverTime.AddHours (-1).Hour + 1;
                            else
                                ;
                    else
                        ;

                    ////НЕ отображать значения за текущий час ???
                    //if (lastHour < (m_valuesHours.Length - 1))
                    //    m_valuesHours[lastHour].valuesFact = 0F;
                    //else ;
                }
                else
                    ;
            }

            return (! (iRes < 0)) || (bErrorCritical == false);
        }

        private bool GetHoursTMSNPsumResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1
                , hour = -1;

            if (table.Rows.Count > 0)
                for (i = 0; i < table.Rows.Count; i++)
                {
                    hour = Int32.Parse(table.Rows[i][@"HOUR"].ToString());

                    m_valuesHours[hour].valuesTMSNPsum = double.Parse(table.Rows[i][@"VALUE"].ToString());
                }
            else
                bRes = false;

            return bRes;
        }

        private bool GetLastMinutesTMResponse(DataTable table_in, DateTime dtReq)
        {
            int iRes = -1;
            int i = -1,
                hour = -1
                ////26.10.2014 u/ ???
                //, offsetUTC = (int)HAdmin.GetUTCOffsetOfMoscowTimeZone().TotalHours
                ;
            double val = -1F;
            DateTime dtVal = DateTime.Now;
            string [] checkFields = null;

            switch (TEC.s_SourceSOTIASSO) {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"ID", @"value", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { @"ID", @"VALUE", @"last_changed_at" };
                    break;
                default:
                    break;
            }

            iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table_in, checkFields) == true ? 0 : -1 : -1;

            if (iRes == -1)
                ;
            else {
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                {
                    DataRow []tgRows = null;

                    if (indxTECComponents < 0)
                    {
                        foreach (TECComponent g in m_localTECComponents)
                        {
                            foreach (TG tg in g.m_listTG)
                            {
                                //for (i = 0; i < tg.m_power_LastMinutesTM.Length; i++)
                                //{
                                //    tg.m_power_LastMinutesTM[i] = 0;
                                //}

                                tgRows = table_in.Select(@"[ID]=" + tg.id_tm);

                                for (i = 0; i < tgRows.Length; i++)
                                {
                                    if (!(tgRows[i]["value"] is DBNull))
                                        if (double.TryParse(tgRows[i]["value"].ToString(), out val) == false)
                                            return false;
                                        else
                                            ;
                                    else
                                        val = 0F;

                                    //if ((!(value < 1)) && (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false))
                                    if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                        return false;
                                    else
                                        ;

                                    ////26.10.2014 u/ ???
                                    //dtVal = dtVal.AddHours(offsetUTC);
                                    hour = dtVal.Hour + 1; //Т.к. мин.59 из прошедшего часа
                                    //if (!(hour < 24)) hour -= 24; else ;
                                    if ((hour > 0) && (! (hour > m_valuesHours.Length)))
                                    {
                                        m_dictValuesTG[tg.m_id].m_power_LastMinutesTM[hour - 0] = val;

                                        //Запрос с учетом значения перехода через сутки
                                        if (val > 1)
                                        {
                                            m_valuesHours[hour - 1].valuesLastMinutesTM += val;
                                            m_dictValuesTECComponent[hour - 0][tg.m_id_owner_gtp].valuesLastMinutesTM += val;
                                        }
                                        else
                                            ;
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
                            //for (i = 0; i < comp.m_listTG [0].m_power_LastMinutesTM.Length; i++)
                            //{
                            //    comp.m_listTG[0].m_power_LastMinutesTM[i] = 0;
                            //}

                            tgRows = table_in.Select(@"[ID]=" + comp.m_listTG[0].id_tm);

                            for (i = 0; i < tgRows.Length; i++)
                            {
                                if (tgRows[i] == null)
                                    continue;
                                else
                                    ;

                                try
                                {
                                    if (double.TryParse(tgRows[i]["value"].ToString(), out val) == false)
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
                                    Logging.Logg().Exception(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                                    dtVal = DateTime.Now.Date;
                                }

                                ////26.10.2014 u/ ???
                                //dtVal = dtVal.AddHours(offsetUTC);
                                hour = dtVal.Hour + 1;
                                if ((hour > 0) && (! (hour > m_valuesHours.Length)))
                                {
                                    m_dictValuesTG[comp.m_listTG[0].m_id].m_power_LastMinutesTM[hour - 0] = val;

                                    if (val > 1)
                                        m_valuesHours[hour - 1].valuesLastMinutesTM += val;
                                    else
                                    ;
                                } else ;
                            }
                        }
                    }
                }
                else {
                    if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    {
                        dtVal = dtReq.Date.AddMinutes(59);
                        List<string> listSensors = new List<string>(m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES).Split(','));
                        int[] arIds = new int[listSensors.Count]
                            , arOwnerGTPIds = new int[listSensors.Count];
                        double[] valLastMins = new double[listSensors.Count + 1];
                        foreach (string strId in listSensors)
                        {
                            TG tg = m_tec.FindTGById(Int32.Parse(strId), TG.INDEX_VALUE.TM, TG.ID_TIME.MINUTES);
                            if (tg == null)
                                return false;
                            else
                                ;

                            arIds[listSensors.IndexOf(strId)] = tg.m_id;
                            arOwnerGTPIds[listSensors.IndexOf(strId)] = tg.m_id_owner_gtp;
                        }

                        for (hour = 0; (hour < m_valuesHours.Length) && (iRes == 0); hour++, dtVal = dtVal.AddHours(1))
                        {
                            valLastMins = avgInterval(table_in
                                            , dtVal
                                            , 60
                                            , listSensors
                                            , out iRes);

                            if (iRes == 0)
                            {
                                foreach (string strId in listSensors)
                                {
                                    int indx = listSensors.IndexOf(strId);
                                    m_dictValuesTG[arIds[indx]].m_power_LastMinutesTM[hour + 1] = valLastMins[indx];
                                    if (indxTECComponents < 0)
                                        m_dictValuesTECComponent[hour + 1][arOwnerGTPIds[indx]].valuesLastMinutesTM += valLastMins[indx];
                                    else
                                        ;
                                }

                                m_valuesHours[hour + 0].valuesLastMinutesTM = valLastMins[listSensors.Count];
                            }
                            else
                                ;
                        }

                        switch (iRes)
                        {
                            case -12:
                                iRes = 0;
                                break;
                            case -2:
                                if (!(hour < dtReq.Hour))
                                    iRes = 0;
                                else
                                    ;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        ;
                }
            }

            return iRes == 0;
        }

        private bool GetMinsFactResponse(DataTable table)
        {
            int i, j = 0, min = 0;
            double minuteVal = 0, value;
            TG tgTmp;
            int id;
            bool end = false;
            DateTime dt, dtNeeded;
            int season = 0, need_season = 0, max_season = 0;
            bool jump = false;

            lastMinError = false;

            /*Form2 f2 = new Form2();
            f2.FillMinTable(table);*/

            if (CheckNameFieldsOfTable(table, new string[] { @"ID", @"DATA_DATE", @"SEASON", @"VALUE0" }) == false)
                return false;
            else
                ;

            lastMin = 0;

            if (table.Rows.Count > 0)
            {
                //Определить 1-ю отметку времени и сезон для этой отметки времени
                //if (table.Columns.Contains(@"DATA_DATE") == true)
                    if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                        return false;
                    else
                        ;
                //else
                //    return false;

                //if (table.Columns.Contains(@"SEASON") == true)
                    if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                        return false;
                    else
                        ;
                //else
                //    return false;

                need_season = max_season = season;
                min = (int)(dt.Minute / 3);
                dtNeeded = dt;
            }
            else
            {
                //Ошибка - нет ни одной строки
                if (currHour == true)
                {
                    if (!((m_curDate.Minute / 3) == 0))
                    {//Ошибка - номер 3-хмин > 1
                        lastMinError = true;
                        //lastMin = ((m_curDate.Minute) / 3) + 1;
                    }
                    else
                        ; //Успех
                }
                else
                    ;

                /*f2.FillMinValues(lastMin, selectedTime, m_tecView.m_valuesMins.valuesFact);
                f2.ShowDialog();*/

                return true;
            }

            //Проверить наличие среди записей "другого" сезона (с большим числом записей)
            for (i = 0; i < table.Rows.Count; i++)
            {
                if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
                    return false;
                else
                    ;

                if (max_season < season)
                {
                    max_season = season;
                    break;
                }
                else
                    ;
            }

            if (currHour == true)
            {//На отображении вызван "текущий" час
                if (! (need_season == max_season))
                {//Среди полученных записей - записи с разными сезонами
                    //m_valuesHours.addonValues = true;
                    //m_valuesHours.hourAddon = lastHour - 1;
                    need_season = max_season;
                }
                else
                    ; //сезон одинаков для всех записей
            }
            else
            {//На отображении вызвана "ретроспектива"
                //if (m_valuesHours.addonValues == true)
                    need_season = max_season;
                //else ;
            }

            for (i = 0; (end == false) && (min < m_valuesMins.Length); min++)
            {
                //При 1-м проходе всегда == false
                if (jump == true)
                {
                    min--;
                }
                else
                {//Всегда выполняется при 1-ом проходе
                    m_valuesMins[min].valuesFact = 0;
                    minuteVal = 0;
                }

                /*MessageBox.Show("min " + min.ToString() + ", lastMin " + lastMin.ToString() + ", i " + i.ToString() +
                                 ", table.Rows.Count " + table.Rows.Count.ToString());*/

                //
                jump = false;
                for (j = 0; j < CountTG; j++, i++)
                {
                    if (i >= table.Rows.Count)
                    {
                        end = true; //Установка признака выхода из цикла 'i'
                        break; //Выход из цикла 'j'
                    }
                    else
                        ;

                    try
                    {
                        if (!DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt))
                            return false;
                        else
                            ;

                        if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
                            return false;
                        else
                            ;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                        dt = DateTime.Now.Date;
                    }

                    if (!(season == need_season))
                    {
                        jump = true;
                        i++;
                        break;
                    }
                    else
                        ;

                    if (dt.CompareTo(dtNeeded) != 0)
                    {
                        break;
                    }
                    else
                        ;

                    if (!int.TryParse(table.Rows[i][@"ID"].ToString(), out id))
                        return false;
                    else
                        ;

                    tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.FACT, (int)TG.ID_TIME.MINUTES);

                    if (tgTmp == null)
                        return false;
                    else
                        ;

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

                    minuteVal += value;
                    m_dictValuesTG[tgTmp.m_id].m_powerMinutes [min] = value / 1000;
                    //tgTmp.receivedMin[min] = true;

                    //Признак получения значения хотя бы за один интервал
                    if (m_dictValuesTG[tgTmp.m_id].m_bPowerMinutesRecieved == false) m_dictValuesTG[tgTmp.m_id].m_bPowerMinutesRecieved = true; else ;
                }

                if (jump == false)
                {
                    dtNeeded = dtNeeded.AddMinutes(3);

                    //MessageBox.Show("end " + end.ToString() + ", minVal " + (minVal / 1000).ToString());

                    if (end == false)
                    {
                        m_valuesMins[min].valuesFact = minuteVal / 1000;
                        lastMin = min + 1;
                    }
                    else
                        ;
                }
                else
                    ;
            }

            /*f2.FillMinValues(lastMin, selectedTime, m_tecView.m_valuesMins.valuesFact);
            f2.ShowDialog();*/

            if (! (lastMin > ((m_curDate.Minute - 1) / 3)))
            {
                lastMinError = true;
                //lastMin = ((selectedTime.Minute - 1) / 3) + 1;
            } else {
            }

            if (lastMin < 0)
            {
                string strMes = @"TecView::GetMinsFactResponse () - lastMin = " + lastMin;
                //Logging.Logg().Error(strMes);
                throw new Exception(strMes);
            }
            else
                ;

            return true;
        }

        private double [] avgInterval (DataTable table, DateTime dtReqBegin, int secInterval, List <string> listSensors, out int iRes) {
            // -1 - 
            // -2 - за пред./интервал кол-во строк < кол-ва ТГ
            // -31 - интерпретация значения поля [ID]
            // -32 - ... [VALUE]
            // -33 - ... [tmdelt]a
            // -34 - ... [last_changed_at]
            // -35 - ... [MINUTE]
            // -4 - не найден объект ТГ по идентификатору [ID]
            // -5 - 
            // -6 -  
            // -7 - нет значений по предыдущ./интервалу для, как миним., одного из ТГ
            // -8 - сумма подинтервалов = 0 для, как миним., одного из ТГ
            // -9 - дата/вр. 1-го знач. за тек/интервал принадлежит интервалу
            // -10 - кол-во записей во входной таблице = 0
            // -11 -
            // -12 - нет дата/время 1-го значения за тек./интервал
            // -13 - нет дата/вр. кр./знач. за тек./интервал
            iRes = 0;
            //double dblRes = 0F;

            int i = -1, indx = -1
                , id = -1
                , tmDelta = -1
                ;
            double val = -1F;
            string strId = string.Empty;

            double [] tgPrevValues = new double [listSensors.Count]
                , tgCurValues = new double[listSensors.Count + 1];
            for (i = 0; i < listSensors.Count; i++) { listSensors[i] = listSensors [i].Trim (); tgPrevValues[i] = tgCurValues[i] = 0F; }
            tgCurValues [listSensors.Count] = 0F;

            DateTime dtReqEnd = dtReqBegin.AddSeconds(secInterval);

            DataRow[] rowsPrevInterval = table.Select(@"last_changed_at<'" + dtReqBegin.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'" +
                    @" AND last_changed_at>='" + dtReqBegin.AddSeconds(-1 * 60).ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'")
                , rowsCurInterval = table.Select(@"last_changed_at>='" + dtReqBegin.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'" +
                    @" AND last_changed_at<'" + dtReqEnd.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'");

            DateTime lastChangedAt = DateTime.MaxValue;
            DateTime[] arBeginInterval
                , arEndInterval = new DateTime[listSensors.Count];

            if (rowsPrevInterval.Length < listSensors.Count)
                iRes = -2;
            else {
                for (i = 0; i < arEndInterval.Length; i++) arEndInterval[i] = DateTime.MinValue;
                foreach (DataRow r in rowsPrevInterval) {
                    strId = r[@"ID"].ToString();
                    if (Int32.TryParse(strId, out id) == false)
                    {
                        iRes = -31;
                        break;
                    }
                    else
                        ;

                    if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                    {
                        iRes = -32;
                        break;
                    }
                    else
                        ;

                    if (Int32.TryParse(r[@"tmdelta"].ToString(), out tmDelta) == false)
                    {
                        iRes = -33;
                        break;
                    }
                    else
                        ;

                    if (DateTime.TryParse(r[@"last_changed_at"].ToString(), out lastChangedAt) == false)
                    {
                        iRes = -34;
                        break;
                    }
                    else
                        lastChangedAt = (DateTime)r[@"last_changed_at"];

                    indx = listSensors.IndexOf (strId);
                    if (arEndInterval[indx] < lastChangedAt)
                    {
                        arEndInterval[indx] = lastChangedAt;
                        tgPrevValues [indx] = val;
                    }
                    else
                        ;                            
                }
            }

            if (iRes == 0)
            {
                for (indx = 0; indx < listSensors.Count; indx ++)                                
                    if (tgPrevValues [indx] < 0)
                    {
                        iRes = -7;
                    }
                    else
                        ;

                if (iRes == 0)
                {
                    int [] sumDelta = new int [listSensors.Count];
                    double [] tgValuesEnd = new double [listSensors.Count];
                    int [] arDeltaEnd =  new int [listSensors.Count];

                    lastChangedAt = DateTime.MaxValue;
                    arBeginInterval = new DateTime [listSensors.Count];
                    for (i = 0; i < arBeginInterval.Length; i++) arBeginInterval [i] = DateTime.MaxValue;
                    for (i = 0; i < arEndInterval.Length; i++) { arEndInterval[i] = DateTime.MinValue; tgValuesEnd[i] = 0F; arDeltaEnd [i] = -1; }

                    foreach (DataRow r in rowsCurInterval)
                    {
                        strId = r[@"ID"].ToString();
                        if (Int32.TryParse(strId, out id) == false)
                        {
                            iRes = -31;
                            break;
                        }
                        else
                            ;

                        if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                        {
                            iRes = -32;
                            break;
                        }
                        else
                            ;

                        if (Int32.TryParse(r[@"tmdelta"].ToString(), out tmDelta) == false)
                        {
                            iRes = -33;
                            break;
                        }
                        else
                            ;

                        if (DateTime.TryParse(r[@"last_changed_at"].ToString(), out lastChangedAt) == false)
                        {
                            iRes = -34;
                            break;
                        }
                        else
                            //Получить 'fff' (милисекнды)
                            lastChangedAt = (DateTime)r[@"last_changed_at"];

                        //Индекс ТГ
                        indx = listSensors.IndexOf (strId);
                        //"Левое" время интервала для ТГ
                        if (arBeginInterval [indx] > lastChangedAt)
                            arBeginInterval [indx] = lastChangedAt;
                        else
                            ;
                        //"Правое" время интервала для ТГ
                        if (arEndInterval [indx] < lastChangedAt)
                        {
                            arEndInterval[indx] = lastChangedAt;
                            tgValuesEnd [indx] = val;
                            arDeltaEnd [indx] = tmDelta;
                        }
                        else
                            ;

                        //Значения за текущ./интервал
                        //if ((val > 1) && (tmDelta > 0))
                        if (tmDelta > 0)
                        {
                            if (val > 0F)
                                tgCurValues [indx] += val * tmDelta;
                            else
                                ;

                            sumDelta [indx] += tmDelta;
                        }
                        else
                            ;
                    }

                    int msecInterval = -1;
                    if (iRes == 0) {                        
                        for (indx = 0; indx < listSensors.Count; indx ++) {
                            //Проверить дата/вр. 1-го знач. за тек./интервал
                            if (! (arBeginInterval[indx] == DateTime.MaxValue))
                            {
                                msecInterval = (int)(arBeginInterval[indx] - dtReqBegin).TotalMilliseconds;
                                if (! (msecInterval < 0))
                                {
                                    tgCurValues[indx] += tgPrevValues[indx] * msecInterval;
                                    sumDelta [indx] += msecInterval;
                                }
                                else {
                                    iRes = -9;
                                    break;
                                }
                            }
                            else {
                                iRes = -12;
                                break;
                            }

                            //Проверить найдена ли дата/вр. кр./знач. за тек./интервал
                            if (!(arEndInterval[indx] == DateTime.MinValue))
                            {
                                //Вычитание НЕ актуального кра./знач. за тек./интервал
                                tgCurValues[indx] -= tgValuesEnd[indx] * arDeltaEnd[indx];
                                sumDelta[indx] -= arDeltaEnd[indx];

                                //msecInterval = (int)(arEndInterval[indx].AddMilliseconds(arDeltaEnd[indx]) - dtReqEnd).TotalMilliseconds;
                                //if (! (msecInterval < 0)) {
                                    //Переход через границу интервала - вычислить актуальную дельту
                                    msecInterval = (int)(dtReqEnd - arEndInterval[indx]).TotalMilliseconds;
                                //} else {
                                    ////НЕТ перехода через границу интервала -
                                    //// - увеличить дельта до конца интервала
                                    //msecInterval = (int)(dtReqEnd - arEndInterval[indx]).TotalMilliseconds;
                                //}

                                if (tgCurValues[indx] == -1F)
                                    tgCurValues[indx] = 0F;
                                else
                                    ;
                                tgCurValues[indx] += tgValuesEnd[indx] * msecInterval;
                                sumDelta [indx] += msecInterval;
                            } else {
                                iRes = -13;
                                break;
                            }

                            if (sumDelta[indx] > 0) // == (dtReqEnd - dtReqBegin).TotalMilliseconds
                            {
                                tgCurValues[indx] = tgCurValues[indx] / sumDelta[indx];
                                if (tgCurValues[listSensors.Count] == -1F)
                                    tgCurValues[listSensors.Count] = 0F;
                                else
                                    ;
                                tgCurValues[listSensors.Count] += tgCurValues[indx];
                            }
                            else {
                                iRes = -8;
                                break;
                            }
                        }

                        if (! (iRes == 0))
                            tgCurValues[listSensors.Count] = 0F;
                        else
                            ;
                    }
                    else
                        ; //Ошибка при заполнении значений тек./интервала
                }
                else
                    ; //iRes == -7 (нет значений по предыдущ./интервалу для, как миним. одного из ТГ)
            }
            else
                ; //Ошибка при заполнении значений предыдущ./интервала

            return tgCurValues;
        }

        private bool GetMinTMResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetMinTMResponse (lastHour=" + lastHour + @", lastMin=" + lastMin + @") - Rows.Count=" + table.Rows.Count);

            if (lastMin == 21)
                return true;
            else
                ;

            string  [] checkFields = null;

            if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                checkFields = new string[] { @"VALUE" };
            else
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    checkFields = new string[] { @"ID", @"VALUE", @"tmdelta", @"last_changed_at" };
                else
                    if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                        checkFields = new string[] { @"ID", @"VALUE" };
                    else
                        ;

            int iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            if (iRes == 0)
            {
                iRes = table.Rows.Count > 0 ? 0 : -10;

                if (iRes == 0)
                {
                    int min = -1;
                    //???
                    if (lastMin == 0) min = lastMin + 1; else min = lastMin;

                    double val = -1F;

                    if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                    {
                        if (double.TryParse (table.Rows [0][@"VALUE"].ToString (), out val) == true)
                            m_valuesMins[min].valuesFact = val;
                        else
                            iRes = -32;
                    }
                    else
                        if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                        {
                            int hour = lastHour - GetSeasonHourOffset(lastHour)
                                ;                                

                            List<string> listSensors = new List<string>(m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES).Split(','));
                            m_valuesMins[min].valuesFact = avgInterval(table
                                                                , m_curDate.Date.AddHours(hour).AddSeconds(180 * (min - 1))
                                                                , 180
                                                                , listSensors
                                                                , out iRes)[listSensors.Count];
                        }
                        else
                            if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                            {
                                int id = -1
                                    ;

                                foreach (DataRow r in table.Rows)
                                {
                                    if (Int32.TryParse(r[@"ID"].ToString(), out id) == false)
                                    {
                                        iRes = -31;
                                        break;
                                    }
                                    else
                                        ;

                                    if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                                    {
                                        iRes = -32;
                                        break;
                                    }
                                    else
                                        ;

                                    //Отладка ???
                                    if (!(val > 0))
                                        val = 0F;
                                    else
                                        ;

                                    m_valuesMins[min].valuesFact += val;
                                }
                            }
                            else
                                ;
                }
                else
                    ; //Нет строк во вХодной таблице
            }
            else
                ; //-1 Нет требуемых полей во входной таблице

            ////???
            //if (bRes == false)
            //{
            //}
            //else
            //{
            //}

            return
                true
                //iRes == 0
                ;
        }

        private bool GetMinsTMResponse(DataTable table)
        {
            string [] checkFields = null;

            if ((TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                || (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE))
                checkFields = new string[] { @"ID", @"VALUE", @"MINUTE" };
            else
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    checkFields = new string[] { @"ID", @"VALUE", @"tmdelta", @"last_changed_at" };
                else
                    ;

            int iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1
                , min = -1
                ;
            double val = -1F;

            lastMinError = false;

            if (iRes == 0)
            {
                iRes = table.Rows.Count > 0 ? 0 : -10; //??? почему -1, ведь это другой тип ошибки, чем отсутствие необходимых полей

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO) {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            int
                                //, id = -1
                                indx = -1;
                            int [] arIds = null;

                            DateTime dtReq = m_curDate.Date.AddHours(lastHour);
                            List<string> listSensors = new List<string>(m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES).Split(','));
                            arIds = new int[listSensors.Count];
                            double[] valMins = new double[listSensors.Count + 1];

                            foreach (string strId in listSensors)
                            {
                                TG tg = m_tec.FindTGById (Int32.Parse (strId), TG.INDEX_VALUE.TM, TG.ID_TIME.MINUTES);
                                if (tg == null)
                                    //iRes = -4
                                    return false;
                                else
                                    ;

                                arIds[listSensors.IndexOf(strId)] = tg.m_id;
                            }

                            for (min = 0; (min < 60) && (iRes == 0); min ++, dtReq = dtReq.AddMinutes (1))
                            {
                                valMins = avgInterval (table
                                                , dtReq
                                                , 60
                                                , listSensors
                                                , out iRes);

                                if (iRes == 0)
                                {
                                    foreach (string strId in listSensors)
                                    {                            
                                        indx = listSensors.IndexOf (strId);
                                        m_dictValuesTG[arIds [indx]].m_powerMinutes[min + 1] = valMins[indx];
                                    }

                                    m_valuesMins[min + 1].valuesFact = valMins[listSensors.Count];
                                }
                                else
                                    ;
                            }
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:                        
                            int id = -1;
                            TG tgTmp = null;
                            Dictionary<int, TG> dictTGRecievedValues = new Dictionary<int, TG>();

                            foreach (DataRow r in table.Rows)
                            {
                                if (Int32.TryParse(r[@"ID"].ToString(), out id) == false)
                                {
                                    iRes = -31;
                                    break;
                                }
                                else
                                    ;

                                if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                                {
                                    iRes = -32;
                                    break;
                                }
                                else
                                    ;

                                if (Int32.TryParse(r[@"MINUTE"].ToString(), out min) == false)
                                {
                                    iRes = -35; //last_changed_at
                                    break;
                                }
                                else
                                    ;

                                tgTmp = null;
                                if (dictTGRecievedValues.ContainsKey(id) == false)
                                {
                                    tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (int)TG.ID_TIME.MINUTES);

                                    if (! (tgTmp == null))
                                        dictTGRecievedValues.Add(id, tgTmp);
                                    else
                                        ;
                                }
                                else
                                    tgTmp = dictTGRecievedValues[id];

                                if (tgTmp == null)
                                {
                                    iRes = -4;
                                    break;
                                }
                                else
                                    ;

                                //Отладка ???
                                if (!(val > 0))
                                    val = 0F;
                                else
                                    ;

                                min ++;
                                m_dictValuesTG[tgTmp.m_id].m_powerMinutes[min] = val;
                                m_valuesMins [min].valuesFact += val;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ; //Кол-во строк == 0                
            }
            else
                ;

            switch (iRes) {
                case -12:
                    iRes = 0;
                    break;
                case -2:
                    break;
                default:
                    break;
            }

            if (! (iRes == 0))
            {
                lastMin = 61;
            }
            else
            {
                switch (TEC.s_SourceSOTIASSO) {
                    case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                        if (min > 0)
                            lastMin = min - 0;
                        else
                            lastMin = 0;
                        break;
                    case TEC.SOURCE_SOTIASSO.AVERAGE:
                    case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                        lastMin = min + 1;
                        break;
                    default:
                        break;
                }
            }

            lastMinError = ! (iRes == 0);

            if ((lastMinError == false) && (!(lastMin > ((m_curDate.Minute - 1) / 1))))
            {
                lastMinError = true;
                //lastMin = ((selectedTime.Minute - 1) / 1) + 1;
            }
            else
            {
            }

            return iRes == 0;
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
                            if ((! (l.Substring(0, 3) == "ПБР")) ||
                                (int.TryParse(l.Substring(3), out iRes) == false) ||
                                (! (iRes > 0)) ||
                                (iRes > 24))
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

        private void GetHoursFactRequest(DateTime date)
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.HOURS)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.HOURS)));
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_AISKUE], m_tec.hoursFactRequest(date, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_AISKUE, TG.ID_TIME.HOURS)));
        }

        private void GetHourTMRequest(DateTime date, int lh)
        {
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hourTMRequest(date, lh, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.HOURS)));
        }

        private void GetHoursTMRequest(DateTime date)
        {
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hoursTMRequest(date, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.HOURS)));
        }

        private void GetMinsFactRequest(int hour)
        {
            //tec.Request(CONN_SETT_TYPE.DATA_ASKUE, tec.minsRequest(selectedTime, hour, sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES]));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.MINUTES)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, TG.ID_TIME.MINUTES)));
            //26.10.2014 г.
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_AISKUE], m_tec.minsFactRequest(m_curDate, hour - GetSeasonHourOffset(hour), m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_AISKUE, TG.ID_TIME.MINUTES)));
        }

        private void GetMinTMRequest(DateTime date, int lh, int lm)
        {
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.minTMRequest(m_curDate, lh - GetSeasonHourOffset(lh), lm, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES)));
        }

        private void GetMinsTMRequest(int hour)
        {
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.minsTMRequest(m_curDate, hour - GetSeasonHourOffset(hour), m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO, TG.ID_TIME.MINUTES)));
        }

        private void GetHoursTMSNPsumRequest()
        {
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hoursTMSNPsumRequest(m_curDate));
        }

        private void GetLastMinutesTMRequest()
        {
            DateTime dtReq = m_curDate.Date;
            //if (dtReq.Kind == DateTimeKind.Unspecified)
            //    dtReq = dtReq.ToLocalTime();
            //else
            //    ;
            int cnt = HAdmin.CountHoursOfDate (dtReq);

            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_SOTIASSO)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_SOTIASSO)));
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(indxTECComponents, CONN_SETT_TYPE.DATA_SOTIASSO), cnt));
        }

        private void GetPPBRValuesRequest()
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], tec.GetPBRValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, m_admin.m_typeFields));
            //m_tec.Request(CONN_SETT_TYPE.PBR, m_tec.GetPBRValueQuery(m_indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, s_typeFields));
            //m_tec.Request(CONN_SETT_TYPE.PBR, m_tec.GetPBRValueQuery(m_indx_TECComponent, selectedTime.Date, s_typeFields));
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.PBR], m_tec.GetPBRValueQuery(indxTECComponents, m_curDate.Date, s_typeFields));
        }

        private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], tec.GetAdminValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
            //m_tec.Request(CONN_SETT_TYPE.ADMIN, m_tec.GetAdminValueQuery(indx_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
            //m_tec.Request(CONN_SETT_TYPE.ADMIN, m_tec.GetAdminValueQuery(m_indx_TECComponent, selectedTime.Date, mode));
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(indxTECComponents, m_curDate.Date, mode));
        }

        protected override void InitializeSyncState()
        {
            m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON];
            base.InitializeSyncState ();
            for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i ++ ) {
                m_waitHandleState [i] = new ManualResetEvent(false);
            }
        }
    }
}
