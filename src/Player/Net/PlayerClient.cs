using System;
using System.Net;
using System.Net.Sockets;
using Common;
using Common.Connection;
using Common.Connection.EventArg;
using Common.DebugUtils;
using Common.Message;
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
using System.Threading.Tasks;
using System.Threading;

namespace Player.Net
{
    public partial class PlayerClient
    {
        private IGame game;

        public IGame Game => game;

        private IMessageHandler messageHandler;
        private IConnection connection;
        private AgentCommandLineOptions options;
        private PlayerSettings settings;
        private Socket serverSocket;
        private CancellationTokenSource keepAliveToken { get; } = new CancellationTokenSource();

        public event Action ReadyForAction;


        public AgentCommandLineOptions Options
        {
            get
            {
                return options;
            }

            set
            {
                options = value;
            }
        }

        public PlayerSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
            }
        }

        private const int NO_PIECE = -1;

        public PlayerClient(IConnection connection, PlayerSettings settings, AgentCommandLineOptions options, IGame game)
        {
            this.connection = connection;
            this.Settings = settings;
            this.Options = options;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
            messageHandler = new MessageHandler(this);
            this.game = game;
        }

        public void Connect()
        {
            connection.StartClient();
        }

        public void Disconnect()
        {
            keepAliveToken.Cancel();
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

            //start sending keep alive bytes
            startKeepAlive(socket);
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var socket = eventArgs.Handler as Socket;

            if (eventArgs.Message.Length > 0) //the message is not the keepalive packet
            {
                ConsoleDebug.Message("New message from: " + socket.GetRemoteAddress() + "\n" + eventArgs.Message);
                messageHandler.HandleMessage((dynamic)XmlMessageConverter.ToObject(eventArgs.Message));
            }
        }


        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            System.Console.WriteLine("New message sent to {0}", address.ToString());
            //var socket = eventArgs.Handler as Socket;
        }


        private void RegisterForNextGameAfterGameEnd()
        {
            /*JoinGame joinGame = new JoinGame()
            {
                preferredTeam = Options.PreferredTeam == "blue"
                    ? Common.Schema.TeamColour.blue
                    : Common.Schema.TeamColour.red,
                preferredRole = Options.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                gameName = Options.GameName,
                playerIdSpecified = false
            };

            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(joinGame));*/
            string xmlMessage = XmlMessageConverter.ToXml(new GetGames());

            connection.SendFromClient(serverSocket, xmlMessage);
        }

        private async Task startKeepAlive(Socket server)
        {
            while (true)
            {
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(Settings.KeepAliveInterval));
                if (keepAliveToken.Token.IsCancellationRequested)
                    break;
                connection.SendFromClient(server, string.Empty);
            }
        }

        public void Send(string data)
        {
            connection.SendFromClient(serverSocket, data);
        }

        public int? DistToPiece()
        {
            return (game.Fields[game.Location.x, game.Location.y] as Common.SchemaWrapper.TaskField)?.DistanceToPiece;
        }

        public void DestroySham()
        {
            if (DistToPiece() > 0)
                MoveToNeighborClosestToPiece();
            else
                PlacePiece();
        }

        public bool IsInGoalArea
            =>
                game.Team == Common.Schema.TeamColour.blue && game.Location.y < game.Board.goalsHeight ||
                game.Team == Common.Schema.TeamColour.red && game.Location.y >= game.Board.tasksHeight + game.Board.goalsHeight;
        private bool left = true;
        public void LookForGoal()
        {
            if (game.Fields[game.Location.x, game.Location.y] == null && !IsInGoalArea)
            {
                Discover();
                return;
            }
            if (!IsInGoalArea)
            {
                if (game.Team == Common.Schema.TeamColour.blue)
                    Move(MoveType.down);
                else
                    Move(MoveType.up);
                return;
            }
            var gf = game.Fields[game.Location.x, game.Location.y] as Common.SchemaWrapper.GoalField;
            if (gf == null || gf.Type == Common.Schema.GoalFieldType.goal || gf.Type == Common.Schema.GoalFieldType.unknown)
                PlacePiece();
            else
            {
                if (game.Team == Common.Schema.TeamColour.blue &&
                    (game.Location.y == 0 && game.Location.x % 2 == 0 ||
                     game.Location.y == game.Board.goalsHeight - 1 && game.Location.x % 2 == 1)
                    ||
                    game.Team == Common.Schema.TeamColour.red &&
                    (game.Location.y == game.Board.tasksHeight + game.Board.goalsHeight * 2 - 1 && game.Location.x % 2 == 1 ||
                     game.Location.y == game.Board.tasksHeight + game.Board.goalsHeight && game.Location.x % 2 == 0))
                {
                    if (left && game.Location.x == 0 || !left && game.Location.x + 1 == game.Board.width)
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
                if (game.Location.x % 2 == 1)
                {
                    Move(MoveType.up);
                }
                else
                {
                    Move(MoveType.down);
                }
            }
        }

        private Common.Schema.Location previousLocation = null;
        public void Move(MoveType direction)
        {
            if (previousLocation != null && game.Location != null && game.Location.x == previousLocation.x && game.Location.y == previousLocation.y)
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
            previousLocation = new Common.Schema.Location() { x = game.Location.x, y = game.Location.y };
            Move m = new Move()
            {
                direction = direction,
                directionSpecified = true,
                gameId = game.GameId,
                playerGuid = game.Guid
            };
            Send(XmlMessageConverter.ToXml(m));
        }

        private Random random = new Random();
        private MoveType RandomMoveType()
        {
            Array values = Enum.GetValues(typeof(MoveType));
            return (MoveType)values.GetValue(random.Next(values.Length));
        }

        public void Discover()
        {
            Common.Schema.Discover d = new Discover()
            {
                gameId = game.GameId,
                playerGuid = game.Guid
            };
            Send(XmlMessageConverter.ToXml(d));
        }

        public void PickUpPiece()
        {
            Common.Schema.PickUpPiece p = new PickUpPiece()
            {
                playerGuid = game.Guid,
                gameId = game.GameId
            };
            Send(XmlMessageConverter.ToXml(p));
        }

        public void PlacePiece()
        {
            Common.Schema.PlacePiece p = new PlacePiece()
            {
                gameId = game.GameId,
                playerGuid = game.Guid
            };
            Send(XmlMessageConverter.ToXml(p));
            game.Pieces.Remove(CarriedPiece);
        }

        public void Test()
        {
            TestPiece t = new TestPiece()
            {
                gameId = game.GameId,
                playerGuid = game.Guid
            };
            Send(XmlMessageConverter.ToXml(t));
        }

        Common.SchemaWrapper.TaskField FieldAt(uint x, uint y)
        {
            try
            {
                return (game.Fields[x, y] as Common.SchemaWrapper.TaskField);
            }
            catch
            {
                return null;
            }
        }


        private MoveType Where(int? d)
        {
            if (FieldAt(game.Location.x + 1, game.Location.y)
                    ?.DistanceToPiece == d)
                return MoveType.right;
            if (FieldAt(game.Location.x - 1, game.Location.y)
                    ?.DistanceToPiece == d)
                return MoveType.left;
            if (FieldAt(game.Location.x, game.Location.y + 1)
                    ?.DistanceToPiece == d)
                return MoveType.up;
            if (FieldAt(game.Location.x, game.Location.y - 1)
                    ?.DistanceToPiece == d)
                return MoveType.down;
            if (game.Location.y <= game.Board.goalsHeight)
                return MoveType.up;
            return MoveType.down;
        }


        public void MoveToNeighborClosestToPiece()
        {
            var t = new[]
            {
                FieldAt(game.Location.x + 1, game.Location.y)
                    ?.DistanceToPiece,
                FieldAt(game.Location.x - 1, game.Location.y)
                    ?.DistanceToPiece,
                FieldAt(game.Location.x, game.Location.y + 1)
                    ?.DistanceToPiece,
                FieldAt(game.Location.x, game.Location.y - 1)
                    ?.DistanceToPiece
            }.Where(u => u.HasValue && u != NO_PIECE).Select(u => u.Value);
            int? d;
            if (t.Count() == 0)
                d = NO_PIECE;
            else
            {
                d = (int?)t.Min();
            }
            if (d >= FieldAt(game.Location.x, game.Location.y)?.DistanceToPiece)
                Discover();
            else
                Move(Where(d));
        }

        public Piece CarriedPiece
        {
            get
            {
                return game.Pieces.SingleOrDefault(
                    pc =>
                        pc.playerIdSpecified && pc.playerId == game.Id);
            }
        }

        public bool HasPiece()
        {
            var carriedPiece =
                game.Pieces.SingleOrDefault(
                    pc =>
                        pc.playerIdSpecified && pc.playerId == game.Id);
            return carriedPiece != null;
        }
    } //class
} //namespace