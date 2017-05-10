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

namespace Player.Net
{
    public class Game : IGame
    {
        private PlayerClient player;
        private PlayerSettings settings;
        private AgentCommandLineOptions options;
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

        public void HandleMessage(Game message)
        {
            Players = message.Players;
            Board = message.Board;
            Location = message.Location;
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
            if (message.GameInfo == null || message.GameInfo.Length == 0 || !message.GameInfo.Where(g => g.gameName == args.Options.GameName).Any())
            {
                Task.Run(() =>
                {
                    Thread.Sleep((int)settings.RetryJoinGameInterval);
                    string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                    player.Send(xmlMessage);
                });
            }
            else
            {
                ConsoleDebug.Good("Games available");
                if (options.GameName == null)
                {
                    ConsoleDebug.Warning("Game name not specified");
                    return;
                }
                if (message.GameInfo.Count(info => info.gameName == options.GameName) == 1)
                {
                    string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                    {
                        gameName = options.GameName,
                        playerIdSpecified = false,
                        preferredRole = options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                        preferredTeam = options?.PreferredTeam == "red" ? Common.Schema.TeamColour.red : Common.Schema.TeamColour.blue
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

    }//class
}
