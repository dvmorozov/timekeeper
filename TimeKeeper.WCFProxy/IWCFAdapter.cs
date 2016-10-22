using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace TimeKeeper.WCFAdapter
{
    [DataContract]
    public class Task_1
    {
        private int _id;
        private string _name;

        [DataMember]
        public int Id { get { return _id; } }
        public string Name { get { return _name; } }

        public Task_1(int id, string name) {
            _id = id;
            _name = name;
        }
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
        void FinishTask();
    }
}
