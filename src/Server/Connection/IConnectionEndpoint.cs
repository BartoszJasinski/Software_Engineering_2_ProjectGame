using Common.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Connection
{
    public interface IConnectionEndpoint
    {
        int Port { get; set; }
        void Listen();
        event EventHandler<ConnectEventArgs> OnConnect;
        event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        //TODO add send event maybe
    }
}
