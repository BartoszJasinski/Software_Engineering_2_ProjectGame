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
using GoalFieldType = Common.Schema.GoalFieldType;
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


        public Piece CarriedPiece
        {
            get
            {
                return Pieces.SingleOrDefault(
                    pc =>
                        pc.playerId == Id);
            }
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
            get { return _type; }

            set { _type = value; }
            }

        public IList<Piece> Pieces
        {
            get { return _pieces; }

            set { _pieces = value; }
            }

        public bool IsInGoalArea
            =>
                Team == TeamColour.blue && Location.y < Board.goalsHeight ||
                Team == TeamColour.red && Location.y >= Board.tasksHeight + Board.goalsHeight;

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
            BoardPrinter.Print(Fields);
            var act = currentState.Action;
            act?.Invoke();
            currentState = currentState.NextState();
            if (act == null)
                Play();
        }

        private MoveType RandomMoveType()
        {
            Array values = Enum.GetValues(typeof(MoveType));
            return (MoveType)values.GetValue(random.Next(values.Length));
        }

        private void Move(MoveType direction)
        {
            if (previousLocation != null && Location != null && Location.x == previousLocation.x && Location.y == previousLocation.y)
            {
                ConsoleDebug.Error("Snake time! =====================================");
                //if (direction==MoveType.up)
                //    direction=MoveType.right;
                //else if (direction == MoveType.right)
                //    direction = MoveType.down;
                //else if (direction == MoveType.down)
                //    direction = MoveType.left;
                //else
                //    direction = MoveType.up;
                direction = RandomMoveType();
            }
            previousLocation=new Location(){x=Location.x,y=Location.y};
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

        private void PlacePiece()
        {
            Common.Schema.PlacePiece p = new PlacePiece()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(p));
            Pieces.Remove(CarriedPiece);
        }

        private void Test()
        {
            TestPiece t = new TestPiece()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(t));
        }

        Wrapper.TaskField FieldAt(uint x, uint y)
        {
            try
            {
                return (Fields[x, y] as Wrapper.TaskField);
            }
            catch
            {
                return null;
            }
        }

        private Location previousLocation=null;
        MoveType Where(int? d)
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
            if (Location.y <= Board.goalsHeight)
                return MoveType.up;
            return MoveType.down;
        }

        private void MoveToNieghborClosestToPiece()
        {
            var t = new[]
            {
                FieldAt(Location.x + 1, Location.y)
                    ?.DistanceToPiece,
                FieldAt(Location.x - 1, Location.y)
                    ?.DistanceToPiece,
                FieldAt(Location.x, Location.y + 1)
                    ?.DistanceToPiece,
                FieldAt(Location.x, Location.y - 1)
                    ?.DistanceToPiece
            }.Where(u => u.HasValue).Select(u => u.Value);
            int? d;
            if (t.Count() == 0)
                d = Int32.MaxValue;
            else
            {
                d = (int?) t.Min();
            }
            if (d >= FieldAt(Location.x, Location.y)?.DistanceToPiece)
                Discover();
            else
                Move(Where(d));
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
                //finding piece
                .AddState("start", Discover)
                .AddState("checkIfOnPiece")
                .AddTransition("start", "checkIfOnPiece")
                .AddState("moving", MoveToNieghborClosestToPiece)
                .AddTransition("checkIfOnPiece", "moving", () => DistToPiece() > 0 || DistToPiece() == null)
                .AddState("checkPieceAfterMove")
                .AddTransition("moving", "checkPieceAfterMove")
                .AddTransition("checkPieceAfterMove", "start", () => DistToPiece() > 0 || DistToPiece() == null)
                .AddState("onPiece", PickUpPiece)
                //picking piece
                .AddTransition("checkIfOnPiece", "onPiece", () => DistToPiece() == 0)
                .AddTransition("checkPieceAfterMove", "onPiece", () => DistToPiece() == 0)
                .AddState("afterPick")
                .AddTransition("onPiece", "afterPick")
                .AddState("notTested", Test)
                .AddState("carryingNormal", LookForGoal)
                //testing piece
                .AddTransition("afterPick", "notTested", HasPiece)
                .AddTransition("afterPick", "start", () => !HasPiece())
                .AddState("tested")
                .AddTransition("notTested", "tested")
                .AddState("carryingSham", DestroySham)
                .AddState("shamPicked")
                .AddTransition("tested", "carryingNormal",
                    () => CarriedPiece != null && CarriedPiece.type == PieceType.normal)
                .AddTransition("tested", "shamPicked", () => CarriedPiece != null && CarriedPiece.type == PieceType.sham)
                .AddTransition("tested", "start", () => CarriedPiece == null)
                //sham madness
                .AddState("shamPicked2", Discover)
                .AddTransition("shamPicked", "shamPicked2")
                .AddState("shamPicked3")
                .AddTransition("shamPicked2", "shamPicked3")
                .AddTransition("shamPicked3", "carryingSham")
                .AddState("afterCarryingSham")
                .AddTransition("carryingSham", "afterCarryingSham")
                .AddTransition("afterCarryingSham", "start", () => !HasPiece())
                .AddTransition("afterCarryingSham", "carryingSham", HasPiece)
                .AddState("afterCarryingNormal")
                //place normal piece
                .AddTransition("carryingNormal", "afterCarryingNormal")
                .AddTransition("afterCarryingNormal", "start", () => !HasPiece())
                .AddTransition("afterCarryingNormal", "carryingNormal", HasPiece)
                .StartingState();
        }

        private void DestroySham()
        {
            if (DistToPiece() > 0)
                MoveToNieghborClosestToPiece();
            else
                PlacePiece();
        }

        private bool left = true;

        private void LookForGoal()
        {
            if (Fields[Location.x, Location.y] == null && !IsInGoalArea)
            {
                Discover();
                return;
            }
            if (!IsInGoalArea)
            {
                if (Team == TeamColour.blue)
                    Move(MoveType.down);
                else
                    Move(MoveType.up);
                return;
            }
            var gf = Fields[Location.x, Location.y] as Wrapper.GoalField;
            if (gf == null || gf.Type == GoalFieldType.goal || gf.Type == GoalFieldType.unknown)
                PlacePiece();
            else
            {
                if (Team == TeamColour.blue &&
                    (Location.y == 0 && Location.x % 2 == 0 ||
                     Location.y == Board.goalsHeight - 1 && Location.x % 2 == 1)
                    ||
                    Team == TeamColour.red &&
                    (Location.y == Board.tasksHeight + Board.goalsHeight * 2 - 1 && Location.x % 2 == 1 ||
                     Location.y == Board.tasksHeight + Board.goalsHeight && Location.x % 2 == 0))
                {
                    if (left && Location.x == 0 || !left && Location.x + 1 == Board.width)
                        left = !left;
                    if (left)
                    {
                        Move(MoveType.left);
                    }
                    else
                    {
                        Move(MoveType.right);
                    }
                    return;
                }
                if (Location.x % 2 == 1)
                {
                    Move(MoveType.up);
                }
                else
                {
                    Move(MoveType.down);
                }
            }
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