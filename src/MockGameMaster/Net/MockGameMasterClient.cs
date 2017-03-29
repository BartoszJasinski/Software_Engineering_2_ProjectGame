using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.Message;
using Common.Schema;
using MockGameMaster.Logic;

namespace GameMaster.Net
{
    
    public class MockGameMasterClient
    {
        private IConnection connection;
       

        public MockGameMasterClient(IConnection connection)
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
            var address = eventArgs.Handler.GetRemoteAddress();
            System.Console.WriteLine("Successful connection with address {0}", address.ToString());
            var socket = eventArgs.Handler as Socket;

            string xmlMessage = XmlMessageConverter.ToXml(XmlMessageGenerator.GetXmlClass());

            connection.SendFromClient(socket, xmlMessage);

            
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            //            var address = eventArgs.Handler.GetRemoteEndPointAddress();
            //            System.Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);

            //            var address = eventArgs.Handler.GetRemoteEndPointAddress();
            //            System.Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);

            var socket = eventArgs.Handler as Socket;

            System.Console.WriteLine("New message from: {0} \n {1}",socket.GetRemoteAddress(),eventArgs.Message);

            string xmlMessage = XmlMessageConverter.ToXml(XmlMessageGenerator.GetXmlClass());

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
