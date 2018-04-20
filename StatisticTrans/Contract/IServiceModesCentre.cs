﻿using ASUTP.Core;
using ASUTP.Database;
using StatisticCommon;
using StatisticTrans;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StatisticTrans.Contract
{
    public class Default
    {
        public static string FetchWaking = "00:47:48";
    }

    namespace ModesCentre
    {
        // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
        [ServiceContract (SessionMode = SessionMode.Required, CallbackContract = typeof (IServiceCallback))]
        public interface IServiceModesCentre : IServiceTransModes
        {
            [OperationContract]
            bool Initialize (ASUTP.Database.ConnectionSettings connSett
                , int iMainSourceData
                , string mcServiceHost
                , MODE_MASHINE modeMashine
                , TimeSpan tsFetchWaking
                , string jEventListener
                , FormChangeMode.MODE_TECCOMPONENT modeTECComponent
                , List<int> listID_TECNotUse);

            #region только Модес-Центр
            [OperationContract (IsOneWay = true)]
            void GetMaketEquipment (FormChangeMode.KeyDevice key, EventArgs<Guid> arg, DateTime date);

            [OperationContract (IsOneWay = true)]
            void DebugEventReloadPlanValues ();

            [OperationContract (IsOneWay = true)]
            void ToDateRequest (DateTime date);

            [OperationContract (IsOneWay = true)]
            void FetchEvent (bool bRemove);

            bool IsServiceOnEvent
            {
                [OperationContract]
                get;
            }
            #endregion
        }
    }

    namespace ModesTerminale
    {
    }
}
