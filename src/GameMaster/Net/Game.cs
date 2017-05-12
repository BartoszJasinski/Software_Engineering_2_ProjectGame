using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.Message;
using GameMaster.Logic;
using Common.DebugUtils;
using Wrapper = Common.SchemaWrapper;
using System.Net.Sockets;
using GameMaster.Logic.Board;

namespace GameMaster.Net
{
    public class Game : IMessageHandler
    {
        private GameMasterClient gameMaster;

        //The two teams
        public Wrapper.Team TeamRed { get; set; }
        public Wrapper.Team TeamBlue { get; set; }
        public IEnumerable<Wrapper.Player> Players
        {
            get
            {
                return TeamRed.Players.Concat(TeamBlue.Players);
            }
        }

        public ulong gameId { get; set; }

        public object BoardLock { get; set; } = new object();

        public bool IsReady => TeamRed.IsFull && TeamBlue.IsFull;
        public Wrapper.AddressableBoard Board { get; set; }

        public GameMasterClient GameMasterClient
        {
            get
            {
                return gameMaster;
            }

            set
            {
                gameMaster = value;
                InitializeTeams();
                InitializeBoard();
            }
        }

        private void InitializeTeams()
        {
            TeamRed = new Wrapper.Team(TeamColour.red, uint.Parse(gameMaster.Settings.GameDefinition.NumberOfPlayersPerTeam));
            TeamBlue = new Wrapper.Team(TeamColour.blue, uint.Parse(gameMaster.Settings.GameDefinition.NumberOfPlayersPerTeam));
        }

        private void InitializeBoard()
        {
            //var boardGenerator = new RandomGoalBoardGenerator(uint.Parse(Settings.GameDefinition.BoardWidth),
            //    uint.Parse(Settings.GameDefinition.TaskAreaLength),
            //    uint.Parse(Settings.GameDefinition.GoalAreaLength),
            //    123);
            var boardGenerator = new SimpleBoardGenerator(uint.Parse(gameMaster.Settings.GameDefinition.BoardWidth),
                uint.Parse(gameMaster.Settings.GameDefinition.TaskAreaLength),
                uint.Parse(gameMaster.Settings.GameDefinition.GoalAreaLength),
                gameMaster.Settings.GameDefinition.Goals);
            Board = boardGenerator.CreateBoard();
        }

        public IList<Wrapper.Piece> Pieces = new List<Wrapper.Piece>();//TODO pieces are not added to this collection

        private Random rng = new Random();

        public void HandleMessage(ConfirmGameRegistration message, Socket handler)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
            gameId = message.gameId;
        }

        public void HandleMessage(JoinGame message, Socket handler)
        {

            lock (BoardLock)
            {
                var selectedTeam = SelectTeamForPlayer(message.preferredTeam);
                //both teams are full
                if (selectedTeam == null)
                {
                    gameMaster.Connection.SendFromClient(handler,
                        XmlMessageConverter.ToXml(new RejectJoiningGame()
                        {
                            gameName = message.gameName,
                            playerId = message.playerId
                        }));

                    gameMaster.Connection.SendFromClient(handler,
                        XmlMessageConverter.ToXml(new GameStarted()
                        {
                            gameId = this.gameId
                        }));

                    return;
                }

                var role = message.preferredRole;
                if (role == PlayerType.leader)
                {
                    if (selectedTeam.HasLeader)
                        role = PlayerType.member;
                }

                var guid = Utils.GenerateGuid();
                var fieldForPlayer = Board.GetEmptyPositionForPlayer(selectedTeam.Color);
                fieldForPlayer.PlayerId = message.playerId;

                if (role == PlayerType.leader)
                    selectedTeam.AddLeader(new Wrapper.Leader(message.playerId, guid, selectedTeam, fieldForPlayer.X,
                        fieldForPlayer.Y));
                else
                    selectedTeam.Players.Add(new Wrapper.Player(message.playerId, guid, selectedTeam, fieldForPlayer.X,
                        fieldForPlayer.Y));

                var answer = new ConfirmJoiningGame();
                answer.playerId = message.playerId;
                answer.privateGuid = guid;
                answer.gameId = this.gameId;
                answer.PlayerDefinition = new Player()
                {
                    id = message.playerId,
                    team = selectedTeam.Color,
                    type = role
                };
                var answerString = XmlMessageConverter.ToXml(answer);
                gameMaster.Connection.SendFromClient(handler, answerString);
            }



            if (IsReady)
            {
                var board = Board;
                var players = Players.Select(p => p.SchemaPlayer).ToArray();
                foreach (var player in Players)
                {
                    var startGame = new Common.Schema.Game()
                    {
                        Board = board.SchemaBoard,
                        playerId = player.Id,
                        PlayerLocation = new Location() { x = player.X, y = player.Y },
                        Players = players
                    };
                    var gameString = XmlMessageConverter.ToXml(startGame);
                    //ConsoleDebug.Message(gameString);
                    gameMaster.Connection.SendFromClient(handler, gameString);
                    //send GameStarted message to server so it won't show it as an open game
                    var gameStarted = new GameStarted() { gameId = gameId };
                    var startedString = XmlMessageConverter.ToXml(gameStarted);
                    gameMaster.Connection.SendFromClient(handler, startedString);
                }
                //place first pieces
                for (int i = 0; i < gameMaster.Settings.GameDefinition.InitialNumberOfPieces; i++)
                {
                    PlaceNewPiece(Board.GetRandomEmptyFieldInTaskArea());
                }
                //start infinite Piece place loop
                GeneratePieces();
            }
        }

