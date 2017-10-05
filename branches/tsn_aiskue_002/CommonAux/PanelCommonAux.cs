﻿using System;
using System.Data.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;
using System.Reflection;

namespace CommonAux
{
    /// <summary>
    /// Класс для описания параметров сигнала
    /// </summary>
    public class SIGNAL
    {
        /// <summary>
        /// Ключ сигнала (идентификатор устройства + № канала опроса)
        /// </summary>
        public struct KEY
        {
            public int m_object;

            public int m_item;

            public KEY(int obj, int item)
            {
                m_object = obj;

                m_item = item;
            }

            public KEY(KEY key)
            {
                m_object = key.m_object;

                m_item = key.m_item;
            }

            public static bool operator ==(KEY key1, KEY key2)
            {
                return (key1.m_object == key2.m_object)
                    && (key1.m_item == key2.m_item);
            }

            public static bool operator !=(KEY key1, KEY key2)
            {
                return (!(key1.m_object == key2.m_object))
                    || (!(key1.m_item == key2.m_item));
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(@"[object={0}, item={1}]", m_object, m_item);
            }
        }
        /// <summary>
        /// Описание сигнала
        /// </summary>
        public string m_strDesc;

        public SIGNAL.KEY m_key;
        /// <summary>
        /// Признак использования сигнала при расчете
        /// </summary>
        public bool m_bUse;
        /// <summary>
        /// Конструктор - дополнительный (с параметрами)
        /// </summary>
        /// <param name="desc">Описание сигнала (наименование)</param>
        /// <param name="idUSPD">Идентификатор УСПД</param>
        /// <param name="id">Номер канала опроса</param>
        public SIGNAL(string desc, int idUSPD, int id, bool bUse)
        {
            m_strDesc = desc;
            m_key = new SIGNAL.KEY(idUSPD, id);
            m_bUse = bUse;
        }

        public SIGNAL(string desc, SIGNAL.KEY key, bool bUse)
        {
            m_strDesc = desc;
            m_key = new SIGNAL.KEY(key);
            m_bUse = bUse;
        }
    }

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

