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
using ASUTP.Database;

namespace CommonAux
{
    /// <summary>
    /// Класс для взаимодействия с базой данных
    /// </summary>
    public partial class GetDataFromDB : ASUTP.Helper.HHandlerDb {
        /// <summary>
        /// Строка, содержащая наименование таблицы в БД, хранящей перечень каналов
        /// </summary>
        public static string DB_TABLE = @"[ID_TSN_ASKUE_2017]";
        /// <summary>
        /// Строка, содержащая наименование таблицы в БД, хранящей путь к шаблону excel
        /// </summary>
        public static string DB_TABLE_SETUP = @"[setup]";
        /// <summary>
        /// Объект, содержащий id записи в таблице, содержащей настройки подключения
        /// </summary>
        public static int ID_AIISKUE_CONSETT = 7001;
        /// <summary>
        /// Объект, содержащий путь к шаблону excel
        /// </summary>
        private string m_strFullPathTemplate;
        /// <summary>
        /// Поля таблицы сигналов
        /// </summary>
        private enum DB_TABLE_DATA {
            ID_USPD, ID_CHANNEL, ID_TEC, ID_TG, Description, GROUP, NAME, USE, ID
        };
        /// <summary>
        /// Номера столбцовы в шаблоне excel
        /// </summary>
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
        /// <summary>
        /// Поля таблицы setup
        /// </summary>
        public enum DB_TABLE_STP
        {
            ID, VALUE, KEY, LAST_UPDATE, ID_UNIT
        };
        /// <summary>
        /// Список всех каналов для каждой ТЭЦ
        /// </summary>
        public List<TEC_LOCAL> m_listTEC;

        private ASUTP.Core.HMark m_markIndxRequestError;
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
        public static string getQueryListChannels()
        {
            string strRes = "SELECT ";
            foreach (DB_TABLE_DATA indx in Enum.GetValues(typeof(DB_TABLE_DATA)))
            {
                strRes += "[" + indx.ToString() + "], ";
            }
            strRes = strRes.Remove(strRes.Length-2, 2);
            strRes += " FROM " + DB_TABLE;
            return strRes;
        }

        /// <summary>
        /// Возвратить таблицу [ID_TSN_AISKUE_2017] из БД конфигурации
        /// </summary>
        /// <param name="iListenerId">Идентификатор подписчика для обращения к БД</param>
        /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
        /// <returns>Таблица - с данными</returns>
        public static DataTable getListChannels(int iListenerId, out int err)
        {
            DataTable tableRes = new DataTable();

            DbConnection connConfigDB;
            string req = string.Empty;

            connConfigDB = DbSources.Sources ().GetConnection (iListenerId, out err);

            if (err == 0)
            {
                req = getQueryListChannels ();

                tableRes = DbTSQLInterface.Select (ref connConfigDB, req, null, null, out err);
            }
            else
            { }

            return tableRes;
        }

        /// <summary>
        /// Возвратить объект с параметрами соединения
        /// </summary>
        /// <param name="iListenerId">Идентификатор подписчика к объекту с установленным соединением с БД</param>
        /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
        /// <returns>Объект с параметрами соединения</returns>
        public ConnectionSettings GetConnSettAIISKUECentre(int iListenerId, out int err)
        {
            DataTable dataTableRes = new DataTable();
            dataTableRes = InitTEC_200.getConnSettingsOfIdSource(iListenerId, ID_AIISKUE_CONSETT, -1, out err);

            return new ConnectionSettings(dataTableRes.Rows[dataTableRes.Rows.Count - 1], -1);
        }

        /// <summary>
        /// Инициализировать список ТЭЦ
        /// </summary>
        /// <param name="iListenerId">Идентификатор подписчика для обращения к БД</param>
        public void InitListTEC (int iListenerId)
        {
            List<TEC> listTEC;

            listTEC = new InitTEC_200 (iListenerId, true, new int [] { 0, (int)TECComponent.ID.LK }, false).tec;

            m_listTEC = new List<TEC_LOCAL> ();
            listTEC.ForEach (tec => m_listTEC.Add (new TEC_LOCAL (tec)));
        }

        /// <summary>
        /// Инициализировать список ТЭЦ
        /// </summary>
        /// <param name="iListenerId">Идентификатор подписчика для обращения к БД</param>
        public void GetIndexOfListSgnls(int i)
        {

        }

