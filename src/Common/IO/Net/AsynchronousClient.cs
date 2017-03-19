using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using Common.Connection;
using Common.EventArg;
using Common.Xml;

namespace Common.IO.Net
{
    public class AsynchronousClient
    {
        private IConnection connection;

        public AsynchronousClient(IConnection connection)
        {
            this.connection = connection;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
        }

        public void Connect()
        {
            connection.StartClient();
        }

        public void Disconnect()
        {
            connection.StopClient();
        }



        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            //some copy-pasta happened here, i feel

            var address = eventArgs.Handler.GetRemoteEndPointAddress();
            System.Console.WriteLine("Successful connection with address {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;
            connection.SendFromClient(socket, "Welcome message");
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
//            var address = eventArgs.Handler.GetRemoteEndPointAddress();
//            System.Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);

            var socket = eventArgs.Handler as Socket;

            string xml = XmlOperations.Serialize();
            connection.SendFromClient(socket, xml);


        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
