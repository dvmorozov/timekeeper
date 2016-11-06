using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TimeKeeper.Core;

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
            TimeExpensesData.Dt = dt;

            var startDt = DateTime.Now;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            data.SetActive("Urgent, not Important", true);

            dt.Now = startDt.AddHours(1);

            var perf = Math.Floor(data.Perf);
            Assert.AreEqual(50, perf);
        }

        //  Single day activities - the simplest case.
        [TestMethod]
        public void TestStatStack1()
        {
            var data = new TimeExpensesData();
            var stack = new StatStack(data);

            var dt = new TestDateTime();
            TimeExpensesData.Dt = dt;
            stack.Dt = dt;

            var startDt = DateTime.Now;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            stack.StartActivity();
            data.SetActive("Urgent, not Important", true);
            stack.StartActivity();

            dt.Now = startDt.AddHours(1);

            data.SetActive("Urgent, Important", false);
            stack.StopActivity();
            data.SetActive("Urgent, not Important", false);
            stack.StopActivity();

            Assert.AreEqual(0, stack.LastDays.Count);
        }

        //  Successive days activities.
        [TestMethod]
        public void TestStatStack2()
        {
            var data = new TimeExpensesData();
            var stack = new StatStack(data);

            var dt = new TestDateTime();
            TimeExpensesData.Dt = dt;
            stack.Dt = dt;

            //  Middle of the day must be used. 
            var startDt = DateTime.Now.Date;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            stack.StartActivity();
            data.SetActive("Urgent, not Important", true);
            stack.StartActivity();

            dt.Now = startDt.AddDays(1);

            data.SetActive("Urgent, Important", false);
            stack.StopActivity();
            data.SetActive("Urgent, not Important", false);
            stack.StopActivity();

            Assert.AreEqual(1, stack.LastDays.Count);
            var intPerf = stack.LastDays[0].IntegralPerf;
            Assert.AreEqual(25, Math.Floor(intPerf));
        }

        //  Two days later.
        [TestMethod]
        public void TestStatStack3()
        {
            var data = new TimeExpensesData();
            var stack = new StatStack(data);

            var dt = new TestDateTime();
            TimeExpensesData.Dt = dt;
            stack.Dt = dt;

            //  Middle of the day must be used. 
            var startDt = DateTime.Now.Date;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            stack.StartActivity();
            data.SetActive("Urgent, not Important", true);
            stack.StartActivity();

            dt.Now = startDt.AddDays(2);

            data.SetActive("Urgent, Important", false);
            stack.StopActivity();
            data.SetActive("Urgent, not Important", false);
            stack.StopActivity();

            Assert.AreEqual(2, stack.LastDays.Count);
            var intPerf = stack.LastDays[0].IntegralPerf;
            Assert.AreEqual(25, Math.Floor(intPerf));
            intPerf = stack.LastDays[1].IntegralPerf;
            Assert.AreEqual(50, Math.Floor(intPerf));
        }

        //  Alternate activities in the middle day.
        [TestMethod]
        public void TestStatStack4()
        {
            var data = new TimeExpensesData();
            var stack = new StatStack(data);

            var dt = new TestDateTime();
            TimeExpensesData.Dt = dt;
            stack.Dt = dt;

            //  Middle of the day must be used. 
            var startDt = DateTime.Now.Date;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            stack.StartActivity();
            data.SetActive("Urgent, not Important", true);
            stack.StartActivity();

            dt.Now = startDt.AddDays(1);

            data.SetActive("Urgent, Important", false);
            stack.StopActivity();
            data.SetActive("Urgent, not Important", false);
            stack.StopActivity();

            //  Pause for 6 hours.
            dt.Now = startDt.AddDays(1).AddHours(6);

            data.SetActive("Urgent, Important", true);
            stack.StartActivity();
            data.SetActive("Urgent, not Important", true);
            stack.StartActivity();

            dt.Now = startDt.AddDays(2);

            data.SetActive("Urgent, Important", false);
            stack.StopActivity();
            data.SetActive("Urgent, not Important", false);
            stack.StopActivity();

            Assert.AreEqual(2, stack.LastDays.Count);
            var intPerf = stack.LastDays[0].IntegralPerf;
            Assert.AreEqual(25, Math.Floor(intPerf));
            intPerf = stack.LastDays[1].IntegralPerf;
            Assert.AreEqual(37, Math.Floor(intPerf));
        }

        [TestMethod]
        public void TestTimeExpensesData2()
        {
            var data = new TimeExpensesData();

            var dt = new TestDateTime();
            TimeExpensesData.Dt = dt;

            //  Middle of the day must be used. 
            var startDt = DateTime.Now.Date;
            //  Sets up "now".
            dt.Now = startDt;

            data.AddCategory("Urgent, Important", true, true);
            data.AddCategory("not Urgent, Important", false, true);
            data.AddCategory("Urgent, not Important", true, false);
            data.AddCategory("not Urgent, not Important", false, false);

            data.SetActive("Urgent, Important", true);
            data.SetActive("Urgent, not Important", true);

            dt.Now = startDt.AddDays(1);

            data.SetActive("Urgent, Important", false);
            data.SetActive("Urgent, not Important", false);

            //  Pause for 6 hours.
            dt.Now = startDt.AddDays(1).AddHours(6);

            data.SetActive("Urgent, Important", true);
            data.SetActive("Urgent, not Important", true);

            dt.Now = startDt.AddDays(2);

            data.SetActive("Urgent, Important", false);
            data.SetActive("Urgent, not Important", false);

            Assert.AreEqual(6, data.InactiveDuration.TotalHours);
        }
    }
}
