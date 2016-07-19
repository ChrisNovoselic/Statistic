using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
//using MySql.Data.MySqlClient;

using HClassLibrary;

namespace StatisticCommon
{
    public class InitTECBase
    {
        //protected TYPE_DATABASE_CFG m_typeDB_CFG { get { return this is InitTEC_200 ? TYPE_DATABASE_CFG.CFG_200 : this is InitTEC_190 ? TYPE_DATABASE_CFG.CFG_190 : TYPE_DATABASE_CFG.UNKNOWN; } }
        
        public class ListTEC : List <TEC>
        {
            public ListTEC () : base ()
            {
            }

            private void update ()
            {
            }
        }

        #region Неиспользуемые конструкторы
        //protected InitTECBase ()
        //{
        //}

        //public InitTECBase(int iListenerId)
        //{
        //    int err = -1;
        //    m_connConfigDB = DbSources.Sources().GetConnection(iListenerId, out err);
        //}
        #endregion

        public ListTEC tec;
        protected /*static*/ DbConnection m_connConfigDB;

        /// <summary>
        /// Возвратить строку запроса для получения списка ТЭЦ
        /// </summary>
        /// <param name="bIgnoreTECInUse">Признак игнорирования поля [InUse] в таблице [TEC_LIST]</param>
        /// <param name="arIdLimits">Диапазон идентификаторов ТЭЦ</param>
        /// <returns>Строка запроса</returns>
        public static string getQueryListTEC(bool bIgnoreTECInUse, int[] arIdLimits)
        {
            string strRes = "SELECT * FROM TEC_LIST ";

            if (bIgnoreTECInUse == false)
                strRes += "WHERE INUSE=1 ";
            else
                ;

            if (bIgnoreTECInUse == true)
            // условие еще не добавлено - добавляем
                strRes += @"WHERE ";
            else
                if (bIgnoreTECInUse == false)
                // условие уже добавлено
                    strRes += @"AND ";
                else
                    ;

            if (!(HStatisticUsers.allTEC == 0))
            {
                strRes += @"ID=" + HStatisticUsers.allTEC.ToString();
            }
            else
                //??? ограничение (временное) для ЛК
                strRes += @"NOT ID<" + arIdLimits[0] + @" AND NOT ID>" + arIdLimits[1];

            return strRes;
        }
        /// <summary>
        /// Возвратить таблицу [TEC_LIST] из БД конфигурации
        /// </summary>
        /// <param name="connConfigDB">Ссылка на объект с установленным соединением с БД</param>
        /// <param name="bIgnoreTECInUse">Признак игнорирования поля [InUse] в таблице [TEC_LIST]</param>
        /// <param name="arIdLimits">Диапазон идентификаторов ТЭЦ</param>
        /// <param name="err">Идентификатор ошибки при выполнении запроса</param>
        /// <returns>Таблица - с данными</returns>
        public static DataTable getListTEC(ref DbConnection connConfigDB, bool bIgnoreTECInUse, int []arIdLimits, out int err)
        {
            string req = getQueryListTEC(bIgnoreTECInUse, arIdLimits);

            return DbTSQLInterface.Select(ref connConfigDB, req, null, null, out err);
        }

        protected DataTable getListTECComponent(string prefix, int id_tec, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString() + @" AND ID!=0", null, null, out err);
        }

        protected static DataTable getListTECComponent(ref DbConnection connConfigDB, string prefix, int id_tec, out int err)
        {
            return DbTSQLInterface.Select(ref connConfigDB, "SELECT * FROM " + prefix + "_LIST WHERE ID_TEC = " + id_tec.ToString() + @" AND ID!=0", null, null, out err);
        }

        protected DataTable getListTG(int id, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM TG_LIST WHERE ID_TEC" + " = " + id.ToString(), null, null, out err);
        }

        public static DataTable getConnSettingsOfIdSource(int idListener, int id_ext, int id_role, out int err)
        {
            DbConnection conn = DbSources.Sources().GetConnection(idListener, out err);
            return getConnSettingsOfIdSource(ref conn, id_ext, id_role, out err);
        }

        public static DataTable getConnSettingsOfIdSource(ref DbConnection conn, int id_ext, int id_role, out int err)
        {
            return ConnectionSettingsSource.GetConnectionSettings(ref conn, id_ext, id_role, out err);
        }

