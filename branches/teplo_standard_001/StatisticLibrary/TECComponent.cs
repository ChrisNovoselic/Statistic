using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace StatisticCommon
{
    /// <summary>
    /// Класс для описания основного компонента ТЭЦ
    /// </summary>
    public class TECComponentBase
    {
        /// <summary>
        /// Перечисление - типы компонентов в отношении его принадлежности к электро-, или тепловой части оборудования ТЭЦ
        /// </summary>
        public enum TYPE : short { UNKNOWN = -1, TEPLO, ELECTRO, COUNT }
        /// <summary>
        /// Тип компонента в отношении его принадлежности к электро-, или тепловой части оборудования ТЭЦ
        /// </summary>
        public TYPE Type {
            get {
                TYPE typeRes = TYPE.UNKNOWN;

                if ((IsVyvod == true)
                    || (IsParamVyvod == true))
                    typeRes = TYPE.TEPLO;
                else
                    if ((IsGTP == true)
                        || (IsPC == true)
                        || (IsTG == true))
                        typeRes = TYPE.ELECTRO;
                    else
                        ;

                return typeRes;
            }
        }
        /// <summary>
        /// Признак оборудования нижнего уровня
        /// </summary>
        public bool IsLowPointDev {
            get {
                bool bRes = false;

                if ((IsTG == true)
                    || (IsParamVyvod))
                    bRes = true;
                else
                    ;

                return bRes;
            }
        }
        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID : int { LK = 10, GTP = 100, GTP_LK = 200, PC = 500, VYVOD = 600, TG = 1000, PARAM_VYVOD = 2000, MAX = 10000 }
        /// <summary>
        /// Краткое наименовнаие компонента
        /// </summary>
        public string name_shr;
        /// <summary>
        /// Нименование для использования в будущем (при расширении)
        /// </summary>
        public string name_future;
        /// <summary>
        /// Идентификатор компонента (из БД конфигурации)
        /// </summary>
        public int m_id;
        /// <summary>
        /// Индекс столбца в книге Excel со значениями РДГ для компонента (в настоящее время только для НТЭЦ-5)
        /// </summary>
        public int m_indx_col_rdg_excel;
        /// <summary>
        /// Коэффициент для тонкой настройки алгоритма сигнализации
        /// </summary>
        public decimal m_dcKoeffAlarmPcur;

        //Копия из 'class PanelStatisticView : PanelStatistic' - из 'PanelStatisticView' класса требуется исключть???
        public volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //Только для особенной ТЭЦ (Бийск) - 3-х, 30-ти мин идентификаторы
        public volatile string m_SensorsString_SOTIASSO = string.Empty;
        public volatile string m_SensorsString_VZLET = string.Empty;
        /// <summary>
        /// Коструктор - основной (без параметров)
        /// </summary>
        public TECComponentBase()
        {
            m_dcKoeffAlarmPcur = -1;
        }
        /// <summary>
        /// Признак принадлежности компонента к группе ГТП
        /// </summary>
        public bool IsGTP { get { return (m_id > (int)ID.GTP) && (m_id < (int)ID.PC); } }
        /// <summary>
        /// Возвратить тип (режим) компонента по указанному идентификатору
        /// </summary>
        /// <param name="id">Идентификатор компонента</param>
        /// <returns>Тип (режим) компонента</returns>
        public static FormChangeMode.MODE_TECCOMPONENT Mode(int id)
        {
            return (id < (int)ID.GTP) == true ? FormChangeMode.MODE_TECCOMPONENT.TEC :
                ((id > (int)ID.GTP) && (id < (int)ID.PC)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > (int)ID.PC) && (id < (int)ID.TG)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                    ((id > (int)ID.TG) && (id < (int)ID.MAX)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                        FormChangeMode.MODE_TECCOMPONENT.ANY;
        }
        /// <summary>
        /// Признак принадлежности компонента к группе щиты управления
        ///  (блочные, групповые)
        /// </summary>
        public bool IsPC { get { return (m_id > (int)ID.PC) && (m_id < (int)ID.VYVOD); } }
        /// <summary>
        /// Признак принадлежности компонента к группе ТГ
        /// </summary>
        public bool IsTG { get { return (m_id > (int)ID.TG) && (m_id < (int)ID.PARAM_VYVOD); } }

        public bool IsVyvod { get { return (m_id > (int)ID.VYVOD) && (m_id < (int)ID.TG); } }
        
        public bool IsParamVyvod { get { return (m_id > (int)ID.PARAM_VYVOD) && (m_id < (int)ID.MAX); } }
    }

    //public partial class TEC {
        /// <summary>
        /// Класс для описания компонента ТЭЦ - ТГ
        /// </summary>
        public class TG : TECComponentBase
        {
            /// <summary>
            /// Перечисление - индексы элементов интерфейса для отображения значений ТГ
            /// </summary>
            public enum INDEX_VALUE : int
            {
                FACT //факт.
                , TM //телемеханика
                , LABEL_DESC //описание (краткое наименование) ТГ
                    , COUNT_INDEX_VALUE
            }; //Количество индексов
            /// <summary>
            /// Перечисление - возможные состояния ТГ
            /// </summary>
            public enum INDEX_TURNOnOff : int { OFF = -1, UNKNOWN, ON };
            /// <summary>
            /// Массив идентификаторов ТГ в АИИС КУЭ (размерность по 'ID_TIME')
            ///  для особенной ТЭЦ (Бийск) различаются 3-х и 30-ти мин идентификаторы
            ///  для остальных - совпадают
            /// </summary>
            public int[] m_arIds_fact;
            /// <summary>
            /// Строковый идентификатор в СОТИАССО
            /// </summary>
            public string m_strKKS_NAME_TM;
            /// <summary>
            /// Идентификаторы "владельцев" для ТГ (ГТП, Б(Гр)ЩУ)
            /// </summary>
            public int m_id_owner_gtp
                , m_id_owner_pc;
            /// <summary>
            /// Признак состояния ТГ
            /// </summary>
            public INDEX_TURNOnOff m_TurnOnOff; //Состояние -1 - выкл., 0 - неизвестно, 1 - вкл. (только для AdminAlarm)
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TG()
            {
                m_arIds_fact = new int[(int)HDateTime.INTERVAL.COUNT_ID_TIME];

                m_id_owner_gtp =
                m_id_owner_pc =
                    //Неизвестный владелец
                    -1;
                m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //Неизвестное состояние
            }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public TG(DataRow row_tg, DataRow row_param_tg)
                : this()
            {
                initTG(row_tg, row_param_tg);
            }

            //public void InitTG(DataRow row_tg, DataRow row_param_tg, out int err)
            //{
            //    err = -1;

            //    name_shr = row_tg["NAME_SHR"].ToString();
            //    m_id = Convert.ToInt32(row_tg["ID"]);
            //    m_id_owner_gtp = Convert.ToInt32(row_tg["ID_GTP"]);

            //    //DataRow[] rows_tg = allParamTG.Select(@"ID_TG=" + dest.m_id);
            //    //dest.m_strKKS_NAME_TM = rows_tg[0][@"KKS_NAME"].ToString();
            //    //dest.m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_3"].ToString());
            //    //dest.m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_30"].ToString());
            //}

            private void initTG(DataRow row_tg, DataRow row_param_tg)
            {
                name_shr = row_tg["NAME_SHR"].ToString();
                if (DbTSQLInterface.IsNameField(row_tg, "NAME_FUTURE") == true) name_future = row_tg["NAME_FUTURE"].ToString(); else ;
                m_id = Convert.ToInt32(row_tg["ID"]);
                if (!(row_tg["INDX_COL_RDG_EXCEL"] is System.DBNull))
                    m_indx_col_rdg_excel = Convert.ToInt32(row_tg["INDX_COL_RDG_EXCEL"]);
                else
                    ;

                m_strKKS_NAME_TM = row_param_tg[@"KKS_NAME"].ToString();
                m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] = Int32.Parse(row_param_tg[@"ID_IN_ASKUE_3"].ToString());
                m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] = Int32.Parse(row_param_tg[@"ID_IN_ASKUE_30"].ToString());
            }
        }
    //} partial class TEC
    /// <summary>
    /// Класс для описания компонента ТЭЦ (ГТП, Б(Гр)ЩУ)
    /// </summary>
    public class TECComponent : TECComponentBase
    {
        /// <summary>
        /// Список идентификаторов в Модес-Центр
        /// </summary>
        public List<int> m_listMCentreId;
        /// <summary>
        /// Список идентификаторов в Модес-Терминале
        /// </summary>
        public List<int> m_listMTermId;
        /// <summary>
        /// Список ТГ
        /// </summary>
        public List<TECComponentBase> m_listLowPointDev;
        /// <summary>
        /// Объект ТЭЦ - "владелец" компонента
        /// </summary>
        public TEC tec;
        /// <summary>
        /// Конструктор - дополнительный
        /// </summary>
        public TECComponent(TEC tec, DataRow rComp) : this (tec)
        {
            name_shr = rComp["NAME_SHR"].ToString(); //rComp["NAME_GNOVOS"]
            if (DbTSQLInterface.IsNameField(rComp, "NAME_FUTURE") == true) this.name_future = rComp["NAME_FUTURE"].ToString(); else ;
            m_id = Convert.ToInt32(rComp["ID"]);
            m_listMCentreId = getMCentreId(rComp);
            m_listMTermId = getMTermId(rComp);
            if ((DbTSQLInterface.IsNameField (rComp, "INDX_COL_RDG_EXCEL") == true) && (!(rComp["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                m_indx_col_rdg_excel = Convert.ToInt32(rComp["INDX_COL_RDG_EXCEL"]);
            else
                ;
            if ((DbTSQLInterface.IsNameField (rComp, "KoeffAlarmPcur") == true) && (!(rComp["KoeffAlarmPcur"] is System.DBNull)))
                m_dcKoeffAlarmPcur = Convert.ToInt32(rComp["KoeffAlarmPcur"]);
            else
                ;
        }
        /// <summary>
        /// Конструктор - дополнительный
        /// </summary>
        private TECComponent(TEC tec)
        {
            this.tec = tec;

            m_listLowPointDev = new List<TECComponentBase>();
            m_listMCentreId =
            m_listMTermId = null;
        }

        protected List<int> getModesId(DataRow r, string col)
        {
            int i = -1
                , id = -1;
            List<int> listRes = new List<int>();

            if ((DbTSQLInterface.IsNameField (r, col) == true) && (!(r[col] is DBNull)))
            {
                string[] ids = r[col].ToString().Split(',');
                for (i = 0; i < ids.Length; i++)
                    if (ids[i].Length > 0)
                        if (Int32.TryParse(ids[i], out id) == true)
                            listRes.Add(id);
                        else
                            listRes.Add(-1);
                    else
                        listRes.Add(-1);
            }
            else
                ;

            return listRes;
        }

        protected List<int> getMCentreId(DataRow r)
        {
            return getModesId(r, @"ID_MC");
        }

        protected List<int> getMTermId(DataRow r)
        {
            return getModesId(r, @"ID_MT");
        }
    }

    public class Vyvod : TECComponent
    {
        public enum ID_PARAM : short { UNKNOWN = -1
            , G_PV = 1, G_OV
            , T_PV, T_OV
            , P_PV, P_OV
            , T2_PV, T2_OV
            , }
        
        public class ParamVyvod : TECComponentBase
        {
            public enum INDEX_VALUE : short
            {
                FACT //факт.
                , DEVIAT //телемеханика
                , LABEL_DESC //описание (краткое наименование) ВЫВОДА
                    , COUNT
            }; //Количество индексов

            public int m_owner_vyvod;

            public ID_PARAM m_id_param;
            public string m_Symbol;
            public int m_typeAgregate;

            public ParamVyvod(DataRow r)
            {
                m_id = Convert.ToInt32(r[@"ID"]);

                Initialize(r);
            }

            public void Initialize(DataRow r)
            {
                int iVzletGrafa = -1;

                m_owner_vyvod = Convert.ToInt32(r[@"ID_VYVOD"]);

                m_id_param = (ID_PARAM)Convert.ToInt16(r[@"ID_PARAM"]);
                iVzletGrafa = (int)r[@"VZLET_GRAFA"];
                m_SensorsString_VZLET =
                    //((string)r[@"KKS_NAME"]).Trim()
                    iVzletGrafa > 0 ? @"Графа_" + iVzletGrafa.ToString() : string.Empty
                    ;

                name_shr = ((string)r[@"NAME_SHR"]).Trim();
                m_Symbol = ((string)r[@"SYMBOL"]).Trim();
                m_typeAgregate = Convert.ToByte(r[@"TYPE_AGREGATE"]);
            }
        }
        ///// <summary>
        ///// Список ТГ
        ///// </summary>
        //public List<ParamVyvod> m_listParam;
        ///// <summary>
        ///// Объект ТЭЦ - "владелец" компонента
        ///// </summary>
        //public TEC tec;

        public bool m_bKomUchet;

        public Vyvod()
            : this(null, new DataRow[] {  })
        {
        }

        public Vyvod(TEC tec, DataRow row_param)
            : this(tec, new DataRow[] { row_param })
        {
        }
        /// <summary>
        /// Конструктор - основной (c параметрами)
        /// </summary>
        /// <param name="tec">Объект-ТЭЦ - родительский по отношению к создаваемому объекту</param>
        /// <param name="r">Строка со значениями свойств создаваемого объекта-ВЫВОДа</param>
        public Vyvod(TEC tec, DataRow []rows_param) : base (tec, rows_param[0])
        {
            //this.tec = tec;

            //m_listParam = new List<ParamVyvod>();

            Initialize(rows_param);
        }
        /// <summary>
        /// Инициализировать вывод значениями свойств всех параметров (по аналогии с ГТП добавить все ТГ)
        /// </summary>
        /// <param name="r">Строки таблицы со значенями свойств всех параметров</param>
        public void Initialize (DataRow []rows_param)
        {
            if (rows_param.Length > 0)
            {
                m_id = Convert.ToInt32(rows_param[0][@"ID_VYVOD"]);
                name_shr = ((string)rows_param[0][@"VYVOD_NAME_SHR"]).Trim ();
                m_bKomUchet = Convert.ToByte(rows_param[0][@"KOM_UCHET"]) == 1 ? true : false;

                foreach (DataRow r in rows_param)
                    InitParam(r);
            }
            else
                Logging.Logg().Error(@"Vyvod::Initialize () - нет ни одной строки со значенями свойств параметров ВЫВОДа...", Logging.INDEX_MESSAGE.NOT_SET);
                ; //??? исключение
        }
        /// <summary>
        /// Игициализировать значения свойства параметра
        /// </summary>
        /// <param name="r">Строка объекта-таблицы со значенями свойств параметра</param>
        /// <returns>Признак результата инициализации</returns>
        public int InitParam(DataRow r)
        {
            int iRes = 0; // ошибрк нет - параметр добавлен

            ParamVyvod pv = null;
            int iIdParam = -1;

            iIdParam = Convert.ToInt32(r[@"ID"]);
            pv =
                //m_listParam.Find(p => { return p.m_id == iIdParam; })
                m_listLowPointDev.Find(p => { return p.m_id == iIdParam; }) as ParamVyvod
                ;

            if (pv == null)
            {
                pv = new ParamVyvod(r);
                //m_listParam.Add(pv);
                m_listLowPointDev.Add(pv);
            }
            else
            {
                iRes = 1;
                // такой параметр уже существует
                // инициализация новыми значенями свойств (кроме ID)
                pv.Initialize(r);
            }

            return iRes;
        }
    }
}