        public async Task HandleMessage(Move message, Socket handler)
        {
            int dx, dy;
            Data resp = new Data();

            await Task.Delay((int)gameMaster.Settings.ActionCosts.MoveDelay);
            Wrapper.Player player = Players.First(p => message.playerGuid == p.Guid);
            gameMaster.Logger.Log(message, player);
            resp.playerId = player.Id;
            dx = message.direction == MoveType.right ? 1 : (message.direction == MoveType.left ? -1 : 0);
            dy = message.direction == MoveType.up ? 1 : (message.direction == MoveType.down ? -1 : 0);
            //if moving behind borders, dont move
            //also don't move where another player is
            //well, that escalated quickly  
            lock (BoardLock)
            {
                if (!message.directionSpecified ||
                    (player.Location.x + dx < 0 || player.Location.x + dx >= Board.Width) ||
                    (player.Location.y + dy < 0 ||
                         player.Location.y + dy >= Board.Height) ||
                        Players.Where(p => p.Location.x == player.Location.x + dx && p.Location.y == player.Location.y + dy).Any() ||
                        Board.IsInEnemyGoalArea(player.Location.y + dy, player.Team.Color))
                {
                    resp.PlayerLocation = player.Location;
                    gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
                    return;
                }

                resp.PlayerLocation = new Location()
                {
                    x = (uint)(player.Location.x + dx),
                    y = (uint)(player.Location.y + dy)
                };
                var oldField = Board.Fields[player.Location.x, player.Location.y];
                player.Location = resp.PlayerLocation;
                //TODO refactor into seperate class maybe
                //add info about new field
                var newField = Board.Fields[player.Location.x, player.Location.y];
                oldField.PlayerId = null;
                newField.PlayerId = player.Id;
                var taskFields = new List<TaskField>();
                var pieceList = new List<Piece>();
                //add info about piece
                if (newField is Wrapper.TaskField)
                {
                    var taskField = newField as Wrapper.TaskField;
                    taskField.AddFieldData(taskFields, null);
                    resp.TaskFields = taskFields.ToArray();
                    if (taskField.PieceId.HasValue)
                        pieceList.Add(
                            Pieces.Where(p => p.Id == taskField.PieceId.Value)
                                .Select(p => p.SchemaPiece)
                                .Single());
                    //we cannot give the player info about piece type from a discover
                    pieceList = pieceList.Select(piece => new Piece()
                    {
                        playerId = piece.playerId,
                        id = piece.id,
                        playerIdSpecified = piece.playerIdSpecified,
                        timestamp = piece.timestamp,
                        type = PieceType.unknown
                    }).ToList();
                    if (pieceList.Count > 0)
                        resp.Pieces = pieceList.ToArray();
                }
            }
            gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
        }

        Wrapper.TaskField FieldAt(Wrapper.Field[,] fields, uint x, uint y)
        {
            try
            {
                return (fields[x, y] as Wrapper.TaskField);
            }
            catch
            {
                return null;
            }
        }

