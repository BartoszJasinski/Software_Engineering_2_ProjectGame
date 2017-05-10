using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using System.Threading;
using Common.Config;
using Common.DebugUtils;
using Common.Message;
using Common.IO.Console;
using Player.Strategy;

namespace Player.Net
{
    public class Game : IGame, IMessageHandler
    {
        private PlayerClient player;
        private ulong _gameId;
        private string _guid;
        private Common.Schema.TeamColour _team;
        private Common.Schema.Player[] _players;
        private IList<Piece> _pieces;
        private GameBoard _board;
        private Common.Schema.PlayerType _type;
        private Common.SchemaWrapper.Field[,] _fields;
        private Random random = new Random();

        private static object joinLock = new object();


        private const int NO_PIECE = -1;

        public Game()
        {
            Pieces = new List<Piece>();
        }

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
                        pc.playerIdSpecified && pc.playerId == Id);
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

        public Common.Schema.Location Location { get; set; }
        

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
                Team == Common.Schema.TeamColour.blue && Location.y < Board.goalsHeight ||
                Team == Common.Schema.TeamColour.red && Location.y >= Board.tasksHeight + Board.goalsHeight;

        public PlayerClient Player
        {
            get
            {
                return player;
            }

            set
            {
                player = value;
            }
        }

        public void HandleMessage(RejectJoiningGame message)
        {
            if (message == null)
                return;
            //try to connect again
            player.Send(XmlMessageConverter.ToXml(new GetGames()));

            return;
        }

