using ASUTP.Core;
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
    public struct PARAMToSaveRDGValues
    {
        public FormChangeMode.KeyDevice key;
        public DateTime date;
        public bool bCallback;

        public PARAMToSaveRDGValues (FormChangeMode.KeyDevice k, DateTime dt, bool cb)
        {
            key = k;
            date = dt;
            bCallback = cb;
        }
    };

    public enum IdPseudoDelegate
    {
        Unknown = -1
        , WaitStart
        , WaitStop
        , WaitStatus
        , ReportError
        , ReportWarning
        , ReportAction
        , ReportClear
        , SetForceDate
        , Ready
        , Error
        , SaveCompleted
    }

    [ServiceContract (SessionMode = SessionMode.Required, CallbackContract = typeof (IServiceCallback))]
    public interface IServiceTransModes
    {
        [OperationContract (IsOneWay = true)]
        void Connect ();

        [OperationContract (Name = "Request-GetRDGValues", IsOneWay = true)]
        void GetRDGValues (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, DateTime date);

        [OperationContract (Name = "Member-GetRDGValues")]
        StatisticCommon.HAdmin.RDGStruct [] GetRDGValues (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract (IsOneWay = true)]
        void SaveRDGValues (StatisticTrans.CONN_SETT_TYPE indx, PARAMToSaveRDGValues param);

        [OperationContract (IsOneWay = true)]
        void ClearRDGValues (StatisticTrans.CONN_SETT_TYPE indx, DateTime date);

        //[OperationContract]
        //int CountHoursOfDate (DateTime date);

        //[OperationContract]
        //HAdmin GetAdminModesCentre ();

        [OperationContract (IsOneWay = true)]
        void Start ();

        [OperationContract (IsOneWay = true)]
        void Stop ();

        [OperationContract (IsOneWay = true)]
        void Activate (StatisticTrans.CONN_SETT_TYPE indx, bool actived);

        //[OperationContract (IsOneWay = true)]
        //void SetDelegateWait (Action fStartWait, Action fStopWait, Action fEvent);

        //[OperationContract (IsOneWay = true)]
        //void SetDelegateReport (Action<string> fError, Action<string> fWarning, Action<string> fAction, Action<bool> fClear);

        //[OperationContract (IsOneWay = true)]
        //void SetDelegateForceDate (Action<DateTime> fForseDate);

        [OperationContract (IsOneWay = true)]
        void SetCurrentDate (StatisticTrans.CONN_SETT_TYPE indx, DateTime date);

        //[OperationContract (IsOneWay = true)]
        //void SetDelegateData (Action<DateTime, bool> fSuccess, Action<int> fError);

        //[OperationContract (IsOneWay = true)]
        //void SetDelegateSaveComplete (Action<int> fSaveComplete);

        [OperationContract (IsOneWay = true)]
        void SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE indx, bool bIgnoreDate);

        [OperationContract]
        ConnectionSettings GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, StatisticCommon.CONN_SETT_TYPE type);

        [OperationContract]
        List<FormChangeMode.KeyDevice> GetListKeyTECComponent (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.MODE_TECCOMPONENT mode, bool bLimitLK);

        [OperationContract]
        string GetNameTECComponent (StatisticTrans.CONN_SETT_TYPE indx, FormChangeMode.KeyDevice key, bool bWithNameTECOwner);

        [OperationContract (IsOneWay = true)]
        void CopyRDGValues (StatisticTrans.CONN_SETT_TYPE indxSource, StatisticTrans.CONN_SETT_TYPE indxDest);

        //[OperationContract (IsOneWay = true)]
        //void SetCurrentRDGValues (StatisticTrans.CONN_SETT_TYPE indxDest, HAdmin.RDGStruct [] valuesSource);

        //[OperationContract (IsOneWay = true)]
        //void SetCurrentRDGValue (StatisticTrans.CONN_SETT_TYPE indxDest, int iHour, HAdmin.RDGStruct valueSource);

        //[OperationContract (IsOneWay = true)]
        //void CopyCurToPrevRDGValues (StatisticTrans.CONN_SETT_TYPE indx);

        //[OperationContract]
        //string GetPBRNumber (StatisticTrans.CONN_SETT_TYPE indx, int iHour);

        [OperationContract (IsOneWay = true)]
        void TECComponentComplete (StatisticTrans.CONN_SETT_TYPE indx, int iState, bool bResult);

        [OperationContract]
        FormChangeMode.KeyDevice PrepareActionRDGValues (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract]
        FormChangeMode.KeyDevice GetFirstTECComponentKey (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract]
        FormChangeMode.KeyDevice GetCurrentKey (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract]
        IDevice GetCurrentDevice (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract]
        bool GetAllowRequested (StatisticTrans.CONN_SETT_TYPE indx);

        [OperationContract]
        bool IsValidate (StatisticTrans.CONN_SETT_TYPE indx);

        int AdminCount
        {
            [OperationContract]
            get;
        }

        [OperationContract]
        string GetFormatDatetime (StatisticTrans.CONN_SETT_TYPE indx, int iHour);

        [OperationContract]
        int GetSeasonHourOffset (StatisticTrans.CONN_SETT_TYPE indx, int iHour);
    }
}
