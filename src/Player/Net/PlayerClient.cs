using System;
using System.Linq;
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
using Player.Strategy;
using Location = Common.Schema.Location;
using Wrapper = Common.SchemaWrapper;

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
        private Common.SchemaWrapper.Field[,] _fields;

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

        public Common.SchemaWrapper.Field[,] Fields
        {
            set { _fields = value; }
            get { return _fields; }
        }

        private State state;


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

            state = BiuldDfa();
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
            state.Action();
            state = state.NextState();
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

        private void Discover()
        {
            Common.Schema.Discover d = new Discover()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(d));

        }

        private void PickUpPiece()
        {
            Common.Schema.PickUpPiece p = new PickUpPiece()
            {
                playerGuid = Guid,
                gameId = GameId
            };
            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(p));
        }

        private void MoveToNieghborClosestToPiece()
        {
            var d = new[]
            {
                (Fields[Location.x + 1, Location.y] as Wrapper.TaskField)
                ?.DistanceToPiece,
                (Fields[Location.x - 1, Location.y] as Wrapper.TaskField)
                ?.DistanceToPiece,
                (Fields[Location.x, Location.y + 1] as Wrapper.TaskField)
                ?.DistanceToPiece,
                (Fields[Location.x, Location.y - 1] as Wrapper.TaskField)
                ?.DistanceToPiece
            }.Where(u => u.HasValue).Select(u => u.Value).Min();

            MoveType where()
            {
                if ((Fields[Location.x + 1, Location.y] as Wrapper.TaskField)
                    ?.DistanceToPiece == d)
                    return MoveType.right;
                if ((Fields[Location.x - 1, Location.y] as Wrapper.TaskField)
                    ?.DistanceToPiece == d)
                    return MoveType.left;
                if ((Fields[Location.x, Location.y + 1] as Wrapper.TaskField)
                    ?.DistanceToPiece == d)
                    return MoveType.up;

                return MoveType.down;
            }

            Move(where());
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

        uint DistToPiece()
        {
            return (Fields[Location.x, Location.y] as Wrapper.TaskField).DistanceToPiece;
        }

        private State BiuldDfa()
        {
            return new DfaBuilder()
                .AddState("start", Discover)
                .AddState("checkIfOnPiece")
                .AddTransition("start", "checkIfOnPiece")
                .AddState("moving")
                .AddTransition("checkIfOnPiece", "moving", () => DistToPiece() > 0)
                .AddState("onPiece", PickUpPiece)
                .AddTransition("checkIfOnPiece", "onPiece", () => DistToPiece() == 0)
                .StartingState();
        }
    } //class
} //namespace