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
        public enum ID_ROLES { KOM_DISP, ADMIN, USER, NSS = 101, MAJOR_MASHINIST, MASHINIST };
        
        public HStatisticUsers(int iListenerId)
            : base(iListenerId)
        {            
        }

        public static bool RoleIsDisp
        {
            get { 
                return Role < ID_ROLES.USER;
                //return Role == ID_ROLES.KOM_DISP;
            }
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

        public static ID_ROLES Role
        {
            get
            {
                ID_ROLES idRoleRes = ID_ROLES.ADMIN;

                return idRoleRes;
            }
        }
    }
}
