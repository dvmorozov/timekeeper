using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace TestTodoistAdapterComm
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NamedPipeClientStream("todoist_adapter");

            client.Connect();

            var reader = new StreamReader(client);
            var writer = new StreamWriter(client);

            writer.WriteLine("GetTaskList");
            writer.Flush();

            var jsonData = reader.ReadToEnd();

            Console.WriteLine(jsonData);
            //  Show results.
            Thread.Sleep(5000);
        }
    }
}
