using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TimeKeeper.WCFProxy
{
    [ServiceContract]
    public interface IWCFProxy
    {
        [OperationContract]
        [WebGet(RequestFormat=WebMessageFormat.Json, ResponseFormat=WebMessageFormat.Json)]
        //Tương tự [WebInvoke(Method="GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string GetMessage();

        [OperationContract]
        [WebInvoke(Method="POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string PostMessage(string userName);
    }
}
