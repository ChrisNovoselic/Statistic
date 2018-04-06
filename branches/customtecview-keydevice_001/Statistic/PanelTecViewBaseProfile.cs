using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Statistic
{
    partial class PanelTecViewBase
    {
        [Serializable]
        public class LabelCustomTecViewProfile
        {
            public FormChangeMode.KeyDevice Key;

            public LabelViewProperties Properties;

            private LabelCustomTecViewProfile ()
            {
            }

            public LabelCustomTecViewProfile (FormChangeMode.KeyDevice key, LabelViewProperties properties)
                : this ()
            {
                Key = key;

                Properties = properties;
            }
        }

        [Serializable]
        public class LabelViewProperties
        {
            public enum BANK_DEFAULT
            {
                DISABLED = 0
                , HOUR_TABLE_GRAPH
                , HOUR_TABLE_GRAPH_OPER
                , HOUR_TABLE_OPER
            }

            public enum VALUE
            {
                DISABLED = -1, OFF, ON
            }

            public enum INDEX_PROPERTIES_VIEW { UNKNOWN = -1, TABLE_MINS, TABLE_HOURS, GRAPH_MINS, GRAPH_HOURS, ORIENTATION, QUICK_PANEL, TABLE_AND_GRAPH, COUNT_PROPERTIES_VIEW };

            private List<VALUE> _values;

            private static Dictionary<BANK_DEFAULT, List<VALUE>> s_defaultValues = new Dictionary<BANK_DEFAULT, List<VALUE>> () {
                { BANK_DEFAULT.DISABLED // все отключено
                    , new List<VALUE>() { VALUE.DISABLED // TABLE_MINS
                        , VALUE.DISABLED // TABLE_HOURS
                        , VALUE.DISABLED // GRAPH_MINS
                        , VALUE.DISABLED // GRAPH_HOURS
                        , VALUE.DISABLED // ORIENTATION
                        , VALUE.DISABLED // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_GRAPH // отобразить часовые таблицу/гистограмму: 0, 1, 0, 1, 0, 0, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.ON // GRAPH_HOURS
                        , VALUE.OFF // ORIENTATION
                        , VALUE.ON // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_GRAPH_OPER // отобразить часовые таблицу/гистограмму/панель с оперативными данными: 0, 1, 0, 1, 0, 1, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.ON // GRAPH_HOURS
                        , VALUE.OFF // ORIENTATION
                        , VALUE.OFF // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_OPER // отобразить часовые таблицу/панель с оперативными данными: 0, 1, 0, 0, -1, 1, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.OFF // GRAPH_HOURS
                        , VALUE.DISABLED // ORIENTATION
                        , VALUE.ON // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
            };

            private LabelViewProperties (List<VALUE> values)
            {
                if (Equals (values, null) == true)
                    _values = new List<VALUE> (s_defaultValues[BANK_DEFAULT.DISABLED]);
                else
                    _values = new List<VALUE> (values);
            }

            public LabelViewProperties (BANK_DEFAULT nameBank)
                : this(s_defaultValues[nameBank])
            {
            }

            public LabelViewProperties (LabelViewProperties dest)
                : this (BANK_DEFAULT.DISABLED)
            {

            }

            public static string GetText (INDEX_PROPERTIES_VIEW indx)
            {
                string [] array = { @"Таблица(мин)", @"Таблица(час)", @"График(мин)", @"График(час)", @"Ориентация", @"Оперативные значения", @"Таблица+Гистограмма" };

                return array [(int)indx];
            }

            public VALUE GetValue (INDEX_PROPERTIES_VIEW indx)
            {
                return _values [(int)indx];
            }

            public void SetValue (INDEX_PROPERTIES_VIEW indx, VALUE value)
            {
                _values [(int)indx] = value;
            }

            public void CopyTo (LabelViewProperties dest)
            {

            }

            public int Length
            {
                get
                {
                    return (int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW;
                }
            }

            public bool IsOn(INDEX_PROPERTIES_VIEW indx)
            {
                return _values [(int)indx] == VALUE.ON;
            }

            public bool IsOff (INDEX_PROPERTIES_VIEW indx)
            {
                return _values [(int)indx] == VALUE.OFF;
            }

            public int CountOn
            {
                get
                {
                    return _values.Count (v => v == VALUE.ON);
                }
            }

            public int GetCountOn(INDEX_PROPERTIES_VIEW start, INDEX_PROPERTIES_VIEW end)
            {
                int iRes = 0;

                for(INDEX_PROPERTIES_VIEW indx = start; !(indx > end); indx ++)
                    iRes += _values[(int)indx] == VALUE.ON ? 1 : 0;

                return iRes;
            }

            public int [] ToArray ()
            {
                return _values.Select (v => (int)v).ToArray ();
            }
        }
    }
}
