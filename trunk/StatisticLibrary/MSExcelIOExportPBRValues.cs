﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GemBox.Spreadsheet;


using System.Threading;
using System.IO;
using ASUTP.MSExcel;
using ASUTP;
using ASUTP.Core;

namespace StatisticCommon
{
    partial class AdminTS_KomDisp
    {
        public static MODE_EXPORT_PBRVALUES ModeDefaultExportPBRValues = MODE_EXPORT_PBRVALUES.MANUAL;

        public struct CONST_EXPORT_PBRVALUES {
            public string MaskDocument;
            public string MaskExtension;

            public int NumberRow_0;

            public int NumberColumn_Date;
            public int NumberRow_Date;
            public string Format_Date;
        }

        public static CONST_EXPORT_PBRVALUES ConstantExportPBRValues;

        public class MSExcelIOExportPBRValues :
#if HCLASSLIBRARY_MSEXCELIO
            MSExcelIO
#else
            object
#endif
        {
            /// <summary>
            /// Интерфейс для реализации возможности присваивания значений в наследуемой структуре
            /// </summary>
            private interface IVALUES {
                int m_iNumberPBR { get; set; }
                /// <summary>
                /// Номер столбца для размещения значений в книге MS Excel
                /// </summary>
                int m_indxColumn { get; set; }
                /// <summary>
                /// Значения для заполнения столбца в книге MS Excel
                /// </summary>
                RDGStruct [] m_data {
                    get; set;
                }
            }
            /// <summary>
            /// Структура для хранения информации по заполнению книги MS Excel
            /// </summary>
            private struct VALUES : IVALUES {
                public int m_iNumberPBR { get; set; }

                public int m_indxColumn { get; set; }

                private RDGStruct [] _data;
                /// <summary>
                /// Значения для заполнения столбца в книге MS Excel
                /// </summary>
                public RDGStruct [] m_data
                {
                    get
                    {
                        return _data;
                    }

                    set
                    {
                        int err = -1;

                        int iNumberPBR = -1;

                        if (!(value == null)) {
                            for (int iHour = 0; iHour < value.Length; iHour++) {
                                iNumberPBR = HAdmin.GetPBRNumber (value [iHour].pbr_number, out err);

                                if ((!(err < 0))
                                    && (m_iNumberPBR < iNumberPBR))
                                    m_iNumberPBR = iNumberPBR;
                                else
                                    ;
                            }

                            _data = new RDGStruct [value.Length];
                            value.CopyTo (_data, 0);
                        } else
                            _data = value;
                    }
                }
            }
            /// <summary>
            /// Перечесление - возможные значения результата выполнения операции
            /// </summary>
            public enum RESULT {
                /// <summary>
                /// Успешное выполнение
                /// </summary>
                OK
                /// <summary>
                /// Требуется выполнение очередной операции (автоматич. режим - по расписанию)
                /// </summary>
                , SHEDULE
                , VISIBLE
                /// <summary>
                /// Ошибка - превышено время ожидания
                /// </summary>
                , ERROR_WAIT
                , ERROR_TEMPLATE
                , ERROR_OPEN
                , ERROR_RETRY
                , ERROR_APP
                    , COUNT
            }
            /// <summary>
            /// Класс для передачи аргументов для события 'Result'
            /// </summary>
            public class EventResultArgs : EventArgs {
                public EventResultArgs ()
                    : base ()
                {
                    Result = RESULT.OK;
                }

                public RESULT Result;
            }

            private EventResultArgs _prevArg;

            private bool _bAllowVisibled;

            public bool AllowVisibled
            {
                get
                {
                    return _bAllowVisibled;
                }

                set
                {
#if HCLASSLIBRARY_MSEXCELIO
#else
                    
                    //??? не забывать о природе 'Visible'
                    Visible = value;
#endif
                    _bAllowVisibled = value;
                }
            }

            private MODE_EXPORT_PBRVALUES _mode;
            /// <summary>
            /// Режим работы объекта
            /// </summary>
            public MODE_EXPORT_PBRVALUES Mode
            {
                get { return _mode; }

                set
                {
                    int due = System.Threading.Timeout.Infinite
                        , period = System.Threading.Timeout.Infinite;

                    if (value == MODE_EXPORT_PBRVALUES.AUTO) {
                        due = dueTime * 1000;
                        period = SEC_SHEDULE_PERIOD_EXPORT_PBR * 1000;
                    } else if (value == MODE_EXPORT_PBRVALUES.MANUAL) {
                        ;
                    } else
                        ;

                    _timerShedule.Change (due, period);

                    _mode = value;
                }
            }
            ///// <summary>
            ///// Интервал времени для 1-го запуска на выполнение задачи по расписанию
            /////  от начала суток (МСК)
            ///// </summary>
            //public TimeSpan m_tsShedule;
            ///// <summary>
            ///// Интервал времени между запусками на выполнение задачи по расписанию
            ///// </summary>
            //public TimeSpan m_tsPeriod;
            /// <summary>
            /// Событие для внешних подписчиков о результате операции экспорта
            /// </summary>
            public event Action<object, EventResultArgs> Result;
            /// <summary>
            /// Дата текущего набора значений для экпорта
            /// </summary>
            private DateTime _date;
            /// <summary>
            /// Словарь со значениями для экспорта
            /// , ключ - идентификатор компонента ТЭЦ
            /// </summary>
            private Dictionary<int, IVALUES> _dictValues;
            /// <summary>
            /// Объект синхронизации - признак занятости объекта выполнением длительной операции при работе с MS Excel
            /// </summary>
            private ManualResetEvent _mnlResetEventBusy;
            /// <summary>
            /// Объект - поток в котором выполняется длитедьная операция
            /// </summary>
            private System.Threading.Thread _thread;
            /// <summary>
            /// Таймеры - для контроля длительности выполнения операции
            ///  и запуска очередной операции по расписанию в автоматическом режиме
            /// </summary>
            private System.Threading.Timer _timerWiat
                , _timerShedule;

            /// <summary>
            /// Признак выполнения в текущий момент операции
            /// </summary>
            public bool Busy { get { return _mnlResetEventBusy.WaitOne (0); } }

            /// <summary>
            /// Конструктор - основной(без аргументов)
            /// </summary>
            public MSExcelIOExportPBRValues ()
                : base ()
            {
                _date = DateTime.MinValue;

                _dictValues = new Dictionary<int, IVALUES> ();

                _mnlResetEventBusy = new ManualResetEvent (false);
                _timerWiat = new System.Threading.Timer (fTimerWait, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _timerShedule = new System.Threading.Timer (fTimerShedule, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                _prevArg = new EventResultArgs () { Result = RESULT.OK };

                Mode = AdminTS_KomDisp.ModeDefaultExportPBRValues;
            }

            /// <summary>
            /// Добавить компонент ТЭЦ, для которго необходимо выполнить экспорт значений
            /// </summary>
            /// <param name="comp">Объект - компонент ТЭЦ</param>
            /// <returns>Признак результата выполнения метода</returns>
            public int AddTECComponent (TECComponent comp)
            {
                return addTECComponent (comp.m_id, comp.m_indx_col_export_pbr_excel);
            }

            /// <summary>
            /// Добавить компонент ТЭЦ, для которго необходимо выполнить экспорт значений
            /// </summary>
            /// <param name="id">Идентификатор компонента ТЭЦ</param>
            /// <param name="indxColToExport">Номер столбца, в котором размещаются значения для этого компонента</param>
            /// <returns>Признак результата выполнения метода</returns>
            public int AddTECComponent (int id, int indxColToExport)
            {
                return addTECComponent (id, indxColToExport);
            }

            /// <summary>
            /// Добавить компонент ТЭЦ, для которго необходимо выполнить экспорт значений
            /// </summary>
            /// <param name="id">Идентификатор компонента ТЭЦ</param>
            /// <param name="indxColToExport">Номер столбца, в котором размещаются значения для этого компонента</param>
            /// <returns>Признак результата выполнения метода</returns>
            private int addTECComponent (int id, int indxColToExport)
            {
                int iRes = Busy == true ? 1 : 0;

                if (iRes == 0) {
                    try {
                        _dictValues.Add (id, new VALUES () { m_data = null, m_indxColumn = indxColToExport });
                    } catch (Exception e) {
                        //??? вероятно, признак инициирования операции экспорта до завершения предыдущей
                        iRes = -1;

                        Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::addTECComponent(id={0}, indx={1}) - дублирование ключа [{0}]..."
                                , id, indxColToExport)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    }
                } else
                    ;

                return iRes;
            }

            /// <summary>
            /// Добавить значения для экспорта для указанного по идентификатору компонента
            /// </summary>
            /// <param name="id">Идентификатор компонента ТЭЦ</param>
            /// <param name="values">Значения для экспорта</param>
            public void AddPBRValues (int id, RDGStruct [] values)
            {
                if (_dictValues.ContainsKey (id) == true) {
                    _dictValues [id].m_data = values;
                    if (_numberPBR < _dictValues [id].m_iNumberPBR)
                        _numberPBR = _dictValues [id].m_iNumberPBR;
                    else
                        ;
                } else
                    // ошибка - ранее не был добавлен необходимый компонент с идентификатором
                    Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::AddPBRValues (ID={0}) - ранее не был добавлен необходимый компонент с идентификатором, указанным в аргументе..."
                            , id)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Выполнить операцию размещения значений в книге MS Excel
            ///  , и в автоматическом режиме сохранить ее
            /// </summary>
            public void Run ()
            {
                // установить признак выполнения операции(занятости)
                _mnlResetEventBusy.Set ();
                // Интервал времени ожидания завершения операции
                TimeSpan tsWait = TimeSpan.FromMilliseconds (MS_WAIT_EXPORT_PBR_MAX);
                // запустить таймер ожидания завершения длительной операции
                _timerWiat.Change((int)tsWait.TotalMilliseconds, System.Threading.Timeout.Infinite);

                //TODO: ...длительная операция
                _thread = new Thread (new ParameterizedThreadStart (run)) { IsBackground = true, Name = string.Format ("MSExcelIOExportPBRValues") };
                _thread.Start (tsWait);
            }

#if HCLASSLIBRARY_MSEXCELIO
            private bool IsReOpen
            {
                get
                {
                    return _prevArg.Result == RESULT.ERROR_APP
                        || _prevArg.Result == RESULT.ERROR_OPEN
                        || _prevArg.Result == RESULT.ERROR_RETRY;
                }
            }
#else
#endif

            /// <summary>
            /// Выполнить операцию размещения значений в книге MS Excel
            ///  , и в автоматическом режиме сохранить ее
            /// </summary>
            private void run (object obj)
            {
                List<FileInfo> listFileInfoDest;
                EventResultArgs arg;
                int iTemplate = 0 // признак продолжения выполнения операции - наличие шаблона
                    , err = 0; // по умолчанию ошибок нет

                try {
#if HCLASSLIBRARY_MSEXCELIO
                    if (IsReOpen == true)
                        ReCreate ();
                    else
                        ;
#else
#endif
                    listFileInfoDest = new List<FileInfo> (Directory.GetFiles (Folder_CSV, string.Format ("*{0}*.{1}", ConstantExportPBRValues.MaskDocument, ConstantExportPBRValues.MaskExtension), SearchOption.TopDirectoryOnly).ToList ()
                        .ConvertAll<FileInfo> (name => new FileInfo (string.Format (@"{0}", name))));

                    if (listFileInfoDest.Count > 1) {
                        // выполнить сортировку, затем открыть "самый новый" файл
                        listFileInfoDest.Sort ((fileInfo1, fileInfo2) => {
                            return (int)(fileInfo1.LastWriteTime - fileInfo2.LastWriteTime).TotalSeconds < 0 ? 1
                                : (int)(fileInfo1.LastWriteTime - fileInfo2.LastWriteTime).TotalSeconds > 0 ? -1
                                    : 0; });

                    } else if (listFileInfoDest.Count > 0) {
                        // файл единственный(вероятно, шаблон) - открыть
                        iTemplate = 1;
                    } else
                        // файл отсутствует - открыть шаблон
                        iTemplate = -1;

                    if (!(iTemplate < 0)) {
#if HCLASSLIBRARY_MSEXCELIO
                        if (IsOpen(Path.GetFileName(listFileInfoDest [0].FullName), out err) == true) {
                            Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - книга с наименованием={0}\\{1} открыта..."
                                    , Folder_CSV, TemplateDocument)
                                , Logging.INDEX_MESSAGE.NOT_SET);

                            if (Mode == MODE_EXPORT_PBRVALUES.AUTO) {
                                Visible = !CloseExcelDoc (Path.GetFileName (listFileInfoDest [0].FullName));
                            } else if (Mode == MODE_EXPORT_PBRVALUES.MANUAL) {
                                Visible = true;
                            } else
                                ;
#else
#endif
#if HCLASSLIBRARY_MSEXCELIO
                        } else {
                            if (Visible == true)
                                Visible = false;
                            else
                                ;
                        }

                        if (Visible == false)
#else
#endif
                            if (openDocument (listFileInfoDest [0].FullName) == 0) {
                                Logging.Logg ().Debug (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - открыли документ {0}...", listFileInfoDest [0].FullName)
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                                #region Заполнить лист книги MS Excel значениями
                                selectWorksheet (1);

                                if (writeValue (ConstantExportPBRValues.NumberColumn_Date, ConstantExportPBRValues.NumberRow_Date, HDateTime.ToMoscowTimeZone ().ToString (ConstantExportPBRValues.Format_Date)) == false)
                                    Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - не удалось сохранить дату/время обновления...")
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                else
                                    Logging.Logg().Action(string.Format("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - сохранили дату/время обновления [столб.={0}, стр.={1}, знач.={2}]..."
                                            , ConstantExportPBRValues.NumberColumn_Date, ConstantExportPBRValues.NumberRow_Date, HDateTime.ToMoscowTimeZone().ToString(ConstantExportPBRValues.Format_Date))
                                        , Logging.INDEX_MESSAGE.NOT_SET);

                                foreach (KeyValuePair<int, IVALUES> pair in _dictValues) {
                                    for (int iHour = 0; iHour < pair.Value.m_data.Length; iHour++)
                                        if (writeValue (pair.Value.m_indxColumn, ConstantExportPBRValues.NumberRow_0 + iHour, pair.Value.m_data [iHour].pbr) == false)
                                            Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - [компонент_ID={0}, час={1}] не удалось сохранить значение {2}"
                                                    , pair.Key, iHour + 1, pair.Value.m_data [iHour].pbr)
                                                , Logging.INDEX_MESSAGE.NOT_SET);
                                        else
                                            ;
                                }
#endregion

                                _previousNameDocument = NameDocument;
                                save (_previousNameDocument);

                                Logging.Logg ().Action (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - сохранили документ {0} с наименованием {1}...", listFileInfoDest [0].FullName, _previousNameDocument)
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                                try {
                                    if ((listFileInfoDest.Count > 0)
                                        && (_previousNameDocument.Equals (listFileInfoDest [0].FullName) == false)
                                        && (TemplateDocument.Equals (listFileInfoDest [0].FullName) == false))
                                        listFileInfoDest [0].Delete ();
                                    else
                                        ;
                                } catch (Exception e) {
                                    Logging.Logg ().Error (string.Format("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - ошибка удаления '{0}', причина={1}..."
                                            , listFileInfoDest [0].FullName, e.Message)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                }

                                if (Mode == MODE_EXPORT_PBRVALUES.AUTO) {
#if HCLASSLIBRARY_MSEXCELIO
                                    CloseExcelDoc ();
#else
#endif
                                    arg = new EventResultArgs () { Result = RESULT.OK };
                                } else {
                                    arg = new EventResultArgs() { Result = RESULT.VISIBLE };
                                }
                            } else {
                                Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - не удалось открыть книгу MS Excel с наименованием={0}..."
                                        , listFileInfoDest [0].FullName)
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                                arg = new EventResultArgs () { Result = RESULT.ERROR_OPEN };
                            }
#if HCLASSLIBRARY_MSEXCELIO
                        else
                        // Видимость 'true' - ожидать действий пользователя
                            arg = new EventResultArgs () { Result = RESULT.ERROR_RETRY };
#else
#endif
                    } else {
                        Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - отсутствует шаблон с наименованием={0}\\{1}..."
                                , Folder_CSV, TemplateDocument)
                            , Logging.INDEX_MESSAGE.NOT_SET);

                        arg = new EventResultArgs () { Result = RESULT.ERROR_TEMPLATE };
                    }
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run () - ..."), Logging.INDEX_MESSAGE.NOT_SET);

