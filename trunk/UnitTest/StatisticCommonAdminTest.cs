﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;
using Statistic;
using ASUTP.Core;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class StatisticCommonAdminTest
    {
        private static PanelAdminKomDisp panel;

        #region Дополнительные атрибуты тестирования

        //При написании тестов можно использовать следующие дополнительные атрибуты:

        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        [ClassInitialize ()]
        public static void StatisticCommonAdminTestInitialize (TestContext testContext)
        {
            int errCode = -1;
            string errMess = string.Empty;
            ASUTP.Database.FIleConnSett fileCS;
            List<ASUTP.Database.ConnectionSettings> listConnSett;

            listConnSett = new List<ASUTP.Database.ConnectionSettings> ();
            fileCS = new ASUTP.Database.FIleConnSett ("connsett.ini", ASUTP.Database.FIleConnSett.MODE.FILE);
            fileCS.ReadSettingsFile (-1, out listConnSett, out errCode, out errMess);
            new DbTSQLConfigDatabase (
                //new ASUTP.Database.ConnectionSettings("CONFIG_DB", "10.100.104.18", "", 1433, "techsite_cfg-2.X.X", "Stat_user", "5tat_u%ser")
                listConnSett [0]
                );
            DbTSQLConfigDatabase.DbConfig ().Register ();

            using (new HStatisticUsers (DbTSQLConfigDatabase.DbConfig ().ListenerId, ASUTP.Helper.HUsers.MODE_REGISTRATION.USER_DOMAINNAME)) {
                ;
            }
            FormMain.formGraphicsSettings = new FormGraphicsSettings (delegate (int arg) {
            }
                , delegate () {
                }
                , false
            );

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            panel = new PanelAdminKomDisp (new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR }));
            Assert.IsNotNull (panel);

            panel.Start ();
            panel.Activate (true);
            Assert.IsTrue (panel.Actived);
        }

        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        [ClassCleanup ()]
        public static void StatisticCommonAdminTestCleanup ()
        {
            panel.Activate (false);
            panel.Stop ();

            Assert.IsFalse (panel.Actived);
        }

        ////TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize ()]
        //public void MyTestInitialize ()
        //{
        //}

        ////TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup ()]
        //public void MyTestCleanup ()
        //{
        //}

        #endregion

        [TestMethod]
        public void Test_SaveGTP_REC ()
        {
            try {
                panel.ModeGetRDGValues = PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY | PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY) == PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST) == PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues = PanelAdmin.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST) == PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY) == PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues |= PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST) == PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST);

                //??? на практике исключение этого флага не потребуется
                //panel.ModeGetRDGValues &= ~PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST;
                //Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);
                //Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST) == PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.ModeGetRDGValues = PanelAdmin.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY) == PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues = PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY) == PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues |= PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST;
                panel.ModeGetRDGValues = PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY) == PanelAdmin.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST) == PanelAdmin.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & PanelAdmin.MODE_GET_RDG_VALUES.EXPORT) == PanelAdmin.MODE_GET_RDG_VALUES.EXPORT);

                Task.Factory.StartNew (() => {
                    System.Threading.Thread.Sleep (6000);
                });
            } catch (InvalidOperationException ioe) {
                Assert.IsTrue (string.Equals (ioe.Message, "PanelAdmin.ModeGetRDGValues::set - взаимоисключающие значения..."));
            }

            //panel.PerformButtonSetClick ();
        }
    }
}
