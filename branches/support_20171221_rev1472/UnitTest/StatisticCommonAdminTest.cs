﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;

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
    }
}
