using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Win32;

namespace TimeKeeper.WCFAdapter
{
    public class WCFAdapter : IWCFAdapter_1_0_0
    {
        public delegate List<Task> GetFullTaskData();

        private GetFullTaskData _getFullTaskData;

        public WCFAdapter()
        {
            //  Initialize the method for requesting task data.
            _getFullTaskData = GetFullTaskDataProcess;
        }

        private string GetTaskAttr(JObject task, string attrName)
        {
            var buffer = Convert.FromBase64String(task.GetValue(attrName).Value<string>());
            return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        public void FinishTask()
        {
            throw new NotImplementedException();
        }

        //  Convert task data into inner format.
        private List<Task> ProcessData(string jsonData)
        {
            dynamic obj = JsonConvert.DeserializeObject(jsonData);

            var result = new List<Task>();
            for (var i = 0; i < obj.tasks.Count - 1; i++)
            {
                var task = obj.tasks[i];
                result.Add(new Task
                {
                    Id = int.Parse(GetTaskAttr(task, "id")),
                    Name = GetTaskAttr(task, "name"),
                    Url = GetTaskAttr(task, "url"),
                    IsArchived = bool.Parse(GetTaskAttr(task, "isArchived"))
                });
            }
            return result;
        }

        //  Request task data from pipe-server.
        private List<Task> GetFullTaskDataPipe()
        {
            using (var client = new NamedPipeClientStream("todoist_adapter"))
            {
                client.Connect();

                var reader = new StreamReader(client);
                var writer = new StreamWriter(client);

                writer.WriteLine("GetTaskList");
                writer.Flush();

                return ProcessData(reader.ReadToEnd());
            }
        }

        //  Request task data via execution of external process.
        private List<Task> GetFullTaskDataProcess()
        {
            Process cmd = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\TodoistAdapter", "FileName", ""),
                    Arguments = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\TodoistAdapter", "Arguments", ""),
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            cmd.Start();
            return ProcessData(cmd.StandardOutput.ReadToEnd());
        }

        //  Perform conversion from inner representation into data contract representation.
        private List<Task_1> ConvertToList_1(List<Task> list)
        {
            var result = new List<Task_1>();
            for (var i = 0; i < list.Count - 1; i++)
            {
                var task = list[i];
                result.Add(new Task_1 { Id = task.Id, Name = task.Name });
            }
            return result;
        }

        //  Return full list of tasks.
        public List<Task_1> GetTaskList()
        {
            return ConvertToList_1(_getFullTaskData());
        }

        //  Return only tasks not marked as archive.
        public List<Task_1> GetActiveTaskList()
        {
            return ConvertToList_1(_getFullTaskData().Where(t => !t.IsArchived).ToList());
        }
    }
}