        public async Task HandleMessage(Discover message, Socket handler)
        {
            Data resp = new Data();
            await Task.Delay((int)gameMaster.Settings.ActionCosts.DiscoverDelay);
            Wrapper.Player currentPlayer = Players.Where(p => p.Guid == message.playerGuid).Single();
            gameMaster.Logger.Log(message, currentPlayer);
            resp.playerId = currentPlayer.Id;
            var taskFields = new List<TaskField>();
            var pieceList = new List<Piece>();

            lock (BoardLock)
            {
                for (int i = (int)currentPlayer.Location.x - 1; i <= (int)currentPlayer.Location.x + 1; i++)
                {
                    if (i < 0 || i >= Board.Width) continue;
                    for (int j = (int)currentPlayer.Location.y - 1; j <= (int)currentPlayer.Location.y + 1; j++)
                    {
                        if (j < 0 || j >= Board.Height) continue;
                        if (Board.Fields[i, j] is Wrapper.TaskField)
                        {
                            var taskField = Board.Fields[i, j] as Wrapper.TaskField;
                            taskField.AddFieldData(taskFields, null);
                            if (taskField.PieceId.HasValue)
                                pieceList.Add(
                                    Pieces.Where(p => p.Id == taskField.PieceId.Value)
                                        .Select(p => p.SchemaPiece)
                                        .Single());
                        }
                    }
                }
            }
            if (taskFields.Count > 0)
                resp.TaskFields = taskFields.ToArray();
            //we cannot give the player info about piece type from a discover
            pieceList = pieceList.Select(piece => new Piece()
            {
                playerId = piece.playerId,
                id = piece.id,
                playerIdSpecified = piece.playerIdSpecified,
                timestamp = piece.timestamp,
                type = PieceType.unknown
            }).ToList();
            if (pieceList.Count > 0)
                resp.Pieces = pieceList.ToArray();
            resp.playerId = currentPlayer.Id;
            gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
        }

