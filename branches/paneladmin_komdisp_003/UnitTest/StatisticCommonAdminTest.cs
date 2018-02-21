﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;
using Statistic;
using ASUTP.Core;
using System.Threading.Tasks;
using ASUTP;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using ASUTP.Forms;

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
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            FormMainBase.s_iMainSourceData = 671;

            Logging.SetMode (Logging.LOG_MODE.FILE_EXE);

            Logging.Logg().PostStart(ASUTP.Helper.ProgramBase.MessageWellcome);

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
            try {
                panel.Activate (false);
                panel.Stop ();

                Logging.Logg().PostStop(ASUTP.Helper.ProgramBase.MessageExit);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

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
        public void Test_Export_PBRValues ()
        {
            panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
            panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.EXPORT;
            Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
            Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
            Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);

            string mesDebug = string.Empty;

            int prevIndex = 0
                , nextIndex = 0;
            Action onEventUnitTestSetDataGridViewAdminCompleted;
            PanelAdminKomDisp.DelegateUnitTestNextIndexExportPBRValuesRequest delegateNextIndexExportPBRValuesRequest;
            Task taskPerformButtonExportPBRValuesClick
                , taskPerformComboBoxTECComponentSelectedIndex;
            TaskStatus taskStatusPerformButtonExportPBRValuesClick
                , taskStatusPerformComboBoxTECComponentSelectedIndex;
            CancellationTokenSource cancelTokenSource;

            onEventUnitTestSetDataGridViewAdminCompleted = null;

            taskPerformButtonExportPBRValuesClick =
            taskPerformComboBoxTECComponentSelectedIndex =
                null;

            cancelTokenSource = new CancellationTokenSource ();
            // вызывается при ретрансляции панелью события имитации отправления запроса на обновление значений
            delegateNextIndexExportPBRValuesRequest = delegate (int next_index, TEC t, TECComponent comp, DateTime date, CONN_SETT_TYPE type, IEnumerable<int> list_id_rec, string [] queries) {
                Assert.IsNotNull (list_id_rec);
                Assert.IsFalse (list_id_rec.ToArray ().Length < 24);
                //TODO: проверка значений массива на истинность (сравнить с идентификаторами из таблицы БД)
            };
            // вызывается при завершении заполнения 'DatagridView' значениями
            onEventUnitTestSetDataGridViewAdminCompleted = delegate () {
                mesDebug = "Handler On 'EventUnitTestSetDataGridViewAdminCompleted'...";

                Logging.Logg ().Debug (mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine (mesDebug);

                if (prevIndex.Equals (nextIndex) == true)
                    // старт задачи сохранения значений
                    taskPerformButtonExportPBRValuesClick = Task.Factory.StartNew (delegate () {
                        panel.PerformButtonExportPBRValuesClick (delegateNextIndexExportPBRValuesRequest);
                    });
                else
                    if (!(nextIndex < 0))
                    taskPerformComboBoxTECComponentSelectedIndex = Task.Factory.StartNew (delegate () {
                        // установить новый индекс (назначить новый компонент-объект)
                        panel.PerformComboBoxTECComponentSelectedIndex (prevIndex = nextIndex);
                    });
                else
                    ;
            };

            try {
            } catch (Exception e) {
            }
        }

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

            string mesDebug = string.Empty;

            int prevIndex = 0
                , nextIndex = 0;
            Action onEventUnitTestSetDataGridViewAdminCompleted;
            PanelAdmin.DelegateUnitTestNextIndexSetValuesRequest delegateNextIndexSetValuesRequest;
            Task taskPerformButtonSetClick
                , taskPerformComboBoxTECComponentSelectedIndex;
            TaskStatus taskStatusPerformButtonSetClick
                , taskStatusPerformComboBoxTECComponentSelectedIndex;
            CancellationTokenSource cancelTokenSource;

            onEventUnitTestSetDataGridViewAdminCompleted = null;

            taskPerformButtonSetClick =
            taskPerformComboBoxTECComponentSelectedIndex =
                null;

            cancelTokenSource = new CancellationTokenSource ();
            // вызывается при ретрансляции панелью события имитации отправления запроса на обновление значений
            delegateNextIndexSetValuesRequest = delegate (int next_index, TEC t, TECComponent comp, DateTime date, CONN_SETT_TYPE type, IEnumerable<int> list_id_rec, string[]queries) {
                Assert.IsNotNull(list_id_rec);
                Assert.IsFalse(list_id_rec.ToArray().Length < 24);
                //TODO: проверка значений массива на истинность (сравнить с идентификаторами из таблицы БД)

                nextIndex = next_index;

                mesDebug = $"ТЭЦ={t.name_shr}; комп.={comp.name_shr};{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT.ToString()}]: [{queries[(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT]}])"
                    + $"{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE.ToString()}]: [{queries[(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE]}]"
                    + $"{Environment.NewLine}(идентификаторы: [{string.Join(";", list_id_rec.Select(id => id.ToString()).ToArray())}]";

                Logging.Logg().Debug(mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine(mesDebug);
            };
            // вызывается при завершении заполнения 'DatagridView' значениями
            onEventUnitTestSetDataGridViewAdminCompleted = delegate () {
                mesDebug = "Handler On 'EventUnitTestSetDataGridViewAdminCompleted'...";

                Logging.Logg().Debug(mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine (mesDebug);
                //// отменить регистрацию, чтобы исключить повторный вызов
                //// повторный вызов произойдет при обновлении информации на панели, который обязательно произойдет при сохранении значений
                //if (Equals (onEventUnitTestSetDataGridViewAdminCompleted, null) == false)
                //    panel.EventUnitTestSetDataGridViewAdminCompleted -= onEventUnitTestSetDataGridViewAdminCompleted;
                //else
                //    System.Diagnostics.Debug.WriteLine (string.Format ("Обработчик события 'Panel::EventUnitTestSetDataGridViewAdminCompleted' не определен..."));

                if (prevIndex.Equals(nextIndex) == true)
                    // старт задачи сохранения значений
                    taskPerformButtonSetClick = Task.Factory.StartNew (delegate () {
                        panel.PerformButtonSetClick(delegateNextIndexSetValuesRequest);
                    });
                else
                    if (!(nextIndex < 0))
                        taskPerformComboBoxTECComponentSelectedIndex = Task.Factory.StartNew(delegate () {
                            // установить новый индекс (назначить новый компонент-объект)
                            panel.PerformComboBoxTECComponentSelectedIndex(prevIndex = nextIndex);
                        });
                    else
                        ;
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
                // исходные состояния задач
                taskStatusPerformButtonSetClick =
                    Equals (taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status : TaskStatus.WaitingForActivation;
                taskStatusPerformComboBoxTECComponentSelectedIndex =
                    Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status : TaskStatus.WaitingForActivation;
                while ((cnt++ < cnt_max)
                    && (!(nextIndex < 0))) {
                    // ожидать
                    Thread.Sleep (1000);
                    // сообщение для индикации ожидания
                    System.Diagnostics.Debug.WriteLine ($"Ожидание: счетчик <{cnt}> из <{cnt_max}>...");
                }
                // состояния задач по завершению цикла
                taskStatusPerformButtonSetClick =
                    Equals(taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status : taskStatusPerformButtonSetClick;
                taskStatusPerformComboBoxTECComponentSelectedIndex =
                    Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status : TaskStatus.WaitingForActivation;
                System.Diagnostics.Debug.WriteLine (string.Format("Окончание ожидания <{0}>, задача-Click is <{1}>, задача-Selected is <{2}>, nextIndex={3}..."
                    , cnt
                    , Equals (taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status.ToString () : "не создана"
                    , Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status.ToString() : "не создана"
                    , nextIndex));
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine (e.Message);

                Assert.IsTrue (false);
            }
        }

        /// <summary>
        /// Оброботчик исключений в потоках и запись их в лог
        /// </summary>
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // here you can log the exception ...
            Logging.Logg().Exception(e.Exception, "::Application_ThreadException () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Оборботчик не перехваченного исключения в текущем домене и запись их в лог
        /// </summary>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // here you can log the exception ...
            Logging.Logg().Exception((Exception)e.ExceptionObject, "::AppDomain_UnhandledException () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
