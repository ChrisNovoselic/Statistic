using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;

namespace StatisticCommon
{
    public interface IFileAppSettings
    {
        bool IsContainKey (string key);

        ASUTP.Helper.ProgramBase.ID_APP GetIdApplication
        {
            get;
        }

        void AddRequired (string key, string value);

        void AddRequired (string key, object value);

        void AddRequired (IEnumerable<KeyValuePair<string, string>> range);

        void AddRequired (IEnumerable<KeyValuePair<string, object>> range);

        string GetValue (string key, string valueDefault = "");

        string GetValueOfMainIndexParameter (FormParameters.PARAMETR_SETUP indx, string valueDefault = "");

        string GetValueOfMainIndexParameter (int indx, string valueDefault = "");

        void SetValue (string key, string value);

        void SetValue (string key, object value);
    }
}
