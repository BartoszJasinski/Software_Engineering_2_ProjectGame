using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using Common.Connection;
using Common.Connection.EventArg;
using Common.Message;
using Common.Schema;
using Common.Xml;

namespace Common.IO.Net
{
    //WE SHOULD PROBABLY DELETE THIS CLASS
    public class AsynchronousClient
    {
        private IConnection connection;
        
        //TESTING ONLY maybe we should change Iconnection a bit 
        private Socket client;

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

        public void Send(string message)
        {
            connection.Send(client, message);
        }

        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            //some copy-pasta happened here, i feel
            var address = eventArgs.Handler.GetRemoteAddress();
            System.Console.WriteLine("Successful connection with address {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;

            //TESTING ONLY maybe we should change Iconnection a bit 
            client = socket;

            connection.SendFromClient(socket, "Welcome message");

            
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            //            var address = eventArgs.Handler.GetRemoteEndPointAddress();
            //            System.Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);

            var socket = eventArgs.Handler as Socket;

            GameFinished gf = new GameFinished();
            gf.gameId = 123;

            string xmlMessage = XmlMessageConverter.ToXml(gf);
            connection.SendFromClient(socket, xmlMessage);


        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