            //public class VALUES_GROUP_TEC5 : VALUES_GROUP
            //{
            //    public VALUES_GROUP_TEC5(List<SIGNAL>listSgnls) : base (listSgnls)
            //    {
            //    }
            //}
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
            /// <param name="dictValues">Словарь с исходными данными для расчета. KEYS=TG, GRII, GRVI, GRVII</param>
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
                    // с VIII-ой группой произойдет вызов только приналичии сигналов
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
        /// Конструктор - основной (без параметров)
        /// </summary>
        public TEC_LOCAL()
        {
            m_listValuesDate = new List<VALUES_DATE>();
            m_arTableResult = new TableResult[Enum.GetValues(typeof(INDEX_DATA)).Length];
            m_arListSgnls = new List<SIGNAL>[Enum.GetValues(typeof(INDEX_DATA)).Length];
            m_Sensors = new string[Enum.GetValues(typeof(INDEX_DATA)).Length];
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
        private class TableResult : List<RecordResult>
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
        private TableResult[] m_arTableResult;
        /// <summary>
        /// Строка шаблон для формирования запроса на выбрку значений ТГ, ТСН
        /// </summary>
        public static string /*[]*/ s_strQueryTemplate = string.Empty; // new string [Enum.GetValues(typeof(INDEX_DATA)).Length];
        /// <summary>
        /// Целочисленный индекс ТЭЦ (из файла конфигурации)
        /// </summary>
        public int m_Index;
        /// <summary>
        /// Строковый идентификатор ТЭЦ (из файла конфигурации)
        /// </summary>
        public string m_strId;
        /// <summary>
        /// Целочисленный идентификатор ТЭЦ (из файла конфигурации)
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
        private static string getSensors(List<SIGNAL> listSgnls)
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
                    Logging.Logg().Error(string.Format(@"TEC::getSensors () - не распознан ни один сигнал для ТЭЦ...")
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                Logging.Logg().Error(string.Format(@"TEC::getSensors () - не определен ни один сигнал для ТЭЦ...")
                    , Logging.INDEX_MESSAGE.NOT_SET);

            return strRes;
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

        private HMark m_markIndxRequestError;
        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="iListenerId">Идентификатор установленного соединения с источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(int iListenerId, DateTime dtStart, DateTime dtEnd)
        {
            int iRes = 0;

            m_listValuesDate.Clear();
            if (m_markIndxRequestError == null)
                m_markIndxRequestError = new HMark(0);
            else
                m_markIndxRequestError.SetOf(0);

            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out iRes);

            if (iRes == 0)
                foreach (INDEX_DATA indx in Enum.GetValues(typeof(INDEX_DATA)))
                {
                    // запросить и обработать результат запроса по получению значений для группы сигналов в указанный диапазон дат
                    iRes = Request(ref dbConn, dtStart, dtEnd, indx);
                    m_markIndxRequestError.Set((int)indx, iRes < 0);
                }
            else
            {
                Logging.Logg().ExceptionDB("FormMain.Tec.Request () - не установлено соединение с DB...");
                iRes = -1;
            }

            iRes = m_markIndxRequestError.Value == 0 ? 0 : -1;
            return iRes;
        }
        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="iListenerId">Идентификатор установленного соединения с источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <param name="indx">Индекс группы сигналов</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(int iListenerId, DateTime dtStart, DateTime dtEnd, INDEX_DATA indx)
        {
            int iRes = 0;
            string query = string.Empty;

            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out iRes);

            iRes = Request(ref dbConn, dtStart, dtEnd, indx);

            return iRes;
        }
        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="dbConn">Ссылка на объект соединения с БД-источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <param name="indx">Индекс группы сигналов</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(ref DbConnection dbConn, DateTime dtStart, DateTime dtEnd, INDEX_DATA indx)
        {
            int iRes = 0
                , err = -1;
            string query = string.Empty;
            DateTime dtQuery;
            TimeSpan tsQuery;

            dtQuery = DateTime.Now;

            m_arTableResult[(int)indx] = null;
            query = getQuery(indx, dtStart, dtEnd);

            if (query.Equals(string.Empty) == false)
            {
                m_arTableResult[(int)indx] = new TableResult(DbTSQLInterface.Select(ref dbConn, query, null, null, out err));

                tsQuery = DateTime.Now - dtQuery;

                Logging.Logg().Action(string.Format(@"TEC.ID={0}, ИНДЕКС={1}, время={4}{2}запрос={3} сек"
                        , m_Id, indx.ToString(), Environment.NewLine, query, tsQuery.TotalSeconds)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                if (err == 0)
                    parseTableResult(dtStart, dtEnd, indx, out err);
                else
                {
                    Logging.Logg().Error(string.Format("TEC.ID={0}, ИНДЕКС={1} не получены данные за {2}{3}Запрос={4}"
                            , m_Id, indx.ToString(), dtEnd, Environment.NewLine, query)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = -1;
                }
            }
            else
            {
                Logging.Logg().Error(string.Format("TEC.ID={0}, группа ИНДЕКС={1} пропущена, не сформирован запрос за {2}"
                        , m_Id, indx.ToString(), dtStart, Environment.NewLine, query)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                iRes = 1; // Предупреждение
            }

            return iRes;
        }
        /// <summary>
        /// Привести полученные значения к часовому формату (из полу-часового)
        /// </summary>
        private void parseTableResult(DateTime dtStart, DateTime dtEnd, INDEX_DATA indx, out int err)
        {
            err = 0;

            //INDEX_DATA indx = INDEX_DATA.TG;
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
        /// Возвратитиь запрос для выборки данных ТГ
        /// </summary>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <returns>Строка запроса</returns>
        private string getQuery(INDEX_DATA indx, DateTime dtStart, DateTime dtEnd)
        {
            string strRes = "SELECT res.[OBJECT], res.[ITEM], SUM(res.[VALUE0]) / COUNT(*)[VALUE0], res.[DATETIME], COUNT(*) as [COUNT] FROM(SELECT[OBJECT], [ITEM], [VALUE0], DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0), [DATA_DATE]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0)) as [DATETIME] FROM[DATA] WHERE[PARNUMBER] = 12 AND([DATA_DATE] > '?DATADATESTART?' AND NOT[DATA_DATE] > '?DATADATEEND?') AND(?SENSORS?) GROUP BY[RCVSTAMP], [OBJECT], [ITEM], [VALUE0], DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0), [DATA_DATE]) / 60.) * 60, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0))) res GROUP BY[OBJECT], [ITEM], [DATETIME] ORDER BY[OBJECT], [ITEM], [DATETIME]";
            //s_strQueryTemplate[(int)indx]
            //s_strQueryTemplate
            ;

            if (m_Sensors[(int)indx].Equals(string.Empty) == false)
            {
                strRes = strRes.Replace(@"?SENSORS?", m_Sensors[(int)indx]);
                strRes = strRes.Replace(@"?DATADATESTART?", dtStart.ToString(@"yyyyMMdd"));
                strRes = strRes.Replace(@"?DATADATEEND?", dtEnd.ToString(@"yyyyMMdd"));
            }
            else
                strRes = string.Empty;

            return strRes;
        }
    }

    public class MSExcelIO : HClassLibrary.MSExcelIO
    {
        public enum INDEX_MSEXCEL_COLUMN { APOWER, SNUZHDY }

        public MSExcelIO(string path) : base()
        {
            OpenDocument(path);
        }
        /// <summary>
        /// Записать результат в книгу MS Excel
        /// </summary>
        /// <param name="table">Таблица с данными для записи</param>
        /// <param name="col">Столбец на листе книги</param>
        public void WriteValues(string nameSheet, int col, int sheeftRow, double[] arValues)
        {
            int row = sheeftRow;

            SelectWorksheet(nameSheet);

            foreach (double val in arValues)
            {
                base.WriteValue(nameSheet, col, row, val.ToString(@"F3"));
                row++;
            }
        }

        public void WriteValues(int col, int sheeftRow, double[] arValues)
        {
            int row = sheeftRow;

            foreach (double val in arValues)
            {
                base.WriteValue(col, row, val.ToString(@"F3"));
                row++;
            }
        }

        public void WriteDate(int col, int sheeftRow, DateTime date)
        {
            base.WriteValue(col, sheeftRow, date.ToShortDateString());
        }
    }

    class DataGridViewValues : DataGridView
    {
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DataGridViewValues() : base()
        {
            addColumns();

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        /// <summary>
        /// Добавить столбцы по количеству часов + столбец для итогового для сигнала значения
        /// </summary>
        private void addColumns()
        {
            int iHour = -1;

            for (iHour = 0; iHour < 24; iHour++)
            {
                Columns.Add((iHour + 1).ToString(@"00"), (iHour + 1).ToString(@"00"));
                // отобразить номер часа в заголовке столбца
                //Columns[ColumnCount - 1].HeaderCell.Value = string.Format(@"{0:HH:mm}", iHour < 23 ? TimeSpan.FromHours(iHour + 1) : TimeSpan.Zero);
            }
            // добавить столбец для итогового для сигнала значения
            Columns.Add(@"DATE", @"Сутки");
        }
        /// <summary>
        /// Идентификатор итоговой строки
        /// </summary>
        private string groupRowTag = @"Группа";
        /// <summary>
        /// Добавить строки для сигналов
        /// </summary>
        /// <param name="listSgnls">Срисок сигналов</param>
        public void AddRowData(List<SIGNAL> listSgnls)
        {
            foreach (SIGNAL sgnl in listSgnls)
            {
                Rows.Add();
                // отобразить наименование сигнала
                Rows[RowCount - 1].HeaderCell.Value = sgnl.m_strDesc;
                //Rows[RowCount - 1].HeaderCell.Size.Width = 200;
                // назначить идентификатор строки
                Rows[RowCount - 1].Tag = new SIGNAL.KEY(sgnl.m_key);

                //Rows[RowCount-1].

                Rows[RowCount - 1].DefaultCellStyle.BackColor = sgnl.m_bUse == true ?
                    System.Drawing.Color.Empty :
                        sgnl.m_bUse == false ? System.Drawing.Color.LightGray :
                            System.Drawing.Color.Gray;
            }
            // добавить строку итого
            Rows.Add();
            Rows[RowCount - 1].HeaderCell.Value = groupRowTag;
            // назначить идентификатор итоговой строки
            Rows[RowCount - 1].Tag = groupRowTag;
        }
        /// <summary>
        /// Удалить все строки(сигналы)
        /// </summary>
        public void ClearRows()
        {
            Rows.Clear();
        }
        /// <summary>
        /// Очистить значения во всех ячейках
        /// </summary>
        public void ClearValues()
        {
            foreach (DataGridViewRow r in Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Value = string.Empty;
        }
        /// <summary>
        /// Отобразить значения
        /// </summary>
        /// <param name="values">Значения для отображения</param>
        public void Update(TEC_LOCAL.VALUES_DATE.VALUES_GROUP values)
        {
            int iHour = -1 // номер столбца (часа)
                , iRow = -1; // номер строки для сигнала
            double value = -1F;
            SIGNAL.KEY key; // ключ сигнала - идетификатор строки

            foreach (DataGridViewRow r in Rows)
            {
                key = values.Keys.ToList().Find(item => { if (r.Tag is SIGNAL.KEY) return item == (SIGNAL.KEY)r.Tag; else return false; });
                // проверить найден ли ключ
                if ((key.m_object > 0)
                    && (key.m_item > 0))
                {
                    iRow = Rows.IndexOf(r);
                    // заполнить значения для сигнала по часам в сутках
                    for (iHour = 0; iHour < 24; iHour++)
                    {
                        value = values[key].m_data[iHour];
                        // отобразить значение
                        r.Cells[iHour].Value = value.ToString(@"F3");
                    }
                    // отобразить значение для сигнала за сутки
                    r.Cells[iHour].Value = values.m_summaSgnls[values.Keys.ToList().IndexOf(key)].ToString(@"F3");
                }
                else
                // ключ для строки не найден
                    if ((r.Tag is string)
                        && (((string)r.Tag).Equals(groupRowTag) == true))
                {
                    // итоговая строка
                    for (iHour = 0; iHour < 24; iHour++)
                        r.Cells[iHour].Value = values.m_summaHours[iHour].ToString(@"F3");

                    r.Cells[iHour].Value = values.m_Summa;
                }
                else
                    Logging.Logg().Error(
                        string.Format(@"View.DataGridViewValues::Update () - не найден столбец для KEY_SIGNAL=[object={0}, item={1}]"
                            , ((SIGNAL.KEY)r.Tag).m_object, ((SIGNAL.KEY)r.Tag).m_item)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }
    partial class PanelCommonAux : PanelStatistic
    {
        /// <summary>
        /// Класс для взаимодействия с базой данных
        /// </summary>
        public class GetDataFromDB
        {
            /// <summary>
            /// Строка, содержащая наименование таблицы в БД, хранящей перечень каналов
            /// </summary>
            public static string DB_TABLE = @"[ID_TSN_ASKUE_2017]";
            /// <summary>
            /// Объект, содержащий id записи в таблице, содержащей настройки подключения
            /// </summary>
            public static int ID_AIISKUE_CONSETT = 7001;
            /// <summary>
            /// Объект, содержащий путь к шаблону excel
            /// </summary>
            private string m_strFullPathTemplate;
            /// <summary>
            /// Объект содержащий идентификатор соединения с БД
            /// </summary>
            private static int _iListenerId;

            public enum INDEX_MSEXCEL_COLUMN { APOWER, SNUZHDY }
            /// <summary>
            /// Поля таблицы настроек
            /// </summary>
            public enum DB_TABLE_AIISKUE
            {
                ID, IP, PORT, DB_NAME, UID, NAME_SHR
            };
            /// <summary>
            /// Поля таблицы паролей
            /// </summary>
            public enum DB_TABLE_PSW
            {
                ID_EXT, ID_ROLE, HASH
            };
            public static string SEC_CONFIG = @"CONFIG"
                , SEC_SELECT = @"SELECT"
                , SEC_TEMPLATE = @"Template";
            /// <summary>
            /// Каталог для размещения шаблонов
            /// </summary>
            public string FullPathTemplate
            {
                get { return m_strFullPathTemplate; }

                set
                {
                    if ((m_strFullPathTemplate == null)
                        || ((!(m_strFullPathTemplate == null))
                            && (m_strFullPathTemplate.Equals(value) == false)))
                    {
                        m_strFullPathTemplate = value;

                        if ((value.Equals(string.Empty) == false)
                        && (Path.GetDirectoryName(value).Equals(string.Empty) == false)
                        && (Path.GetFileName(value).Equals(string.Empty) == false)) ;
                        //SetSecValueOfKey(SEC_TEMPLATE, Environment.UserDomainName + @"\" + Environment.UserName, value);
                        else
                            ;
                    }
                    else
                        ;
                }
            }

            /// <summary>
            /// Возвратить строку запроса для получения списка каналов
            /// </summary>
            /// <returns>Строка запроса</returns>
            public static string getQueryListTEC()
            {

                string strRes = "SELECT * FROM " + DB_TABLE;
                return strRes;
            }

            /// <summary>
            /// Возвратить таблицу [ID_TSN_AISKUE_2017] из БД конфигурации
            /// </summary>
            /// <param name="connConfigDB">Ссылка на объект с установленным соединением с БД</param>
            /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
            /// <returns>Таблица - с данными</returns>
            public static DataTable getListChannels(ref DbConnection connConfigDB, out int err)
            {
                string req = getQueryListTEC();
                return DbTSQLInterface.Select(ref connConfigDB, req, null, null, out err);
            }

            /// <summary>
            /// Возвратить объект с параметрами соединения
            /// </summary>
            /// <param name="iListenerId">Ссылка на объект с установленным соединением с БД</param>
            /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
            /// <returns>Объект с параметрами соединения</returns>
            public ConnectionSettings GetConnSettAIISKUECentre(ref int iListenerId, out int err)
            {
                DataTable dataTableRes = new DataTable();
                dataTableRes = InitTEC_200.getConnSettingsOfIdSource(iListenerId, ID_AIISKUE_CONSETT, -1, out err);

                ConnectionSettings connSettRes = new ConnectionSettings(dataTableRes.Rows[dataTableRes.Rows.Count - 1], 1);
                
                return connSettRes;
            }

            /// <summary>
            /// Загрузка всех каналов из базы данных
            /// </summary>
            public void InitChannels(DbConnection m_connConfigDB, List<TEC_LOCAL> m_listTEC)
            {
                int err = -1;

                List<SIGNAL> listRes = new List<SIGNAL>();
                SIGNAL signal;

                DataTable list_channels = null;

                //Получить список каналов, используя статическую функцию
                list_channels = getListChannels(ref m_connConfigDB, out err);

                for (int i = 0; i < list_channels.Rows.Count; i++)
                {
                    try
                    {
                        signal = new SIGNAL(Convert.ToString(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.DESCRIPTION)]),
                            Convert.ToInt32(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.USPD)]),
                            Convert.ToInt32(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.CHANNEL)]),
                            Convert.ToBoolean(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.USE)])
                        );
                        m_listTEC[Convert.ToInt32(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.ID_TEC)]) - 1].m_arListSgnls[Convert.ToInt32(Enum.Parse(typeof(TEC_LOCAL.INDEX_DATA), Convert.ToString(list_channels.Rows[i].ItemArray[Convert.ToInt32(DB_TABLE_DATA.GROUP)])))].Add(signal);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, string.Format(@"Ошибка получения списка каналов"), Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }
        }

        /// <summary>
        /// Конструктор панели
        /// </summary>
        /// <param name="displayMode">Параметр, определяющий режим отображения</param>
        public PanelCommonAux(int displayMode)
        {
            m_displayMode = displayMode;

            int err = -1;
            // зарегистрировать соединение/получить идентификатор соединения
            int _iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            DbConnection m_connConfigDB = DbSources.Sources().GetConnection(_iListenerId, out err);

            m_listTEC = GetListTEC(new InitTEC_200(_iListenerId, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec);

            GetDataFromDB GD = new GetDataFromDB();
            GD.InitChannels(m_connConfigDB, m_listTEC);

            foreach (TEC_LOCAL t in m_listTEC)
            {
                t.InitSensors();
            }

            //Получить параметры соединения с источником данных
            m_connSettAIISKUECentre = GD.GetConnSettAIISKUECentre(ref _iListenerId, out err);

            InitializeComponents();

            foreach (TEC_LOCAL tec in m_listTEC)
            {
                m_listBoxTEC.Items.Add(tec.m_strNameShr);
            }

            m_listBoxTEC.SelectedIndex = 0;

            m_listBoxTEC.Tag = INDEX_CONTROL.LB_TEC;
            m_listBoxTEC.SelectedIndexChanged += listBox_SelectedIndexChanged;

            m_labelEndDate.Text = m_monthCalendarEnd.SelectionStart.ToShortDateString();
            m_labelStartDate.Text = m_monthCalendarStart.SelectionStart.ToShortDateString();

            if (m_displayMode == 0)
            {
                m_btnExit.Visible = false;
            }

            //Установить начальные признаки готовности к экспорту
            m_markReady = new HMark(0);

            FullPathTemplate = string.Empty;

            m_arMSEXEL_PARS = new string[] { "", "Tepmlate.xls", "Sheet1", "1", "5", "25", "1.1" };

            //Установить обработчики событий
            EventNewPathToTemplate += new DelegateStringFunc(onNewPathToTemplate);

            string[] sumGroups = new string[] { "sum TG", "sum TSN", "sum GRII", "sum GRVI", "sum GRVII", "sum GRVIII" };

            for (int i = 0; i < sumGroups.Count(); i++)
            {
                m_sumValues.Rows.Add();
                m_sumValues.Rows[i].Cells[0].Value = sumGroups[i];
                m_sumValues.Rows[i].Cells[1].Value = "0";
            }
        }
        /// <summary>
        /// Обработчик события установки нового значения для пути к шаблону
        /// </summary>
        /// <param name="path">Строка - полный путь к шаблону</param>
        private void onNewPathToTemplate(string path)
        {
            //Проверить строку на наличие в ней значения - изменить состояние программы
            m_markReady.Set((int)INDEX_READY.TEMPLATE, path.Equals(string.Empty) == false);
            //Установить признак дотупности для элементов интерфейса экспорта в книгу MS Excel
            // п. главного меню + кнопка на панели быстрого доступа
            enableBtnExcel(State == STATE.READY);
        }
        /// <summary>
        /// Включить/отключить доступность интерфейса экспорта в книгу MS Excel
        /// </summary>
        /// <param name="bEnabled">Признак включения/отключения</param>
        private void enableBtnExcel(bool bEnabled)
        {
            m_btnStripButtonExcel.Enabled =
                bEnabled;
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            //m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        public static string _ExecutingAssemlyDirectoryName { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        /// <summary>
        /// Режим отображения (в составе статистики/самостоятельная вкладка)
        /// </summary>
        protected int m_displayMode;
        /// <summary>
        /// Объект с параметрами соединения с источником данных
        /// </summary>
        protected ConnectionSettings m_connSettAIISKUECentre;
        /// <summary>
        /// Список ТЭЦ с параметрами из файла конфигурации
        /// </summary>
        protected List<TEC_LOCAL> m_listTEC;
        /// <summary>
        /// Объект для инициализации входных параметров
        /// </summary>
        protected GetDataFromDB m_GetDataFromDB;
        /// <summary>
        /// Возвраить список объектов ТЭЦ (без ЛК38)
        /// </summary>
        /// <returns>Список объектов ТЭЦ</returns>
        public List<TEC_LOCAL> GetListTEC(List<TEC> tec)
        {
            List<TEC_LOCAL> listRes = new List<TEC_LOCAL>();
            TEC_LOCAL tec_local;

            for (int i = 0; i < tec.Count - 1; i++)
            {
                tec_local = new TEC_LOCAL();

                tec_local.m_Id = tec[i].m_id;
                tec_local.m_strNameShr = tec[i].name_shr;

                List<int> list_column = new List<int>();

                foreach (string s in tec[i].GetAddingParameter(TEC.ADDING_PARAM_KEY.COLUMN_TSN_EXCEL).ToString().Split(','))
                {
                    list_column.Add(Convert.ToInt32(s));
                }

                tec_local.m_arMSExcelNumColumns = list_column.ToArray();

                for (int j = 0; j < tec_local.m_arListSgnls.Count(); j++)
                {
                    tec_local.m_arListSgnls[j] = new List<SIGNAL>();
                }

                listRes.Add(tec_local);
            }
            return listRes;
        }

        private enum INDEX_CONTROL : short { LB_TEC, LB_GROUP_SIGNAL }

        /// <summary>
        /// Поля таблицы сигналов
        /// </summary>
        public enum DB_TABLE_DATA
        {
            ID, ID_TEC, GROUP, NAME, DESCRIPTION, USPD, CHANNEL, USE
        };

        private const string MS_EXCEL_FILTER = @"Книга MS Excel 2010 (*.xls, *.xlsx)|*.xls;*.xlsx";
        /// <summary>
        /// Перечисление причин, влияющих на готовность к экспорту значений
        /// </summary>
        private enum INDEX_READY { TEMPLATE, DATE }
        /// <summary>
        /// Объект содержащий признаки готовности к экспорту значений
        /// </summary>
        HMark m_markReady;

        private enum INDEX_MSEXCEL_PARS
        {
            /// <summary>
            /// Полный путь к (исходному) шаблону
            /// </summary>
            TEMPLATE_PATH_DEFAULT
            /// <summary>
            /// Наименование (исходного) шаблона
            /// </summary>
            , TEMPLATE_NAME
            /// <summary>
            /// Наименование листа в книге-шаблоне
            /// </summary>
            , SHEET_NAME
            /// <summary>
            /// Номер столбца для отображения даты
            /// </summary>
            , COL_DATADATE
            /// <summary>
            /// Смещение относительно 1-ой строки в книге MS Excel
            ///  для записи значений за 1-ый день месяца
            /// </summary>
            , ROW_START
            /// <summary>
            /// Количество строк в книге MS Excel на сутки
            /// </summary>
            , ROWCOUNT_DATE
            /// <summary>
            /// Версия шаблона книги MS Excel
            /// </summary>
            , TEMPLATE_VER
            /// <summary>
            /// Количество параметров для шаблона книги MS Excel
            /// </summary>
            , COUNT
        }
        /// <summary>
        /// Массив с параметрами шаблона книги MS Excel
        /// </summary>
        private string[] m_arMSEXEL_PARS;
        /// <summary>
        /// Перечисление возможных состояний приложения
        /// </summary>
        private enum STATE { UNKNOWN = -1, READY, ERROR }

        private bool m_bVisibleLog;
        /// <summary>
        /// Признак видимости элемента управления с содержанием лог-сообщений
        /// </summary>
        private bool VisibleLog { get { return m_bVisibleLog; } set { m_bVisibleLog = value; } }

        //private STATE m_State;
        /// <summary>
        /// Состояние приложения
        /// </summary>
        STATE State { get { return (m_markReady.IsMarked((int)INDEX_READY.TEMPLATE) && m_markReady.IsMarked((int)INDEX_READY.DATE)) == true ? STATE.READY : STATE.ERROR; } }

        private string m_strFullPathTemplate;
        /// <summary>
        /// Строка - полный путь для шаблона MS Excel
        /// </summary>
        /// 

        private string FullPathTemplate
        {
            get { return m_strFullPathTemplate; }

            set
            {
                if ((m_strFullPathTemplate == null)
                    || ((!(m_strFullPathTemplate == null))
                        && (m_strFullPathTemplate.Equals(value) == false)))
                {
                    m_strFullPathTemplate = value;

                    if ((value.Equals(string.Empty) == false)
                        && (Path.GetDirectoryName(value).Equals(string.Empty) == false)
                        && (Path.GetFileName(value).Equals(string.Empty) == false))
                        EventNewPathToTemplate(m_strFullPathTemplate);
                    else
                        ;
                }
                else
                    ;
            }
        }

        private int setFullPathTemplate(string strFullPathTemplate)
        {
            int iRes = 0; // исходное состояние - нет ошибки

            iRes = validateTemplate(strFullPathTemplate);
            if (iRes == 0)
            {
                // сохранить каталог с крайним прошедшим
                FullPathTemplate =
                    strFullPathTemplate;
            }
            else
                ;
            return iRes;
        }

        /// <summary>
        /// Событие при назначении нового пути для шаблона MS Excel
        /// </summary>
        private event DelegateStringFunc EventNewPathToTemplate;

        private System.Windows.Forms.Button m_btnLoad;
        private System.Windows.Forms.Button m_btnOpen;
        private System.Windows.Forms.Button m_btnExit;
        private System.Windows.Forms.Button m_btnStripButtonExcel;
        private System.Windows.Forms.ListBox m_listBoxTEC;
        private System.Windows.Forms.ListBox m_listBoxGrpSgnl;
        private System.Windows.Forms.MonthCalendar m_monthCalendarStart;
        private System.Windows.Forms.MonthCalendar m_monthCalendarEnd;
        private System.Windows.Forms.Label m_labelTEC;
        private System.Windows.Forms.Label m_labelGrpSgnl;
        private System.Windows.Forms.Label m_labelValues;
        private System.Windows.Forms.Label m_labelStartDate;
        private System.Windows.Forms.Label m_labelEndDate;

        private List<System.Windows.Forms.Label> m_labelsGroup;

        private List<DataGridViewValues> m_dgvValues;
        private DataGridView m_sumValues;

        /// <summary>
        /// Требуется переменная конструктора
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Определить размеры ячеек макета панели
        /// </summary>
        /// <param name="cols">Количество столбцов в макете</param>
        /// <param name="rows">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(100, 100);
        }

        protected virtual void InitializeComponents()
        {
            #region Инициализация переменных
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_btnLoad = new System.Windows.Forms.Button();
            this.m_btnOpen = new System.Windows.Forms.Button();
            this.m_btnExit = new System.Windows.Forms.Button();
            this.m_btnStripButtonExcel = new System.Windows.Forms.Button();

            this.m_listBoxTEC = new System.Windows.Forms.ListBox();
            this.m_listBoxGrpSgnl = new System.Windows.Forms.ListBox();
            this.m_monthCalendarStart = new System.Windows.Forms.MonthCalendar();
            this.m_monthCalendarEnd = new System.Windows.Forms.MonthCalendar();
            this.m_labelTEC = new System.Windows.Forms.Label();
            this.m_labelGrpSgnl = new System.Windows.Forms.Label();
            this.m_labelValues = new System.Windows.Forms.Label();
            this.m_labelStartDate = new System.Windows.Forms.Label();
            this.m_labelEndDate = new System.Windows.Forms.Label();

            m_sumValues = new DataGridView();

            this.m_dgvValues = new List<DataGridViewValues>();
            this.m_labelsGroup = new List<System.Windows.Forms.Label>();
            for ( int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues.Add(new DataGridViewValues());
                ((System.ComponentModel.ISupportInitialize)(this.m_dgvValues[i])).BeginInit();

                m_labelsGroup.Add(new System.Windows.Forms.Label());
            }

            ((System.ComponentModel.ISupportInitialize)(this.m_sumValues)).BeginInit();

            #endregion

            components = new System.ComponentModel.Container();

            this.SuspendLayout();

            this.Controls.Add(m_btnLoad, 81, 40); this.SetColumnSpan(m_btnLoad, 18); this.SetRowSpan(m_btnLoad, 5);
            this.Controls.Add(m_btnOpen, 81, 45); this.SetColumnSpan(m_btnOpen, 18); this.SetRowSpan(m_btnOpen, 5);
            this.Controls.Add(m_btnExit, 81, 94); this.SetColumnSpan(m_btnExit, 18); this.SetRowSpan(m_btnExit, 5);
            this.Controls.Add(m_btnStripButtonExcel, 81, 50); this.SetColumnSpan(m_btnStripButtonExcel, 18); this.SetRowSpan(m_btnStripButtonExcel, 5);
            this.Controls.Add(m_listBoxTEC, 61, 40); this.SetColumnSpan(m_listBoxTEC, 18); this.SetRowSpan(m_listBoxTEC, 20);
            this.Controls.Add(m_monthCalendarStart, 60, 8); this.SetColumnSpan(m_monthCalendarStart, 15); this.SetRowSpan(m_monthCalendarStart, 15);
            this.Controls.Add(m_monthCalendarEnd, 80, 8); this.SetColumnSpan(m_monthCalendarEnd, 15); this.SetRowSpan(m_monthCalendarEnd, 15);
            this.Controls.Add(m_labelTEC, 62, 37); this.SetColumnSpan(m_labelTEC, 11); this.SetRowSpan(m_labelTEC, 2);
            this.Controls.Add(m_labelValues, 8, 2); this.SetColumnSpan(m_labelValues, 30); this.SetRowSpan(m_labelValues, 2);
            this.Controls.Add(m_labelStartDate, 65, 6); this.SetColumnSpan(m_labelStartDate, 8); this.SetRowSpan(m_labelStartDate, 2);
            this.Controls.Add(m_labelEndDate, 85, 6); this.SetColumnSpan(m_labelEndDate, 8); this.SetRowSpan(m_labelEndDate, 2);

            for (int i = 0; i < m_dgvValues.Count; i++)
            {
                this.Controls.Add(m_dgvValues[i], 8, 5 + i*15); this.SetColumnSpan(m_dgvValues[i], 50); this.SetRowSpan(m_dgvValues[i], 15);
                this.Controls.Add(m_labelsGroup[i], 2, 7 + i*15); this.SetColumnSpan(m_labelsGroup[i], 5); this.SetRowSpan(m_labelsGroup[i], 2);
            }
            this.Controls.Add(m_sumValues, 61, 60); this.SetColumnSpan(m_sumValues, 38); this.SetRowSpan(m_sumValues, 35);

            this.ResumeLayout();

            initializeLayoutStyle();

            #region Параметры элементов управления
            // 
            // m_btnLoad
            // 
            this.m_btnLoad.Name = "m_btnLoad";
            this.m_btnLoad.TabIndex = 0;
            this.m_btnLoad.Text = "Load";
            this.m_btnLoad.UseVisualStyleBackColor = true;
            this.m_btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            this.m_btnLoad.Width = m_monthCalendarStart.Width;
            // 
            // m_btnOpen
            // 
            this.m_btnOpen.Name = "m_btnSave";
            this.m_btnOpen.TabIndex = 1;
            this.m_btnOpen.Text = "Open";
            this.m_btnOpen.UseVisualStyleBackColor = true;
            this.m_btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            this.m_btnOpen.Width = m_monthCalendarStart.Width;
            // 
            // m_btnExit
            // 
            this.m_btnExit.Name = "m_btnExit";
            this.m_btnExit.TabIndex = 2;
            this.m_btnExit.Text = "Exit";
            this.m_btnExit.UseVisualStyleBackColor = true;
            this.m_btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.m_btnExit.Width = m_monthCalendarStart.Width;
            this.m_btnExit.Visible = false;
            // 
            // m_btnStripButtonExcel
            // 
            this.m_btnStripButtonExcel.Name = "m_btnStripButtonExcel";
            this.m_btnStripButtonExcel.TabIndex = 0;
            this.m_btnStripButtonExcel.Text = "Export";
            this.m_btnStripButtonExcel.UseVisualStyleBackColor = true;
            this.m_btnStripButtonExcel.Click += new System.EventHandler(this.btnStripButtonExcel_Click);
            this.m_btnStripButtonExcel.Enabled = false;
            this.m_btnStripButtonExcel.Width = m_monthCalendarStart.Width;
            // 
            // m_listBoxTEC
            // 
            this.m_listBoxTEC.FormattingEnabled = true;
            this.m_listBoxTEC.Name = "m_listBoxTEC";
            this.m_listBoxTEC.TabIndex = 3;
            this.m_listBoxTEC.Width = m_monthCalendarStart.Width;
            // 
            // m_listBoxGrpSgnl
            // 
            this.m_listBoxGrpSgnl.FormattingEnabled = true;
            this.m_listBoxGrpSgnl.Name = "m_listBoxGrpSgnl";
            this.m_listBoxGrpSgnl.TabIndex = 4;
            // 
            // m_monthCalendar
            // 
            this.m_monthCalendarStart.Name = "m_monthCalendar";
            this.m_monthCalendarStart.TabIndex = 5;
            // 
            // monthCalendar2
            // 
            this.m_monthCalendarEnd.Name = "monthCalendar2";
            this.m_monthCalendarEnd.TabIndex = 6;
            // 
            // m_labelTEC
            // 
            this.m_labelTEC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_labelTEC.AutoSize = true;
            this.m_labelTEC.Name = "m_labelTEC";
            this.m_labelTEC.TabIndex = 2;
            this.m_labelTEC.Text = "Подразделение";
            // 
            // m_labelGrpSgnl
            // 
            this.m_labelGrpSgnl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_labelGrpSgnl.AutoSize = true;
            this.m_labelGrpSgnl.Name = "m_labelGrpSgnl";
            this.m_labelGrpSgnl.TabIndex = 3;
            this.m_labelGrpSgnl.Text = "Группа сигналов";
            // 
            // m_labelValues
            // 
            this.m_labelValues.AutoSize = true;
            this.m_labelValues.Name = "m_labelValues";
            this.m_labelValues.TabIndex = 5;
            this.m_labelValues.Text = "Значения сигналов (сутки)";
            // 
            // m_labelStartDate
            // 
            this.m_labelStartDate.AutoSize = true;
            this.m_labelStartDate.Name = "m_labelStartDate";
            this.m_labelStartDate.TabIndex = 5;
            this.m_labelStartDate.Text = "";
            // 
            // m_labelEndDate
            // 
            this.m_labelEndDate.AutoSize = true;
            this.m_labelEndDate.Name = "m_labelEndDate";
            this.m_labelEndDate.TabIndex = 5;
            this.m_labelEndDate.Text = "";
            // 
            // m_labelsGroup
            // 
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                this.m_labelsGroup[i].Name = Enum.GetName(typeof(TEC_LOCAL.INDEX_DATA), i).ToString();
                this.m_labelsGroup[i].TabIndex = 5;
                this.m_labelsGroup[i].Text = Enum.GetName(typeof(TEC_LOCAL.INDEX_DATA), i).ToString();
            }
            // 
            // m_dgvValues
            // 
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                this.m_dgvValues[i].AllowUserToAddRows = false;
                this.m_dgvValues[i].AllowUserToDeleteRows = false;
                this.m_dgvValues[i].AllowUserToOrderColumns = true;
                this.m_dgvValues[i].AllowUserToResizeColumns = false;
                this.m_dgvValues[i].AllowUserToResizeRows = false;
                this.m_dgvValues[i].Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.m_dgvValues[i].AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
                this.m_dgvValues[i].AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
                this.m_dgvValues[i].ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
                dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
                dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
                dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
                dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
                this.m_dgvValues[i].DefaultCellStyle = dataGridViewCellStyle1;
                this.m_dgvValues[i].Name = "m_dgvValues" + i.ToString();
                this.m_dgvValues[i].RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                this.m_dgvValues[i].RowTemplate.ReadOnly = true;
                this.m_dgvValues[i].RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.m_dgvValues[i].SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                this.m_dgvValues[i].TabIndex = 4;
            }
            // 
            // m_sumValues
            // 
            this.m_sumValues.RowHeadersVisible = false;
            this.m_sumValues.AllowUserToAddRows = false;
            this.m_sumValues.AllowUserToDeleteRows = false;
            this.m_sumValues.AllowUserToOrderColumns = true;
            this.m_sumValues.AllowUserToResizeColumns = false;
            this.m_sumValues.AllowUserToResizeRows = false;
            this.m_sumValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_sumValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.m_sumValues.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            //this.m_sumValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.m_sumValues.DefaultCellStyle = dataGridViewCellStyle2;
            this.m_sumValues.Name = "m_sumValues";
            //this.m_sumValues.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.m_sumValues.RowTemplate.ReadOnly = true;
            this.m_sumValues.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_sumValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_sumValues.TabIndex = 4;
            this.m_sumValues.ColumnCount = 2;
            this.m_sumValues.ColumnHeadersVisible = false;



            #endregion

            m_listBoxTEC.Tag = INDEX_CONTROL.LB_TEC;
            m_listBoxTEC.SelectedIndexChanged += listBox_SelectedIndexChanged;
            m_listBoxGrpSgnl.Tag = INDEX_CONTROL.LB_GROUP_SIGNAL;
            m_listBoxGrpSgnl.SelectedIndexChanged += listBox_SelectedIndexChanged;
            m_monthCalendarStart.DateChanged += monthCalendar_DateChanged;
            m_monthCalendarEnd.DateChanged += monthCalendarEnd_DateChanged;
        }

