using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace TimeKeeper.WCFAdapter
{
    [DataContract]
    public class Task_1
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsArchived { get; set; }
    }

    [ServiceContract]
    //  It seems that the interface becomes more like adapter.
    public interface IWCFAdapter_1_0_0
    {
        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<Task_1> GetTaskList();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //  https://action.mindjet.com/task/14772437
        List<Task_1> GetActiveTaskList();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void FinishTask();
    }
}
