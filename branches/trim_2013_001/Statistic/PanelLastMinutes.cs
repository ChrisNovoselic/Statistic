using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelLastMinutes : TableLayoutPanel
    {
        List <Label> m_listLabelDateTime;

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        static HLabelStyles[] s_arLabelStyles = {new HLabelStyles(Color.Black, Color.LightGray, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, Color.LightGray, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, Color.LightGray, 12F, ContentAlignment.MiddleRight),
                                                new HLabelStyles(Color.Black, Color.LightGray, 12F, ContentAlignment.MiddleLeft)};
        static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;

        static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }

        AdminTS m_admin;
        
        enum StatesMachine : int
        {
            Init_TM,
            LastMinutes_TM,
            PBRValues,
            AdminValues
        };

        public DelegateFunc delegateEventUpdate;

        public int m_msecPeriodUpdate;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        public bool m_bIsActive;

        public StatusStrip m_stsStrip;

        public PanelLastMinutes(List<TEC> listTec, StatusStrip stsStrip, AdminTS admin)
        {
            int i = -1;

            m_stsStrip = stsStrip;
            m_admin = admin;
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.ColumnCount = listTec.Count + 1;
            this.RowCount = 1;

            //Создание панели с дата/время
            TableLayoutPanel panelDateTime = new TableLayoutPanel();
            panelDateTime.Dock = DockStyle.Fill;
            panelDateTime.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
            panelDateTime.ColumnCount = 1;
            panelDateTime.RowCount = 24 + COUNT_FIXED_ROWS; //Наименования: ТЭЦ + компонент ТЭЦ

            DateTime datetimeNow = DateTime.Now.Date;
            //Пустая ячейка (дата)
            panelDateTime.Controls.Add(new HLabel(s_arLabelStyles[(int)INDEX_LABEL.DATETIME]), 0, 0);
            ((HLabel)panelDateTime.Controls[0]).Text = datetimeNow.Date.ToString(@"dd:MM.yyyy");
            panelDateTime.SetRowSpan(panelDateTime.Controls[0], COUNT_FIXED_ROWS);

            m_listLabelDateTime = new List<Label> ();

            datetimeNow = datetimeNow.AddMinutes(59);
            for (i = 0; i < 24; i ++) {
                m_listLabelDateTime.Add(HLabel.createLabel(datetimeNow.ToString(@"HH:mm"), s_arLabelStyles[(int)INDEX_LABEL.DATETIME]));

                panelDateTime.Controls.Add(m_listLabelDateTime[m_listLabelDateTime.Count - 1], 0, i + COUNT_FIXED_ROWS);

                datetimeNow = datetimeNow.AddHours(1);
            }

            for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
            {
                panelDateTime.RowStyles.Add(fRowStyle());
            }
            
            int iPercentColDatetime = 6;
            this.Controls.Add(panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, iPercentColDatetime));

            int iCountSubColumns = 0;
            
            for (i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i]), i + 1, 0);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).CountTECComponent; //Слева столбец дата/время
            }

            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (i = 0; i < listTec.Count; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (100 - iPercentColDatetime) / iCountSubColumns * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public PanelLastMinutes(IContainer container, List<TEC> listTec, StatusStrip stsStrip, AdminTS admin)
            : this(listTec, stsStrip, admin)
        {
            container.Add(this);
        }

        public void SetDelegate(DelegateFunc dStatus)
        {
            this.delegateEventUpdate = dStatus;
        }

        public void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Start(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public void Stop()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Stop(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                ;

            m_bIsActive = active;

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Activate(active); else ;
                    i++;
                }
                else
                    ;
            }
        }

        partial class PanelTecLastMinutes
        {
            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();
            }

            #endregion
        }

        public partial class PanelTecLastMinutes : TableLayoutPanel
        {
            private TEC m_tec;
            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            private List<TG> m_listSensorId2TG;

            private Dictionary<int, Label[]> m_dictLabelVal;

            private object lockValue;

            private bool m_bIsActive,
                        m_bIsStarted,
                        m_bUpdate;

            private TecView.valuesS valuesHours;

            DataTable m_tablePBRResponse;

            private volatile string sensorsString_TM;

            private Thread m_taskThread;
            private Semaphore m_semaState;
            private volatile bool m_bThreadIsWorking;
            private volatile bool m_bIsNewState;
            private volatile List<StatesMachine> m_states;
            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            private DelegateFunc delegateUpdateGUI_TM;

            public PanelTecLastMinutes(TEC tec)
            {
                InitializeComponent();

                m_tec = tec;
                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;
                m_list_TECComponents = new List<TECComponentBase> ();
                m_dictLabelVal = new Dictionary<int, Label[]>();

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить наименование станции
                Label lblNameTEC = HLabel.createLabel(m_tec.name, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);
                
                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        this.Controls.Add(HLabel.createLabel(g.name_shr.Split (' ')[1], PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]), CountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Память под ячейки со значениями
                        m_dictLabelVal.Add(g.m_id, new Label[24]);
                        
                        for (i = 0; i < 24; i ++)
                        {
                            m_dictLabelVal[g.m_id][i] = HLabel.createLabel (0.ToString (@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]);
                            this.Controls.Add(m_dictLabelVal[g.m_id][i], CountTECComponent, i + COUNT_FIXED_ROWS);
                        }

                        //Добавить компонент ТЭЦ (ГТП)
                        m_list_TECComponents.Add(g);
                    }
                    else
                        ;
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                for (i = 0; i < CountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / CountTECComponent));
                }

                this.SetColumnSpan(lblNameTEC, CountTECComponent);

                lockValue = new object();
                m_listSensorId2TG = new List<TG>(); //[this.RowCount - COUNT_FIXED_ROWS];
                sensorsString_TM = string.Empty;
                m_states = new List<StatesMachine>();
                //delegateUpdateGUI_TM = ShowTMPower;

                valuesHours = new TecView.valuesS();
                valuesHours.valuesFact = new double[24];
                valuesHours.valuesPBR = new double[24];
                valuesHours.valuesPBRe = new double[24];
                valuesHours.valuesUDGe = new double[24];
                valuesHours.valuesDiviation = new double[24];
            }

            public void Start()
            {
                if (m_bIsStarted == true)
                    return;
                else
                    ;

                m_bIsStarted = true;

                m_tec.StartDbInterfaces();

                m_bThreadIsWorking = true;

                m_taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
                m_taskThread.Name = @"Интерфейс к данным (" + GetType ().Name + "): " + m_tec.name + @" - текущие значения...";
                m_taskThread.IsBackground = true;

                m_semaState = new Semaphore(1, 1);

                m_semaState.WaitOne();
                m_taskThread.Start();

                //Милисекунды до первого запуска функции таймера
                double msecUpdate = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 1, 0) - DateTime.Now).TotalMilliseconds;

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, (Int64) msecUpdate, ((PanelLastMinutes)Parent).m_msecPeriodUpdate - 1);

                m_bUpdate = false;
            }

            public void Stop()
            {
                if (m_bIsStarted == false)
                    return;
                else
                    ;

                m_evTimerCurrent.Reset();
                m_timerCurrent.Dispose();

                m_bIsStarted = false;
                bool joined;
                m_bThreadIsWorking = false;
                lock (lockValue)
                {
                    m_bIsNewState = true;
                    m_states.Clear();
                }

                if (m_taskThread.IsAlive)
                {
                    try
                    {
                        m_semaState.Release(1);
                    }
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - TecView.Stop () - sem.Release(1)"); }

                    joined = m_taskThread.Join(1000);
                    if (!joined)
                        m_taskThread.Abort();
                    else
                        ;
                }

                m_tec.StopDbInterfaces();

                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).errored_state = false;
                }
            }

            private void ChangeState()
            {
                m_bIsNewState = true;
                m_states.Clear();

                if ((sensorsString_TM.Equals(string.Empty) == false))
                {
                }
                else
                {
                    m_states.Add(StatesMachine.Init_TM);
                }

                m_states.Add(StatesMachine.LastMinutes_TM);
                m_states.Add(StatesMachine.PBRValues);
                m_states.Add(StatesMachine.AdminValues);
            }

            public void Activate(bool active)
            {
                if (m_bIsActive == active)
                    return;
                else
                    ;

                m_bIsActive = active;

                if (m_bIsActive == true)
                {
                    lock (lockValue)
                    {
                        ChangeState();

                        try
                        {
                            m_semaState.Release(1);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    lock (lockValue)
                    {
                        m_bIsNewState = true;
                        m_states.Clear();
                        ((PanelLastMinutes)Parent).errored_state =
                        ((PanelLastMinutes)Parent).actioned_state = false;
                    }
                }
            }

            private void ErrorReport(string error_string)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).last_error = error_string;
                    ((PanelLastMinutes)Parent).last_time_error = DateTime.Now;
                    ((PanelLastMinutes)Parent).errored_state = true;
                    ((PanelLastMinutes)Parent).m_stsStrip.BeginInvoke(((PanelLastMinutes)Parent).delegateEventUpdate);
                }
            }

            private void ActionReport(string action_string)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).last_action = action_string;
                    ((PanelLastMinutes)Parent).last_time_action = DateTime.Now;
                    ((PanelLastMinutes)Parent).actioned_state = true;
                    ((PanelLastMinutes)Parent).m_stsStrip.BeginInvoke(((PanelLastMinutes)Parent).delegateEventUpdate);
                }
            }

            private void ShowLastMinutes()
            {
                foreach (TG tg in m_listSensorId2TG)
                {
                }
            }

            private void PanelTecCurPower_TextChangedValue(object sender, EventArgs ev)
            {
                double val = -1.0;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true)
                {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else
                        clr = Color.Green;
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            private void GetCurrentTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_TM, m_tec.currentTMRequest(sensorsString_TM));
            }

            private bool GetCurrentTMResponse(DataTable table)
            {
                bool bRes = true;
                int i = -1,
                    id = -1;
                double value = -1;
                TG tgTmp;

                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    foreach (TG t in g.TG)
                    {
                        for (i = 0; i < t.power.Length; i++)
                        {
                            t.power_TM = 0;
                        }
                    }
                }

                for (i = 0; i < table.Rows.Count; i++)
                {
                    if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                        return false;
                    else
                        ;

                    tgTmp = FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                    if (tgTmp == null)
                        return false;
                    else
                        ;

                    if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                        return false;
                    else
                        ;

                    //if (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out m_dtLastChangedAt) == false)
                    //    return false;
                    //else
                    //    ;

                    switch (m_tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            //value *= 20;
                            break;
                        default:
                            break;
                    }

                    tgTmp.power_TM = value;
                }

                return bRes;
            }

            private TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
            {
                for (int i = 0; i < m_listSensorId2TG.Count; i++)
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if (m_listSensorId2TG[i].ids_fact[(int)id_type] == id)
                                return m_listSensorId2TG[i];
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if (m_listSensorId2TG[i].id_tm == id)
                                return m_listSensorId2TG[i];
                            else
                                ;
                            break;
                        default:
                            break;
                    }

                return null;
            }

            private void GetSensorsTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_TM, m_tec.sensorsTMRequest());
            }

            private bool GetSensorsTMResponse(DataTable table)
            {
                bool bRes = true;

                string s;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    //Шаблон для '[0](["NAME"])'
                    //Формирование правильное имя турбиногенератора
                    s = TEC.getNameTG(m_tec.m_strTemplateNameSgnDataTM, table.Rows[i][0].ToString().ToUpper());

                    //if (num_TECComponent < 0)
                    {//ТЭЦ в полном составе
                        int j = -1;
                        for (j = 0; j < m_tec.list_TECComponents.Count; j++)
                        {
                            int k = -1;
                            for (k = 0; k < m_tec.list_TECComponents[j].TG.Count; k++)
                            {
                                if (m_tec.list_TECComponents[j].TG[k].name_shr.Equals(s) == true)
                                {
                                    m_tec.list_TECComponents[j].TG[k].id_tm = int.Parse(table.Rows[i][1].ToString());

                                    m_listSensorId2TG.Add(m_tec.list_TECComponents[j].TG[k]); ;

                                    //Прерывание внешнего цикла
                                    j = m_tec.list_TECComponents.Count;
                                    break;
                                }
                                else
                                    ;
                            }
                        }
                    }
                    /*else
                    {// Для ТЭЦ НЕ в полном составе (ГТП, ЩУ, ТГ)
                        int k = -1;
                        for (k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                        {
                            if (tec.list_TECComponents[num_TECComponent].TG[k].name_shr.Equals(s) == true)
                            {
                                tec.list_TECComponents[num_TECComponent].TG[k].id_tm = int.Parse(table.Rows[i][1].ToString());
                                break;
                            }
                            else
                                ;
                        }
                    }*/
                }

                for (int i = 0; i < m_listSensorId2TG.Count; i++)
                {
                    if (!(m_listSensorId2TG[i] == null))
                    {
                        if (sensorsString_TM.Equals(string.Empty) == true)
                        {
                            sensorsString_TM = "[dbo].[states_real_his].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
                        }
                        else
                        {
                            sensorsString_TM += " OR [dbo].[states_real_his].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
                        }
                    }
                    else
                    {
                        ErrorReportSensors(ref table);

                        return false;
                    }
                }

                return bRes;
            }

            private void ErrorReportSensors(ref DataTable src)
            {
                string error = "Ошибка определения идентификаторов датчиков в строке ";
                for (int j = 0; j < src.Rows.Count; j++)
                    error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

                error = error.Substring(0, error.LastIndexOf(","));
                ErrorReport(error);
            }

            private void StateRequest(StatesMachine state)
            {
                switch (state)
                {
                    case StatesMachine.Init_TM:
                        ActionReport("Получение идентификаторов датчиков.");
                        switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                            case TEC.TEC_TYPE.BIYSK:
                                GetSensorsTMRequest();
                                break;
                            //case TEC.TEC_TYPE.BIYSK:
                            //break;
                            default:
                                break;
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        ActionReport("Получение текущих значений.");
                        //adminValuesReceived = false;
                        GetCurrentTMRequest();
                        break;
                    case StatesMachine.PBRValues:
                        ActionReport("Получение данных плана.");
                        //adminValuesReceived = false;
                        GetPBRValuesRequest();
                        break;
                    case StatesMachine.AdminValues:
                        ActionReport("Получение административных значений.");
                        //adminValuesReceived = false;
                        GetAdminValuesRequest(((PanelLastMinutes)Parent).m_admin.m_typeFields);
                        break;
                }
            }

            private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
            {
                error = false;
                table = null;

                switch (state)
                {
                    case StatesMachine.Init_TM:
                        switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                            case TEC.TEC_TYPE.BIYSK:
                                return m_tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        return m_tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
                    case StatesMachine.PBRValues:
                        return ((PanelLastMinutes)Parent).m_admin.GetResponse(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], out error, out table);
                    case StatesMachine.AdminValues:
                        return ((PanelLastMinutes)Parent).m_admin.GetResponse(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], out error, out table);
                }

                error = true;

                return false;
            }

            private bool StateResponse(StatesMachine state, DataTable table)
            {
                bool result = false;
                switch (state)
                {
                    case StatesMachine.Init_TM:
                        switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                            case TEC.TEC_TYPE.BIYSK:
                                result = GetSensorsTMResponse(table);
                                break;
                        }
                        if (result == true)
                        {
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        result = GetCurrentTMResponse(table);
                        if (result == true)
                        {
                        }
                        else
                            ;
                        break;
                    case StatesMachine.PBRValues:
                        result = GetPBRValuesResponse(table);
                        if (result == true)
                        {
                        }
                        else
                            ;
                        break;
                    case StatesMachine.AdminValues:
                        result = GetAdminValuesResponse(table);
                        if (result == true)
                        {
                            this.BeginInvoke(delegateUpdateGUI_TM);
                        }
                        else
                            ;
                        break;
                }

                if (result == true)
                    lock (lockValue)
                    {
                        ((PanelLastMinutes)Parent).errored_state =
                        ((PanelLastMinutes)Parent).actioned_state = false;
                    }
                else
                    ;

                return result;
            }

            private void StateErrors(StatesMachine state, bool response)
            {
                string error = string.Empty,
                        reason = string.Empty,
                        waiting = string.Empty;

                switch (state)
                {
                    case StatesMachine.Init_TM:
                        reason = @"идентификаторов датчиков (телемеханика)";
                        waiting = @"Переход в ожидание";
                        break;
                    case StatesMachine.LastMinutes_TM:
                        reason = @"значений крайних минут часа";
                        //waiting = @"Ожидание " + (((PanelLastMinutes)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
                        waiting = @"Переход в ожидание";
                        break;
                    case StatesMachine.PBRValues:
                        reason = @"данных плана";
                        break;
                    case StatesMachine.AdminValues:
                        reason = @"административных значений";
                        break;
                }

                if (response)
                    reason = @"разбора " + reason;
                else
                    reason = @"получения " + reason;

                error = "Ошибка " + reason + ".";

                if (waiting.Equals(string.Empty) == true)
                    error += " " + waiting + ".";
                else
                    ;

                ErrorReport(error);
            }

            private void TecView_ThreadFunction(object data)
            {
                int index;
                StatesMachine currentState;

                while (m_bThreadIsWorking)
                {
                    m_semaState.WaitOne();

                    index = 0;

                    lock (lockValue)
                    {
                        if (m_states.Count == 0)
                            continue;
                        currentState = m_states[index];
                        m_bIsNewState = false;
                    }

                    while (true)
                    {
                        bool error = true;
                        bool dataPresent = false;
                        DataTable table = null;
                        for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !m_bIsNewState; i++)
                        {
                            if (error)
                                StateRequest(currentState);

                            error = false;
                            for (int j = 0; (j < DbInterface.MAX_WAIT_COUNT) && (dataPresent == false) && (error == false) && (m_bIsNewState == false); j++)
                            {
                                System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                                dataPresent = StateCheckResponse(currentState, out error, out table);
                            }
                        }

                        bool responseIsOk = true;
                        if (dataPresent && !error && !m_bIsNewState)
                            responseIsOk = StateResponse(currentState, table);

                        if ((!responseIsOk || !dataPresent || error) && !m_bIsNewState)
                        {
                            StateErrors(currentState, !responseIsOk);
                            lock (lockValue)
                            {
                                if (m_bIsNewState == false)
                                {
                                    m_states.Clear();
                                    m_bIsNewState = true;
                                }
                            }
                        }

                        index++;

                        lock (lockValue)
                        {
                            if (index == m_states.Count)
                                break;
                            if (m_bIsNewState)
                                break;
                            currentState = m_states[index];
                        }
                    }
                }
                try
                {
                    m_semaState.Release(1);
                }
                catch
                {
                }
            }

            private void TimerCurrent_Tick(Object stateInfo)
            {
                lock (lockValue)
                {
                    if (m_bIsActive == true)
                    {
                        ChangeState();

                        try
                        {
                            m_semaState.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    else
                        ;
                }
            }

            private void GetPBRValuesRequest()
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).m_admin.Request(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], m_tec.GetPBRValueQuery(-1, DateTime.Now.Date, AdminTS.TYPE_FIELDS.STATIC));
                }
            }

            private bool GetPBRValuesResponse(DataTable table)
            {
                bool bRes = true;

                if (!(table == null))
                    m_tablePBRResponse = table.Copy();
                else
                    ;

                return bRes;
            }

            private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).m_admin.Request(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(-1, DateTime.Now.Date, mode));
                }
            }

            private bool GetAdminValuesResponse(DataTable table_in)
            {
                DateTime date = DateTime.Now.Date
                                , dtPBR;
                int hour;

                double currPBRe;
                int offsetPrev = -1
                    //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePBRResponse.Rows.Count
                    , i = -1, j = -1,
                    offsetUDG, offsetPlan, offsetLayout;
                
                double[,] valuesPBR = new double[/*tec.list_TECComponents.Count*/m_list_TECComponents.Count, 25];
                double[,] valuesREC = new double[m_list_TECComponents.Count, 25];
                int[,] valuesISPER = new int[m_list_TECComponents.Count, 25];
                double[,] valuesDIV = new double[m_list_TECComponents.Count, 25];

                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                m_tablePBRResponse = restruct_table_pbrValues(m_tablePBRResponse);
                offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_list_TECComponents.Count * 3) : m_tablePBRResponse.Columns.Count;

                table_in = restruct_table_adminValues(table_in);

                //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
                //    try { table_in.Columns.Remove("ID_COMPONENT"); }
                //    catch (Exception excpt)
                //    {
                //        /*
                //        Logging.Logg().LogExceptionToFile(excpt, "catch - TecView.GetAdminValuesResponse () - ...");
                //        */
                //    }
                //else
                //    ;

                // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                for (i = 0; i < m_tablePBRResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
                            if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                            {
                                offsetPrev = i;
                                //foreach (TECComponent g in tec.list_TECComponents)
                                for (j = 0; j < m_list_TECComponents.Count; j++)
                                {
                                    if ((offsetPlan + j * 3) < m_tablePBRResponse.Columns.Count)
                                        valuesPBR[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3];
                                    else
                                        valuesPBR[j, 24] = 0.0;
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - TecView.GetAdminValuesResponse () - ..."); }
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
                for (i = 0; i < m_tablePBRResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;

                    if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if (hour == 0 && dtPBR.Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_list_TECComponents.Count; j++)
                            {
                                try
                                {
                                    if (((offsetPlan + (j * 3)) < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                        valuesPBR[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)];
                                    else
                                        ;

                                    DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    if (row_in.Length > 0)
                                    {
                                        if (row_in.Length > 1)
                                            ; //Ошибка...
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            valuesREC[j, hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                        else
                                            valuesREC[j, hour - 1] = 0;

                                        if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                            valuesISPER[j, hour - 1] = (int)row_in[0][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                            valuesDIV[j, hour - 1] = (double)row_in[0][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        valuesREC[j, hour - 1] = 0;
                                    }
                                }
                                catch
                                {
                                }
                                //j++;
                            }
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
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_list_TECComponents.Count; j++)
                            {
                                try
                                {
                                    valuesPBR[j, hour - 1] = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            valuesREC[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + j * 3];
                                        else
                                            valuesREC[j, hour - 1] = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
                                            valuesISPER[j, hour - 1] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
                                            valuesDIV[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        valuesREC[j, hour - 1] = 0;
                                    }
                                }
                                catch
                                {
                                }
                                //j++;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                for (i = 0; i < 24; i++)
                {
                    for (j = 0; j < m_list_TECComponents.Count; j++)
                    {
                        valuesHours.valuesPBR[i] += valuesPBR[j, i];
                        if (i == 0)
                        {
                            currPBRe = (valuesPBR[j, i] + valuesPBR[j, 24]) / 2;
                            valuesHours.valuesPBRe[i] += currPBRe;
                        }
                        else
                        {
                            currPBRe = (valuesPBR[j, i] + valuesPBR[j, i - 1]) / 2;
                            valuesHours.valuesPBRe[i] += currPBRe;
                        }

                        valuesHours.valuesUDGe[i] += currPBRe + valuesREC[j, i];

                        if (valuesISPER[j, i] == 1)
                            valuesHours.valuesDiviation[i] += (currPBRe + valuesREC[j, i]) * valuesDIV[j, i] / 100;
                        else
                            valuesHours.valuesDiviation[i] += valuesDIV[j, i];
                    }
                    /*valuesHours.valuesPBR[i] = 0.20;
                    valuesHours.valuesPBRe[i] = 0.20;
                    valuesHours.valuesUDGe[i] = 0.20;
                    valuesHours.valuesDiviation[i] = 0.05;*/
                }
                
                //Внимание!!! Сезон не учитывается
                    valuesHours.valuesPBRAddon = valuesHours.valuesPBR[valuesHours.hourAddon];
                    valuesHours.valuesPBReAddon = valuesHours.valuesPBRe[valuesHours.hourAddon];
                    valuesHours.valuesUDGeAddon = valuesHours.valuesUDGe[valuesHours.hourAddon];
                    valuesHours.valuesDiviationAddon = valuesHours.valuesDiviation[valuesHours.hourAddon];
                
                
                return true;
            }

            private DataTable restruct_table_pbrValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_PBR"; // m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME]

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

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

                    //Преобразование таблицы
                    for (i = 0; i < table_in.Columns.Count; i++)
                    {
                        if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                            && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                            && (!(table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)))
                        //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                        {
                            cols_data.Add(table_in.Columns[i]);
                        }
                        else
                            if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                                || (table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                            {
                                table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                            }
                            else
                                ;
                    }

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    if (m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        table_in_restruct.Columns[m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                    else
                        ;

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);

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
                                if (m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                                    table_in_restruct.Rows[indx_row][m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                                else
                                    ;
                            }
                            else
                                indx_row = k;

                            for (k = 0; k < cols_data.Count; k++)
                            {
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }

            private DataTable restruct_table_adminValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_ADMIN"; // m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

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

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

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

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);

                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
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
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }
        }
    }
}
