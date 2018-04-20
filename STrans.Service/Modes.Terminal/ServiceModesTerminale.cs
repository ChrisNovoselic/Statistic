﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using STrans.Service.ModesCentre;
using StatisticCommon;
using StatisticTrans;
using ASUTP;
using StatisticTransModes;
using ASUTP.Database;
using StatisticTrans.Contract.ModesCentre;
using StatisticTrans.Contract;
using ASUTP.Core;
using System.Threading.Tasks;

namespace STrans.Service.ModesTerminale
{
    [ServiceBehavior (ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, UseSynchronizationContext =true)]
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "ServiceServiceModesTerminale" в коде и файле конфигурации.
    public class ServiceModesTerminale : ServiceTransModes, StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale
    {
        /// <summary>
        /// Создать объект опроса значений источника
        /// </summary>
        /// <param name="connSett">Объект с параметрами соединения</param>
        /// <param name="iMainSourceData">Идентификатор базового(основного) источника данных - сервера ИС Статистика</param>
        /// <param name="modeMashine">Режим работы службы</param>
        /// <param name="tsFetchWaking">Интервал "до пробуждения"</param>
        /// <param name="modeTECComponent">Режим списка компонентов ТЭЦ</param>
        /// <param name="listID_TECNotUse">Список идентификаторов ТЭЦ, не использующихся при операциях сохранения</param>
        /// <returns>Результат(успех) создания объекта</returns>
        public bool Initialize (ASUTP.Database.ConnectionSettings connSett
            , int iMainSourceData
            , MODE_MASHINE modeMashine
            , TimeSpan tsFetchWaking
            , FormChangeMode.MODE_TECCOMPONENT modeTECComponent
            , List<int> listID_TECNotUse)
        {
            bool bRes = false;

            StatisticTrans.CONN_SETT_TYPE indx = StatisticTrans.CONN_SETT_TYPE.SOURCE;
            int i = -1;
            bool bIgnoreTECInUse = false;

            bRes = preInitialize (connSett, iMainSourceData);

            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int [] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR });

            try {
                m_arAdmin [(Int16)indx] = new AdminMT (); // tsFetchWaking
                m_arAdmin [(Int16)indx].InitTEC (modeTECComponent, markQueries, bIgnoreTECInUse, new int [] { 0, (int)TECComponent.ID.LK });
                removeTEC (m_arAdmin [(Int16)indx], listID_TECNotUse);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, "FormMainTransMC::FormMainTransMC ()", Logging.INDEX_MESSAGE.NOT_SET);

                ////ErrorReport("Ошибка соединения. Переход в ожидание.");
                ////setUIControlConnectionSettings(i);
            }

            return postInitialize(modeTECComponent, listID_TECNotUse);
        }
    }
}