        public void HandleMessage(Data message)
        {
            if (message.PlayerLocation != null)
            {
               Location = message.PlayerLocation;
                ConsoleDebug.Message($"My location: ({message.PlayerLocation.x}, {message.PlayerLocation.y})");
            }
            if (message.TaskFields != null)
            {
                Common.SchemaWrapper.TaskField[] taskFields = new Common.SchemaWrapper.TaskField[message.TaskFields.Length];
                for (int i = 0; i < taskFields.Length; i++)
                    taskFields[i] = new Common.SchemaWrapper.TaskField(message.TaskFields[i]);
                FieldsUpdater(Fields, taskFields); ;
            }
            if (message.GoalFields != null)
            {
                Common.SchemaWrapper.GoalField[] goalFields = new Common.SchemaWrapper.GoalField[message.GoalFields.Length];
                for (int i = 0; i < goalFields.Length; i++)
                    goalFields[i] = new Common.SchemaWrapper.GoalField(message.GoalFields[i]);
                FieldsUpdater(Fields, goalFields);
            }
            if (message.Pieces != null)
            {
                foreach (Piece piece in message.Pieces)
                {
                    lock (joinLock)
                    {
                        if (piece.playerIdSpecified)
                        {
                            Pieces = Pieces.Where(piece1 => piece1.playerId != piece.playerId).ToList();
                        }
                        if (Pieces.Count(p => p.id == piece.id) == 0)
                            Pieces.Add(piece);
                        else
                        {

                            var pp = Pieces.Single(p => p.id == piece.id);
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
                BoardPrinter.Print(Fields);
                System.Console.ReadLine();
                player.Disconnect();
            }

            player.Play();
        }

        public void HandleMessage(AcceptExchangeRequest message)
        {
            //the other player accepted our request, send data to him
            DataMessageBuilder builder = new DataMessageBuilder(message.senderPlayerId);

            builder.SetGoalFields(Fields.Cast<Common.Schema.Field>().Where(f => f is Common.Schema.GoalField).Cast<Common.Schema.GoalField>());
            builder.SetTaskFields(Fields.Cast<Common.Schema.Field>().Where(f => f is TaskField).Cast<TaskField>());
            builder.SetPieces(Pieces);

            var data = builder.GetXml();
            player.Send(data);
            //do not call play, we call play already when we get our data
        }

        public void HandleMessage(object message)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        public void HandleMessage(RejectKnowledgeExchange message)
        {
            player.Play();
        }

        public void HandleMessage(KnowledgeExchangeRequest message)
        {
            var fromPlayer = Players.Where(p => p.id == message.senderPlayerId).Single();

            bool accept = false;
            //it is our leader, we have to listen to him or we are the leader
            if (fromPlayer.team == Team
                && (fromPlayer.type == PlayerType.leader || Type == PlayerType.leader))
                accept = true;
            else    //decide if we really want to exchange information
                accept = true;

            if (accept)
            {
                //when you accept an information exchange you have to send an AuthorizeKnowledgeExchange to the gm
                var exchange = new AuthorizeKnowledgeExchange()
                {
                    gameId = GameId,
                    playerGuid = Guid,
                    withPlayerId = fromPlayer.id
                };
                player.Send(XmlMessageConverter.ToXml(exchange));
                //do not play, wait for data answer
            }
            else
            {
                //otherwise send a RejectKnowledgeExchange directly to the player
                var reject = new RejectKnowledgeExchange()
                {
                    playerId = fromPlayer.id,
                    senderPlayerId = Id
                };
                player.Send(XmlMessageConverter.ToXml(reject));
                //after reject we have to play, otherwise we will be stuck (because reject does not generate an answer)
                player.Play();
            }
        }

        public void HandleMessage(Common.Schema.Game message)
        {
            Players = message.Players;
            Board = message.Board;
            Location = message.PlayerLocation;
            Fields = new Common.SchemaWrapper.Field[message.Board.width, 2 * message.Board.goalsHeight + message.Board.tasksHeight];
            ConsoleDebug.Good("Game started");
            player.Play();
        }

        public void HandleMessage(ConfirmJoiningGame message)
        {

            if (message == null)
                return;

            ConsoleDebug.Good(message.gameId.ToString());

            Id = message.playerId;
            GameId = message.gameId;
            Guid = message.privateGuid;
            Team = message.PlayerDefinition.team;
            Type = message.PlayerDefinition.type;

            return;
        }

        public void HandleMessage(RegisteredGames message)
        {
            if (message.GameInfo == null || message.GameInfo.Length == 0 || !message.GameInfo.Where(g => g.gameName == player.Options.GameName).Any())
            {
                Task.Run(() =>
                {
                    Thread.Sleep((int)player.Settings.RetryJoinGameInterval);
                    string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                    player.Send(xmlMessage);
                });
            }
            else
            {
                ConsoleDebug.Good("Games available");
                if (player.Options.GameName == null)
                {
                    ConsoleDebug.Warning("Game name not specified");
                    return;
                }
                if (message.GameInfo.Count(info => info.gameName == player.Options.GameName) == 1)
                {
                    string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                    {
                        gameName = player.Options.GameName,
                        playerIdSpecified = false,
                        preferredRole = player.Options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                        preferredTeam = player.Options?.PreferredTeam == "red" ? Common.Schema.TeamColour.red : Common.Schema.TeamColour.blue
                    });
                    player.Send(xmlMessage);
                }
            }
        }

        private static void FieldsUpdater(Common.SchemaWrapper.Field[,] oldTaskFields, Common.SchemaWrapper.Field[] newTaskFields)
        {
            foreach (Common.SchemaWrapper.Field taskField in newTaskFields)
            {
                oldTaskFields[taskField.X, taskField.Y] = taskField;
            }
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
            previousLocation = new Common.Schema.Location() { x = Location.x, y = Location.y };
            Move m = new Move()
            {
                direction = direction,
                directionSpecified = true,
                gameId = _gameId,
                playerGuid = _guid
            };
            player.Send(XmlMessageConverter.ToXml(m));
        }

        private void Discover()
        {
            Common.Schema.Discover d = new Discover()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            player.Send(XmlMessageConverter.ToXml(d));
        }

        private void PickUpPiece()
        {
            Common.Schema.PickUpPiece p = new PickUpPiece()
            {
                playerGuid = Guid,
                gameId = GameId
            };
            player.Send(XmlMessageConverter.ToXml(p));
        }

        private void PlacePiece()
        {
            Common.Schema.PlacePiece p = new PlacePiece()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            player.Send(XmlMessageConverter.ToXml(p));
            Pieces.Remove(CarriedPiece);
        }

        private void Test()
        {
            TestPiece t = new TestPiece()
            {
                gameId = GameId,
                playerGuid = Guid
            };
            player.Send(XmlMessageConverter.ToXml(t));
        }

        Common.SchemaWrapper.TaskField FieldAt(uint x, uint y)
        {
            try
            {
                return (Fields[x, y] as Common.SchemaWrapper.TaskField);
            }
            catch
            {
                return null;
            }
        }

        private Common.Schema.Location previousLocation = null;
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
            }.Where(u => u.HasValue && u != NO_PIECE).Select(u => u.Value);
            int? d;
            if (t.Count() == 0)
                d = NO_PIECE;
            else
            {
                d = (int?)t.Min();
            }
            if (d >= FieldAt(Location.x, Location.y)?.DistanceToPiece)
                Discover();
            else
                Move(Where(d));
        }

