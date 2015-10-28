using System;
using System.Collections.Generic;
using System.Text;

namespace StatisticCommon
{
    /// <summary>
    /// Класс для описания основного компонента ТЭЦ
    /// </summary>
    public class TECComponentBase
    {
        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID : int { GTP = 100, PC = 500, TG = 1000, MAX = 10000 }
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
            return ((id > (int)ID.GTP) && (id < (int)ID.PC)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > (int)ID.PC) && (id < (int)ID.TG)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                    ((id > (int)ID.TG) && (id < (int)ID.MAX)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                        FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;
        }
        /// <summary>
        /// Признак принадлежности компонента к группе щиты управления
        ///  (блочные, групповые)
        /// </summary>
        public bool IsPC { get { return (m_id > (int)ID.PC) && (m_id < (int)ID.TG); } }
        /// <summary>
        /// Признак принадлежности компонента к группе ТГ
        /// </summary>
        public bool IsTG { get { return (m_id > (int)ID.TG) && (m_id < (int)ID.MAX); } }
    }
    /// <summary>
    /// Класс для описания компонента ТЭЦ - ТГ
    /// </summary>
    public class TG : TECComponentBase
    {
        /// <summary>
        /// Перечисление - идентификаторы периодов времени
        /// </summary>
        public enum ID_TIME : int { UNKNOWN = -1, MINUTES, HOURS, COUNT_ID_TIME };
        /// <summary>
        /// Перечисление - индексы элементов интерфейса для отображения значений ТГ
        /// </summary>
        public enum INDEX_VALUE : int { FACT //факт.
                                        , TM //телемеханика
                                        , LABEL_DESC //описание (краткое наименование) ТГ
                                        , COUNT_INDEX_VALUE }; //Количество индексов
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
        public int m_id_owner_gtp,
                    m_id_owner_pc;
        /// <summary>
        /// Признак состояния ТГ
        /// </summary>
        public INDEX_TURNOnOff m_TurnOnOff; //Состояние -1 - выкл., 0 - неизвестно, 1 - вкл. (только для AdminAlarm)
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public TG()
        {
            m_arIds_fact = new int[(int)ID_TIME.COUNT_ID_TIME];

            m_id_owner_gtp =
            m_id_owner_pc =
                //Неизвестный владелец
                -1;
            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //Неизвестное состояние
        }
    }
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
        public List<TG> m_listTG;
        /// <summary>
        /// Объект ТЭЦ - "владелец" компонента
        /// </summary>
        public TEC tec;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public TECComponent(TEC tec)
        {
            this.tec = tec;

            m_listTG = new List<TG>();
            m_listMCentreId =
            m_listMTermId = null;
        }
    }
}
