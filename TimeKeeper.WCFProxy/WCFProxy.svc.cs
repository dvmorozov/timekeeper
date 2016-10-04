using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TimeKeeper.WCFProxy
{
    public class WCFProxy : IWCFProxy
    {
        public string GetMessage()
        {
            return "Welcome to tungnt.net from GetMessage() WCF REST Service";
        }

        public string PostMessage(string userName)
        {
            return string.Format("Welcome {0} to tungnt.net from PostMessage() WCF REST Service", userName);
        }
    }
}
