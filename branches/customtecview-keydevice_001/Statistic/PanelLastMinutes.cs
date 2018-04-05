﻿using System;
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
using ASUTP.Core;
using ASUTP.Control;
using ASUTP;

namespace Statistic
{
    partial class PanelLastMinutes
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

            //Создание панели с дата/время
            m_panelDateTime = new PanelDateTime();
        }

        #endregion
    }

    /// <summary>
    /// Класс для описания объекта расчетов по контролю
    ///  мощности 4% (+2%/-2%)
    /// </summary>
    public class Hd2PercentControl
    {
        /// <summary>
        /// Индексы массива со значенями для условий(правил) проверки нахождения значения в диапазоне
        /// </summary>
        private enum INDEX_RULE
        {
            PERCENT2, POWER3MWt
        }
        /// <summary>
        /// Структура для хранения значений по одному из условий(правил) проверки нахождения значения в диапазоне
        /// </summary>
        struct VALUE_RULE
        {
            /// <summary>
            /// Признак применения правила
            /// </summary>
            public bool Use;
            /// <summary>
            /// Реперная точка - значение с которым производится сравнение
            /// </summary>
            public double Reper;
            /// <summary>
            /// Относительное(от уровня) значение сравниваемое с реперной точкой
            ///  , вычисляется на этапе проверки
            /// </summary>
            public double Relative;
            /// <summary>
            /// Для отображения всплывающей подсказки
            /// </summary>
            public double Deviation { get { return Reper + Relative; } }
        }
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public Hd2PercentControl() { }
        /// <summary>
        /// Функция расчета 
        /// </summary>
        /// <param name="values">Объект со значениями компонента ТЭЦ - входные данные</param>
        /// <param name="bPmin">Признак использования в алгоритме ветви с использованием "Мощность минимальная"</param>
        /// <param name="err">Признак ошибки при выполнении расчета</param>
        /// <returns>Строка - результат (для отображения)</returns>
        public string Calculate(TecView.values values, bool bPmin, out int err)
        {
            string strRes = string.Empty; //Строка - результат
            double valuesBaseCalculate = -1F; //Основная величина по которой производится расчет

            VALUE_RULE[] valuesRule = new VALUE_RULE[] {
                new VALUE_RULE() { Use = true, Reper = -1F, Relative = -1F }
                , new VALUE_RULE() { Use = true, Reper = 3F, Relative = -1F }
            };
            double delta = -1.0;
            int iReverse = 0; //Признак направления отклонения (по умолчанию - нет)
            INDEX_RULE indxRule = INDEX_RULE.PERCENT2;
            bool bAbs = false; //Признак абсолютного значения (по умолчанию - нет)

            //Проверить наличие внешней команды
            if (values.valuesForeignCommand == true)
            {//Есть внешняя команда
                valuesBaseCalculate = values.valuesUDGe;
                //Установить признак отклонения "вверх"
                iReverse = 1;
                //Установить признак абсолютного значения
                bAbs = true;
            }
            else
            {//Нет внешней команды
                if (values.valuesPBR == values.valuesPmax)
                {
                    valuesBaseCalculate = values.valuesPBR;
                    iReverse = 1;
                }
                else
                    //Проверить признак использования ветви "Мощность минимальная"
                    if (bPmin == true)
                    //Использовать ветвь "Мощность минимальная"
                    if (values.valuesPBR == values.valuesPmin)
                    {//Установить значение величины-основания
                        valuesBaseCalculate = values.valuesPBR;
                        //Установить признак отклонения "вниз"
                        iReverse = -1;
                    }
                    else
                        ;
                else
                    ;
            }
            //Проверить установлена ли величина-основание
            if (valuesBaseCalculate > 1)
            {
                //Произвести расчет по величине-основании
                strRes += @"Уров=" + valuesBaseCalculate.ToString(@"F2");
                strRes += @"; ПБР=" + values.valuesPBR.ToString(@"F2") + @"; Pmax=" + values.valuesPmax.ToString(@"F2");
                //Проверить признак использования ветви "Мощность минимальная"
                if (bPmin == true)
                {
                    strRes += @"; Pmin=" + values.valuesPmin.ToString(@"F2");
                }
                else;
                //Проверить признак наличия значения за крайнюю минуту часа
                if (values.valuesLastMinutesTM > 1)
                {
                    //Проверить признак направления отклонения
                    if (!(iReverse == 0))
                    {//Есть признак отклонения
                        delta = iReverse * (valuesBaseCalculate - values.valuesLastMinutesTM);
                        //Проверить признак абсолютного значения
                        if (bAbs == true)
                            delta = Math.Abs(delta);
                        else
                            ;
                    }
                    else
                        ;
                    //Определить реперную точку для 1-го правила (2%)
                    valuesRule[(int)INDEX_RULE.PERCENT2].Reper = valuesBaseCalculate / 100 * 2;

                    if (valuesRule[(int)INDEX_RULE.PERCENT2].Reper < 1)
                        valuesRule[(int)INDEX_RULE.PERCENT2].Reper = 1;
                    else
                        ;
                    //!!!Реперная точка для 2-го правила - константа

                    if (valuesBaseCalculate > 1)
                        for (indxRule = INDEX_RULE.PERCENT2; indxRule < (INDEX_RULE)Enum.GetValues(typeof(INDEX_RULE)).Length; indxRule++)
                            if (valuesRule[(int)indxRule].Use == true)
                                valuesRule[(int)indxRule].Relative = delta - valuesRule[(int)indxRule].Reper;
                            else
                                continue;
                    else
                        ;

                    if (!(iReverse == 0))
                    {
                        for (indxRule = INDEX_RULE.PERCENT2; indxRule < (INDEX_RULE)Enum.GetValues(typeof(INDEX_RULE)).Length; indxRule++)
                            if ((valuesRule[(int)indxRule].Use == true)
                                && (valuesRule[(int)indxRule].Relative > 0))
                                break;
                            else
                                ;

                        if (indxRule < (INDEX_RULE)Enum.GetValues(typeof(INDEX_RULE)).Length)
                            err = 1;
                        else
                        {
                            indxRule = INDEX_RULE.PERCENT2;
                            err = 0;
                        }
                    }
                    else
                    {
                        indxRule = INDEX_RULE.PERCENT2;
                        err = 0;
                    }

                    strRes += @"; Откл=" + valuesRule[(int)indxRule].Deviation.ToString(@"F1");
                    strRes += string.Format(@"({0}%", ((valuesRule[(int)INDEX_RULE.PERCENT2].Deviation / valuesBaseCalculate) * 100).ToString(@"F1"));

                    if (valuesRule[(int)INDEX_RULE.POWER3MWt].Use == true)
                        strRes += string.Format(@",{0}{1}МВт"
                            , (valuesRule[(int)INDEX_RULE.POWER3MWt].Relative > 0) ? @">" : @"<"
                            , valuesRule[(int)INDEX_RULE.POWER3MWt].Reper.ToString(@"F0"));
                    else
                        ;

                    strRes += @")";
                }
                else
                {
                    err = 0;

                    strRes += @";Откл=" + 0.ToString(@"F1") + @"(" + 0.ToString(@"F1") + @"%)";
                }
            }
            else
            {
                err = 0;

                strRes += @"Уров=---.-";
                strRes += @"; ПБР=" + values.valuesPBR.ToString(@"F2") + @"; Pmax=" + values.valuesPmax.ToString(@"F2");
                if (bPmin == true)
                {
                    strRes += @"; Pmin=" + values.valuesPmin.ToString(@"F2");
                }
                else;

                strRes += @"; Откл=--(--%)";
            }

            return strRes;
        }
        /// <summary>
        /// Строка для формирования подписей (подсказок) для полученных значений
        /// </summary>
        public static string StringToolTipEmpty = @"Уров=---.-; Откл=--(--%)";
    }

    public partial class PanelLastMinutes : PanelStatisticWithTableHourRows
    {
        /// <summary>
        /// Панель для отображения меток времени (левая часть основной панели)
        /// </summary>
        private PanelDateTime m_panelDateTime;
        /// <summary>
        /// Объект синхронизации, является аргументом метода обратного вызова таймера
        ///  , служит для досрочного(по команде) прерывания этого метода
        /// </summary>
        private ManualResetEvent m_evTimerCurrent;
        private
            System.Threading.Timer //Вариант №0
            //System.Windows.Forms.Timer //Вариант №1
                m_timerCurrent;

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        /// <summary>
        /// Цвет для отображения заголовков каждого из объектов панелей ТЭЦ(и их компонентов) со значениями
        /// </summary>
        private static Color s_clrBackColorLabelVal = Color.FromArgb(219, 223, 227);
        /// <summary>
        /// Массив со стилями для отображения заголовков каждого из объектов панелей ТЭЦ(и их компонентов) со значениями
        /// </summary>
        private static HLabelStyles[] s_arLabelStyles = {
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor, 14F, ContentAlignment.MiddleCenter)
            , new HLabelStyles(FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor, 12F, ContentAlignment.MiddleCenter)
            , new HLabelStyles(FormMain.formGraphicsSettings.FontColor
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal : FormMain.formGraphicsSettings.BackgroundColor
                , 10F
                , ContentAlignment.MiddleRight)
            , new HLabelStyles(FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor, 12F, ContentAlignment.MiddleCenter)
        };
        /// <summary>
        /// Количество фиксированных строк в макете (кроме строк с номерами/наименованиями меток времени - часов)
        /// </summary>
        private static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;
        /// <summary>
        /// Возвратить стиль строки в макете размещения элементов на панели
        /// </summary>
        /// <returns>Стиль для строки</returns>
        private static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }
        /// <summary>
        /// Перечисление - возможные состояния для объекта обращения к данным
        /// </summary>
        enum StatesMachine : int
        {
            Init_TM,
            LastMinutes_TM,
            PBRValues,
            AdminValues
        };
        /// <summary>
        /// Период обновления значений на панел
        ///  , при этом инициируется обновлне=ение значение и на дочерних панелях(для ТЭЦ и их компонентов)
        /// </summary>
        public int m_msecPeriodUpdate;
        /// <summary>
        /// Событие инициации процедуры изменения состояния
        /// </summary>
        private event Action<DateTime> EventChangeDateTime;
        /// <summary>
        /// Конструктор - основной (с аргументами)
        /// </summary>
        /// <param name="listTec">Список ТЭЦ для построения списка дочерних панелей, и, в дальнейшем опроса БД, заполнения их значениями</param>
        public PanelLastMinutes(List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            for(INDEX_LABEL indx = (INDEX_LABEL)0; indx < INDEX_LABEL.COUNT_INDEX_LABEL; indx ++) {
                if (indx == INDEX_LABEL.VALUE_COMPONENT)
                    s_arLabelStyles [(int)indx].m_backColor = BackColor == SystemColors.Control ? s_clrBackColorLabelVal : BackColor;
                else
                    s_arLabelStyles [(int)indx].m_backColor = BackColor;
                s_arLabelStyles [(int)indx].m_foreColor = ForeColor;
            }

            InitializeComponent();

            this.ColumnCount = 1;
            this.RowCount = 1;

            float fPercentColDatetime = 8F;
            this.Controls.Add(m_panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fPercentColDatetime));
            // кол-во под/столбцов в каждй из панели для ТЭЦ (кол-во компонентов ТЭЦ - ГТП)
            int iCountSubColumns = 0;

            for (int i = 0; i < listTec.Count; i ++)
                // фильтр ТЭЦ-ЛК
                if (!(listTec[i].m_id > (int)TECComponent.ID.LK))
                {
                    this.Controls.Add(new PanelTecLastMinutes(listTec[i]/*, fErrRep, fWarRep, fActRep, fRepClr*/), i + 1, 0);
                    EventChangeDateTime += new Action<DateTime>(((PanelTecLastMinutes)this.Controls[i + 1]).OnEventChangeDateTime);
                    iCountSubColumns += ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent; //Слева столбец дата/время

                    this.ColumnCount++;
                }
                else
                    ;

            initializeLayoutStyle(iCountSubColumns);
            // период обновления = 1 ч
            m_msecPeriodUpdate = 60 * 60 * 1000;
        }
        /// <summary>
        /// Конструктор - дополнительный (с аргументами)
        /// </summary>
        /// <param name="container">Контейнер для панели</param>
        /// <param name="listTec">Список ТЭЦ для построения списка дочерних панелей</param>
        public PanelLastMinutes (IContainer container, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(listTec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {
            container.Add(this);
        }
        /// <summary>
        /// Метод для назначения делегатов (в т.ч. и в подчиненных панелях), для взаимодействия со строкой состояния главного окна приложения
        /// </summary>
        /// <param name="ferr">Метод(делегат) для передачи/отображения сообщения об ошибке</param>
        /// <param name="fwar">Метод(делегат) для передачи/отображения сообщения о предупреждении</param>
        /// <param name="fact">Метод(делегат) для передачи/отображения сообщения о действии</param>
        /// <param name="fclr">Метод(делегат) для указания об очтстке строки состояния от предыдущих сообщений</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ptcp in this.Controls)
                if (ptcp is PanelTecLastMinutes)
                    (ptcp as PanelTecLastMinutes).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }

        protected override void formMain_ForeColorChanged (object sender, EventArgs e)
        {
            Color formForeColor = Color.Empty;

            formForeColor = (sender as Form).ForeColor;

            foreach (INDEX_LABEL indx in Enum.GetValues (typeof (INDEX_LABEL))) {
                if (indx == INDEX_LABEL.COUNT_INDEX_LABEL)
                    continue;
                else
                    ;

                s_arLabelStyles [(int)indx].m_foreColor = formForeColor;
            }

            base.formMain_ForeColorChanged (sender, e);

            //??? вызывается в базовом методе
            //foreach (Control ptcp in this.Controls)
            //    if (ptcp is PanelTecLastMinutes)
            //        (ptcp as PanelTecLastMinutes).ForeColor = ForeColor;
            //    else
            //        ;
        }
        /// <summary>
        /// Метод обработки события - изменение цвета фона (при изменении цветовой схемы)
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события(без значения)</param>
        protected override void formMain_BackColorChanged (object sender, EventArgs e)
        {
            Color formBackColor = Color.Empty
                , backColor = Color.Empty;

            formBackColor = (sender as Form).BackColor;

            foreach (INDEX_LABEL indx in Enum.GetValues (typeof (INDEX_LABEL))) {
                if (indx == INDEX_LABEL.COUNT_INDEX_LABEL)
                    continue;
                else
                    ;

                switch (indx) {
                    case INDEX_LABEL.VALUE_COMPONENT:
                        backColor = formBackColor == SystemColors.Control ? s_clrBackColorLabelVal : formBackColor;
                        break;
                    case INDEX_LABEL.DATETIME:
                    case INDEX_LABEL.NAME_TEC:
                    case INDEX_LABEL.NAME_COMPONENT:
                    default:
                        backColor = formBackColor == SystemColors.Control ? FormMain.formGraphicsSettings.BackgroundColor : formBackColor;
                        break;
                }

                s_arLabelStyles [(int)indx].m_backColor = backColor;
            }

            base.formMain_BackColorChanged (sender, e);

            //??? вызывается в базовом методе
            //foreach (Control ptcp in this.Controls)
            //    if (ptcp is PanelTecLastMinutes)
            //        (ptcp as PanelTecLastMinutes).BackColor = BackColor;
            //    else
            //        ;
        }

        public override void Start()
        {
            base.Start();
            
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Start();
                    //else ;
                    i++;
                }
                else
                    ;
            }

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new
                System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, getMSecUpdate (666666), Timeout.Infinite)
                //System.Windows.Forms.Timer ()
                ;
            ////Вариант №1
            //m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
            //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL;
            //m_timerCurrent.Start ();
            //Для отладки
            //m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, m_msecPeriodUpdate - 1);

            //setDatetimePicker(m_panelDateTime.m_dtprDate.Value - HAdmin.GetOffsetOfCurrentTimeZone());
        }

        /// <summary>
        /// Милисекунды до первого запуска функции таймера
        /// <param name="shift">Дополнительное смещение, определяется в ~ от времени потребного для гарантированного получения значений (крайн. мин. часа)</param>
        /// </summary>
        /// <returns>Кол-во секунд до 1-го выполнения метода обратного вызова таймера
        ///  , остальные - ежечасно
        ///  </returns>
        private Int64 getMSecUpdate (Int64 shift)
        {
            Int64 iRes = -1;
            //Милисекунды от начала часа
            iRes = DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000;
            // милисекунды до окончания часа
            iRes = 60 * 60 * 1000 - iRes;
            iRes += shift;

            return iRes;
        }

        public override void Stop()
        {
            int i = 0;

            if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Stop();
                    //else ;
                    i++;
                }
                else
                    ;
            }

            base.Stop ();
        }

        protected override void initTableHourRows()
        {
            //Перестраиваем "шкалу" времени
            //m_panelDateTime.initTableHourRows(); ??? Панель сама инициирует изменение даты, т.к. 'календарь' принадлежит ей
        }

        //???
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            if (!(this.ColumnStyles.Count == 1))
                throw new Exception(@"PanelLastMinutes::initializeLayoutStyle () - ...");
            else
                ;

            float fPercentColDatetime = this.ColumnStyles[0].Width;
            
            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (int i = 0; i < this.ColumnCount - 1; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ((float)100 - fPercentColDatetime) / cols * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);
            
            if (bRes == false)
                return bRes;
            else
                ;

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Activate(active);
                    //else ;
                    i++;
                }
                else
                    ;
            }

            if (Actived == true)
                EventChangeDateTime (m_panelDateTime.m_dtprDate.Value);
            else
                ;

            return bRes;
        }

        private void setDatetimePicker(DateTime dtSet)
        {
            m_panelDateTime.m_dtprDate.Value = dtSet;

            initTableHourRows ();
        }

        private void TimerCurrent_Tick (object obj)
        //private void TimerCurrent_Tick(object obj, EventArgs ev)
        {
            if (! (m_timerCurrent == null))
                //Вариант №0
                m_timerCurrent.Change(m_msecPeriodUpdate, System.Threading.Timeout.Infinite);
                ////Вариант №1
                //if (m_timerCurrent.Interval == ProgramBase.TIMER_START_INTERVAL)
                //{
                //    m_timerCurrent.Interval = (int)getMSecUpdate (666666);
                    
                //    return ;
                //}
                //else
                //    if (! (m_timerCurrent.Interval == m_msecPeriodUpdate))
                //        m_timerCurrent.Interval = m_msecPeriodUpdate;
                //    else
                //        ;
            else
                ;

            if (Actived == true)
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateDateFunc(setDatetimePicker), HDateTime.ToMoscowTimeZone(DateTime.Now));
                else
                    Logging.Logg().Error(@"PanelLastMinutes::TimerCurrent_Tick () - ... BeginInvoke (setDatetimePicker) - ...", Logging.INDEX_MESSAGE.D_001);
            else
                ;
        }

        public override void UpdateGraphicsCurrent (int type)
        {
            getTypedControls (this, new Type [] { typeof (PanelTecLastMinutes) }).Cast<PanelTecLastMinutes> ().ToList ().ForEach (panel => panel.UpdateGraphicsCurrent () );
        }

        partial class PanelDateTime
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

                this.m_dtprDate = new DateTimePicker();
                this.m_dtprDate.Dock = DockStyle.Fill;
                //this.m_dtprDate.ValueChanged += new EventHandler(((PanelLastMinutes)Parent).OnDateTimeValueChanged);
                m_dtprDate.Value = HDateTime.ToMoscowTimeZone (DateTime.Now);
                this.m_dtprDate.ValueChanged += new EventHandler(OnDateTimeValueChanged);

                this.m_btnUpdate = new Button ();
                this.m_btnUpdate.Dock = DockStyle.Fill;
                this.m_btnUpdate.Text = @"Обнов.";
                this.m_btnUpdate.Click += new EventHandler(OnDateTimeValueChanged);
            }

            #endregion
        }

        private partial class PanelDateTime : PanelStatisticWithTableHourRows
        {
            public DateTimePicker m_dtprDate;
            private Dictionary<int, Label> m_dictLabelTime;

            private Button m_btnUpdate;

            public PanelDateTime()
                : base (MODE_UPDATE_VALUES.ACTION, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
            {
                InitializeComponent();

                Initialize();
            }

            public PanelDateTime(IContainer container)
                : this()
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;

                //int cntHours = 

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить дату
                //Label lblDate = HLabel.createLabel(dtNow.ToString (@"dd.MM.yyyy"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(m_dtprDate, 0, 0);
                //this.SetRowSpan(m_dtprDate, COUNT_FIXED_ROWS);

                m_dictLabelTime = new Dictionary<int,Label> ();

                //Добавить кнопку принудительного обновления
                this.Controls.Add(this.m_btnUpdate, 0, 1);
                //this.SetRowSpan(this.m_btnUpdate, COUNT_FIXED_ROWS);

                initTableHourRows();

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6F));

                //m_dtprDate.Value = DateTime.Now; Иначе Парент == null ???
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            protected override void initTableHourRows()
            {
                DateTime dt = m_dtprDate.Value.Date;
                bool bSeason = false
                    , bChangedCountRows = false;
                int h = -1
                    , cntHours = -1;

                if (dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                {
                    bSeason = true;
                    cntHours = 25;
                }
                else
                    cntHours = 24;

                if (m_dictLabelTime.Count == 0)
                {
                    for (h = 0; h < (cntHours + COUNT_FIXED_ROWS - 0); h++)
                    {
                        this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                    }

                    for (h = 0; h < cntHours; h++)
                    {
                        m_dictLabelTime[h] = HLabel.createLabel(@"--:--", PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]);
                        this.Controls.Add(m_dictLabelTime[h], 0, (h) + COUNT_FIXED_ROWS);
                    }

                    bChangedCountRows = true;
                }
                else
                {
                    if (bSeason == true)
                    {
                        if (m_dictLabelTime.Count < cntHours)
                        {
                            m_dictLabelTime.Add(24, HLabel.createLabel(dt.ToString(@"--:--"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]));
                            this.Controls.Add(m_dictLabelTime[24], 0, 24 + COUNT_FIXED_ROWS);

                            this.RowStyles.Add(PanelLastMinutes.fRowStyle());

                            bChangedCountRows = true;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        if (m_dictLabelTime.Count > cntHours)
                        {
                            this.Controls.Remove(m_dictLabelTime[24]);
                            m_dictLabelTime.Remove(cntHours);

                            this.RowStyles.RemoveAt(this.RowStyles.Count - 1);

                            bChangedCountRows = true;
                        }
                        else
                        {
                        }
                    }
                }

                if (bChangedCountRows == true)
                {
                    DateTime dtSel = m_dtprDate.Value.Date;
                    int offset = 0;
                    dtSel = dtSel.AddMinutes(59);
                    for (h = 0; h < m_dictLabelTime.Count; h++)
                    {
                        m_dictLabelTime[h].Text = dtSel.ToString(@"HH:mm");
                        if (bSeason == true)
                            if (!(h == HAdmin.SeasonDateTime.Hour))
                            {
                                dtSel = dtSel.AddHours(1);

                                offset = HAdmin.GetSeasonHourOffset(dtSel.Date, h);
                                if ((offset > 0) && ((h - 1) == HAdmin.SeasonDateTime.Hour))
                                {
                                    m_dictLabelTime[h].Text += @"*";
                                }
                                else
                                    ;
                            }
                            else
                                ;
                        else
                            dtSel = dtSel.AddHours(1);
                    }
                }
                else
                {
                }
            }

            public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
            {
                throw new NotImplementedException();
            }

            private void OnDateTimeValueChanged(object obj, EventArgs ev)
            {
                DateTime dt = m_dtprDate.Value;
                ((PanelLastMinutes)Parent).EventChangeDateTime(dt);

                initTableHourRows();
            }

            protected override void formMain_ForeColorChanged (object sender, EventArgs e)
            {
                base.formMain_ForeColorChanged (sender, e);

                foreach (Label label in m_dictLabelTime.Values)
                    label.ForeColor = ForeColor;
            }

            protected override void formMain_BackColorChanged (object sender, EventArgs e)
            {
                base.formMain_BackColorChanged (sender, e);

                foreach (Label label in m_dictLabelTime.Values)
                    label.BackColor = BackColor;
            }

            public override void UpdateGraphicsCurrent (int type)
            {
                throw new NotImplementedException ();
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

        public partial class PanelTecLastMinutes : HPanelCommon
        {
            private List<Label> m_listLabelNames;

            public class TecViewLastMinutes : TecView
            {
                /// <summary>
                /// Конструктор - основной (с аргументом)
                /// </summary>
                /// <param name="id_tec">Иденификатор ТЭЦ</param>
                public TecViewLastMinutes(int id_tec)
                    : base(new FormChangeMode.KeyDevice () { Id = id_tec, Mode = FormChangeMode.MODE_TECCOMPONENT.TEC }, TECComponentBase.TYPE.ELECTRO)
                {
                }

                public override void ChangeState()
                {
                    lock (m_lockState) { GetRDGValues(FormChangeMode.KeyDevice.Empty, DateTime.MinValue); }

                    base.ChangeState(); //Run
                }

                public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
                {
                    ClearStates();

                    ClearValues();

                    if (m_tec.GetReadySensorsStrings (_type) == false)
                        AddState((int)StatesMachine.InitSensors);
                    else ;

                    adminValuesReceived = false;

                    AddState((int)StatesMachine.PPBRValues);
                    AddState((int)StatesMachine.AdminValues);
                    //AddState((int)StatesMachine.CurrentTimeView);
                    AddState((int)StatesMachine.LastMinutes_TM);
                }
            }
            
            public int m_id_tec { get { return m_tecView.m_tec.m_id; } }

            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            //Для отображения значений
            private List <Dictionary<int, Label>> m_listDictLabelVal;
            private List <Dictionary<int, ToolTip>> m_listDictToolTip;

            //private Dictionary<int, TecView.valuesTECComponent> m_dictValuesHours;
            TecViewLastMinutes m_tecView;

            public PanelTecLastMinutes(StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : base (-1, -1)
            {
                InitializeComponent();

                m_listLabelNames = new List<Label> ();

                m_tecView = new TecViewLastMinutes(tec.m_id);

                //Признаки для регистрации соединения с необходимыми источниками данных
                HMark markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_SOTIASSO });
                //markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
                //markQueries.Marked((int)CONN_SETT_TYPE.PBR);
                //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

                m_tecView.updateGUI_LastMinutes = new DelegateFunc(showLastMinutesTM);
                ForeColorChanged += onForeColorChanged;
                BackColorChanged += onBackColorChanged;

                Initialize();
            }

            private void onForeColorChanged (object sender, EventArgs e)
            {
                if (Equals (m_listLabelNames, null) == false)
                    foreach (Label label in m_listLabelNames)
                        label.ForeColor = ForeColor;
                else
                    ;

                if (Equals (m_listDictLabelVal, null) == false)
                    foreach (Dictionary<int, Label> listLabels in m_listDictLabelVal)
                        foreach (Label label in listLabels.Values)
                            if (label.BackColor.Equals (s_arLabelStyles [(int)INDEX_LABEL.VALUE_COMPONENT].m_backColor) == true)
                                label.ForeColor = s_arLabelStyles [(int)INDEX_LABEL.VALUE_COMPONENT].m_foreColor;
                            else
                                ;
                else
                    ;
            }

            private void onBackColorChanged (object sender, EventArgs e)
            {
                if (Equals (m_listLabelNames, null) == false)
                    foreach (Label label in m_listLabelNames)
                        label.BackColor = BackColor;
                else
                    ;

                if (Equals (m_listDictLabelVal, null) == false)
                    foreach (Dictionary<int, Label> listLabels in m_listDictLabelVal)
                        foreach (Label label in listLabels.Values)
                            label.BackColor = s_arLabelStyles [(int)INDEX_LABEL.VALUE_COMPONENT].m_backColor;
                else
                    ;
            }

            public PanelTecLastMinutes(IContainer container, StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc frepClr*/)
                : this(tec/*, fErrRep, fWarRep, fActRep, frepClr*/)
            {
                container.Add(this);
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            private void Initialize()
            {
                int i = -1;
                m_list_TECComponents = new List<TECComponentBase> ();
               
                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = ((HDateTime.ToMoscowTimeZone(DateTime.Now)).Date.Equals(HAdmin.SeasonDateTime.Date) ? 25 : 24) + COUNT_FIXED_ROWS;

                for (i = 0; i < this.RowCount; i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                //Добавить наименование станции
                m_listLabelNames.Add(HLabel.createLabel(m_tecView.m_tec.name_shr, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]));
                this.Controls.Add(m_listLabelNames[m_listLabelNames.Count - 1], 0, 0);

                foreach (TECComponent g in m_tecView.m_tec.ListTECComponents)
                    if (g.IsGTP == true)
                    {
                        //Добавить наименование ГТП
                        m_listLabelNames.Add (HLabel.createLabel (g.name_shr.Split (' ') [1], PanelLastMinutes.s_arLabelStyles [(int)INDEX_LABEL.NAME_COMPONENT]));
                        this.Controls.Add(m_listLabelNames[m_listLabelNames.Count -1], CountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Добавить компонент ТЭЦ (ГТП)
                        m_list_TECComponents.Add(g);
                    }
                    else
                        ;

                //initTableHourRows();

                for (i = 0; i < CountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / CountTECComponent));
                }

                if (CountTECComponent > 0)
                    // подпись к наименованию ТЭЦ всегда 1-ая
                    this.SetColumnSpan(m_listLabelNames [0], CountTECComponent);
                else
                    ;
            }

            public void SetDelegateReport (DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            {
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            }

            public override void Start()
            {
                base.Start ();

                if (m_tecView.IsStarted == true)
                    return;
                else
                    ;

                m_tecView.Start ();
            }

            public override void Stop()
            {
                if (m_tecView.IsStarted == false)
                    return;
                else
                    ;

                m_tecView.Stop();

                m_tecView.ReportClear(true);

                base.Stop ();
            }

            private void changeState()
            {
                //m_tecView.m_curDate = ... получено при обработке события
                //m_tecView.m_curDate = m_tecView.m_curDate.Add(-HAdmin.GetUTCOffsetOfCurrentTimeZone ());

                m_tecView.ChangeState ();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate (active);

                m_tecView.Activate(active);

                if (m_tecView.Actived == true)
                {
                    //ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }

                return bRes;
            }

            /// <summary>
            /// Обработчие события - изменение значения свойства
            /// </summary>
            /// <param name="datetimeNew">Новое значение даты/времени</param>
            public void OnEventChangeDateTime (DateTime datetimeNew) {
                m_tecView.m_curDate = datetimeNew;
                //m_tecView.m_curDate = new DateTime(((DateTime)obj).Year
                //  , ((DateTime)obj).Month
                //  , ((DateTime)obj).Day
                //  , ((DateTime)obj).Hour
                //  , ((DateTime)obj).Minute
                //  , ((DateTime)obj).Millisecond
                //  , DateTimeKind.Unspecified);

                initTableHourRows();

                changeState ();
            }

            private void addRow(int indx)
            {
                //Память под ячейки со значениями
                m_listDictLabelVal.Add(new Dictionary<int, Label>());
                m_listDictToolTip.Add(new Dictionary<int, ToolTip>());

                int col = 0;
                foreach (TECComponent g in m_tecView.m_tec.ListTECComponents)
                {
                    if (g.IsGTP == true)
                    {
                        m_listDictLabelVal[indx].Add(g.m_id, HLabel.createLabel(0.ToString(@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]));

                        m_listDictToolTip[indx].Add(g.m_id, new ToolTip());
                        m_listDictToolTip[indx][g.m_id].IsBalloon = true;
                        m_listDictToolTip[indx][g.m_id].ShowAlways = true;
                        m_listDictToolTip[indx][g.m_id].SetToolTip(m_listDictLabelVal[indx][g.m_id], Hd2PercentControl.StringToolTipEmpty);

                        this.Controls.Add(m_listDictLabelVal[indx][g.m_id], col++, indx + COUNT_FIXED_ROWS);

                        this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                    }
                    else
                        ;
                }
            }

            private void initTableHourRows()
            {
                bool bSeason = false
                    , bChangedCountRows = false;
                int hour = -1
                    , cntHours = -1;

                if (! (m_tecView.m_curDate.Year == 1))
                {
                    if (m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                    {
                        bSeason = true;
                        cntHours = 25;
                    }
                    else
                        cntHours = 24;

                    if ((m_listDictLabelVal == null) || (m_listDictToolTip == null))
                    {
                        m_listDictLabelVal = new List<Dictionary<int, Label>>();
                        m_listDictToolTip = new List<Dictionary<int, ToolTip>>();
                    }
                    else
                        ;

                    if (m_listDictLabelVal.Count == 0)
                    {
                        for (hour = 0; hour < cntHours; hour++)
                        {
                            addRow(hour);
                        }

                        //for (hour = 0; hour < COUNT_FIXED_ROWS; hour++)
                        //{
                        //    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                        //}
                    }
                    else
                    {
                        if (!(m_listDictLabelVal.Count == cntHours))
                        {
                            if (m_listDictLabelVal.Count > cntHours)
                            {
                                foreach (Label lbl in m_listDictLabelVal[cntHours].Values)
                                {
                                    this.Controls.Remove(lbl);
                                }

                                m_listDictLabelVal.RemoveAt(cntHours);
                                m_listDictToolTip.RemoveAt(cntHours);

                                this.RowStyles.RemoveAt(this.RowStyles.Count - 1);
                            }
                            else
                            {
                                if (m_listDictLabelVal.Count < cntHours)
                                {
                                    addRow(24);

                                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                                }
                                else
                                {
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                    ; //Дата/время не известны
            }

            private void showLastMinutesTM()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowLastMinutesTM));
                else
                    Logging.Logg().Error(@"PanelTecLastMinutes::showLastMinutesTM () - ... BeginInvoke (ShowLastMinutesTM) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void ShowLastMinutesTM()
            {
                Label label = null;
                Color clrBackColor;
                int warn = -1
                    , cntWarn = -1
                    ;
                Hd2PercentControl d2PercentControl = new Hd2PercentControl ();
                string strToolTip = string.Empty
                    , strWarn = string.Empty;

                foreach (TECComponent g in m_list_TECComponents)
                {
                    cntWarn = 0;
                    for (int hour = 1; hour < m_listDictLabelVal.Count + 1; hour++)
                    {
                        label = m_listDictLabelVal [hour - 1] [g.m_id];
                        clrBackColor = s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT].m_backColor;
                        strToolTip = string.Empty;

                        bool bPmin = false;
                        if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                        strToolTip = d2PercentControl.Calculate(m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id], bPmin, out warn);

                        m_listDictToolTip[hour - 1][g.m_id].SetToolTip(label, strToolTip);

                        if (m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM > 1)
                        {
                            if ((! (warn == 0)) &&
                                (m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM > 1))
                            {
                                cntWarn++;
                                if (cntWarn > 3)
                                    clrBackColor = Color.Red;
                                else
                                    clrBackColor = Color.Yellow;
                            }
                            else
                                cntWarn = 0;

                            if (cntWarn > 0) {
                                //strWarn = cntWarn + @":";
                            }
                            else
                                strWarn = string.Empty;

                            label.Text = strWarn + m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM.ToString(@"F2");
                        }
                        else {
                            cntWarn = 0;
                            label.Text = 0.ToString(@"F0");
                        }

                        if (clrBackColor.Equals (s_arLabelStyles [(int)INDEX_LABEL.VALUE_COMPONENT].m_backColor) == true)
                            label.ForeColor = s_arLabelStyles [(int)INDEX_LABEL.VALUE_COMPONENT].m_foreColor;
                        else
                            label.ForeColor = SystemColors.ControlText;
                        label.BackColor = clrBackColor;
                    }
                }
            }

            public void UpdateGraphicsCurrent ()
            {
                showLastMinutesTM ();
            }
        }
    }
}