        protected DataTable getConnSettingsOfIdSource(int id_ext, int id_role, out int err)
        {
            return ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, id_ext, id_role, out err);
        }

        public DataTable getConnSettingsOfIdSource(int id)
        {
            int err = 0;

            return DbTSQLInterface.Select(ref m_connConfigDB, "SELECT * FROM SOURCE WHERE ID = " + id.ToString(), null, null, out err);
        }
    }

    public class InitTEC_200 : InitTECBase
    {
        // комментарий для тестирования подключения к redmine #1001
        // комментарий для тестирования подключения к redmine #1002
        // комментарий для тестирования подключения к redmine #1003
        /// <summary>
        /// Возвратить результат запроса "Параметры всех ТГ"
        /// </summary>
        /// <param name="ver">Версия набора параметров (0 - самая новая, 1 - предыдущая, и т.д.)</param>
        /// <param name="err">Признак наличия ошибки при выполнении</param>
        /// <returns>Объект таблицы с результатами запроса</returns>
        private DataTable getALL_PARAM_TG(int ver, out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (" + ver + @")", null, null, out err);
        }
        /// <summary>
        /// Возвратить результат запроса "Все параметры всех выводов"
        /// </summary>        
        /// <param name="err">Признак наличия ошибки при выполнении метода</param>
        /// <returns>Объект-таблица с результатами запроса</returns>
        private DataTable getALL_ParamVyvod(out int err)
        {
            return DbTSQLInterface.Select(ref m_connConfigDB
                , @"SELECT pnt.[ID_TEC], tl.NAME_SHR as TEC_NAME_SHR"
                        + @", pnt.[KKS_NAME], pnt.[ID_VYVOD]"
                        + @", vl.NAME_SHR as VYVOD_NAME_SHR, vl.KOM_UCHET"
                        + @", par.[ID], par.[NAME_SHR], par.[SYMBOL], par.TYPE_AGREGATE"
                    + @" FROM [dbo].[ID_POINT_ASKUTE] pnt"
                    + @" LEFT JOIN [dbo].[TEC_LIST] tl ON tl.ID = pnt.ID_TEC"
                    + @" LEFT JOIN [dbo].[ID_PARAM_ASKUTE] par ON par.ID = pnt.ID_PARAM"
                    + @" LEFT JOIN [dbo].[VYVOD_LIST] vl ON vl.ID = pnt.ID_VYVOD"
                , null, null, out err);
        }

        /// <summary>
        /// Список ВСЕХ компонентов (ТЭЦ, ГТП, ЩУ, ТГ)
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД концигурации</param>
        /// <param name="bIgnoreTECInUse">Признак использования поля [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">Массив-диапазон допустимых идентификаторов ТЭЦ</param>
        /// <param name="bUseData">Признак возможности обращения к данным компонентов собираемого списка</param>
        public InitTEC_200(int idListener, bool bIgnoreTECInUse, int [] arTECLimit, bool bUseData)
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - вход...");

            int err = -1;

            tec = new ListTEC ();
            //m_user = new Users(idListener);
            //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - получение объекта MySqlConnection...");

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            int indx = -1
                , indx_tec = -1;
            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы
            DataTable list_tec = null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , list_tg = null
                , all_PARAM_TG = null,  all_ParamVyvod = null; // семантика отличается т.к. ТГ не аналог "вывода". "Вывод" аналог ГТП(ЩУ), Параметр "вывода" аналог ТГ.

            all_PARAM_TG = getALL_PARAM_TG (0, out err);            

            if (err == 0) {
                //Получить список ТЭЦ, используя статическую функцию
                list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

                if (err == 0)
                {
                    for (int i = 0; i < list_tec.Rows.Count; i++)
                    {
                        //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - list_tec.Rows[i][\"ID\"] = " + list_tec.Rows[i]["ID"]);

                        if ((HStatisticUsers.allTEC == 0) || (HStatisticUsers.allTEC == Convert.ToInt32(list_tec.Rows[i]["ID"]))
                            /*|| (HStatisticUsers.RoleIsDisp == true)*/)
                        {
                            //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - tec.Count = " + tec.Count);

                            //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                            //Создание объекта ТЭЦ
                            tec.Add(new TEC(list_tec.Rows[i], bUseData));

                            indx_tec = tec.Count - 1;
                            EventTECListUpdate += tec[indx_tec].PerformUpdate;

                            indx = -1;
                            foreach (KeyValuePair<CONN_SETT_TYPE, string> pair in TEC.s_dictIdConfigDataSources)
                                if ((err == 0)
                                    && ((list_tec.Rows[i][pair.Value] is DBNull) == false))
                                    tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i][pair.Value]), -1, out err), (int)pair.Key);
                                else
                                    ; // break

                            if (err == 0)
                            {
                                //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - tec.Add () = Ok");

                                #region Добавить ТГ для ТЭЦ
                                list_tg = getListTG(Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                if (err == 0)
                                    for (int k = 0; k < list_tg.Rows.Count; k++)
                                    {
                                        tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], list_tg.Rows[k]));

                                        indx = tec[indx_tec].list_TECComponents.Count - 1;

                                        tec[indx_tec].list_TECComponents[indx].m_listTG.Add(new TG(list_tg.Rows[k], all_PARAM_TG.Select(@"ID_TG=" + tec[indx_tec].list_TECComponents[indx].m_id)[0]));
                                    }
                                else
                                    ; //Ошибка получения списка ТГ
                                #endregion

                                #region Добавить компоненты ТЭЦ (ГТП, ЩУ)
                                for (int c = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; !(c > (int)FormChangeMode.MODE_TECCOMPONENT.PC); c++)
                                {
                                    list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(c), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                    //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - list_TECComponents.Count = " + list_TECComponents.Rows.Count);

                                    if (err == 0)
                                        try
                                        {
                                            for (int j = 0; j < list_TECComponents.Rows.Count; j++)
                                            {
                                                //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - ...tec[indx_tec].list_TECComponents.Add(new TECComponent...");

                                                tec[indx_tec].list_TECComponents.Add(new TECComponent(tec[indx_tec], list_TECComponents.Rows[j]));

                                                indx = tec[indx_tec].list_TECComponents.Count - 1;

                                                if (err == 0)
                                                    tec[indx_tec].InitTG(indx, all_PARAM_TG.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + tec[indx_tec].list_TECComponents[indx].m_id));
                                                //InitTG(tec[indx_tec].list_TECComponents, indx, all_PARAM_TG.Select(@"ID_" + FormChangeMode.getPrefixMode(c) + @"=" + tec[indx_tec].list_TECComponents[indx].m_id));
                                                else
                                                    ; //Ошибка получения списка ТГ
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Logging.Logg().Exception(e, "InitTEC::InitTEC (3 параметра) - ...for (int j = 0; j < list_TECComponents.Rows.Count; j++)...", Logging.INDEX_MESSAGE.NOT_SET);
                                        }
                                    else
                                        ; //Ошибка при получении списка компонентов
                                }
                                #endregion
                            }
                            else
                                ; //Ошибка получения параметров соединений с БД
                            //} else ;

                            //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - list_TG = Ok");
                        }
                        else
                            ;
                    } // for i

                    #region Получить параметры выводов
                    all_ParamVyvod = getALL_ParamVyvod(out err);

                    if (err == 0)
                    {
                        tec.ForEach(t => t.InitParamVyvod(all_ParamVyvod.Select(@"ID_TEC=" + t.m_id)));                        
                    }
                    else
                        ; // ошибка получения параметров вывода
                    #endregion
                }
                else
                    ; //Ошибка получения списка ТЭЦ
            }
            else
                ; //Ошибка получения всех параметров для всех ТГ

            //DbTSQLInterface.CloseConnection(m_connConfigDB, out err);

            //Logging.Logg().Debug("InitTEC::InitTEC (3 параметра) - вЫход...");
        }
        /// <summary>
        /// Список компонентов (ТЭЦ, ГТП, ЩУ, ТГ) в ~ от индекса компонента
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД концигурации</param>
        /// <param name="indx">Индекс компонента - значение из перечисления 'FormChangeMode.MODE_TECCOMPONENT' ('0' или > '0' означает TEC(GTP, PC, TG), '-1' означает VYVOD)</param>
        /// <param name="bIgnoreTECInUse">Признак использования поля [TEC_LIST].[InUse]</param>
        /// <param name="arTECLimit">Массив-диапазон допустимых идентификаторов ТЭЦ</param>
        /// <param name="bUseData">Признак возможности обращения к данным компонентов собираемого списка</param>
        public InitTEC_200(int idListener, Int16 indx, bool bIgnoreTECInUse, int []arTECLimit, bool bUseData) //indx = {GTP или PC}
        {
            //Logging.Logg().Debug("InitTEC::InitTEC (4 параметра) - вход...");

            tec = new ListTEC ();

            int err = 0
                , id_comp = -1
                , indx_comp = -1;
            // подключиться к бд, инициализировать глобальные переменные, выбрать режим работы
            DataTable list_tec= null // = DbTSQLInterface.Select(connSett, "SELECT * FROM TEC_LIST"),
                , list_TECComponents = null
                , all_PARAM_DETAIL = null;

            //Logging.Logg().Debug("InitTEC::InitTEC (4 параметра) - получение объекта MySqlConnection...");
            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            //Использование статической функции
            list_tec = getListTEC(ref m_connConfigDB, bIgnoreTECInUse, arTECLimit, out err);

            if (!(indx < 0))
                all_PARAM_DETAIL = getALL_PARAM_TG (0, out err);
            else
                all_PARAM_DETAIL = getALL_ParamVyvod(out err);

            if (err == 0)
                for (int i = 0; i < list_tec.Rows.Count; i ++) {

                    //Logging.Logg().Debug("InitTEC::InitTEC (4 параметра) - Создание объекта ТЭЦ: " + i);

                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {

                        //Создание объекта ТЭЦ
                        tec.Add(new TEC(list_tec.Rows[i], bUseData));

                        EventTECListUpdate += tec[i].PerformUpdate;

                        foreach (KeyValuePair<CONN_SETT_TYPE, string> pair in TEC.s_dictIdConfigDataSources)
                            if ((err == 0)
                                && ((list_tec.Rows[i][pair.Value] is DBNull) == false))
                                tec[i].connSettings(ConnectionSettingsSource.GetConnectionSettings(ref m_connConfigDB, Convert.ToInt32(list_tec.Rows[i][pair.Value]), -1, out err), (int)pair.Key);
                            else
                                ; // break

                        if (err == 0)
                        // получить список компонентов, с учетом типа компонентов по 'indx'
                            if (!(indx < 0))
                            {// инициализация "обычных" компонентов ТЭЦ
                                list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                if (err == 0)
                                    for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                    {
                                        id_comp = Convert.ToInt32 (list_TECComponents.Rows[indx_comp][@"ID"]);
                                        tec[i].AddTECComponent(list_TECComponents.Rows[indx_comp]);
                                        tec[i].InitTG(indx_comp, all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(indx) + @"=" + id_comp));
                                    }
                                else
                                    ; //Ошибка ???
                            }
                            else
                            {// инициализация "необычных компонентов" - ВЫВОДов
                                list_TECComponents = getListTECComponent(FormChangeMode.getPrefixMode(indx), Convert.ToInt32(list_tec.Rows[i]["ID"]), out err);

                                if (err == 0)
                                    for (indx_comp = 0; indx_comp < list_TECComponents.Rows.Count; indx_comp++)
                                    {
                                        id_comp = Convert.ToInt32(list_TECComponents.Rows[indx_comp][@"ID"]);
                                        tec[i].AddVyvod(all_PARAM_DETAIL.Select(@"ID_" + FormChangeMode.getPrefixMode(indx) + @"=" + id_comp));
                                    }
                                else
                                    ; //Ошибка ???
                            }
                        else
                            ; // ошибка при получении идентификаторов источников данных
                    //} else ;
                }
            else
                ; //Ошибка получения списка ТЭЦ

            //Logging.Logg().Debug("InitTEC::InitTEC (4 параметра) - вЫход...");
        }

        /// <summary>
        /// Событие для инициирования обновления параметров ТЭЦ для всех созданных списков (List <TEC>)
        ///  при указании обработчика события для 'TEC.EventUpdate'
        /// </summary>
        public static event DelegateIntFunc EventTECListUpdate;

        public class TECListUpdateEventArgs : EventArgs
        {
            public int m_iListenerId;
        }

        public static void PerformTECListUpdate (int iListenerId)
        {
            if (! (EventTECListUpdate == null))
                EventTECListUpdate (iListenerId);
            else
                ;
        }

        public static void OnTECUpdate (object obj, EventArgs ev)
        {
            TEC tec = obj as TEC;
            int iListenerId = (ev as TECListUpdateEventArgs).m_iListenerId
                , err = -1;
            DataTable tableRes;
            DataRow []selRows;

            DbConnection connConfigDB = DbSources.Sources ().GetConnection (iListenerId, out err);

            tableRes = getListTEC(ref connConfigDB, true, new int[] { 0, (int)TECComponent.ID.GTP }, out err);
            //??? обновление параметров ТЭЦ (например: m_IdSOTIASSOLinkSourceTM)
            //tec.Update (tableRes);

            tableRes = getListTECComponent (ref connConfigDB, @"GTP", tec.m_id, out err);
            // обновление параметров ГТП
            if (tableRes.Columns.IndexOf("KoeffAlarmPcur") > 0)            
                // поиск ГТП
                foreach (TECComponent tc in tec.list_TECComponents)
                    if (tc.IsGTP == true)
                    {
                        selRows = tableRes.Select (@"ID=" + tc.m_id);
                        // проверить наличие значения
                        if ((selRows.Length == 1)
                            && (!(selRows[0]["KoeffAlarmPcur"] is System.DBNull)))
                            // обновить значение коэффициента
                            tc.m_dcKoeffAlarmPcur = Convert.ToInt32(selRows[0]["KoeffAlarmPcur"]);
                        else
                            ;
                    }
                    else
                        ;
            else
                ;
        }

        #region Дополнительные методы для реализации задачи Расчет теплосети
        //public InitTEC_200(List<TEC> listTec)
        //{
        //    tec = new ListTEC ();
        //    tec.AddRange(listTec);

        //    tec.ForEach(getVyvod);
        //}

        ////public InitTEC_200(ListTEC listTec) : this (listTec.)
        ////{
        ////}

        //private void getVyvod (TEC t)
        //{
        //}
        #endregion
    }
}
