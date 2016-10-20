using System;
using System.Security;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TimeKeeper.WCFAdapter
{
    public class WCFAdapter : IWCFAdapter_1_0_0
    {
        public void FinishTask()
        {
            throw new NotImplementedException();
        }

        public List<Task_1> GetTaskList()
        {
            return new List<Task_1>();
        }
    }
}
