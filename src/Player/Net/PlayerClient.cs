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
    public class PlayerClient
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
            JoinGame joinGame = new JoinGame()
            {
                preferredTeam = Options.PreferredTeam == "blue"
                    ? Common.Schema.TeamColour.blue
                    : Common.Schema.TeamColour.red,
                preferredRole = Options.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                gameName = Options.GameName,
                playerIdSpecified = false
            };

            connection.SendFromClient(serverSocket, XmlMessageConverter.ToXml(joinGame));
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


        MoveType Where(int? d)
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


        private class MessageHandler : IMessageHandler
        {
            private object pieceLock;

            public MessageHandler(PlayerClient player)
            {
                Player = player;
                pieceLock = new object();
            }


            PlayerClient Player { get; set; }

            public void HandleMessage(RejectJoiningGame message)
            {
                if (message == null)
                    return;
                //try to connect again
                Player.Send(XmlMessageConverter.ToXml(new GetGames()));

                return;
            }

            public void HandleMessage(Data message)
            {
                if (message.PlayerLocation != null)
                {
                    Player.game.Location = message.PlayerLocation;
                    ConsoleDebug.Message($"My location: ({message.PlayerLocation.x}, {message.PlayerLocation.y})");
                }
                if (message.TaskFields != null)
                {
                    Common.SchemaWrapper.TaskField[] taskFields = new Common.SchemaWrapper.TaskField[message.TaskFields.Length];
                    for (int i = 0; i < taskFields.Length; i++)
                        taskFields[i] = new Common.SchemaWrapper.TaskField(message.TaskFields[i]);
                    Player.game.UpdateFields(taskFields); ;
                }
                if (message.GoalFields != null)
                {
                    Common.SchemaWrapper.GoalField[] goalFields = new Common.SchemaWrapper.GoalField[message.GoalFields.Length];
                    for (int i = 0; i < goalFields.Length; i++)
                        goalFields[i] = new Common.SchemaWrapper.GoalField(message.GoalFields[i]);
                    Player.game.UpdateFields(goalFields);
                }
                if (message.Pieces != null)
                {
                    foreach (Piece piece in message.Pieces)
                    {
                        lock (pieceLock)
                        {
                            if (piece.playerIdSpecified)
                            {
                                Player.game.Pieces = Player.game.Pieces.Where(piece1 => piece1.playerId != piece.playerId).ToList();
                            }
                            if (Player.game.Pieces.Count(p => p.id == piece.id) == 0)
                                Player.game.Pieces.Add(piece);
                            else
                            {

                                var pp = Player.game.Pieces.Single(p => p.id == piece.id);
                                pp.playerId = piece.playerId;
                                pp.playerIdSpecified = piece.playerIdSpecified;
                                pp.timestamp = piece.timestamp;
                                if (pp.type == PieceType.unknown)
                                    pp.type = piece.type;

                            }

                        }
                    }
                    //args.PlayerClient.Pieces.Clear();
                    //foreach (var piece in message.Pieces)
                    //{
                    //    args.PlayerClient.Pieces.Add(piece);
                    //}
                }
                if (message.gameFinished == true)
                {
                    ConsoleDebug.Good("\nGame just ended\n");
                    BoardPrinter.Print(Player.game.Fields);
                    //System.Console.ReadLine();
                    //player.Disconnect();
                    Player.Connect();
                    return;
                }

                Player.ReadyForAction?.Invoke();
            }

            public void HandleMessage(AcceptExchangeRequest message)
            {
                //the other player accepted our request, send data to him
                DataMessageBuilder builder = new DataMessageBuilder(message.senderPlayerId);

                builder.SetGoalFields(Player.game.Fields.Cast<Common.Schema.Field>().Where(f => f is Common.Schema.GoalField).Cast<Common.Schema.GoalField>());
                builder.SetTaskFields(Player.game.Fields.Cast<Common.Schema.Field>().Where(f => f is TaskField).Cast<TaskField>());
                builder.SetPieces(Player.game.Pieces);

                var data = builder.GetXml();
                Player.Send(data);
                //do not call play, we call play already when we get our data
            }

            public void HandleMessage(object message)
            {
                ConsoleDebug.Warning("Unknown Type");
            }


            public void HandleMessage(RejectKnowledgeExchange message)
            {
                Player.ReadyForAction?.Invoke();
            }

            public void HandleMessage(KnowledgeExchangeRequest message)
            {
                var fromPlayer = Player.game.Players.Where(p => p.id == message.senderPlayerId).Single();

                bool accept = false;
                //it is our leader, we have to listen to him or we are the leader
                if (fromPlayer.team == Player.game.Team
                    && (fromPlayer.type == PlayerType.leader || Player.game.Type == PlayerType.leader))
                    accept = true;
                else    //decide if we really want to exchange information
                    accept = true;

                if (accept)
                {
                    //when you accept an information exchange you have to send an AuthorizeKnowledgeExchange to the gm
                    var exchange = new AuthorizeKnowledgeExchange()
                    {
                        gameId = Player.game.GameId,
                        playerGuid = Player.game.Guid,
                        withPlayerId = fromPlayer.id
                    };
                    Player.Send(XmlMessageConverter.ToXml(exchange));
                    //do not play, wait for data answer
                }
                else
                {
                    //otherwise send a RejectKnowledgeExchange directly to the player
                    var reject = new RejectKnowledgeExchange()
                    {
                        playerId = fromPlayer.id,
                        senderPlayerId = Player.game.Id
                    };
                    Player.Send(XmlMessageConverter.ToXml(reject));
                    //after reject we have to play, otherwise we will be stuck (because reject does not generate an answer)
                    Player.ReadyForAction?.Invoke();
                }
            }

            public void HandleMessage(Common.Schema.Game message)
            {
                Player.game.Players = message.Players;
                Player.game.Board = message.Board;
                Player.game.Location = message.PlayerLocation;
                Player.game.Fields = new Common.SchemaWrapper.Field[message.Board.width, 2 * message.Board.goalsHeight + message.Board.tasksHeight];
                ConsoleDebug.Good("Game started");
                Player.ReadyForAction?.Invoke();
            }

            public void HandleMessage(ConfirmJoiningGame message)
            {

                if (message == null)
                    return;

                ConsoleDebug.Good(message.gameId.ToString());

                Player.game.Id = message.playerId;
                Player.game.GameId = message.gameId;
                Player.game.Guid = message.privateGuid;
                Player.game.Team = message.PlayerDefinition.team;
                Player.game.Type = message.PlayerDefinition.type;

                return;
            }

            public void HandleMessage(RegisteredGames message)
            {
                if (message.GameInfo == null || message.GameInfo.Length == 0 || !message.GameInfo.Where(g => g.gameName == Player.Options.GameName).Any())
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep((int)Player.Settings.RetryJoinGameInterval);
                        string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                        Player.Send(xmlMessage);
                    });
                }
                else
                {
                    ConsoleDebug.Good("Games available");
                    if (Player.Options.GameName == null)
                    {
                        ConsoleDebug.Warning("Game name not specified");
                        return;
                    }
                    if (message.GameInfo.Count(info => info.gameName == Player.Options.GameName) == 1)
                    {
                        string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                        {
                            gameName = Player.Options.GameName,
                            playerIdSpecified = false,
                            preferredRole = Player.Options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                            preferredTeam = Player.Options?.PreferredTeam == "red" ? Common.Schema.TeamColour.red : Common.Schema.TeamColour.blue
                        });
                        Player.Send(xmlMessage);
                    }
                }
            }

        }
    } //class
} //namespace