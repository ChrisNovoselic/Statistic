﻿using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

using GemBox.Spreadsheet;
//using Excel = Microsoft.Office.Interop.Excel;

using HClassLibrary;
using StatisticCommon;


namespace Statistic
{
    partial class PanelAdminLK : PanelAdmin
    {
        public class AdminTS_LK : AdminTS_TG
        {
            /// <summary>
            /// Семафор для формирования списка компонентов
            /// </summary>
            public Semaphore m_semaIndxTECComponents;

            /// <summary>
            /// Список предыдущих значений RDG
            /// </summary>
            public List<RDGStruct[]> m_listPrevRDGValues;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="arMarkPPBRValues"></param>
            public AdminTS_LK(bool[] arMarkPPBRValues)
                : base(arMarkPPBRValues)
            {
                _tsOffsetToMoscow = HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE;
            }

            /// <summary>
            /// Формирование списка компонентов в зависимости от выбранного в ComboBox
            /// </summary>
            /// <param name="id">ИД компонента в ComboBox</param>
            public override void FillListIndexTECComponent(int id)
            {
                lock (m_lockSuccessGetData)
                {
                    m_listTECComponentIndexDetail.Clear();

                    //Сначала - ГТП
                    foreach (TECComponent comp in allTECComponents)
                        if (comp.tec.m_id > (int)TECComponent.ID.LK)
                            if ((comp.m_id == GetIdTECComponent(id)) && //Принадлежит ТЭЦ
                                ((comp.IsGTP == true) /*|| //Является ГТП
                            ((comp.m_id > 1000) && (comp.m_id < 10000))*/)) //Является ТГ
                            {
                                m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp));

                                foreach (TG tg in comp.m_listTG)
                                    foreach (TECComponent comp_tg in allTECComponents)
                                        if (comp_tg.m_id == tg.m_id)
                                            m_listTECComponentIndexDetail.Add(allTECComponents.IndexOf(comp_tg));
                            }
                            else
                                ;
                        else
                            ;

                    m_listTECComponentIndexDetail.Sort();
                    m_listCurRDGValues.Clear();
                }
            }

            /// <summary>
            /// Метод создания потока получения значений без даты
            /// </summary>
            /// <param name="obj"></param>
            protected override void threadGetRDGValuesWithoutDate(object obj)
            {
                int indxEv = -1;

                //lock (m_lockSuccessGetData)
                //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);

                    if (indxEv == 0)
                    {
                        m_semaIndxTECComponents.WaitOne();

                        base.BaseGetRDGValue(/*m_typeFields,*/ indx, DateTime.MinValue);
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
            /// <param name="obj"></param>
            protected override void threadGetRDGValuesWithDate(object date)
            {
                int indxEv = -1;

                for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                    ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                //lock (m_lockSuccessGetData)
                //{
                foreach (int indx in m_listTECComponentIndexDetail)
                {
                    indxEv = WaitHandle.WaitAny(m_waitHandleState);
                    if (indxEv == 0)
                    {
                        m_semaIndxTECComponents.WaitOne();//Ожидание изменения состояния семафора

                        base.BaseGetRDGValue(indx, (DateTime)date);

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

                for (int a = 0; a < m_listCurRDGValues.Count; a++)
                {
                    for (int i = 0; i < 24; i++)
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
            public override Errors SaveChanges()
            {
                Errors errRes = Errors.NoError,
                        bErr = Errors.NoError;
                int indxEv = -1;

                m_evSaveChangesComplete.Reset();

                lock (m_lockResSaveChanges)
                {
                    m_listResSaveChanges.Clear();
                }

                int prevIndxTECComponent = indxTECComponents;

                foreach (RDGStruct[] curRDGValues in m_listCurRDGValues)
                {
                    bErr = Errors.NoError;

                    for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < (INDEX_WAITHANDLE_REASON.ERROR + 1); i++)
                        ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                    if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.TG)
                    {
                        indxEv = WaitHandle.WaitAny(m_waitHandleState);
                        if (indxEv == 0)
                        {
                            indxTECComponents = m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

                            curRDGValues.CopyTo(m_curRDGValues, 0);

                            bErr = base.BaseSaveChanges();
                        }
                        else
                            break;
                    }
                    else
                        if (modeTECComponent(m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)]) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                        {
                            indxEv = WaitHandle.WaitAny(m_waitHandleState);
                            if (indxEv == 0)
                            {
                                indxTECComponents = m_listTECComponentIndexDetail[m_listCurRDGValues.IndexOf(curRDGValues)];

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

                        if (!(bErr == Errors.NoError) && (errRes == Errors.NoError))
                            errRes = bErr;
                        else
                            ;
                    }
                }

                indxTECComponents = prevIndxTECComponent;

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
            protected override void initTEC()
            {
                allTECComponents.Clear();

                foreach (StatisticCommon.TEC t in this.m_list_tec)
                    //Logging.Logg().Debug("Admin::InitTEC () - формирование компонентов для ТЭЦ:" + t.name);
                    if (t.m_id > (int)TECComponent.ID.LK)
                        if (t.list_TECComponents.Count > 0)
                            foreach (TECComponent g in t.list_TECComponents)
                                allTECComponents.Add(g);
                        else
                            allTECComponents.Add(t.list_TECComponents[0]);
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
                public static string []m_col_name = { "Час", "Значение", "Температура" };

                /// <summary>
                /// Индексы колонок
                /// </summary>
                public enum INDEX_COLUMN : int { Unknowun = -1, Time, Power, Temperature }

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
                                                    row[2] = w.Rows[r.Index + 1].Cells[i + 2].Value.ToString().Trim();//Значение температуры

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
                        Logging.Logg().Error("PanelAdminLK : getCSV - ошибка при открытии потока" + e.Message, Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return dataTableRes;
                }
            }

            protected override void /*bool*/ ImpRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }

            protected override int ImpRDGExcelValuesResponse()
            {
                throw new NotImplementedException();
            }

            protected override void /*bool*/ ExpRDGExcelValuesRequest()
            {
                throw new NotImplementedException();
            }
        }
    }
}
