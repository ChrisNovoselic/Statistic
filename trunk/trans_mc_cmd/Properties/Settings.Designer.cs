﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.18408
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace trans_mc_cmd.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Driver={MySQL ODBC 3.51 Driver};database=techsite_cfg;server=10.100.104.22;uid=te" +
            "chsite;pwd=;port=3306")]
        public string TechsiteMySQLconnectionString {
            get {
                return ((string)(this["TechsiteMySQLconnectionString"]));
            }
            set {
                this["TechsiteMySQLconnectionString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("xXi9N8pfMrJ9/NeFs/00Cw==")]
        public string accessPart {
            get {
                return ((string)(this["accessPart"]));
            }
            set {
                this["accessPart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("asfd9")]
        public string accessKey {
            get {
                return ((string)(this["accessKey"]));
            }
            set {
                this["accessKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ne1843")]
        public string Modes_Centre_Service_Host_Name {
            get {
                return ((string)(this["Modes_Centre_Service_Host_Name"]));
            }
            set {
                this["Modes_Centre_Service_Host_Name"] = value;
            }
        }
    }
}
