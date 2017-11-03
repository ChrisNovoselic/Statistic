﻿using System;
using System.Data.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Globalization;

using StatisticCommon;
using System.Reflection;
using ASUTP;

namespace CommonAux
{
    /// <summary>
    /// Класс для описания ТЭЦ с параметрами сигналов
    /// </summary>
    public class TEC_LOCAL
    {
        /// <summary>
        /// Список всех значений за весь диапазон дат
        /// </summary>
        public List<VALUES_DATE> m_listValuesDate;
        /// <summary>
        /// Класс для хранения значений за сутки (все группы сигналов)
        /// </summary>
        public struct VALUES_DATE
        {
            /// <summary>
            /// Структура для хранения значений за сутки
            /// </summary>
            public struct VALUES_SIGNAL
            {
                public double[] m_data;

                public bool m_bUse;

                public VALUES_SIGNAL(bool bUse)
                {
                    m_data = new double[24];

                    m_bUse = bUse;
                }
            }
            /// <summary>
            /// Класс для хранения значений группы сигналов
            /// </summary>
            public class VALUES_GROUP : Dictionary<SIGNAL.KEY, VALUES_SIGNAL>
            {
                /// <summary>
                /// Перечисление для методов расчета потерь эл./эн. в сети ТЭЦ (стандартный/специальный)
                /// </summary>
                public enum MODE : short { STANDARD, EPOTERI }
                //!!! сумма значений каждого из массивов 'sgnlValues', 'grpValues' д.б. равны между собой
                /// <summary>
                /// Cумма для группы сигналов по часам
                /// </summary>
                public double[] m_summaHours;
                /// <summary>
                /// Cумма для сигналов за сутки (по-сигнально)
                ///  индекс сигнала соответствует индексу ключа сигнала в коллекции ключей объекта
                /// </summary>
                public List<double> m_summaSgnls;
                /// <summary>
                /// Сумма значений всех сигналов группы за сутки
                /// </summary>
                public double m_Summa;

                private MODE _mode;
                /// <summary>
                /// Конструктор - основной (без параметров)
                /// </summary>
                private VALUES_GROUP(MODE mode)
                {
                    _mode = mode;

                    initialize();
                }

                public VALUES_GROUP(List<SIGNAL> listSgnls, MODE mode) : this(mode)
                {
                    InitSignals(listSgnls);
                }
                /// <summary>
                /// Инициализация полей объекта
                /// </summary>
                private void initialize()
                {
                    m_summaHours = new double[24];

                    if (m_summaSgnls == null)
                        m_summaSgnls = new List<double>();
                    else
                        m_summaSgnls.Clear();

                    m_Summa = 0F;
                }

