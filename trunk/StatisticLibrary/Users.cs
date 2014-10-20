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

        public static bool RoleIsDisp
        {
            get { return ((Role == ID_ROLES.ADMIN) || (Role == ID_ROLES.KOM_DISP) || (Role == ID_ROLES.NSS)); }
        }

        public static bool RoleIsAdmin
        {
            get
            {
                return Role == ID_ROLES.ADMIN;
            }
        }

        public static bool RoleIsNSS
        {
            get
            {
                return Role == ID_ROLES.NSS;
            }
        }

        public static bool RoleIsOperationPersonal
        {
            get
            {
                return (Role == ID_ROLES.NSS) || (Role == ID_ROLES.MAJOR_MASHINIST) || (Role == ID_ROLES.MASHINIST);
            }
        }
    }
}
