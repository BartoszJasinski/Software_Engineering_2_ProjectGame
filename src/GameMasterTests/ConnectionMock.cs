using Common.Connection;
using Common.Connection.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameMasterTests
{
    public class ConnectionMock : IConnection
    {
        public event EventHandler<ConnectEventArgs> OnConnection;
        public event EventHandler<MessageRecieveEventArgs> OnMessageRecieve;
        public event EventHandler<MessageSendEventArgs> OnMessageSend;

        public void ConnectCallback(IAsyncResult ar)
        {

        }

        public void Receive(Socket client)
        {

        }

        public void ReceiveCallback(IAsyncResult ar)
        {

        }

        public void Send(Socket client, string data)
        {

        }

        public void SendCallback(IAsyncResult ar)
        {

        }

        public void SendFromClient(Socket client, string data)
        {
            var m = data;
        }

        public void StartClient()
        {

        }

        public void StopClient()
        {

        }
    }
}
