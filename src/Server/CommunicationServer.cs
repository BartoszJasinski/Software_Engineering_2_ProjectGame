using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Connection;
using Common.Connection.EventArg;
using Server.Connection;
using Common;

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

            var address = eventArgs.Handler.GetRemoteAddress();
            Console.WriteLine("New client connected with address {0}", address.ToString());
        }

        private void OnMessage(object sender, MessageRecieveEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            Console.WriteLine("SERVER \n New message from {0}: {1}", address, eventArgs.Message);


//            /////test
//            dynamic xmlObject = XmlMessageConverter.ToObject(KeepAliveCutter.Cut(eventArgs.Message));
//            Console.WriteLine("\n \n" + xmlObject.ToString() + "\n \n");
//            //// end test

            connectionEndpoint.SendFromServer(eventArgs.Handler, eventArgs.Message);
        }
    }
}
