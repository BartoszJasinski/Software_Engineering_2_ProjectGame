using System;
using System.Net.Sockets;
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

    }
}
