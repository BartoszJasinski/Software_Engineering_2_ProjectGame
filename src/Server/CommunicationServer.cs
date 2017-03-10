using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Connection;

namespace Server
{
    public class CommunicationServer
    {
        private IConnectionEndpoint connectionEndpoint;

        public CommunicationServer(IConnectionEndpoint connectionEndpoint)
        {
            this.connectionEndpoint = connectionEndpoint;
            connectionEndpoint.OnConnect += OnClientConnect;
            connectionEndpoint.OnMessageRecieve += OnMessage;
        }

        public void Start()
        {
            connectionEndpoint.Listen();
        }

        private void OnClientConnect(object sender, ConnectEventArgs eventArgs)
        {
            //TODO extension method to get address from socket
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("New client connected with address {0}", address.ToString());
        }

        private void OnMessage(object sender, MessageRecieveEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("New message from {0}: {1}", address, eventArgs.Message);
            connectionEndpoint.SendFromServer(eventArgs.Handler, "Answer!");
        }
    }
}