        /// <summary>
        /// Загрузка всех каналов из базы данных
        /// </summary>
        /// <param name="iListenerId">Идентификатор подписчика для обращения к БД</param>
        public void InitChannels(int iListenerId)
        {
            int err = -1;

            DataTable table_channels = null;

            //Получить список каналов, используя статическую функцию
            table_channels = getListChannels(iListenerId, out err);

            m_listTEC.ForEach (tec => {
                try {
                    (from DataRow row1
                        in table_channels.Select ($"{DB_TABLE_DATA.ID_TEC}={tec.m_Id}") // выбрать сигналы только ТЭЦ по ее идентификатору
                        group row1 by (TEC_LOCAL.INDEX_DATA)Enum.Parse (typeof (TEC_LOCAL.INDEX_DATA), Convert.ToString (row1.ItemArray [Convert.ToInt32 (DB_TABLE_DATA.GROUP)]))
                        into groupTECSignals // группа сигналов
                            select new {
                                // индекс группы (из перечисления 'TEC_LOCAL.INDEX_DATA')
                                Index = groupTECSignals.Key
                                // перечень сигналов для группы
                                , Values = from DataRow row2
                                    in groupTECSignals
                                    select new SIGNAL (Convert.ToString (row2.ItemArray [Convert.ToInt32 (DB_TABLE_DATA.Description)]),
                                        Convert.ToInt32 (row2.ItemArray [Convert.ToInt32 (DB_TABLE_DATA.ID_USPD)]),
                                        Convert.ToInt32 (row2.ItemArray [Convert.ToInt32 (DB_TABLE_DATA.ID_CHANNEL)]),
                                        Convert.ToBoolean (row2.ItemArray [Convert.ToInt32 (DB_TABLE_DATA.USE)]))
                            }).ToList ()
                        // получен список
                        .ForEach(grp => {
                            tec.m_arListSgnls [(int)grp.Index].AddRange (grp.Values);
                        });
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"CommonAux.GetDataFromDB::InitChannels () - получение списка каналов для подразделения: {0}...", tec.m_strNameShr), Logging.INDEX_MESSAGE.NOT_SET);
                }
            });
        }

        public void InitSensors ()
        {
            foreach (TEC_LOCAL t in m_listTEC)
                t.InitSensors ();
        }

        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="tec">Станция, для которой необходимо получить значения</param>
        /// <param name="iListenerId">Идентификатор установленного соединения с источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(TEC_LOCAL tec, int iListenerId, DateTime dtStart, DateTime dtEnd)
        {
            int iRes = 0;

            tec.m_listValuesDate.Clear();
            if (m_markIndxRequestError == null)
                m_markIndxRequestError = new ASUTP.Core.HMark (0);
            else
                m_markIndxRequestError.SetOf(0);

            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out iRes);

            if (iRes == 0)
                foreach (TEC_LOCAL.INDEX_DATA indx in Enum.GetValues(typeof(TEC_LOCAL.INDEX_DATA)))
                {
                    ActionReport($"Получение значения для {indx.ToString ()} {tec.m_strNameShr}");
                    // запросить и обработать результат запроса по получению значений для группы сигналов в указанный диапазон дат
                    iRes = Request(tec, ref dbConn, dtStart, dtEnd, indx);
                    m_markIndxRequestError.Set((int)indx, iRes < 0);

                    ActionReport($"Получены значения для {indx.ToString()} {tec.m_strNameShr}");
                }
            else
            {
                Logging.Logg().ExceptionDB("FormMain.Tec.Request () - не установлено соединение с DB...");
                iRes = -1;
            }

            iRes = m_markIndxRequestError.Value == 0 ? 0 : -1;

            ReportClear(true);
            return iRes;
        }

        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="tec">Станция, для которой необходимо получить значения</param>
        /// <param name="iListenerId">Идентификатор установленного соединения с источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <param name="indx">Индекс группы сигналов</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(TEC_LOCAL tec, int iListenerId, DateTime dtStart, DateTime dtEnd, TEC_LOCAL.INDEX_DATA indx)
        {
            int iRes = 0;
            string query = string.Empty;

            DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out iRes);

            iRes = Request(tec, ref dbConn, dtStart, dtEnd, indx);

            return iRes;
        }

        /// <summary>
        /// Получить все (ТГ, ТСН) значения для станции
        /// </summary>
        /// <param name="tec">Станция, для которой необходимо получить значения</param>
        /// <param name="dbConn">Ссылка на объект соединения с БД-источником данных</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <param name="indx">Индекс группы сигналов</param>
        /// <returns>Результат выполнения - признак ошибки (0 - успех)</returns>
        public int Request(TEC_LOCAL tec, ref DbConnection dbConn, DateTime dtStart, DateTime dtEnd, TEC_LOCAL.INDEX_DATA indx)
        {
            int iRes = 0
                , err = -1;
            string query = string.Empty;
            DateTime dtQuery;
            TimeSpan tsQuery;

            ActionReport("Получение значения для " + indx.ToString() + " " + tec.m_strNameShr);

            dtQuery = DateTime.Now;

            tec.m_arTableResult[(int)indx] = null;
            query = getQuery(tec, indx, dtStart, dtEnd);

            if (query.Equals(string.Empty) == false)
            {
                tec.m_arTableResult[(int)indx] = new TEC_LOCAL.TableResult(DbTSQLInterface.Select(ref dbConn, query, null, null, out err));

                tsQuery = DateTime.Now - dtQuery;

                Logging.Logg().Action(string.Format(@"TEC.ID={0}, ИНДЕКС={1}, время={4}{2}запрос={3} сек"
                        , tec.m_Id, indx.ToString(), Environment.NewLine, query, tsQuery.TotalSeconds)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                if (err == 0)
                {
                    tec.parseTableResult(dtStart, dtEnd, indx, out err);
                    ActionReport("Получены значения для " + indx.ToString() + " " + tec.m_strNameShr);
                }
                else
                {
                    Logging.Logg().Error(string.Format("TEC.ID={0}, ИНДЕКС={1} не получены данные за {2}{3}Запрос={4}"
                            , tec.m_Id, indx.ToString(), dtEnd, Environment.NewLine, query)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = -1;
                }
            }
            else
            {
                Logging.Logg().Error(string.Format("TEC.ID={0}, группа ИНДЕКС={1} пропущена, не сформирован запрос за {2}"
                        , tec.m_Id, indx.ToString(), dtStart, Environment.NewLine, query)
                    , Logging.INDEX_MESSAGE.NOT_SET);

                iRes = 1; // Предупреждение
            }

            ReportClear(true);
            return iRes;
        }

        /// <summary>
        /// Возвратитиь запрос для выборки данных ТГ
        /// </summary>
        /// <param name="tec">Станция, для которой необходимо получить значения</param>
        /// <param name="indx">Индекс текущей группы сигналов</param>
        /// <param name="dtStart">Дата - начало</param>
        /// <param name="dtEnd">Дата - окончание</param>
        /// <returns>Строка запроса</returns>
        private string getQuery(TEC_LOCAL tec, TEC_LOCAL.INDEX_DATA indx, DateTime dtStart, DateTime dtEnd)
        {
            string strRes = @"SELECT res.[OBJECT], res.[ITEM], SUM(res.[VALUE0]) / COUNT(*)[VALUE0], res.[DATETIME], COUNT(*) as [COUNT]
                            FROM(
                            SELECT[OBJECT], [ITEM], [VALUE0], 
                            DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0), [DATA_DATE]) / 60.) * 60,
                            DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0)) as [DATETIME]
                            FROM[DATA]
                            WHERE[PARNUMBER] = 12 AND[OBJTYPE] = 0 AND([DATA_DATE] > '?DATADATESTART?' AND NOT[DATA_DATE] > '?DATADATEEND?')
                            AND(?SENSORS?)
                            GROUP BY[RCVSTAMP], [OBJECT], [OBJTYPE], [ITEM], [VALUE0], [SEASON],
                            DATEADD(MINUTE, ceiling(DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0), [DATA_DATE]) / 60.) * 60,
                            DATEADD(DAY, DATEDIFF(DAY, 0, '?DATADATESTART?'), 0))) res
                            GROUP BY[OBJECT], [ITEM], [DATETIME]
                            ORDER BY[OBJECT], [ITEM], [DATETIME]";

            if (tec.m_Sensors[(int)indx].Equals(string.Empty) == false)
            {
                strRes = strRes.Replace(@"?SENSORS?", tec.m_Sensors[(int)indx]);
                strRes = strRes.Replace(@"?DATADATESTART?", dtStart.ToString(@"yyyyMMdd"));
                strRes = strRes.Replace(@"?DATADATEEND?", dtEnd.ToString(@"yyyyMMdd"));
            }
            else
                strRes = string.Empty;

            return strRes;
        }

        public override void Stop ()
        {
            ClearValues ();

            base.Stop ();
        }

        public override void StartDbInterfaces()
        {
            throw new NotImplementedException();
        }

        public override void ClearValues()
        {
            m_listTEC.Clear ();
        }

        protected override int StateCheckResponse(int state, out bool error, out object outobj)
        {
            throw new NotImplementedException();
        }

        protected override int StateRequest(int state)
        {
            throw new NotImplementedException();
        }

        protected override int StateResponse(int state, object obj)
        {
            throw new NotImplementedException();
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }
    }
}
