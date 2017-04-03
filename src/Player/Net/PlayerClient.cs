using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
using Player.Logic;
using Common.Config;
using Common.IO.Console;
using Common.Schema;

namespace Player.Net
{
    public class PlayerClient
    {
        public IConnection Connection { get; set; }
        public PlayerSettings Settings { get; set; }
        public AgentCommandLineOptions Options { get; set; }
        public ulong Id {get; set;}

        public PlayerClient(IConnection connection, PlayerSettings settings, AgentCommandLineOptions options)
        {
            this.Connection = connection;
            this.Settings = settings;
            this.Options = options;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
        }

        public void Connect()
        {
            Connection.StartClient();
        }

        public void Disconnect()
        {
            Connection.StopClient();
        }


        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            var address = eventArgs.Handler.GetRemoteAddress();
            ConsoleDebug.Ordinary("Successful connection with address " + address.ToString());
            var socket = eventArgs.Handler as Socket;

            string xmlMessage = XmlMessageConverter.ToXml(new GetGames());

            Connection.SendFromClient(socket, xmlMessage);
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            ConsoleDebug.Message("New message from: " + socket.GetRemoteAddress() + "\n" + eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic) XmlMessageConverter.ToObject(eventArgs.Message),
                this, socket);
        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;
        }
    } //class
} //namespace