﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO; //Path
using System.Data; //DataTable
using System.Data.Common; //DbConnection

using HClassLibrary;
//using StatisticCommon;

namespace biysktmora
{
    public class HBiyskTMOra : HStates
    {
        protected struct SIGNAL
        {
            public int id;
            public string table;
            public SIGNAL (int id, string table)
            {
                this.id = id;
                this.table = table;
            }
        }
        protected SIGNAL [] arSignals;

        enum StatesMachine {
            CurrentTimeSource
            , CurrentTimeDest
            , SourceValues
            , DestValues
        }

        private DateTime dtStart;
        public DataTable results;
        public string host = @"10.220.2.5", port = @"1521"
            , dataSource = @"ORCL"
            , uid = @"arch_viewer"
            , pswd = @"1"
            , query = string.Empty;

        public HBiyskTMOra()
        {
            dtStart = DateTime.Now;

            //Инициализировать массив сигналов
            arSignals = new SIGNAL[] { new SIGNAL(20001, @"TAG_000046")
                                        , new SIGNAL(20002, @"TAG_000047")
                                        , new SIGNAL(20003, @"TAG_000048")
                                        , new SIGNAL(20004, @"TAG_000049")
                                        , new SIGNAL(20005, @"TAG_000050")
                                        , new SIGNAL(20006, @"TAG_000051")
                                        , new SIGNAL(20007, @"TAG_000052")
                                        , new SIGNAL(20008, @"TAG_000053")
                                    };

            setQuery(DateTime.Now.AddMinutes(-1));
        }

        public void UpdateQuery ()
        {
            UpdateQuery(DateTime.Now.AddSeconds(-66));
        }

        public void UpdateQuery(DateTime dtStart, int secInterval = 66)
        {            
            setQuery (dtStart, secInterval);
        }

        private void setQuery(DateTime dtStart, int secInterval = 66)
        {
            query = string.Empty;
            int iPrev = 0, iDel = 0, iCur = 0;

            if (! (results == null))
            {
                iPrev = results.Rows.Count;
                DataRow[] rowsDel = results.Select(@"DATETIME<'" + dtStart.ToString(@"yyyy/MM/dd HH:mm:ss") + @"'");

                iDel = rowsDel.Length;
                if (rowsDel.Length > 0)
                {
                    foreach (DataRow r in rowsDel)
                        results.Rows.Remove (r);

                    results.AcceptChanges ();
                }
                else
                    ;

                iCur = results.Rows.Count;
            }
            else
                ;

            Console.WriteLine(@"Обнавление запроса: [было=" + iPrev + @", удалено=" + iDel + @", осталось=" + iCur + @"]");

            string strUnion = @" UNION "
                //Строки для условия "по дате/времени"
                , strStart = dtStart.ToString(@"yyyyMMdd HHmmss")
                , strEnd = dtStart.AddSeconds(secInterval).ToString(@"yyyyMMdd HHmmss");
            //Формировать зпрос
            foreach (SIGNAL s in arSignals)
            {
                query += @"SELECT " + s.id + @" as ID, VALUE, QUALITY, DATETIME FROM ARCH_SIGNALS." + s.table + @" WHERE DATETIME BETWEEN"
                + @" to_timestamp('" + strStart + @"', 'yyyymmdd hh24miss')" + @" AND"
                + @" to_timestamp('" + strEnd + @"', 'yyyymmdd hh24miss')"
                + strUnion
                ;
            }

            //Удалить "лишний" UNION
            query = query.Substring(0, query.Length - strUnion.Length);
            ////Установить сортировку
            //query += @" ORDER BY DATETIME DESC";
        }

        public void ChangeState ()
        {
            ClearStates ();

            states.Add((int)StatesMachine.CurrentTimeSource);
            states.Add((int)StatesMachine.SourceValues);

            try
            {
                semaState.Release(1);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"HBiyskTMOra::ChangeState () - semaState.Release(1)...");
            }
        }

        public override void StartDbInterfaces ()
        {
            m_dictIdListeners.Add (0, new int [] { 0 });
            register (0, new ConnectionSettings(@"OraSOTIASSO-ORD", host, Int32.Parse(port), dataSource, uid, pswd), @"", 0);
        }

        public override void Start()
        {
            StartDbInterfaces ();

            base.Start();
        }

        public override void ClearValues()
        {
        }

        protected override int StateCheckResponse(int state, out bool error, out DataTable table)
        {
            return Response(out error, out table);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTimeSource:
                    GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.Oracle, m_dictIdListeners[0][0]);
                    break;
                case (int)StatesMachine.CurrentTimeDest:                    
                    break;
                case (int)StatesMachine.SourceValues:
                    UpdateQuery ();
                    Request (m_dictIdListeners[0][0], query);
                    break;
                case (int)StatesMachine.DestValues:
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int state, DataTable table)
        {
            int iRes = 0;

            switch (state)
            {
                case (int)StatesMachine.CurrentTimeSource:
                    Console.WriteLine(((DateTime)table.Rows[0][0]).ToString(@"dd.MM.yyyy HH:mm:ss.fff"));
                    break;
                case (int)StatesMachine.SourceValues:
                    Console.WriteLine(@"Получено строк: " + table.Rows.Count);
                    if (results == null)
                    {
                        results = new DataTable ();
                    }
                    else
                        ;

                    if (results.Rows.Count == 0)
                    {
                        results = table.Copy ();
                    }
                    else
                        ;

                    int iPrev = -1, iAdd = -1, iCur = -1;
                    iPrev = 0; iAdd = 0; iCur = 0;
                    iPrev = results.Rows.Count;

                    DataRow [] arDupl;
                    foreach (DataRow rRes in results.Rows)
                    {
                        arDupl = table.Select(@"ID=" + rRes[@"ID"] + @" AND " + @"DATETIME='" + ((DateTime)rRes[@"DATETIME"]).ToString(@"yyyy/MM/dd HH:mm:ss.fff") + @"'");
                        foreach (DataRow rDel in arDupl)
                            table.Rows.Remove (rDel);
                    }

                    iAdd = table.Rows.Count;
                    results.Merge(table);
                    iCur = results.Rows.Count;
                    Console.WriteLine(@"Объединение таблицы-рез-та: [было=" + iPrev + @", добавлено=" + iAdd + @", стало=" + iCur + @"]");
                    DataTable tableChanged = results.GetChanges();
                    if (! (tableChanged == null))
                        Console.WriteLine(@"Изменено строк: " + tableChanged.Rows.Count);
                    else
                        Console.WriteLine(@"Изменено строк: " + 0);                    
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override void StateErrors(int state, bool response)
        {
            string unknownErr = @"Неизвестная ошибка"
                , msgErr = unknownErr;

            switch (state)
            {
                case (int)StatesMachine.CurrentTimeSource: //Ошибка получения даты/времени сервера-источника
                    msgErr = @"получения даты/времени сервера-источника";
                    break;
                default:
                    break;
            }

            if (msgErr.Equals (unknownErr) == false)
                msgErr = @"Ошибка " + msgErr;
            else
                ;

            Console.WriteLine(msgErr);
        }

        protected override void StateWarnings(int state, bool response)
        {
            switch (state)
            {
                case (int)StatesMachine.CurrentTimeSource: //Предупреждение при получении даты/времени сервера-источника
                    break;
                default:
                    break;
            }
        }
    }
}