        int? DistToPiece()
        {
            return (Fields[Location.x, Location.y] as Common.SchemaWrapper.TaskField)?.DistanceToPiece;
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
                if (Team == Common.Schema.TeamColour.blue)
                    Move(MoveType.down);
                else
                    Move(MoveType.up);
                return;
            }
            var gf = Fields[Location.x, Location.y] as Common.SchemaWrapper.GoalField;
            if (gf == null || gf.Type == Common.Schema.GoalFieldType.goal || gf.Type == Common.Schema.GoalFieldType.unknown)
                PlacePiece();
            else
            {
                if (Team == Common.Schema.TeamColour.blue &&
                    (Location.y == 0 && Location.x % 2 == 0 ||
                     Location.y == Board.goalsHeight - 1 && Location.x % 2 == 1)
                    ||
                    Team == Common.Schema.TeamColour.red &&
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
                        pc.playerIdSpecified && pc.playerId == Id);
            return carriedPiece != null;
        }

        private MoveType RandomMoveType()
        {
            Array values = Enum.GetValues(typeof(MoveType));
            return (MoveType)values.GetValue(random.Next(values.Length));
        }

   

        public State BiuldDfa()
        {
            return new DfaBuilder()
               //finding piece
               .AddState("start")
               .AddState("discover", Discover)
               .AddTransition("start", "discover")
               .AddState("moving", MoveToNieghborClosestToPiece)
               .AddTransition("discover", "moving", () => DistToPiece() > 0 || DistToPiece() == null)
               .AddTransition("moving", "discover", () => DistToPiece() > 0 || DistToPiece() == null)
               .AddState("onPiece", PickUpPiece)
               //picking piece
               .AddTransition("discover", "onPiece", () => DistToPiece() == 0)
               .AddTransition("moving", "onPiece", () => DistToPiece() == 0)
               .AddState("notTested", Test)
               .AddTransition("onPiece", "notTested")
               .AddState("carryingNormal", LookForGoal)
               //testing piece
               .AddTransition("onPiece", "notTested", HasPiece)
               .AddTransition("onPiece", "discover", () => !HasPiece())
               .AddState("carryingSham", DestroySham)
               .AddState("shamPicked", Discover)
               .AddTransition("notTested", "carryingNormal",
                   () => CarriedPiece != null && CarriedPiece.type == PieceType.normal)
               .AddTransition("notTested", "shamPicked", () => CarriedPiece != null && CarriedPiece.type == PieceType.sham)
               .AddTransition("notTested", "discover", () => CarriedPiece == null)
               //destroying sham
               .AddTransition("shamPicked", "carryingSham")
               .AddTransition("carryingSham", "discover", () => !HasPiece())
               //place normal piece               
               .AddTransition("carryingNormal", "discover", () => !HasPiece())

               .StartingState();
        }

        public void PrintBoard()
        {
            BoardPrinter.Print(Fields);
        }
    }//class
}
