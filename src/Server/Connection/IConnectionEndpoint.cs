using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Connection.EventArg;

namespace Server.Connection
{
    public interface IConnectionEndpoint
    {
        int Port { get; set; }
        void Listen();
        void SendFromServer(Socket handler, string message);
        event EventHandler<ConnectEventArgs> OnConnect;
        event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        event EventHandler<ConnectEventArgs> OnDisconnected;
        //TODO add send event maybe
    }
}
