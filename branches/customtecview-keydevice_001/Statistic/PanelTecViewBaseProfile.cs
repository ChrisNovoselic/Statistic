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

            /// <summary>
            /// Значение признака ориентации размещения таблиц, графиков
            /// </summary>
            private PanelTecViewBase.LabelViewProperties.VALUE m_prevOrientation;

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

                m_prevOrientation = GetValue (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION);
            }

            public LabelViewProperties ()
                : this(s_defaultValues [BANK_DEFAULT.HOUR_TABLE_GRAPH_OPER])
            {
            }

            public LabelViewProperties (BANK_DEFAULT nameBank)
                : this(s_defaultValues[nameBank])
            {
            }

            public LabelViewProperties (LabelViewProperties dest)
                : this (dest._values)
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

            /// <summary>
            /// Установить новое значение для свойства
            /// </summary>
            /// <param name="indx">Индекс свойства</param>
            /// <param name="newVal">Новое значение свойства</param>
            public void SetProperty (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW indx, PanelTecViewBase.LabelViewProperties.VALUE newVal)
            {
                SetValue (indx, newVal);

                int cnt = 0;
                PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW i = PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                // сколько таблиц/гистограмм отображается
                cnt = GetCountOn (0, PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS);

                if (cnt > 1) {
                    if (cnt > 2) {
                        //if (cnt > 3) {
                        if (indx < PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS)
                            //3-й установленный признак - таблица: снять с отображения графики
                            SetGraphOff ();
                        else
                            //3-й установленный признак - график: снять с отображения таблицы
                            SetTableOff ();

                        cnt -= 2;
                        //} else ;
                    } else
                        ;

                    if (cnt > 1)
                        if (IsOrientationDisabled == true)
                            if (PreviousOrientation == PanelTecViewBase.LabelViewProperties.VALUE.DISABLED)
                                //Вертикально - по умолчанию
                                SetValue (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION
                                    , PanelTecViewBase.LabelViewProperties.VALUE.OFF);
                            else
                                OrientationRecovery ();
                        else
                            ; //Оставить "как есть"
                    else
                        OrientationDisabled ();
                } else {
                    OrientationDisabled ();
                }
            }

            public void SetTableOff ()
            {
                SetValue (INDEX_PROPERTIES_VIEW.TABLE_MINS, PanelTecViewBase.LabelViewProperties.VALUE.OFF); //Снять с отображения
                SetValue (INDEX_PROPERTIES_VIEW.TABLE_HOURS, PanelTecViewBase.LabelViewProperties.VALUE.OFF); //Снять с отображения
            }

            public void SetGraphOff ()
            {
                SetValue (INDEX_PROPERTIES_VIEW.GRAPH_MINS, PanelTecViewBase.LabelViewProperties.VALUE.OFF); //Снять с отображения
                SetValue (INDEX_PROPERTIES_VIEW.GRAPH_HOURS, PanelTecViewBase.LabelViewProperties.VALUE.OFF); //Снять с отображения
            }

            public bool IsOrientationDisabled
            {
                get
                {
                    return _values [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] == VALUE.DISABLED;
                }
            }

            public VALUE PreviousOrientation
            {
                get
                {
                    return m_prevOrientation;
                }
            }

            /// <summary>
            /// Блокировать возможность выбора "ориентация сплиттера"
            /// </summary>
            public void OrientationDisabled ()
            {
                //Запомнить предыдущее стостояние "ориентация сплиттера"
                m_prevOrientation = _values [(int)INDEX_PROPERTIES_VIEW.ORIENTATION];
                //Блокировать возможность выбора "ориентация сплиттера"
                _values [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = VALUE.DISABLED;
            }

            /// <summary>
            /// Восстановить значение "ориентация сплиттера"
            /// </summary>
            public void OrientationRecovery ()
            {
                _values [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = m_prevOrientation;
                
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
