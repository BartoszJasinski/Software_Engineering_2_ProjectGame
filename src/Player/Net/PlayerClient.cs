using System;
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
        private Common.Schema.TeamColour _team;
        private Common.Schema.Player[] _players;
        private GameBoard _board;
        private Common.Schema.PlayerType _type;
        private Common.SchemaWrapper.Field[,] _fields;
        private Random random = new Random();

        public ulong GameId
        {
            get { return _gameId; }
            set { _gameId = value; }
        }

        public ulong Id { get; set; }

        public Common.Schema.Player[] Players
        {
            set { _players = value; }
            get { return _players; }
        }

        public Common.SchemaWrapper.Field[,] Fields
        {
            set { _fields = value; }
            get { return _fields; }
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

        public Common.Schema.TeamColour Team
        {
            get { return _team; }
            set { _team = value; }
        }

        public Location Location { get; set; }

        public PlayerType Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

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

        public void Play()
        {
            Array values = Enum.GetValues(typeof(MoveType));
            MoveType randomMove = (MoveType)values.GetValue(random.Next(values.Length));
            Move(randomMove);
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

        private void RegisterForNextGameAfterGameEnd()
        {
            JoinGame joinGame = new JoinGame()
            {
                teamColour = options.PreferredTeam == "blue"
                    ? Common.Schema.TeamColour.blue
                    : Common.Schema.TeamColour.red,
                preferredRole = options.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                gameName = options.GameName,
                playerIdSpecified = false
            };

            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(joinGame));
        }

    } //class
} //namespace