        private void monthCalendarEnd_DateChanged(object sender, DateRangeEventArgs e)
        {
            m_labelEndDate.Text = m_monthCalendarEnd.SelectionStart.ToShortDateString();
        }

        private void monthCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearValues();
            }
            m_labelStartDate.Text = m_monthCalendarStart.SelectionStart.ToShortDateString();
        }

        private void updateRowData()
        {
            int rowWight = 200;
            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearRows();
                m_dgvValues[i].AddRowData(m_listTEC[m_listBoxTEC.SelectedIndex].m_arListSgnls[i]);
                m_dgvValues[i].RowHeadersWidthSizeMode =
                DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                m_dgvValues[i].RowHeadersWidth = rowWight;
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((INDEX_CONTROL)((Control)sender).Tag == INDEX_CONTROL.LB_TEC)
            {
                updateRowData();
            }
            else
                if ((INDEX_CONTROL)((Control)sender).Tag == INDEX_CONTROL.LB_GROUP_SIGNAL)
                updateRowData();
            else
                ;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку на панели быстрого доступа "Открыть"
        /// </summary>
        /// <param name="sender">Объект-инициатор события</param>
        /// <param name="e">Аргумент события</param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            bool date_tmp;
            date_tmp = ((object)1 == (object)1);

            //Создать форму диалога выбора файла-шаблона MS Excel
            using (FileDialog formChoiсeTemplate = new OpenFileDialog())
            {
                string strPathToTemplate = FullPathTemplate;
                int iErr = 0;

                //Установить исходные параметры для формы диалога
                if (FullPathTemplate.Equals(string.Empty) == true)
                    FullPathTemplate =
                        //Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments)
                        m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_PATH_DEFAULT]
                        + @"\"
                        + m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_NAME]
                        ;
                else
                    ;
                formChoiсeTemplate.InitialDirectory = Path.GetDirectoryName(FullPathTemplate);
                formChoiсeTemplate.Title = @"Указать книгу MS Excel-шаблон";
                formChoiсeTemplate.CheckPathExists =
                formChoiсeTemplate.CheckFileExists =
                    true;
                formChoiсeTemplate.Filter = MS_EXCEL_FILTER;
                //Отобразить форму диалога
                if (formChoiсeTemplate.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    iErr = setFullPathTemplate(formChoiсeTemplate.FileName);
                else
                    iErr = 1;

                if (!(iErr == 0))
                    switch (iErr)
                    {
                        case 1: //...отмена действия
                            //labelLog.Text += @"отменено пользователем..." + @"шаблон: " + (FullPathTemplate.Length > 0 ? FullPathTemplate : @"не указан...");
                            break;
                        case -1: //...шаблон не прошел проверку
                            MessageBox.Show("Ошибка при проверке шаблона.\r\nОбратитесь в службу поодержки (тел.: 0-4444, 289-04-37).", @"Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            //labelLog.Text += @"ошибка проверки..." + @"шаблон: " + (FullPathTemplate.Length > 0 ? FullPathTemplate : @"не указан...");
                            break;
                        case -2:
                            break;
                        default:
                            break;
                    }
                else
                {
                    m_btnStripButtonExcel.Enabled = true;
                };

                //labelLog.Text += Environment.NewLine;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            int iRes = -1
                , iListenerId = -1;
            string msg = string.Empty;
            TEC_LOCAL.INDEX_DATA indx;
            TEC_LOCAL tec = m_listTEC[m_listBoxTEC.SelectedIndex];
            TEC_LOCAL.VALUES_DATE.VALUES_GROUP dictIndxValues;

            for (int i = 0; i <= Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII); i++)
            {
                m_dgvValues[i].ClearValues();
            }

            //delegateStartWait();

            //Установить соединение с источником данных
            iListenerId = DbSources.Sources().Register(m_connSettAIISKUECentre, false, @"");
            if (!(iListenerId < 0))
            {
                Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);

                for (indx = 0; indx <= TEC_LOCAL.INDEX_DATA.GRVIII; indx++)
                {
                    if (m_listBoxTEC.SelectedIndex != m_listTEC.Count-2 && indx == TEC_LOCAL.INDEX_DATA.GRVIII)
                    {
                        indx++; break;
                    }

                    tec.ClearValues(m_monthCalendarStart.SelectionStart.Date, indx);

                    iRes = tec.Request(iListenerId
                        , m_monthCalendarStart.SelectionStart.Date //SelectionStart всегда == SelectionEnd, т.к. MultiSelect = false
                        , m_monthCalendarStart.SelectionEnd.Date.AddDays(1)
                        , indx);

                    if (!(iRes < 0))
                    {
                        dictIndxValues = tec.m_listValuesDate.Find(item => { return item.m_dataDate == m_monthCalendarStart.SelectionStart.Date; }).m_dictData[indx];
                        switch (indx)
                        {
                            case TEC_LOCAL.INDEX_DATA.TG:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TG)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.TSN:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.TSN)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRII)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVI:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVI)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVII)].Rows.Count - 1].Cells[24].Value);
                                break;
                            case TEC_LOCAL.INDEX_DATA.GRVIII:
                                m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Update(dictIndxValues);
                                m_sumValues.Rows[Convert.ToInt32(indx)].Cells[1].Value = Convert.ToString(m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Rows[m_dgvValues[Convert.ToInt32(TEC_LOCAL.INDEX_DATA.GRVIII)].Rows.Count - 1].Cells[24].Value);
                                break;
                        }

                        if (iRes == 0)
                        {
                            msg = @"Получены значения для: " + tec.m_strNameShr;
                            Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                        {
                            msg = @"Ошибка при получении значений для: " + tec.m_strNameShr;
                            Logging.Logg().Error(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        Logging.Logg().Warning(string.Format(@"FormMain::btnLoadClick () - нет результата запроса за {0} либо группа сигналов INDEX={1} пустая..."
                                , m_monthCalendarStart.SelectionStart.Date, indx)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
            {
                //delegateStopWait();
                throw new Exception(@"FormMain::btnLoad_Click () - не установлено соединение с источником данных...");
            }

            //delegateStopWait();

            //Разорвать соединение с источником данных
            DbSources.Sources().UnRegister(iListenerId);
            Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.FindForm().Close();
        }

        /// <summary>
        /// Обработчик событий: нажатие на кнопку на панели быстрого доступа "экспортВMSExcel"
        /// </summary>
        /// <param name="sender">Объект - инициатор события</param>
        /// <param name="e">Аргумент события</param>
        private void btnStripButtonExcel_Click(object sender, EventArgs e)
        {
            int iRes = -1, iErr = -1
                , iListenerId = -1;
            double[] arWriteValues;
            HMark markErr = new HMark(0);
            string msg = string.Empty;
            Logging.Logg().Action(@"Экспорт в MS Excel - начало ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (m_connSettAIISKUECentre == null)
                throw new Exception(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - нет параметров для установки соединения с источником данных...");
            else
            { }

            //delegateStartWait();

            //Установить соединение с источником данных
            iListenerId = DbSources.Sources().Register(m_connSettAIISKUECentre, false, @"");
            if (!(iListenerId < 0))
            {
                Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);

                foreach (TEC_LOCAL t in m_listTEC)
                {
                    iRes = t.Request(iListenerId, m_monthCalendarStart.SelectionStart.Date, m_monthCalendarEnd.SelectionStart.Date.AddDays(1));

                    if (!(iRes < 0))
                    {
                        msg = string.Format(@"Получены значения для: {0}", t.m_strNameShr);
                        Logging.Logg().Debug(msg, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    else
                    {
                        msg = @"Ошибка при получении значений для: " + t.m_strNameShr;
                        Logging.Logg().Error(msg, Logging.INDEX_MESSAGE.NOT_SET);
                        markErr.Set(m_listTEC.IndexOf(t), true);
                    }
                }
            }
            else
            {
                //delegateStopWait();
                throw new Exception(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - не установлено соединение с источником данных...");
            }

            //delegateStopWait();

            //Разорвать соединение с источником данных
            DbSources.Sources().UnRegister(iListenerId);

            //Создать форму диалога выбора файла-шаблона MS Excel
            using (FileDialog formChoiseResult = new SaveFileDialog())
            {
                msg = @"Сохранить результат в: ";

                //Установить исходные параметры для формы диалога
                //formChoiseResult.InitialDirectory =                
                formChoiseResult.Title = @"Указать книгу MS Excel-результат";
                formChoiseResult.CheckPathExists = true;
                formChoiseResult.CheckFileExists = false;
                formChoiseResult.Filter = MS_EXCEL_FILTER;
                //Отобразить диалог для выбора книги MS Excel для сохранения рез-та
                if (formChoiseResult.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //delegateStartWait();

                    if (File.Exists(formChoiseResult.FileName) == false)
                        File.Copy(FullPathTemplate, formChoiseResult.FileName);
                    else
                        ;

                    MSExcelIO excel = new MSExcelIO(formChoiseResult.FileName);
                    //Проверить корректность шаблона
                    if (validateTemplate(excel) == 0)
                    {
                        //Установить текущим лист книги с именем из конфигурационного файла
                        excel.SelectWorksheet(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.SHEET_NAME]);
                        //Установить начальные значения для строк на листе книги MS Excel
                        int iColDataDate = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.COL_DATADATE])
                            , iRowStartMSExcel = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.ROW_START])
                            , iRowCountDateMSExcel = Int32.Parse(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.ROWCOUNT_DATE])
                            //, indx = -1
                            , iRowStart;
                        List<DateTime> listWriteDates = new List<DateTime>();
                        // сохранить активную мощность
                        foreach (TEC_LOCAL t in m_listTEC)
                            if (markErr.IsMarked(m_listTEC.IndexOf(t)) == false)
                                foreach (TEC_LOCAL.VALUES_DATE valsDate in t.m_listValuesDate)
                                {
                                    try
                                    {
                                        //Определить начальную строку по дате набора значений
                                        iRowStart = iRowStartMSExcel + (valsDate.m_dataDate.Day - 1) * iRowCountDateMSExcel;

                                        if (listWriteDates.IndexOf(valsDate.m_dataDate) < 0)
                                        {
                                            listWriteDates.Add(valsDate.m_dataDate);

                                            excel.WriteDate(iColDataDate, iRowStart, valsDate.m_dataDate);
                                        }
                                        else
                                            ;

                                        //Сохранить набор значений на листе книги MS Excel
                                        excel.WriteValues(t.m_arMSExcelNumColumns[(int)GetDataFromDB.INDEX_MSEXCEL_COLUMN.APOWER]
                                            , iRowStart
                                            , valsDate.m_dictData[TEC_LOCAL.INDEX_DATA.TG].m_summaHours);
                                        // получить набор значений для записи в соответствии с вариантом расчета
                                        arWriteValues = valsDate.GetValues(out iErr);
                                        if (iErr == 0)
                                            //Сохранить набор значений на листе книги MS Excel
                                            excel.WriteValues(t.m_arMSExcelNumColumns[(int)GetDataFromDB.INDEX_MSEXCEL_COLUMN.SNUZHDY]
                                                , iRowStart
                                                , arWriteValues);
                                        else
                                            Logging.Logg().Error(string.Format(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - TEC.ИД={0}, дата={1}, отсутствуют необходимые для расчета группы..."
                                                    , t.m_Id, valsDate.m_dataDate)
                                                , Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                    catch (Exception exc)
                                    {
                                        Logging.Logg().Exception(exc
                                            , string.Format(@"FormMain::экспортВMSExcelToolStripMenuItem_Click () - TEC.ИД={0}, дата={1}"
                                                , t.m_Id, valsDate.m_dataDate)
                                            , Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                }
                            else
                                ;

                        excel.SaveExcel(formChoiseResult.FileName);

                        //Закрыть книгу MS Excel
                        excel.Dispose();
                        excel = null;

                        msg += formChoiseResult.FileName;

                    }
                    else
                        ;

                    //delegateStopWait();
                }
                else
                {
                    msg += @"отменено пользователем...";
                }

                Logging.Logg().Action(msg, Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Проверить шаблон на корректность использования
        /// </summary>
        /// <param name="path">Строка - полный путь для шаблона</param>
        /// <returns>Признак проверки (0 - успех)</returns>
        private int validateTemplate(string path)
        {
            int iRes = 0;

            MSExcelIO excel = null;

            if (File.Exists(path) == true)
            {
                try
                {
                    excel = new MSExcelIO(path);
                }
                catch (Exception e)
                {
                    iRes = -1;

                    Logging.Logg().Exception(e, @"FormMain::validateTemplate (" + path + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (iRes == 0)
                    iRes = validateTemplate(excel as MSExcelIO);
                else
                    ;

                excel.CloseExcelDoc();
                excel.Dispose();
            }
            else
                iRes = -2;

            return iRes;
        }

        /// <summary>
        /// Проверить шаблон на возможность использования по назначению
        /// </summary>
        /// <param name="excel">Шаблон для экспорта данных</param>
        /// <returns>Признак проверки (0 - успех)</returns>
        private int validateTemplate(MSExcelIO excel)
        {
            int iRes = 0;

            try
            {
                excel.SelectWorksheet(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.SHEET_NAME]);
            }
            catch (Exception e)
            {
                iRes = -2;

                Logging.Logg().Exception(e, @"FormMain::validateTemplate (" + @"???ПУТЬ_К_ФАЙЛУ " + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            string ver = string.Empty;

            if (iRes == 0)
            {
                ver = excel.ReadValue(1, 1);

                iRes = ver.Equals(m_arMSEXEL_PARS[(int)INDEX_MSEXCEL_PARS.TEMPLATE_VER]) == true ? 0 : -3;
            }
            else
                ;

            return iRes;
        }
    }
}