                    arg = new EventResultArgs () { Result = RESULT.ERROR_APP };
                }

                // остановить таймер ожидания завершения длительной операции
                stop_TimerWait ();

                clearContext ();

                _mnlResetEventBusy.Reset ();

                // при внештатном завершении потока - продолжить логгирование и работу приложения в целом 
                try { Logging.Logg ().Debug (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run() - завершение потока..."), Logging.INDEX_MESSAGE.NOT_SET); } catch  { }

                Result (this, _prevArg = arg);
            }

#if HCLASSLIBRARY_MSEXCELIO
            public override bool Visible
            {
                get
                {
                    return base.Visible;
                }

                set
                {
                    //??? непонятно что выполнять
                    // 1) MS Excel изменить видимость
                    // 2) открыть/закрыть файл-результат
                    base.Visible = value;
                }
            }
#else
            private bool _visible;

            public bool Visible
            {
                get
                {
                    return _visible;
                }

                set
                {
                    int err = -1;

                    Action<object> fThread = delegate (object obj)
                    {
                        MSExcelIO msExcelIO = new MSExcelIO();
                        if (value == true) {
                            if ((msExcelIO.IsValidate == true)
                                && (string.IsNullOrEmpty(_previousNameDocument) == false)) {
                                msExcelIO.OpenDocument(_previousNameDocument);
                                msExcelIO.Visible =
                                _visible =
                                    true;
                            } else
                                _visible = false;
                        } else if (value == false) {
                            //TODO: найти открытый документ и закрыть его
                            if ((msExcelIO.IsValidate == true)
                                && (string.IsNullOrEmpty(_previousNameDocument) == false)
                                && (msExcelIO.IsOpen(_previousNameDocument, out err) == true))
                                msExcelIO.CloseExcelDoc(_previousNameDocument);
                            else
                                ;

                            _visible = value;
                        } else
                            ;
                    };

                    new Thread(new ParameterizedThreadStart(fThread)).Start();
                }
            }
#endif

            /// <summary>
            /// Метод обратного вызова для таймера контроля длительности выполнения операции
            /// </summary>
            /// <param name="obj">Объект-аргумент, использующийся при вызове методе</param>
            private void fTimerWait (object obj)
            {
                abort ();
            }

            /// <summary>
            /// Метод обратного вызова для таймера при автоматическом экспорте значений
            /// </summary>
            /// <param name="obj">Объект-аргумент, использующийся при вызове методе</param>
            private void fTimerShedule (object obj)
            {
                if (Busy == false)
                    Result (this, new EventResultArgs () { Result = RESULT.SHEDULE });
                else
                    Logging.Logg ().Warning (string.Format ("AdminTS_KomDisp::fTimerShedule () - пропущен очередной запуск задачи по расписанию...")
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Установить дату, за которую требуется экспортировать значения
            /// </summary>
            /// <param name="date">Дата экспорта</param>
            /// <returns>Признак результата выполнения метода</returns>
            public bool SetDate (DateTime date)
            {
                bool bRes = false;

                if (_date.Equals (DateTime.MinValue) == true) {
                    _date = date;

                    bRes = true;
                } else
                // дата уже установлена
                    if (_date.Equals (date) == true)
                    // успех - установленная ранее дата совпадает с текущей
                    bRes = true;
                else
                    // ошибка - добавление массива значений для одного из компонентов за другую дату по сравнению с предыдущими компонентами
                    //  , вероятно, признак инициирования пользователем новой операции до завершения пред.
                    Logging.Logg ().Error (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::SetDate({0}) - добавление массива значений для одного из компонентов за другую дату {1} по сравнению с предыдущими компонентами"
                            , date.ToString (), _date)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                return bRes;
            }

            /// <summary>
            /// Очистить конекст(значения для экспорта) при завершении операции
            /// </summary>
            private void clearContext ()
            {
                _numberPBR = -1;

                _date = DateTime.MinValue;

                _dictValues.Clear ();
            }

            /// <summary>
            /// Наименование шаблона документа MS Excel при экспорте
            /// </summary>
            public string TemplateDocument
            {
                get
                {
                    return string.Format (@"{0}\{1}.{2}"
                        , Folder_CSV, ConstantExportPBRValues.MaskDocument, ConstantExportPBRValues.MaskExtension);
                }
            }

            /// <summary>
            /// Наименование документа, использовавшееся в крайней операции экспорта
            ///  , т.к. значение 'NameDocument' имеет зависимость от текущего времени
            /// </summary>
            private string _previousNameDocument;

            ///// <summary>
            ///// Актуальное наименование документа MS Excel при экспорте
            ///// </summary>
            //private string ActualNameDocument
            //{
            //    get
            //    {
            //        return ;
            //    }
            //}

            /// <summary>
            /// Наименование документа MS Excel при экспорте
            /// </summary>
            public string NameDocument
            {
                get
                {
                    return string.Format (@"{0}\{1}-{2}-{3}.{4}"
                        , Folder_CSV, ConstantExportPBRValues.MaskDocument, HDateTime.ToMoscowTimeZone ().ToString ("yyyyMMdd-HHmm"), _numberPBR.ToString ("00"), ConstantExportPBRValues.MaskExtension);
                }
            }

            private int _numberPBR;
            /// <summary>
            /// Количество секунд до очередной операции экспорта значений
            ///  в автоматическом режиме
            /// </summary>
            private int dueTime
            {
                get
                {
                    int iRes = -1;

                    DateTime dtNow
                        , dtStart;

                    dtNow = HDateTime.ToMoscowTimeZone ();
                    dtStart = dtNow.Date.AddSeconds (SEC_SHEDULE_START_EXPORT_PBR);

                    if ((dtNow - dtStart).TotalSeconds > 0)
                        iRes = SEC_SHEDULE_PERIOD_EXPORT_PBR - (int)(dtNow - dtStart).TotalSeconds % SEC_SHEDULE_PERIOD_EXPORT_PBR;
                    else if ((dtNow - dtStart).TotalSeconds < 0)
                        iRes = (int)(dtStart - dtNow).TotalSeconds % SEC_SHEDULE_PERIOD_EXPORT_PBR;
                    else if ((dtNow - dtStart).TotalSeconds == 0)
                        iRes = 0;
                    else
                        ;

                    return iRes;
                }
            }

            private void abort ()
            {
                if ((_thread.IsAlive == true)
                    && (_thread.Join (MS_WAIT_EXPORT_PBR_ABORT) == false)) {
                    _thread.Interrupt ();

                    clearContext ();

                    _mnlResetEventBusy.Reset ();

                    Logging.Logg ().Warning (string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::Run() - непредвиденное завершение потока...")
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    Result (this, new EventResultArgs () { Result = RESULT.ERROR_WAIT });
                } else
                    ;
            }

            private void stop_TimerWait ()
            {
                _timerWiat.Change (System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }

            public void Abort ()
            {
                abort ();
            }

#if HCLASSLIBRARY_MSEXCELIO
#else
            ExcelFile _msExcelFile;

            ExcelWorksheet _msExcelSheet;
#endif

            private int openDocument (string name)
            {
                int iRes = -1;

#if HCLASSLIBRARY_MSEXCELIO
                iRes = OpenDocument (name);
#else
                try {
                    _msExcelFile = ExcelFile.Load (name);

                    iRes = 0;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::openDocument(путь={0}) - ...", name), Logging.INDEX_MESSAGE.NOT_SET);
                }
#endif

                return iRes;
            }

            private void selectWorksheet (int num)
            {
#if HCLASSLIBRARY_MSEXCELIO
                SelectWorksheet (num);
#else
                try {
                    _msExcelSheet = _msExcelFile.Worksheets[num - 1];
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("MSExcelIOExportPBRValues::selectWorksheet(номер={0}) - ...", num), Logging.INDEX_MESSAGE.NOT_SET);
                }
#endif
            }

            private bool writeValue (int col, int row, string value)
            {
                bool bRes = false;

#if HCLASSLIBRARY_MSEXCELIO
                bRes = WriteValue (col, row, value);
#else
                try {
                    ExcelColumn msExcelColumn = _msExcelSheet.Columns [col];
                    msExcelColumn.Cells [row].Value = value;

                    bRes = true;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::writeValue(столбец={0}, строка={1}, значение={2}) - ..."
                            , col, row, value)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
#endif
                return bRes;
            }

            private bool writeValue (int col, int row, double value)
            {
                bool bRes = false;

#if HCLASSLIBRARY_MSEXCELIO
                bRes = WriteValue (col, row, value);
#else
                try {
                    ExcelColumn msExcelColumn = _msExcelSheet.Columns [col - 1];
                    msExcelColumn.Cells [row - 1].Value = value;

                    bRes = true;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::writeValue(столбец={0}, строка={1}, значение={2}) - ..."
                            , col, row, value)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
#endif
                return bRes;
            }

            private bool save (string name)
            {
                bool bRes = false;

#if HCLASSLIBRARY_MSEXCELIO
                bRes = SaveExcel (name);
#else
                try {
                    _msExcelFile.Save(name);

                    bRes = true;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, string.Format ("AdminTS_KomDisp.MSExcelIOExportPBRValues::save(наименовнаие={0}) - ...", name), Logging.INDEX_MESSAGE.NOT_SET);
                }
#endif
                return bRes;
            }
        }
    }
}
