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
using System.Collections.Generic;
using System.Linq;
using Player.Strategy;
using TeamColour = Common.Schema.TeamColour;
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
        private Common.Schema.TeamColour _team;
        private Common.Schema.Player[] _players;
        private IList<Piece> _pieces;
        private GameBoard _board;
        private Common.Schema.PlayerType _type;
        private Common.SchemaWrapper.Field[,] _fields;
        private Random random = new Random();
        private State currentState;

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
            get { return _board; }
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

        public IList<Piece> Pieces
        {
            get
            {
                return _pieces;
            }

            set
            {
                _pieces = value;
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
            Pieces = new List<Piece>();
            currentState = BiuldDfa();
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
            ConsoleDebug.Error(currentState.Name);
            var act = currentState.Action;
            act?.Invoke();
            currentState = currentState.NextState();
            if (act == null)
                Play();
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

        Wrapper.TaskField FieldAt(uint x, uint y)
        {
            try
            {
                return (Fields[x,y] as Wrapper.TaskField);
            }
            catch
            {
                return null;
            }
        }

        private void MoveToNieghborClosestToPiece()
        {
            var t = new[]
            {
                FieldAt(Location.x+1,Location.y)
                ?.DistanceToPiece,
                FieldAt(Location.x-1,Location.y)
                ?.DistanceToPiece,
                FieldAt(Location.x,Location.y+1)
                ?.DistanceToPiece,
                FieldAt(Location.x,Location.y-1)
                ?.DistanceToPiece
            }.Where(u => u.HasValue).Select(u => u.Value);
            int? d;
            if (t.Count() == 0)
                d = null;
            else
            {
                d = (int?) t.Min();
            }
            MoveType where()
            {
                if (FieldAt(Location.x + 1, Location.y)
                    ?.DistanceToPiece == d)
                    return MoveType.right;
                if (FieldAt(Location.x - 1, Location.y)
                    ?.DistanceToPiece == d)
                    return MoveType.left;
                if (FieldAt(Location.x, Location.y + 1)
                    ?.DistanceToPiece == d)
                    return MoveType.up;
                if (FieldAt(Location.x, Location.y - 1)
                    ?.DistanceToPiece == d)
                    return MoveType.down;
                if (Location.x<=Board.goalsHeight)
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

        uint? DistToPiece()
        {
            return (Fields[Location.x, Location.y] as Wrapper.TaskField)?.DistanceToPiece;
        }

        private State BiuldDfa()
        {
            return new DfaBuilder()
                .AddState("start", Discover)
                .AddState("checkIfOnPiece")
                .AddTransition("start", "checkIfOnPiece")
                .AddState("moving", MoveToNieghborClosestToPiece)
                .AddTransition("checkIfOnPiece", "moving", () => DistToPiece() > 0 || DistToPiece() == null)
                .AddState("checkPieceAfterMove")
                .AddTransition("moving", "checkPieceAfterMove")
                .AddTransition("checkPieceAfterMove", "moving", () => DistToPiece() > 0 || DistToPiece() == null)
                .AddState("onPiece", PickUpPiece)
                .AddTransition("checkIfOnPiece", "onPiece", () => DistToPiece() == 0)
                .AddTransition("checkPieceAfterMove", "onPiece", () => DistToPiece() == 0)
                .AddState("carrying")
                .AddTransition("onPiece","carrying",HasPiece)
                .StartingState();
        }

        private void LookForGoal()
        {
            throw new NotImplementedException();
        }

        private bool HasPiece()
        {
            var carriedPiece =
                    Pieces.SingleOrDefault(
                        pc =>
                            pc.playerId == Id);
            return carriedPiece != null;
        }
    } //class
} //namespace