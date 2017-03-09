using Common.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Connection
{
    public interface IConnection
    {
        void StartClient();
        void ConnectCallback(IAsyncResult ar);
        void Receive(Socket client);
        void ReceiveCallback(IAsyncResult ar);
        void Send(Socket client, String data);
        void SendCallback(IAsyncResult ar);

        event EventHandler<ConnectEventArgs> OnConnection;
        event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        event EventHandler<MessageSendEventArgs> OnMessageSend;


    }
}
