using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatisticTrans;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class StatisticTrans
    {
        [TestMethod]
        public void TestMethodIsTomorrow ()
        {
            List<TimeSpan> timeSpanes = new List<TimeSpan> { new TimeSpan (3, 4, 5) };

            List<Tuple<DateTime, DateTime, bool>> put = new List<Tuple<DateTime, DateTime, bool>> {
                Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 20, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 21, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 22, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 27, 23, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 1, 0, 0), false)

                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 20, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 21, 0, 0), true)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 28, 23, 0, 0), true)

                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 0, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 1, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 29, 21, 0, 0), false)
                , Tuple.Create<DateTime, DateTime, bool> (new DateTime(2018, 03, 28), new DateTime (2018, 03, 30, 9, 0, 0), false)
            };

            put.ForEach(verify => 
                timeSpanes.ForEach( timeSpan =>
                    Assert.AreEqual(verify.Item3, FormMainTrans.IsTomorrow (verify.Item1, verify.Item2, timeSpan), $"[{verify.Item1}, {verify.Item2}] - разница=<{verify.Item1.AddDays(1)- verify.Item2.Add (timeSpan)}, {(verify.Item1.AddDays (1) - verify.Item2.Add(timeSpan)).TotalDays}>")));
        }
    }
}
