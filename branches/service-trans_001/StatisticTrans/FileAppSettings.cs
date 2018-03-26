using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using ASUTP.Helper;

namespace StatisticTrans
{
    public class FileAppSettings : IFileAppSettings
    {
        private static FileAppSettings _this;

        private KeyValueConfigurationCollection _config;

        public ProgramBase.ID_APP GetIdApplication
        {
            get
            {
                return (ProgramBase.ID_APP)Enum.Parse(typeof(ProgramBase.ID_APP), getValue("iapp"));
            }
        }

        private FileAppSettings ()
            //: base ()
        {
            _config = ConfigurationManager.OpenExeConfiguration (Assembly.GetEntryAssembly ().Location).AppSettings.Settings;

            new List<Tuple<string, string>> {
                new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER), false.ToString())
                , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY), false.ToString())
                , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE), 671.ToString())
                , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME), @"21.10.2014 03:00")
                , new Tuple<string, string> (FormParameters.GetNameParametersOfIndex ((int)FormParameters.PARAMETR_SETUP.SEASON_ACTION), "-1")
                , new Tuple<string, string> ("iapp", "-1")
                , new Tuple<string, string> ("debug", "False")
                , new Tuple<string, string> (@"ИгнорДатаВремя-techsite", false.ToString())
                , new Tuple<string, string> (@"ОкноНазначение", @"Конвертер (...)")
                , new Tuple<string, string> (@"ID_TECNotUse", string.Empty)
                , new Tuple<string, string> (@"ОпросСохранениеППБР", string.Join(",", new bool[] { true, false }))
                , new Tuple<string, string> (@"ОпросСохранениеАдминЗнач", string.Join(",", new bool[] { true, false }))
                , new Tuple<string, string> (@"ID_TECNotUse", string.Empty)
            }.ToList().ForEach(elem => addRequired(elem.Item1, elem.Item2));
        }

        public static FileAppSettings This ()
        {
            if (Equals (_this, null) == true)
                _this = new FileAppSettings ();
            else
                ;

            return _this;
        }

        private void addRequired (string key, string value)
        {
            if (IsContainKey(key) == false)
                setValue(key, value);
            else {
            // это обязательные элементы, при наличии - перезаписывать не требуется
                //elem.Value = value;

                //_required.Move (_required.IndexOf(elem), 0);
            }
        }

        public bool IsContainKey (string key)
        {
            return _config.AllKeys.Contains (key);
        }

        public void AddRequired (string key, string value)
        {
            addRequired (key, value);
        }

        public void AddRequired (string key, object value)
        {
            addRequired (key, value.ToString());
        }

        public void AddRequired (IEnumerable<KeyValuePair<string, string>> range)
        {
            range.ToList ().ForEach (pair => AddRequired (pair.Key, pair.Value));
        }

        public void AddRequired (IEnumerable<Tuple<string, string>> range)
        {
            range.ToList ().ForEach (pair => AddRequired (pair.Item1, pair.Item2));
        }

        public void AddRequired (IEnumerable<KeyValuePair<string, object>> range)
        {
            range.ToList ().ForEach (pair => AddRequired(pair.Key, pair.Value));
        }

        private string getValue (string key, string valueDefault = "")
        {
            return _config.AllKeys.Contains (key) == true
                ? _config [key].Value
                    : valueDefault;
        }

        public string GetValue (string key, string valueDefault = "")
        {
            return getValue(key, valueDefault);
        }

        public string GetValueOfMainIndexParameter (FormParameters.PARAMETR_SETUP indx, string valueDefault = "")
        {
            return getValue (FormParameters.GetNameParametersOfIndex ((int)indx), valueDefault);
        }

        public string GetValueOfMainIndexParameter (int indx, string valueDefault = "")
        {
            return GetValueOfMainIndexParameter ((FormParameters.PARAMETR_SETUP)indx, valueDefault);
        }

        private void setValue (string key, string value)
        {
            if (IsContainKey(key) == false)
                _config.Add (key, value);
            else
                _config [key].Value = value;
            _config.CurrentConfiguration.Save (ConfigurationSaveMode.Modified, true);

            ConfigurationManager.RefreshSection ("appSettings");
        }

        public void SetValue (string key, string value)
        {
            setValue (key, value);
        }

        public void SetValue (string key, object value)
        {
            setValue (key, value.ToString());
        }
    }
}
