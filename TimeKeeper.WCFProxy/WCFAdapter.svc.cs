using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeKeeper.WCFAdapter
{
    public class WCFAdapter : IWCFAdapter_1_0_0
    {
        private string GetTaskAttr(JObject task, string attrName)
        {
            var buffer = Convert.FromBase64String(task.GetValue(attrName).Value<string>());
            return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

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
            dynamic obj = JsonConvert.DeserializeObject(jsonData);

            var result = new List<Task_1>();
            for (var i = 0; i < obj.tasks.Count - 1; i++)
            {
                var task = obj.tasks[i];
                result.Add(new Task_1 { Id = int.Parse(GetTaskAttr(task, "id")), Name = GetTaskAttr(task, "name") });
            }
            return result;
        }
    }
}
