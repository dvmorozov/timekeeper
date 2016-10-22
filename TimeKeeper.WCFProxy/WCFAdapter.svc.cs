﻿using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;

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
            var client = new NamedPipeClientStream("todoist_adapter");

            client.Connect();

            var reader = new StreamReader(client);
            var writer = new StreamWriter(client);

            writer.WriteLine("GetTaskList");
            writer.Flush();

            var jsonData = reader.ReadToEnd();

            return new List<Task_1>();
        }
    }
}