        public async Task HandleMessage(PickUpPiece message, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)gameMaster.Settings.ActionCosts.PickUpDelay);
            Wrapper.Player currentPlayer = Players.Single(p => p.Guid == message.playerGuid);
            gameMaster.Logger.Log(message, currentPlayer);
            lock (BoardLock)
            {
                Wrapper.Piece piece =
                         Pieces.FirstOrDefault(
                         pc =>
                             pc.Location.x == currentPlayer.Location.x && pc.Location.y == currentPlayer.Location.y &&
                             !pc.PlayerId.HasValue);
                if (piece == null || Pieces.Any(pc => pc.PlayerId == currentPlayer.Id))
                {
                    ConsoleDebug.Warning("No piece here or you have already a piece!");
                    //send empty piece collection
                    resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                        .SetPieces(new Piece[0])
                        .GetXml();
                }
                else
                {
                    ConsoleDebug.Warning("Piece picked up!");
                    piece.PlayerId = currentPlayer.Id;
                    var taskField = Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.TaskField;
                    if (taskField != null) //update taskField
                    {
                        ConsoleDebug.Warning("Updating TaskField");
                        taskField.PieceId = null;
                        Board.UpdateDistanceToPiece(Pieces);
                        //clocest neighbour to piece + 1
                        // taskField.DistanceToPiece = new[]
                        // {
                        //     FieldAt(gameMaster.Board.Fields, currentPlayer.X + 1, currentPlayer.Y)
                        //     ?.DistanceToPiece,
                        //     FieldAt(gameMaster.Board.Fields, currentPlayer.X - 1, currentPlayer.Y)
                        //     ?.DistanceToPiece,
                        //     FieldAt(gameMaster.Board.Fields, currentPlayer.X, currentPlayer.Y + 1)
                        //     ?.DistanceToPiece,
                        //     FieldAt(gameMaster.Board.Fields, currentPlayer.X, currentPlayer.Y - 1)
                        //     ?.DistanceToPiece
                        //}.Where(u => u.HasValue).Select(u => u.Value).Min() + 1;
                    }
                    resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                         .AddPiece(new Piece()
                         {
                             id = piece.Id,
                             timestamp = piece.TimeStamp,
                             playerId = currentPlayer.Id,
                             playerIdSpecified = true,
                             type = PieceType.unknown
                         })
                         .GetXml();
                }
            }
            GameMasterClient.Connection.SendFromClient(handler, resp);
        }

        public async void HandleMessage(TestPiece message, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)GameMasterClient.Settings.ActionCosts.TestDelay);
            Wrapper.Player currentPlayer = Players.Single(p => p.Guid == message.playerGuid);
            GameMasterClient.Logger.Log(message, currentPlayer);
            lock (BoardLock)
            {
                Wrapper.Piece piece =
                     Pieces.SingleOrDefault(
                         pc =>
                             pc.PlayerId == currentPlayer.Id);
                if (piece == null) // not carrying anything
                {
                    ConsoleDebug.Warning("Not carrying a piece!");
                    piece = Pieces.SingleOrDefault(pc => pc.Location.Equals(currentPlayer.Location));
                }
                if (piece == null)
                {
                    ConsoleDebug.Warning("Not on a piece!");
                    //send empty piece collection
                    resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                        .SetPieces(new Piece[0])
                        .GetXml();
                }
                else
                {
                    ConsoleDebug.Warning("On a piece!");
                    resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                         .AddPiece(piece.SchemaPiece)
                         .GetXml();
                }
            }
            GameMasterClient.Connection.SendFromClient(handler, resp);
        }

        public async Task HandleMessage(PlacePiece message, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)GameMasterClient.Settings.ActionCosts.PlacingDelay);
            Wrapper.Player currentPlayer = Players.Single(p => p.Guid == message.playerGuid);
            GameMasterClient.Logger.Log(message, currentPlayer);
            var dmb = new DataMessageBuilder(currentPlayer.Id);
            lock (BoardLock)
            {
                Wrapper.Piece carriedPiece =
                     Pieces.SingleOrDefault(
                         pc =>
                             pc.PlayerId == currentPlayer.Id);

                if (carriedPiece != null && !IsPlayerInGoalArea(currentPlayer))
                {
                    Wrapper.Piece lyingPiece =
                        Pieces.SingleOrDefault(
                            pc =>
                                pc.PlayerId != currentPlayer.Id && pc.Location.Equals(currentPlayer.Location));

                    if (lyingPiece == null) //leaving our piece there
                    {
                        carriedPiece.PlayerId = null;
                        carriedPiece.Location.x = currentPlayer.Location.x;
                        carriedPiece.Location.y = currentPlayer.Location.y;
                        (Board.Fields[carriedPiece.Location.x, carriedPiece.Location.y] as Wrapper.TaskField).PieceId = carriedPiece.Id;
                        Board.UpdateDistanceToPiece(Pieces);
                    }
                    else //destroying piece
                    {
                        Pieces.Remove(carriedPiece);
                    }
                    Wrapper.TaskField tf =
                        Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.TaskField;
                    resp = dmb.AddTaskField(tf.SchemaField as TaskField).GetXml();
                    GameMasterClient.Connection.SendFromClient(handler, resp);
                    return;
                }
                if (carriedPiece == null ||
                    carriedPiece.Type == PieceType.sham)
                {
                    if (IsPlayerInGoalArea(currentPlayer))
                    {
                        //send empty piece collection
                        resp = dmb
                   .SetGoalFields(new GoalField[0])
                   .GetXml();
                    }
                    else
                    {
                        resp = dmb
                            .SetTaskFields(new TaskField[0])
                            .GetXml();
                    }
                    GameMasterClient.Connection.SendFromClient(handler, resp);
                    return;
                }
                Wrapper.GoalField gf = Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.GoalField;
                // remove piece and goal
                if (gf.Type == GoalFieldType.goal)
                {
                    gf.Type = GoalFieldType.nongoal;
                }
                Pieces.Remove(carriedPiece);

                bool blueWon = false;

                MakeDecision.EndGame(Board, TeamColour.blue);
                if (MakeDecision.endGame)
                    blueWon = true;
                else
                    MakeDecision.EndGame(Board, TeamColour.red);

                if (MakeDecision.endGame)
                {
                    EndGameEventArgs args;
                    GameMasterClient.CancelToken.Cancel();

                    if (blueWon)
                    {
                        ConsoleDebug.Good("Blue team won!");
                        args = new EndGameEventArgs(TeamBlue, TeamRed);
                    }
                    else
                    {
                        ConsoleDebug.Good("Red team won!");
                        args = new EndGameEventArgs(TeamRed, TeamBlue);
                    }

                    foreach (var player in Players)
                    {
                        string endGameResponse = new DataMessageBuilder(player.Id, MakeDecision.endGame)
                             .SetWrapperTaskFields(Board.GetTaskFields())
                             .SetWrapperGoalFields(Board.GetGoalFields())
                             .SetWrapperPieces(Pieces)
                             .SetPlayerLocation(player.Location)
                             .GetXml();

                        GameMasterClient.Connection.SendFromClient(handler, endGameResponse);
                    }

                    OnGameEnd(this, args);
                }

                resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                 .AddGoalField(gf.SchemaField as GoalField)
                 .GetXml();
            }
            GameMasterClient.Connection.SendFromClient(handler, resp);

        }

        public async Task HandleMessage(AuthorizeKnowledgeExchange message, Socket handler)
        {
            var playerFrom = Players.Where(p => p.Guid == message.playerGuid).Single();
            var playerTo = Players.Where(p => p.Id == message.withPlayerId).SingleOrDefault();

            await Task.Delay((int)GameMasterClient.Settings.ActionCosts.KnowledgeExchangeDelay);
            if (playerTo == null)
            {
                var permanentReject = new RejectKnowledgeExchange()
                {
                    permanent = true,
                    playerId = playerFrom.Id
                };
                GameMasterClient.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(permanentReject));
                return;
            }

            if (playerTo.OpenExchangeRequests.Contains(playerFrom.Id))
            {
                var accept = new AcceptExchangeRequest()
                {
                    playerId = playerTo.Id,
                    senderPlayerId = playerFrom.Id
                };

                playerTo.OpenExchangeRequests.Remove(playerFrom.Id);
                GameMasterClient.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(accept));
                return;
            }

            var response = new KnowledgeExchangeRequest()
            {
                playerId = playerTo.Id,
                senderPlayerId = playerFrom.Id
            };

            playerFrom.OpenExchangeRequests.Add(playerTo.Id);
            GameMasterClient.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(response));
        }

        public void HandleMessage(PlayerDisconnected message, Socket handler)
        {
            ConsoleDebug.Error($"Player disconnected! Player id: {message.playerId}");
        }

        public void HandleMessage(object message, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        private static ulong pieceid = 0;

        public event EventHandler<EndGameEventArgs> OnGameEnd;

        public void PlaceNewPiece(Common.SchemaWrapper.TaskField field)
        {

            var pieceType = rng.NextDouble() < GameMasterClient.Settings.GameDefinition.ShamProbability ? PieceType.sham : PieceType.normal;
            lock (BoardLock)
            {
                var newPiece = new Wrapper.Piece((ulong)Pieces.Count, pieceType, DateTime.Now);
                newPiece.Id = pieceid++;
                if (field == null)
                {
                    ConsoleDebug.Warning("There are no empty places for a new Piece!");
                    return;   //TODO BUSYWAITING HERE probably
                }
                //remove old piece
                if (field.PieceId != null)
                {
                    var oldPiece = Pieces.Where(p => p.Id == field.PieceId.Value).Single();
                    Pieces.Remove(oldPiece);
                }
                field.PieceId = newPiece.Id;
                newPiece.Location = new Location() { x = field.X, y = field.Y };
                Pieces.Add(newPiece);
                Board.UpdateDistanceToPiece(Pieces);
                ConsoleDebug.Good($"Placed new Piece at: ({ field.X }, {field.Y})");
            }
            //BoardPrinter.Print(Board);
        }

        public void PrintBoard()
        {
            BoardPrinter.PrintAlternative(Board);
        }

        public Wrapper.Team SelectTeamForPlayer(TeamColour preferredTeam)
        {
            var selectedTeam = preferredTeam == TeamColour.blue ? TeamBlue : TeamRed;
            var otherTeam = preferredTeam == TeamColour.blue ? TeamRed : TeamBlue;

            if (selectedTeam.IsFull)
                selectedTeam = otherTeam;

            //both teams are full
            if (selectedTeam.IsFull)
            {
                return null;
            }

            return selectedTeam;
        }
        public bool IsPlayerInGoalArea(Wrapper.Player p)
        {
            if (p.Team.Color == TeamColour.blue && p.Y < Board.GoalsHeight)
                return true;
            return p.Team.Color == TeamColour.red && p.Y >= Board.Height - Board.GoalsHeight;
        }

        public async Task GeneratePieces()
        {
            while (true)
            {
                if (gameMaster.CancelToken.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromMilliseconds(gameMaster.Settings.GameDefinition.PlacingNewPiecesFrequency));
                if (gameMaster.CancelToken.Token.IsCancellationRequested)
                    break;
                PlaceNewPiece(Board.GetRandomEmptyFieldInTaskArea());
            }
        }
    }
}
