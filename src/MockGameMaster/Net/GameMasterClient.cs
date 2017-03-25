using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using Common.Connection;
using Common.Connection.EventArg;
using Common.Message;
using Common.Schema;
using MockGameMaster.Logic;

namespace Common.IO.Net
{
    
    public class GameMasterClient
    {
        private IConnection connection;
        
        //TESTING ONLY maybe we should change Iconnection a bit 
        private Socket client;

        public GameMasterClient(IConnection connection)
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

            //            var address = eventArgs.Handler.GetRemoteEndPointAddress();
            //            System.Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);

            var socket = eventArgs.Handler as Socket;

            System.Console.WriteLine("New message from: {0} \n {1}",socket.GetRemoteAddress(),eventArgs.Message);

            string xmlMessage = XmlMessageConverter.ToXml(RandXmlClass.GetXmlClass());

            connection.SendFromClient(socket, xmlMessage);


        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            //System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;

        }





    }//class
}//namespace
