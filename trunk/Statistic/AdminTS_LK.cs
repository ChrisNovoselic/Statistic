﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using GemBox.Spreadsheet;
//using Excel = Microsoft.Office.Interop.Excel;

using StatisticCommon;

namespace Statistic {
    partial class PanelAdminLK : PanelAdmin
    {
        public class AdminTS_LK : AdminTS_TG
        {
            /// <summary>
            /// Семафор для формирования списка компонентов
            /// </summary>
            public Semaphore m_semaIndxTECComponents;

            ///// <summary>
            ///// Список предыдущих значений RDG
            ///// </summary>
            //public List<RDGStruct[]> m_listPrevRDGValues;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="arMarkPPBRValues">Массив признаков, определяющих необходимость использования тех или иных типов источников данных</param>
            public AdminTS_LK(bool[] arMarkPPBRValues)
                : base(arMarkPPBRValues, TECComponentBase.TYPE.ELECTRO)
            {
                _tsOffsetToMoscow = ASUTP.Core.HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
            }

            private int currentIndexTECComponent
            {
                get
                {
                    return getIndexTECComponent (CurrentKey);
                }
            }

            private int getIndexTECComponent (FormChangeMode.KeyDevice key)
            {
                int iRes = -1;

                try {
                    iRes = allTECComponents.IndexOf (allTECComponents.Find (comp => comp.m_id == key.Id));
                } catch (Exception e) {
                    ASUTP.Logging.Logg ().Exception (e, $@"AdminTS::getIndexTECComponent (key={key.Id}) - компонент не найден...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }

                return iRes;
            }

            /// <summary>
            /// Идентификатор компонента ТЭЦ
            /// </summary>
            /// <param name="indx">индекс в массиве 'все компоненты'</param>
            /// <returns>идентификатор</returns>
            public int GetIdTECComponent (int indx = -1)
            {
                int iRes = -1;

                if (indx < 0)
                    indx = currentIndexTECComponent;
                else
                    ;

                if ((!(indx < 0))
                    && (indx < allTECComponents.Count))
                    iRes = allTECComponents [indx].m_id;
                else
                    ;

                return iRes;
            }

            /// <summary>
            /// Формирование списка компонентов в зависимости от выбранного в ComboBox
            /// </summary>
            /// <param name="id">ИД компонента в ComboBox</param>
            public override void FillListKeyTECComponentDetail(int id)
            {
                lock (m_lockSuccessGetData)
                {
                    m_listKeyTECComponentDetail.Clear();

                    //Сначала - ГТП
                    foreach (TECComponent comp in allTECComponents)
                        if (comp.tec.m_id > (int)TECComponent.ID.LK)
                            if ((comp.m_id == id) //Принадлежит ТЭЦ
                                && (comp.IsGTP_LK == true)) //Является ГТП_ЛК
                            {
                                foreach (TECComponentBase tg in comp.ListLowPointDev)
                                    m_listKeyTECComponentDetail.Add (new FormChangeMode.KeyDevice () {
                                        Id = tg.m_id
                                        , Mode = FormChangeMode.MODE_TECCOMPONENT.TG
                                    });
                                //??? зачем в список детализации помещать компонент верхнего(родительского) уровня
                                //m_listKeyTECComponentDetail.Add (new FormChangeMode.KeyDevice () {
                                //    Id = comp.m_id
                                //    , Mode = FormChangeMode.MODE_TECCOMPONENT.GTP
                                //});
                            } else
                                ;
                        else
                            ;

                    m_listKeyTECComponentDetail.Sort();
                    ClearListRDGValues();
                }
            }

            /// <summary>
            /// Метод создания потока получения значений без даты
            /// </summary>
            /// <param name="obj">Объект, передаваемый в качестве параметра при запуске потока</param>
            protected override void threadGetRDGValuesWithoutDate(object obj)
            {
                int indxEv = -1;

                //lock (m_lockSuccessGetData)
                //{
                foreach (FormChangeMode.KeyDevice key in m_listKeyTECComponentDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);

                    if (indxEv == 0)
                    {
                        m_semaIndxTECComponents.WaitOne();

                        base.BaseGetRDGValue(key, DateTime.MinValue);
                        m_listPrevRDGValues = new List<RDGStruct[]>(m_listCurRDGValues);
                    }
                    else
                        break;
                }
                //}

                //m_bSavePPBRValues = true;
            }

            /// <summary>
            /// Метод создания потока получения значений с датой
            /// </summary>
            /// <param name="obj">Объект, передаваемый в качестве параметра при запуске потока</param>
            protected override void threadGetRDGValuesWithDate(object date)
            {
                int indxEv = -1;

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                //lock (m_lockSuccessGetData)
                //{
                foreach (FormChangeMode.KeyDevice key in m_listKeyTECComponentDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                    {
                        m_semaIndxTECComponents.WaitOne();//Ожидание изменения состояния семафора

                        base.BaseGetRDGValue(key, (DateTime)date);

                        m_listPrevRDGValues = new List<RDGStruct[]>(m_listCurRDGValues);
                    }
                    else
                        break;
                }
                //}

                //m_bSavePPBRValues = true;
            }

            /// <summary>
            /// Метод проверки на измененные значения
            /// </summary>
            /// <returns>Было ли изменено</returns>
            public override bool WasChanged()
            {
                bool bRes = false;

                for (int a = 0;
                    (a < m_listCurRDGValues.Count)
                        && (a < m_listPrevRDGValues.Count);
                    a++)
                {
                    for (int i = 0;
                        (i < 24)
                            && (i < m_listCurRDGValues [a].Length)
                            && (i < m_listPrevRDGValues [a].Length);
                        i++)
                    {
                        if (m_listPrevRDGValues[a][i].pbr.Equals(m_listCurRDGValues[a][i].pbr) /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/  == false)
                            return true;
                        else
                            ;
                        if (m_listPrevRDGValues[a][i].pmin != m_listCurRDGValues[a][i].pmin /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                            return true;
                        else
                            ;
                        if (m_listPrevRDGValues[a][i].deviationPercent != m_listCurRDGValues[a][i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                            return true;
                        else
                            ;
                        if (m_listPrevRDGValues[a][i].deviation != m_listCurRDGValues[a][i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                            return true;
                        else
                            ;
                    }
                }

                return bRes;
            }

            /// <summary>
            /// Сохранение внесенных изменений
            /// </summary>
            /// <returns>Ошибка выполнения</returns>
            public override ASUTP.Helper.Errors SaveChanges ()
            {
                ASUTP.Helper.Errors errRes = ASUTP.Helper.Errors.NoError,
                        bErr = ASUTP.Helper.Errors.NoError;
                int indxEv = -1;

                m_evSaveChangesComplete.Reset();

                lock (m_lockResSaveChanges)
                {
                    m_listResSaveChanges.Clear();
                }

                FormChangeMode.KeyDevice prevKeyTECComponent = CurrentKey;

                foreach (RDGStruct[] curRDGValues in m_listCurRDGValues)
                {
                    bErr = ASUTP.Helper.Errors.NoError;

                    for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                        ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                    if (m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)].Mode == FormChangeMode.MODE_TECCOMPONENT.TG)
                    {
                        indxEv = WaitHandle.WaitAny(m_waitHandleState);
                        if (indxEv == 0)
                        {
                            CurrentKey = m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                            curRDGValues.CopyTo(m_curRDGValues, 0);

                            bErr = base.BaseSaveChanges();
                        }
                        else
                            break;
                    }
                    else
                        if (m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)].Mode == FormChangeMode.MODE_TECCOMPONENT.GTP)
                        {
                            indxEv = WaitHandle.WaitAny(m_waitHandleState);
                            if (indxEv == 0)
                            {
                                CurrentKey = m_listKeyTECComponentDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                                curRDGValues.CopyTo(m_curRDGValues, 0);

                                bErr = base.BaseSaveChanges();
                            }
                            else
                                break;
                        }
                    ;

                    lock (m_lockResSaveChanges)
                    {
                        m_listResSaveChanges.Add(bErr);

                        if (!(bErr == ASUTP.Helper.Errors.NoError)
                            && (errRes == ASUTP.Helper.Errors.NoError))
                            errRes = bErr;
                        else
                            ;
                    }
                }

                CurrentKey = prevKeyTECComponent;

                //if (indxEv == 0)
                //if (errRes == Errors.NoError)
                m_evSaveChangesComplete.Set();
                //else ;

                if (!(saveComplete == null)) saveComplete(); else ;

                return errRes;
            }

            //protected void /*bool*/ ExpRDGExcelValuesRequest()
            //{
            //}

            /// <summary>
            /// Инициализация синхронизации состояния
            /// </summary>
            protected override void InitializeSyncState()
            {
                m_semaIndxTECComponents = new Semaphore(1, 1);

                base.InitializeSyncState();
            }

            /// <summary>
            /// Метод получения ТЭЦ компонентов
            /// </summary>
            protected override void initTECComponents()
            {
                allTECComponents.Clear();

                foreach (StatisticCommon.TEC t in this.m_list_tec)
                    //Logging.Logg().Debug("Admin::InitTEC () - формирование компонентов для ТЭЦ:" + t.name);
                    if (t.m_id > (int)TECComponent.ID.LK)
                        if (t.ListTECComponents.Count > 0)
                            foreach (TECComponent g in t.ListTECComponents)
                                allTECComponents.Add(g);
                        else
                            allTECComponents.Add(t.ListTECComponents[0]);
            }

            public int GetCurrentIndexTECComponent ()
            {
                return m_listKeyTECComponentDetail.IndexOf(CurrentKey);
            }

            /// <summary>
            /// Получение m_id ТЭЦ которой пренадлежит компонент
            /// </summary>
            /// <param name="indx">ИД компонента</param>
            /// <returns>Возвращает ИД ТЭЦ</returns>
            public int GetIdTECOwnerTECComponent(int indx = -1)
            {
                int iRes = -1;

                iRes = allTECComponents[indx].tec.m_id;

                return iRes;
            }

            public DataTable ImportExcel(string path, out int err)
            {
                err = -1;
                DataTable table_res = new DataTable();
                table_res = Get_DataTable_From_Excel.getDT(path, m_curDate.Date, out err);

                if (err > (int)Get_DataTable_From_Excel.INDEX_ERR.NOT_ERR)
                {
                    warningReport(Get_DataTable_From_Excel.str_err[err]);
                }
                return table_res;
            }

            private class Get_DataTable_From_Excel
            {
                public enum INDEX_ERR : int { NOT_ERR, NOT_WORKSHEET, NOT_DATA }

                public static string[] str_err = { "Ошибок нет", "Не найден лист с выбранным месяцем!", "Нет данных за выбранную дату!" };

                /// <summary>
                /// Массив имен месяцев
                /// </summary>
                public static string[] mounth = { "UNKNOWN", "ЯНВАРЬ", "ФЕВРАЛЬ", "МАРТ", "АПРЕЛЬ", "МАЙ", "ИЮНЬ", "ИЮЛЬ", "АВГУСТ", "СЕНТЯБРЬ", "ОКТЯБРЬ", "НОЯБРЬ", "ДЕКАБРЬ" };

                /// <summary>
                /// Массив имен колонок
                /// </summary>
                public static string[] m_col_name = { "Час", "Температура", "Значение" };

                /// <summary>
                /// Индексы колонок
                /// </summary>
                public enum INDEX_COLUMN : int { Unknowun = -1, Time, Temperature, Power }

                /// <summary>
                /// Получение таблицы из Excel
                /// </summary>
                /// <param name="path">Путь к документу</param>
                /// <param name="data">Дата для которой получить значения</param>
                /// <returns>Таблица со значениями</returns>
                public static DataTable getDT(string path, DateTime data, out int err)
                {
                    err = -1;
                    DataTable dtExportCSV = new DataTable();

                    dtExportCSV = getCSV(path, data, out err);

                    return dtExportCSV;
                }

                /// <summary>
                /// Метод выборки значений из Excell
                /// </summary>
                /// <param name="path">Путь к документу</param>
                /// <param name="date">Дата за которую необходимо получить значения</param>
                /// <returns>Возвращает таблицу со значениями</returns>
                private static DataTable getCSV(string path, DateTime date, out int err)
                {
                    err = -1;
                    DataTable dataTableRes = new DataTable();
                    string name_worksheet = string.Empty;
                    DataRow[] rows = new DataRow[25];
                    int col_worksheet = 0;
                    bool start_work = false;

                    for (int i = 0; i < m_col_name.Length; i++)
                        dataTableRes.Columns.Add(m_col_name[i]);//Добавление колонок в таблицу

                    //Открыть поток чтения файла...
                    try
                    {
                        ExcelFile excel = new ExcelFile();
                        excel.LoadXls(path);//загружаем в созданный экземпляр документ Excel
                        name_worksheet = "Расчет " + mounth[date.Month];//Генерируем имя необходимого листа в зависимости от переданной даты

                        foreach (ExcelWorksheet w in excel.Worksheets)//Перебор листов
                        {
                            if (w.Name.Equals(name_worksheet, StringComparison.InvariantCultureIgnoreCase))//Если имя совпадает с сгенерируемым нами то
                            {
                                start_work = true;
                                foreach (ExcelRow r in w.Rows)//перебор строк документа
                                {
                                    if (r.Cells[0].Value != null)//Если значение строки не пусто то
                                        if (r.Cells[0].Value.ToString() == date.Date.ToString())//Если дата в строке совпадает с переданной то
                                        {
                                            for (int i = 0; i < 24; i++)//Перебор ячеек со значениями по часам
                                            {
                                                object[] row = new object[3];
                                                row[0] = i.ToString();//Час

                                                if (r.Cells[i + 2].Value == null)//Если ячейка пуста то
                                                    row[(int)INDEX_COLUMN.Power] = 0.ToString("F2");//0 в формате (0.00)
                                                else
                                                    row[(int)INDEX_COLUMN.Power] = r.Cells[i + 2].Value.ToString().Trim();//Значение ПБР

                                                if (w.Rows[r.Index + 1].Cells[i + 2].Value == null)
                                                    row[(int)INDEX_COLUMN.Temperature] = string.Empty;
                                                else
                                                    row[(int)INDEX_COLUMN.Temperature] = w.Rows[r.Index + 1].Cells[i + 2].Value.ToString().Trim();//Значение температуры

                                                dataTableRes.Rows.Add(row);//Добавляем строку в таблицу
                                            }
                                        }

                                    if (dataTableRes.Rows.Count >= 24) //Если количестко строк стало равным ли больше 24 то прерываем перебор
                                    {
                                        //err = (int)INDEX_ERR.NOT_ERR;
                                        break;
                                    }

                                }
                            }
                            else
                            {
                                if (dataTableRes.Rows.Count == 0 && col_worksheet == excel.Worksheets.Count - 1 && start_work == true)
                                {
                                    err = (int)INDEX_ERR.NOT_DATA;
                                    break;
                                }
                                else
                                {
                                    if (dataTableRes.Rows.Count >= 24 && start_work == true)
                                    {
                                        err = (int)INDEX_ERR.NOT_ERR;
                                        break;
                                    }
                                    else
                                        err = (int)INDEX_ERR.NOT_WORKSHEET;
                                }
                            }
                            col_worksheet++;
                        }

                    }
                    catch (Exception e)
                    {
                        ASUTP.Logging.Logg().Error("PanelAdminLK : getCSV - ошибка при открытии потока" + e.Message, ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return dataTableRes;
                }
            }

            protected override void /*bool*/ impRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }

            protected override int impRDGExcelValuesResponse()
            {
                throw new NotImplementedException();
            }

            protected override void /*bool*/ expRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }
        }
    }
}
