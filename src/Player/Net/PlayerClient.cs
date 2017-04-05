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
using Location = Common.Schema.Location;

namespace Player.Net
{
    public class PlayerClient
    {
        private IConnection connection;
        private PlayerSettings settings;
        private AgentCommandLineOptions options;
        private Socket serverSocket;
        private ulong _gameId;
        private string _guid;
        private Common.Schema.Player[] _players;
        private GameBoard _board;

        public ulong GameId
        {
            get { return _gameId; }
            set { _gameId = value; }
        }

        public ulong Id { get; set; }

        public Common.Schema.Player[] Players
        {
            set { _players = value; }
        }

        public GameBoard Board
        {
            set { _board = value; }
        }

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public Location Location { get; set; }

        public PlayerClient(IConnection connection, PlayerSettings settings, AgentCommandLineOptions options)
        {
            this.connection = connection;
            this.settings = settings;
            this.options = options;
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
            ConsoleDebug.Ordinary("Successful connection with address " + address.ToString());
            var socket = eventArgs.Handler as Socket;
            serverSocket = socket;
            string xmlMessage = XmlMessageConverter.ToXml(new GetGames());

            connection.SendFromClient(socket, xmlMessage);
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            ConsoleDebug.Message("New message from: " + socket.GetRemoteAddress() + "\n" + eventArgs.Message);

            BehaviorChooser.HandleMessage((dynamic) XmlMessageConverter.ToObject(eventArgs.Message),
                new PlayerMessageHandleArgs(connection, eventArgs.Handler, settings, options, this));
        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;
        }

        private void Move(MoveType direction)
        {
            Move m = new Move()
            {
                direction = direction,
                directionSpecified = true,
                gameId = _gameId,
                playerGuid = _guid
            };
            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(m));
        }

    } //class
} //namespace