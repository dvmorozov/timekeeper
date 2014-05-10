using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TimeKeeper;

namespace TestTimeKeeper
{
    [TestClass]
    public class UnitTest1
    {
        public class TestDateTime : IDateTime
        {
            private DateTime _now;

            public DateTime Now
            {
                get
                {
                    return _now;
                }
                set
                {
                    _now = value;
                }
            }
        }

        [TestMethod]
        //  Test of performance calculation.
        public void TestTimeExpensesData1()
        {
            var data = new TimeExpensesData();

            var dt = new TestDateTime();
            data.Dt = dt;

            var startDt = DateTime.Now;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("urgent, important", true, true);
            data.AddCategory("not urgent, important", false, true);
            data.AddCategory("urgent, not important", true, false);
            data.AddCategory("not urgent, not important", false, false);

            data.SetActive("urgent, important", true);
            data.SetActive("urgent, not important", true);

            dt.Now = startDt.AddHours(1);

            var perf = Math.Floor(data.Perf);
            Assert.AreEqual(50, perf);
        }

        [TestMethod]
        public void TestStatStack1()
        {
            var data = new TimeExpensesData();
            var stack = new StatStack(data);

            var dt = new TestDateTime();
            data.Dt = dt;
            stack.Dt = dt;

            var startDt = DateTime.Now;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("urgent, important", true, true);
            data.AddCategory("not urgent, important", false, true);
            data.AddCategory("urgent, not important", true, false);
            data.AddCategory("not urgent, not important", false, false);

            data.SetActive("urgent, important", true);
            stack.StartActivity();
            data.SetActive("urgent, not important", true);
            stack.StartActivity();

            dt.Now = startDt.AddHours(1);

            data.SetActive("urgent, important", false);
            stack.StopActivity();
            data.SetActive("urgent, not important", false);
            stack.StopActivity();

            Assert.AreEqual(0, stack.LastDays.Count);
        }
    }
}
