using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;
using Statistic;
using ASUTP.Core;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class StatisticCommonAdminTest {
        struct ASSERT {
            public string input;
            public int expected_err;
            public int expected_ret;
            public int actual;
        }

        [TestMethod]
        public void Test_GetPBRNumber ()
        {
            int err = -1
                , iRes = -1;
            ;

            string [] input = {
                ""
                , $"{HAdmin.PBR_PREFIX}"
                , $"П{HAdmin.PBR_PREFIX}"
                , $"ПП{HAdmin.PBR_PREFIX}89"
                , $"ПП12{HAdmin.PBR_PREFIX}"
                , $"П{HAdmin.PBR_PREFIX}-1"
                , $"{HAdmin.PBR_PREFIX}2"
                , $"-4{HAdmin.PBR_PREFIX}"
                , $"{HAdmin.PBR_PREFIX}24"
            };

            int [] expected_err = {
                -1
                , -1
                , 1
                , -1
                , -1
                , -1
                , 0
                , -1
                , 0
            };

            int [] expected_ret = {
                -1
                , -1
                , 0
                , 0
                , 0
                , 0
                , 2
                , 0
                , 24
            };

            int [] actual = new int[expected_ret.Length];

            for (int i = 0; i < input.Length; i++) {
                Assert.AreEqual (expected_ret [i], HAdmin.GetPBRNumber (input [i], out err));
                Assert.AreEqual (expected_err [i], err);
            }
        }

        [TestMethod]
        public void Test_SaveGTP_REC ()
        {
            PanelAdminKomDisp panel;

            int errCode = -1;
            string errMess = string.Empty;
            ASUTP.Database.FIleConnSett fileCS;
            List<ASUTP.Database.ConnectionSettings> listConnSett;

            listConnSett = new List<ASUTP.Database.ConnectionSettings> ();
            fileCS = new ASUTP.Database.FIleConnSett ("connsett.ini", ASUTP.Database.FIleConnSett.MODE.FILE);
            fileCS.ReadSettingsFile (-1, out listConnSett, out errCode, out errMess);
            new DbTSQLConfigDatabase (
                //new ASUTP.Database.ConnectionSettings("CONFIG_DB", "10.100.104.18", "", 1433, "techsite_cfg-2.X.X", "Stat_user", "5tat_u%ser")
                listConnSett[0]
                );
            DbTSQLConfigDatabase.DbConfig ().Register ();

            using (new HStatisticUsers (DbTSQLConfigDatabase.DbConfig ().ListenerId, ASUTP.Helper.HUsers.MODE_REGISTRATION.USER_DOMAINNAME)) { ; }
            FormMain.formGraphicsSettings = new FormGraphicsSettings (delegate (int arg) { }
                , delegate () { }
                , false
            );

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            panel = new PanelAdminKomDisp (new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR }));
            panel.Start ();
            panel.Activate (true);

            Task.Factory.StartNew (() => {
                System.Threading.Thread.Sleep (6000);
            });

            panel.PerformButtonSetClick ();

            panel.Activate (false);
            panel.Stop ();

            Assert.AreNotEqual (panel, null);
        }
    }
}
