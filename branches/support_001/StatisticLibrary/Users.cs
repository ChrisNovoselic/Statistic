using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace StatisticCommon
{
    public class HStatisticUsers : HUsers
    {
        //Идентификаторы из БД
        public enum ID_ROLES { UNKNOWN, KOM_DISP = 1, ADMIN, USER, NSS = 101, MAJOR_MASHINIST, MASHINIST, SOURCE_DATA = 501,
                            COUNT_ID_ROLES = 7};

        //Идентификаторы из БД
        public enum ID_ALLOWED {
            UNKNOWN = -1
            , SOURCEDATA_CHANGED = 1 //Вкл\выкл группу пунктов меню
            , TAB_PBR_KOMDISP
            , AUTO_TAB_PBR_KOMDISP
            , ALARM_KOMDISP
            , AUTO_ALARM_KOMDISP
            , TAB_PBR_NSS
            , MENUITEM_SETTING_ADMIN_DB_CONGIG
            , MENUITEM_SETTING_ADMIN_DB_SOURCEDATA
            , MENUITEM_SETTING_ADMIN_STATEUSERS
            , MENUITEM_SETTING_ADMIN_PSW_KOMDISP_CHANGE
            , MENUITEM_SETTING_ADMIN_PSW_ADMIN_CHANGE
            , MENUITEM_SETTING_ADMIN_PSW_NSS_CHANGE
            , MENUITEM_SETTING_ADMIN_TECCOMPONENT_CHANGE
            , MENUITEM_SETTING_ADMIN_USERS_CHANGE
            , MENUITEM_SETTING_ADMIN_ROLES_ALLOWED_CHANGE
            , MENUCONTEXTITEM_PANELQUICKDATA_FORECASTEE
            , MENUCONTEXTITEM_PANELQUICKDATA_TMVALUES
            , MENUCONTEXTITEM_TABLEHOURS_COLUMN_59MIN
            , MENUITEM_SETTINGS_PARAMETERS_APP
            , MENUITEM_SETTINGS_PARAMETERS_TGBIYSK
            , APP_AUTO_RESET
            , SOURCEDATA_ASKUE_PLUS_SOTIASSO //Вкл\выкл пункт меню
            , MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB
            , PROFILE_SETTINGS_CHANGEMODE
            , PROFILE_VIEW_ADDINGTABS
            ,
        };



        public HStatisticUsers(int iListenerId)
            : base(iListenerId)
        {
            Initialize(@"Пользователь: " + DomainName + @", (id=" + Id + @"), роль: " + Role + @", id_tec=" + allTEC);
        }

        public static ID_ROLES Role
        {
            get
            {
                return (m_DataRegistration == null) ? ID_ROLES.UNKNOWN : ((!((int)INDEX_REGISTRATION.ID_TEC < m_DataRegistration.Length)) || (m_DataRegistration[(int)INDEX_REGISTRATION.ROLE] == null)) ? ID_ROLES.ADMIN : (ID_ROLES)m_DataRegistration[(int)INDEX_REGISTRATION.ROLE];
            }
        }

        public static bool RoleIsKomDisp
        {
            get
            {
                return Role == ID_ROLES.KOM_DISP;
            }
        }

        public static bool RoleIsDisp
        {
            get
            {
                return ((Role == ID_ROLES.ADMIN) || (Role == ID_ROLES.KOM_DISP) || (Role == ID_ROLES.NSS));
            }
        }

        public static bool RoleIsAdmin
        {
            get
            {
                return Role == ID_ROLES.ADMIN;
            }
        }

        //public static bool RoleIsNSS
        //{
        //    get
        //    {
        //        return Role == ID_ROLES.NSS;
        //    }
        //}

        //public static bool RoleIsOperationPersonal
        //{
        //    get
        //    {
        //        return (Role == ID_ROLES.NSS) || (Role == ID_ROLES.MAJOR_MASHINIST) || (Role == ID_ROLES.MASHINIST);
        //    }
        //}
    }
}
