using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;
using Statistic;
using ASUTP.Core;
using System.Threading.Tasks;
using ASUTP;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

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
            Logging.SetMode (Logging.LOG_MODE.FILE_EXE);

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

            panel = new PanelAdminKomDisp (new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR }));
            panel.SetDelegateReport (delegate (string mes) {
            // error
                 System.Diagnostics.Debug.WriteLine ($"Error: {mes}");
            }, delegate (string mes) {
            // warning
                System.Diagnostics.Debug.WriteLine ($"Warning: {mes}");
            }, delegate (string mes) {
            // action
                System.Diagnostics.Debug.WriteLine ($"Action: {mes}");
            }, delegate (bool bClear) {
                // clear
            });

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            Assert.IsNotNull (panel);

            panel.Start ();
            panel.InitializeComboBoxTecComponent (FormChangeMode.MODE_TECCOMPONENT.GTP);
            panel.Activate (true);
            Assert.IsTrue (panel.Actived);

            //Task.Factory.StartNew (() => {
            //    System.Threading.Thread.Sleep (42000);
            //});
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
            #region Проверка переключения режимов работы
            try {
                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY | AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                //??? на практике исключение этого флага не потребуется
                //panel.ModeGetRDGValues &= ~AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                //Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                //Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
            } catch (InvalidOperationException ioe) {
                Assert.IsTrue (string.Equals (ioe.Message, "PanelAdmin.ModeGetRDGValues::set - взаимоисключающие значения..."));
            }
            #endregion

            Action onEventUnitTestSetDataGridViewAdminCompleted
                , taskPerformButtonSetClick;
            Task task;
            TaskStatus taskStatus;
            CancellationTokenSource cancelTokenSource;

            onEventUnitTestSetDataGridViewAdminCompleted = null;
            taskPerformButtonSetClick = null;

            task = null;

            cancelTokenSource = new CancellationTokenSource ();

            taskPerformButtonSetClick = delegate () {
                panel.PerformButtonSetClick ((TEC t, TECComponent comp, DateTime date, string [] query) => {
                    Assert.IsNotNull (query);

                    System.Diagnostics.Debug.WriteLine ($"ТЭЦ={t.name_shr}; комп.={comp.name_shr};{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT.ToString ()}]: [{query [(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT]}])"
                        + $"{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE.ToString ()}]: [{query [(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE]}]");
                });
            };

            onEventUnitTestSetDataGridViewAdminCompleted = delegate () {
                System.Diagnostics.Debug.WriteLine ("Handler On 'EventUnitTestSetDataGridViewAdminCompleted'...");
                // отменить регистрацию, чтобы исключить повторный вызов
                // повторный вызов произойдет при обновлении информации на панели, который обязательно произойдет при сохранении значений
                if (Equals (onEventUnitTestSetDataGridViewAdminCompleted, null) == false)
                    panel.EventUnitTestSetDataGridViewAdminCompleted -= onEventUnitTestSetDataGridViewAdminCompleted;
                else
                    System.Diagnostics.Debug.WriteLine (string.Format ("Обработчик события 'Panel::EventUnitTestSetDataGridViewAdminCompleted' не определен..."));

                if (Equals (task, null) == true)
                    task = Task.Factory.StartNew (taskPerformButtonSetClick);
                else
                    System.Diagnostics.Debug.WriteLine (string.Format ("Пропустить старт задачи..."));
            };

            try {
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.EventUnitTestSetDataGridViewAdminCompleted += onEventUnitTestSetDataGridViewAdminCompleted;

                //task.ContinueWith (t => {
                //}
                //, cancelTokenSource.Token);

                int cnt = 0
                    , cnt_max = 26;
                taskStatus = Equals (task, null) == false ? task.Status : TaskStatus.WaitingForActivation;
                while ((cnt++ < cnt_max)
                    && ((!(taskStatus == TaskStatus.Canceled))
                    && (!(taskStatus == TaskStatus.Faulted))
                    && (!(taskStatus == TaskStatus.RanToCompletion)))) {
                    Thread.Sleep (1000);
                    taskStatus = Equals (task, null) == false ? task.Status : taskStatus;
                    System.Diagnostics.Debug.WriteLine ($"Ожидание: счетчик <{cnt}> из <{cnt_max}>...");
                }
                System.Diagnostics.Debug.WriteLine (string.Format("Окончание ожидания <{0}>, задача <{1}>..."
                    , cnt
                    , Equals (task, null) == false ? task.Status.ToString () : "не создана"));
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine (e.Message);

                Assert.IsTrue (false);
            }
        }

        private void onSetDataGridViewAdminComleted ()
        {
        }
    }
}