                public void InitSignals(List<SIGNAL> listSgnls)
                {
                    foreach (SIGNAL sgnl in listSgnls)
                    {
                        Add(sgnl.m_key, new VALUES_SIGNAL(sgnl.m_bUse));

                        m_summaSgnls.Add(0F);
                    }
                }
                /// <summary>
                /// Установить значение для сигнала по ключу, индексу часа
                /// </summary>
                /// <param name="key">Ключ сигнала</param>
                /// <param name="iHour">Индекс часа</param>
                /// <param name="value">Устанавливаемое значение</param>
                public void SetValue(SIGNAL.KEY key, int iHour, double value)
                {
                    int indxKey = Keys.ToList().IndexOf(key);

                    this[key].m_data[iHour] = value;

                    if (indxKey < m_summaSgnls.Count)
                    {
                        m_summaSgnls[indxKey] += value;

                        if (this[key].m_bUse == true)
                        {
                            if (_mode == MODE.STANDARD)
                            {
                                m_summaHours[iHour] += value;
                                m_Summa += value;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                    else
                        throw new Exception(string.Format(@"TEC.VALUES_DATE.VALUES_GROUP::SetValue () - для сигнала key={0} за час={1}..."
                            , key.ToString(), iHour));
                }
                /// <summary>
                /// Специальный метод расчета потерь эл./эн. в сети ТЭЦ
                ///  только для группы в режиме 'MODE.EPOTERI'
                /// </summary>
                public void CompleteSetValues()
                {
                    int[,] Koef = new int[,] { { 40000, 45, 168 }
                        , { 40000, 45, 168 }
                        , { 40000, 41, 163 }
                        , { 40000, 41, 163 }
                        , { 40000, 33, 168 }
                        , { 40000, 33, 168 }
                    };

                    double[] k1 = new double[] { (double)Koef[0, 1] / 2 //4
                        , (double)Koef[1, 1] / 2 //4
                        , (double)Koef[2, 1] / 2 //4
                        , (double)Koef[3, 1] / 2 //4
                        , (double)Koef[4, 1] / 2 //4
                        , (double)Koef[5, 1] / 2 //4
                    };
                    double[] k2 = new double[] { 1/*2*/ * Koef[0, 2] / Math.Pow(Koef[0, 0], 2)
                        , 1/*2*/ * Koef[1, 2] / Math.Pow(Koef[1, 0], 2)
                        , 1/*2*/ * Koef[2, 2] / Math.Pow(Koef[2, 0], 2)
                        , 1/*2*/ * Koef[3, 2] / Math.Pow(Koef[3, 0], 2)
                        , 1/*2*/ * Koef[4, 2] / Math.Pow(Koef[4, 0], 2)
                        , 1/*2*/ * Koef[5, 2] / Math.Pow(Koef[5, 0], 2)
                    };

                    int o = 4369;
                    int[,] items = new int[,] { { 25, 29, 27, 31} //P-, P-, Q-, Q-
                        , { 26, 30, 28, 32 } //P+, P+, Q+, Q+
                        , { 33, 37, 35, 39 } //P-, P-, Q-, Q-
                        , { 34, 38, 36, 40 } //P+, P+, Q+, Q+
                        , { 41, 43, 42, 44 } //P-, P-, Q-, Q-
                        , { 73, 75, 74, 76 } //P+, P+, Q+, Q+
                    };

                    int iHour = -1
                        , iPoteri = -1;
                    SIGNAL.KEY key;
                    double e;
                    double[,] values = new double[6 + 1, 24 + 1]; // "+1" для суммарных значений

                    if (_mode == MODE.EPOTERI)
                    {
                        for (iPoteri = 0; iPoteri < 6; iPoteri++)
                        {
                            for (iHour = 0; iHour < 24; iHour++)
                            {
                                e = (Math.Pow(this[new SIGNAL.KEY(o, items[iPoteri, 0])].m_data[iHour] * 1000 + this[new SIGNAL.KEY(o, items[iPoteri, 1])].m_data[iHour] * 1000, 2)
                                    + Math.Pow(this[new SIGNAL.KEY(o, items[iPoteri, 2])].m_data[iHour] * 1000 + this[new SIGNAL.KEY(o, items[iPoteri, 3])].m_data[iHour] * 1000, 2));

                                if (iPoteri % 2 == 0)
                                {
                                    values[iPoteri, iHour] = -k1[iPoteri] - k2[iPoteri] * e;

                                    m_summaHours[iHour] -= values[iPoteri, iHour];
                                }
                                else
                                    if (iPoteri % 2 == 1)
                                {
                                    values[iPoteri, iHour] = k1[iPoteri] + k2[iPoteri] * e;

                                    m_summaHours[iHour] += values[iPoteri, iHour];
                                }
                                else
                                    ;

                                values[iPoteri, 24] += values[iPoteri, iHour];
                            }
                        }

                        for (iHour = 0; iHour < 24; iHour++)
                        {
                            m_summaHours[iHour] /= 1000;
                            m_Summa += m_summaHours[iHour];
                        }
                    }
                    else
                        ;
                }
                /// <summary>
                /// Очистиь значения объекта, привести в начальное (при создании) состояние
                /// </summary>
                public void ClearValues()
                {
                    Clear();

                    initialize();
                }
            }
            /// <summary>
            /// Метка времени текущего объекта
            /// </summary>
            public DateTime m_dataDate;
            /// <summary>
            /// Словарь значений для групп сигналов (ключ - индекс группы сигналов, значение - группа сигналов)
            /// </summary>
            public Dictionary<INDEX_DATA, VALUES_GROUP> m_dictData;

            /// <summary>
            /// Конструктор специальный для реализации алгоритма расчета ТСН по ВАРИАНТУ №2
            /// </summary>
            /// <param name="dt">Дата для набора значений нового экземпляра</param>
            public VALUES_DATE(DateTime dt)
            {
                m_dataDate = dt;

                m_dictData = new Dictionary<INDEX_DATA, VALUES_GROUP>();
            }

            /// <summary>
            /// Конструктор - дополнительный (с парметрами)
            /// </summary>
            /// <param name="dt">Дата - метка времени для набора значений</param>
            /// <param name="indx">Индекс группы сигналов</param>
            /// <param name="listRecRes">Список значений в наборе</param>
            public VALUES_DATE(DateTime dt, INDEX_DATA indx, List<SIGNAL> listSgnls, List<RecordResult> listRecRes)
            {
                m_dataDate = dt;

                m_dictData = new Dictionary<INDEX_DATA, VALUES_GROUP>();

                SetValues(indx, listSgnls, listRecRes);
            }

            /// <summary>
            /// Инициализировать список доступных сигналов в группе
            ///  не вызывается при отсутствии сигналов
            /// </summary>
            /// <param name="indx">Индекс-идентификатор группы сигналов</param>
            /// <param name="listSgnls">Список доступных сигналов</param>
            private void initSignals(INDEX_DATA indx, List<SIGNAL> listSgnls)
            {
                if (m_dictData.ContainsKey(indx) == false)
                {
                    // с VIII-ой группой произойдет вызов только при наличии сигналов
                    m_dictData.Add(indx, new VALUES_GROUP(
                        listSgnls
                        , (!(indx == INDEX_DATA.GRVIII)) ?
                            VALUES_GROUP.MODE.STANDARD :
                            VALUES_GROUP.MODE.EPOTERI));
                }
                else
                    m_dictData[indx].InitSignals(listSgnls);
            }

            /// <summary>
            /// Установить значения для группы сигналов по индексу группы
            /// </summary>
            /// <param name="indx">Индекс группы сигналов</param>
            /// <param name="listSgnls">Список сигналов</param>
            /// <param name="listRecRes">Список значений</param>
            public void SetValues(INDEX_DATA indx, List<SIGNAL> listSgnls, List<RecordResult> listRecRes)
            {
                initSignals(indx, listSgnls);

                SetValues(indx, listRecRes);
            }

            /// <summary>
            /// Установить значения для группы сигналов по индексу группы
            /// </summary>
            /// <param name="indx">Индекс группы сигналов</param>
            /// <param name="listRecRes">Список значений</param>
            public void SetValues(INDEX_DATA indx, List<RecordResult> listRecRes)
            {
                SIGNAL.KEY keySgnl; // ключ для сигнала
                int iHour = -1; // индекс часа
                MethodBase methodBase = MethodBase.GetCurrentMethod();
                string errMsg = string.Format(@"{0}.{1}::{2} () - ", methodBase.Module, methodBase.DeclaringType, methodBase.Name);

                if (m_dictData.ContainsKey(indx) == true)
                {
                    foreach (RecordResult r in listRecRes)
                    {
                        keySgnl = new SIGNAL.KEY(r.m_key);

                        if (m_dictData[indx].ContainsKey(keySgnl) == true)
                        {
                            // дата всегда одна и та же за исключением одной записи
                            iHour = r.m_dtRec.Hour > 0 ? r.m_dtRec.Hour - 1 : 23;
                            m_dictData[indx].SetValue(keySgnl, iHour, r.m_value);
                        }
                        else
                            Logging.Logg().Error(string.Format(@"{0}в словаре c INDEX={1} не инициализирован сигнал key={2}"
                                    , errMsg, indx.ToString(), keySgnl.ToString())
                                , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    m_dictData[indx].CompleteSetValues();
                }
                else
                    Logging.Logg().Error(string.Format(@"{0}в словаре не инициализирована группа сигналов INDEX={1}..."
                            , errMsg, indx.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Установить значение для сигнала группы, по ключу, за указанный час
            /// </summary>
            /// <param name="indx">Индекс группы сигналов</param>
            /// <param name="keySgnl">Ключ сигнала</param>
            /// <param name="iHour">Индекс часа в сутках</param>
            /// <param name="value">Устанавливаемое значение</param>
            public void SetValue(INDEX_DATA indx, SIGNAL.KEY keySgnl, int iHour, double value)
            {
                MethodBase methodBase = MethodBase.GetCurrentMethod();
                string errMsg = string.Format(@"{0}.{1}::{2} () - ", methodBase.Module, methodBase.DeclaringType, methodBase.Name);

                if (m_dictData.ContainsKey(indx) == true)
                    if (m_dictData[indx].ContainsKey(keySgnl) == true)
                        m_dictData[indx][keySgnl].m_data[iHour] = value;
                    else
                        Logging.Logg().Error(string.Format(@"{0}в словаре c INDEX={1} не инициализирован сигнал key={2}"
                                , errMsg, indx.ToString(), keySgnl.ToString())
                            , Logging.INDEX_MESSAGE.NOT_SET);
                else
                    Logging.Logg().Error(string.Format(@"{0}в словаре не инициализирована группа сигналов INDEX={1}..."
                            , errMsg, indx.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Возвратить значения (одни сутки + все часы) в соответствии с ~ ВАРИАНТом расчета
            /// </summary>
            /// <returns>Объект со значениями (одни сутки)</returns>
            public double[] GetValues(out int err)
            {
                err = 0;

                double[] arRes = new double[24];

                int iHour = -1;

                //// вариант №1
                //if (m_dictData.ContainsKey(INDEX_DATA.TSN) == true)
                //    m_dictData[INDEX_DATA.TSN].m_summaHours.CopyTo(arRes, 0);
                //else
                //    err = -1;

                // вариант №2
                if ((m_dictData.ContainsKey(INDEX_DATA.TG) == true)
                    && (m_dictData.ContainsKey(INDEX_DATA.GRII) == true)
                    && (m_dictData.ContainsKey(INDEX_DATA.GRVI) == true)
                    && (m_dictData.ContainsKey(INDEX_DATA.GRVII) == true))
                    for (iHour = 0; iHour < 24; iHour++)
                    {
                        arRes[iHour] = m_dictData[INDEX_DATA.TG].m_summaHours[iHour]
                            - (m_dictData[INDEX_DATA.GRVII].m_summaHours[iHour]
                                + m_dictData[INDEX_DATA.GRVI].m_summaHours[iHour]
                                - m_dictData[INDEX_DATA.GRII].m_summaHours[iHour]);

                        // только для НТЭЦ-5 MODE=[EPOTERI]
                        if (m_dictData.ContainsKey(INDEX_DATA.GRVIII) == true)
                            // для MODE.STANDARD д.б. всегда == 0
                            arRes[iHour] += m_dictData[INDEX_DATA.GRVIII].m_summaHours[iHour];
                        else
                            ;
                    }
                else
                    err = -1;

                return arRes;
            }

            /// <summary>
            /// Обязательный для переопределения метод сравнения
            /// </summary>
            /// <param name="obj">Объект для срвнения</param>
            /// <returns>Результат сравнения</returns>
            public override bool Equals(object obj)
            {
                return this == (VALUES_DATE)obj; ;
            }

            /// <summary>
            /// Обязательный для переопределения метод сравнения
            /// </summary>
            /// <returns>Результат сравнения</returns>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(VALUES_DATE obj1, VALUES_DATE obj2)
            {
                return (obj1.m_dataDate == obj2.m_dataDate)
                    //&& (obj1.m_dictData == obj2.m_dictData)
                    ;
            }

            public static bool operator !=(VALUES_DATE obj1, VALUES_DATE obj2)
            {
                return ((!(obj1.m_dataDate == obj2.m_dataDate))
                    //|| (!(obj1.m_dictData == obj2.m_dictData))
                    );
            }
        }
        /// <summary>
        /// Перечисление типов данных для результата
        /// </summary>
        public enum INDEX_DATA
        {
            TG, TSN, GRII, GRVI, GRVII, GRVIII
        };
        /// <summary>
        /// Таблица - список записей результирующего набора
        /// </summary>
        public class TableResult : List<RecordResult>
        {
            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="table">Таблица - результат запроса</param>
            public TableResult(DataTable table)
            {
                foreach (DataRow r in table.Rows)
                    Add(new RecordResult()
                    {
                        m_key = new SIGNAL.KEY(
                            (int)r[@"OBJECT"]
                            , (int)r[@"ITEM"])
                        ,
                        m_dtRec = (DateTime)r[@"DATETIME"]
                        ,
                        m_value = (double)r[@"VALUE0"] / 1000
                        ,
                        m_count = (int)r[@"COUNT"]
                    });
            }
        }
        /// <summary>
        /// Объект для представления одной записи результирующего набора
        /// </summary>
        public struct RecordResult
        {
            /// <summary>
            /// Ключ записи
            /// </summary>
            public SIGNAL.KEY m_key;
            /// <summary>
            /// Дата/время - метка времени значения
            /// </summary>
            public DateTime m_dtRec;
            /// <summary>
            /// Значение сигнала
            /// </summary>
            public double m_value;
            /// <summary>
            /// Количество строк, участвовавшие для получения значения при агрегации
            /// </summary>
            public int m_count;

            public bool m_bUse;
        }
        /// <summary>
        /// Массив таблиц с результатами запросов
        /// </summary>
        public TableResult[] m_arTableResult;
        /// <summary>
        /// Целочисленный идентификатор ТЭЦ
        /// </summary>
        public int m_Id;
        /// <summary>
        /// Строка - краткое наименование ТЭЦ
        /// </summary>
        public string m_strNameShr;
        /// <summary>
        /// Массив списков сигналов по-группно (ТГ, ТСН, GRII и т.д.)
        /// </summary>
        public List<SIGNAL>[] m_arListSgnls;
        /// <summary>
        /// Строка с идентификаторами сигналов ТГ, ТСН
        ///  для использования в запросе при выборке значений
        /// </summary>
        public string[] m_Sensors;
        /// <summary>
        /// Массив с номерами столбцов в шаблоне (книге MS Excel) для сохранения значений этой ТЭЦ
        /// </summary>
        public int[] m_arMSExcelNumColumns;

        /// <summary>
        /// Конструктор - дополнительный (без параметров)
        /// </summary>
        private TEC_LOCAL()
        {
            m_listValuesDate = new List<VALUES_DATE>();
            m_arTableResult = new TableResult[Enum.GetValues(typeof(INDEX_DATA)).Length];
            m_arListSgnls = new List<SIGNAL>[Enum.GetValues(typeof(INDEX_DATA)).Length];
            m_Sensors = new string[Enum.GetValues(typeof(INDEX_DATA)).Length];
        }

        /// <summary>
        /// Когструктор - основной (с аргументом)
        /// </summary>
        /// <param name="tec">Исходный(базовый) объект ТЭЦ</param>
        public TEC_LOCAL(TEC tec)
            : this()
        {
            this.m_Id = tec.m_id;
            this.m_strNameShr = tec.name_shr;

            List<int> list_column = new List<int>();

            this.m_arMSExcelNumColumns =
                tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.COLUMN_TSN_EXCEL).ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((string s) => {
                        return string.IsNullOrEmpty(s) == false ? Convert.ToInt32(s) : -1;
                    }
                ).ToArray();

            for (int j = 0; j < this.m_arListSgnls.Count(); j++)
                this.m_arListSgnls[j] = new List<SIGNAL>();
        }
        /// <summary>
        /// Инициализация строки с идентификаторами сигналов
        ///  для использования в запросе при выборке значений
        /// </summary>
        public void InitSensors()
        {
            foreach (INDEX_DATA indx in Enum.GetValues(typeof(INDEX_DATA)))
                m_Sensors[(int)indx] = getSensors(m_arListSgnls[(int)indx]);
        }

        /// <summary>
        /// Возвратить строку с идентификаторами сигналов для указанного списка
        /// </summary>
        /// <param name="listSgnls">Список сигналов для которых требуется возвратить строку</param>
        /// <returns>Строка с идентификаторами сигналов</returns>
        private string getSensors(List<SIGNAL> listSgnls)
        {
            string strRes = string.Empty;

            List<int> listIdUSPD = new List<int>();
            List<string> sensorsUSPD = new List<string>();
            string strOR = @" OR ";

            if ((!(listSgnls == null))
                && (listSgnls.Count > 0))
            {
                foreach (SIGNAL s in listSgnls)
                {
                    if (listIdUSPD.IndexOf(s.m_key.m_object) < 0)
                    {
                        listIdUSPD.Add(s.m_key.m_object);

                        sensorsUSPD.Add(@"([OBJECT] = " + s.m_key.m_object + @" AND [ITEM] IN (" + s.m_key.m_item);
                    }
                    else
                        sensorsUSPD[listIdUSPD.IndexOf(s.m_key.m_object)] += (@"," + s.m_key.m_item);
                }

                foreach (string s in sensorsUSPD)
                {
                    //Добавить завершающие скобки (1-я для IN, 2-я для [OBJECT])
                    strRes += s + @"))";
                    strRes += strOR;
                }

                if (strRes.Equals(string.Empty) == false)
                    strRes = strRes.Substring(0, strRes.Length - strOR.Length);
                else
                    Logging.Logg().Error($"TEC::getSensors () - не распознан ни один сигнал для ТЭЦ ID={m_Id}..."
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                Logging.Logg().Error($"TEC::getSensors () - не определен ни один сигнал для ТЭЦ ID={m_Id}..."
                    , Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
        }

        /// <summary>
        /// Привести полученные значения к часовому формату (из полу-часового)
        /// </summary>
        public void parseTableResult(DateTime dtStart, DateTime dtEnd, INDEX_DATA indx, out int err)
        {
            err = 0;

            TableResult table;
            List<RecordResult> rowsDate;
            VALUES_DATE valuesDate;

            table = m_arTableResult[(int)indx];

            for (DateTime dtRec = dtStart; dtRec < dtEnd; dtRec += TimeSpan.FromDays(1))
            {
                // = (DateTime)r[@"DATA_DATE"];
                valuesDate = m_listValuesDate.Find(item => { return item.m_dataDate == dtRec; });
                rowsDate = table.FindAll(item => { return (item.m_dtRec > dtRec) && (!(item.m_dtRec > dtRec.AddDays(1))); });

                if (valuesDate.m_dataDate.Equals(DateTime.MinValue) == true)
                    m_listValuesDate.Add(new VALUES_DATE(dtRec, indx, m_arListSgnls[(int)indx], rowsDate));
                else
                {
                    //valuesDate.InitSignals(indx, m_arListSgnls[(int)indx]);

                    valuesDate.SetValues(indx, m_arListSgnls[(int)indx], rowsDate);
                }
            }
        }

        /// <summary>
        /// Очистить значения
        /// </summary>
        /// <param name="dtReq">Дата за которую требуется очистить значения</param>
        /// <param name="indx">Индекс группы сигналов</param>
        public void ClearValues(DateTime dtReq, INDEX_DATA indx)
        {
            VALUES_DATE valuesDate;

            valuesDate = m_listValuesDate.Find(item => { return item.m_dataDate == dtReq; });
            // проверить успешность поиска объекта
            if ((valuesDate.m_dataDate > DateTime.MinValue)
                && (!(valuesDate.m_dictData == null))
                && (valuesDate.m_dictData.ContainsKey(indx) == true))
                // объект найден и содержит необходимый ключ - индекс группы сигналов
                valuesDate.m_dictData[indx].ClearValues();
            else
                ; // объект не найден
        }
    }
}
