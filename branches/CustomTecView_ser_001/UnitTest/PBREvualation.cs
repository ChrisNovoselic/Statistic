﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatisticCommon;

namespace UnitTest {
    /// <summary>
    /// Сводное описание для PBREvualation
    /// </summary>
    [TestClass]
    public class PBREvualation {
        public PBREvualation ()
        {
            //
            // TODO: добавьте здесь логику конструктора
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты тестирования
        //
        // При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        // ClassInitialize используется для выполнения кода до запуска первого теста в классе
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // TestInitialize используется для выполнения кода перед запуском каждого теста 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // TestCleanup используется для выполнения кода после завершения каждого теста
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

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
                , $"{StatisticCommon.HAdmin.PBR_PREFIX}"
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

            int [] actual = new int [expected_ret.Length];

            for (int i = 0; i < input.Length; i++) {
                Assert.AreEqual (expected_ret [i], HAdmin.GetPBRNumber (input [i], out err));
                Assert.AreEqual (expected_err [i], err);
            }
        }
    }
}
