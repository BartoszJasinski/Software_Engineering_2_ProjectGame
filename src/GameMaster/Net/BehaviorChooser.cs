using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using Wrapper = Common.SchemaWrapper;
using System.Net.Sockets;
using GameMaster.Logic;

namespace GameMaster.Net
{
    public static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
    {
        public static void HandleMessage(ConfirmGameRegistration message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
            gameMaster.Id = message.gameId;
        }

        public static void HandleMessage(JoinGame message, GameMasterClient gameMaster, Socket handler)
        {

            lock (gameMaster.BoardLock)
            {
                var selectedTeam = gameMaster.SelectTeamForPlayer(message.teamColour);
                //both teams are full
                if (selectedTeam == null)
                {
                    gameMaster.Connection.SendFromClient(handler,
                        XmlMessageConverter.ToXml(new RejectJoiningGame()
                        {
                            gameName = message.gameName,
                            playerId = message.playerId
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
                var fieldForPlayer = gameMaster.Board.GetEmptyPositionForPlayer(selectedTeam.Color);
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
                answer.gameId = gameMaster.Id;
                answer.PlayerDefinition = new Player()
                {
                    id = message.playerId,
                    team = selectedTeam.Color,
                    type = role
                };
                var answerString = XmlMessageConverter.ToXml(answer);
                gameMaster.Connection.SendFromClient(handler, answerString);
            }



            if (gameMaster.IsReady)
            {
                var board = gameMaster.Board;
                var players = gameMaster.Players.Select(p => p.SchemaPlayer).ToArray();
                foreach (var player in gameMaster.Players)
                {
                    var startGame = new Game()
                    {
                        Board = board.SchemaBoard,
                        playerId = player.Id,
                        PlayerLocation = new Location() { x = player.X, y = player.Y },
                        Players = players
                    };
                    var gameString = XmlMessageConverter.ToXml(startGame);
                    //ConsoleDebug.Message(gameString);
                    gameMaster.Connection.SendFromClient(handler, gameString);
                }
                //place first pieces
                gameMaster.PlaceNewPieces((int)gameMaster.Settings.GameDefinition.InitialNumberOfPieces);
                //start infinite Piece place loop
                gameMaster.GeneratePieces();
            }
        }

        public static async Task HandleMessage(Move message, GameMasterClient gameMaster, Socket handler)
        {
            int dx, dy;
            Data resp = new Data();

            await Task.Delay((int)gameMaster.Settings.ActionCosts.MoveDelay);
            Wrapper.Player player = gameMaster.Players.First(p => message.playerGuid == p.Guid);
            gameMaster.Logger.Log(message, player);
            resp.playerId = player.Id;
            dx = message.direction == MoveType.right ? 1 : (message.direction == MoveType.left ? -1 : 0);
            dy = message.direction == MoveType.up ? 1 : (message.direction == MoveType.down ? -1 : 0);
            //if moving behind borders, dont move
            //also don't move where another player is
            //well, that escalated quickly  
            lock (gameMaster.BoardLock)
            {
                if (!message.directionSpecified ||
                    (player.Location.x + dx < 0 || player.Location.x + dx >= gameMaster.Board.Width) ||
                    (player.Location.y + dy < 0 ||
                         player.Location.y + dy >= gameMaster.Board.Height) ||
                        gameMaster.Players.Where(p => p.Location.x == player.Location.x + dx && p.Location.y == player.Location.y + dy).Any() ||
                        gameMaster.Board.IsInEnemyGoalArea(player.Location.y + dy, player.Team.Color))
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
                var oldField = gameMaster.Board.Fields[player.Location.x, player.Location.y];
                player.Location = resp.PlayerLocation;
                //TODO refactor into seperate class maybe
                //add info about new field
                var newField = gameMaster.Board.Fields[player.Location.x, player.Location.y];
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
                            gameMaster.Pieces.Where(p => p.Id == taskField.PieceId.Value)
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

        static Wrapper.TaskField FieldAt(Wrapper.Field[,] fields, uint x, uint y)
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

        public static async Task HandleMessage(Discover message, GameMasterClient gameMaster, Socket handler)
        {
            Data resp = new Data();
            await Task.Delay((int)gameMaster.Settings.ActionCosts.DiscoverDelay);
            Wrapper.Player currentPlayer = gameMaster.Players.Where(p => p.Guid == message.playerGuid).Single();
            gameMaster.Logger.Log(message, currentPlayer);
            resp.playerId = currentPlayer.Id;
            var taskFields = new List<TaskField>();
            var pieceList = new List<Piece>();

            lock (gameMaster.BoardLock)
            {
                for (int i = (int)currentPlayer.Location.x - 1; i <= (int)currentPlayer.Location.x + 1; i++)
                {
                    if (i < 0 || i >= gameMaster.Board.Width) continue;
                    for (int j = (int)currentPlayer.Location.y - 1; j <= (int)currentPlayer.Location.y + 1; j++)
                    {
                        if (j < 0 || j >= gameMaster.Board.Height) continue;
                        if (gameMaster.Board.Fields[i, j] is Wrapper.TaskField)
                        {
                            var taskField = gameMaster.Board.Fields[i, j] as Wrapper.TaskField;
                            taskField.AddFieldData(taskFields, null);
                            if (taskField.PieceId.HasValue)
                                pieceList.Add(
                                    gameMaster.Pieces.Where(p => p.Id == taskField.PieceId.Value)
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

        public static async Task HandleMessage(PickUpPiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)gameMaster.Settings.ActionCosts.PickUpDelay);
               Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
               gameMaster.Logger.Log(message, currentPlayer);
               lock (gameMaster.BoardLock)
               {
                   Wrapper.Piece piece =
                            gameMaster.Pieces.FirstOrDefault(
                            pc =>
                                pc.Location.x == currentPlayer.Location.x && pc.Location.y == currentPlayer.Location.y &&
                                !pc.PlayerId.HasValue);
                   if (piece == null || gameMaster.Pieces.Any(pc => pc.PlayerId == currentPlayer.Id))
                   {
                        //send empty piece collection
                        resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                            .SetPieces(new Piece[0])
                            .GetXml();
                   }
                   else
                   {
                       piece.PlayerId = currentPlayer.Id;
                       var taskField = gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.TaskField;
                       if (taskField != null) //update taskField
                        {
                           taskField.PieceId = null;
                            //clocest neighbour to piece + 1
                            taskField.DistanceToPiece = new[]
                            {
                            FieldAt(gameMaster.Board.Fields, currentPlayer.X + 1, currentPlayer.Y)
                            ?.DistanceToPiece,
                            FieldAt(gameMaster.Board.Fields, currentPlayer.X - 1, currentPlayer.Y)
                            ?.DistanceToPiece,
                            FieldAt(gameMaster.Board.Fields, currentPlayer.X, currentPlayer.Y + 1)
                            ?.DistanceToPiece,
                            FieldAt(gameMaster.Board.Fields, currentPlayer.X, currentPlayer.Y - 1)
                            ?.DistanceToPiece
                       }.Where(u => u.HasValue).Select(u => u.Value).Min() + 1;
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
               gameMaster.Connection.SendFromClient(handler, resp);
        }

        public static async void HandleMessage(TestPiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)gameMaster.Settings.ActionCosts.TestDelay);
               Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
               gameMaster.Logger.Log(message, currentPlayer);
               lock (gameMaster.BoardLock)
               {
                   ConsoleDebug.Warning("0");
                   Wrapper.Piece piece =
                        gameMaster.Pieces.SingleOrDefault(
                            pc =>
                                pc.PlayerId == currentPlayer.Id);
                   ConsoleDebug.Warning("1");
                   if (piece == null) // not carrying anything
                    {
                       piece = gameMaster.Pieces.SingleOrDefault(pc => pc.Location.Equals(currentPlayer.Location));
                   }
                   ConsoleDebug.Warning("2");
                   if (piece == null)
                   {
                        //send empty piece collection
                        resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                            .SetPieces(new Piece[0])
                            .GetXml();
                   }
                   else
                   {
                       resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                            .AddPiece(piece.SchemaPiece)
                            .GetXml();
                   }
                   ConsoleDebug.Warning("3");
               }
               gameMaster.Connection.SendFromClient(handler, resp);
        }

        public static async Task HandleMessage(PlacePiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            await Task.Delay((int)gameMaster.Settings.ActionCosts.PlacingDelay);
               Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
               gameMaster.Logger.Log(message, currentPlayer);
               var dmb = new DataMessageBuilder(currentPlayer.Id);
               lock (gameMaster.BoardLock)
               {
                   Wrapper.Piece carriedPiece =
                        gameMaster.Pieces.SingleOrDefault(
                            pc =>
                                pc.PlayerId == currentPlayer.Id);

                   if (carriedPiece != null && !gameMaster.IsPlayerInGoalArea(currentPlayer))
                   {
                       Wrapper.Piece lyingPiece =
                           gameMaster.Pieces.SingleOrDefault(
                               pc =>
                                   pc.PlayerId != currentPlayer.Id && pc.Location.Equals(currentPlayer.Location));

                       if (lyingPiece == null) //leaving our piece there
                        {
                           carriedPiece.PlayerId = null;
                           carriedPiece.Location.x = currentPlayer.Location.x;
                           carriedPiece.Location.y = currentPlayer.Location.y;
                           (gameMaster.Board.Fields[carriedPiece.Location.x, carriedPiece.Location.y] as Wrapper.TaskField).PieceId = carriedPiece.Id;
                           gameMaster.Board.UpdateDistanceToPiece(gameMaster.Pieces);
                       }
                       else //destroying piece
                        {
                           gameMaster.Pieces.Remove(carriedPiece);
                       }
                       Wrapper.TaskField tf =
                           gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.TaskField;
                       resp = dmb.AddTaskField(tf.SchemaField as TaskField).GetXml();
                       gameMaster.Connection.SendFromClient(handler, resp);
                       return;
                   }
                   if (carriedPiece == null ||
                       carriedPiece.Type == PieceType.sham)
                   {
                       if (gameMaster.IsPlayerInGoalArea(currentPlayer))
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
                       gameMaster.Connection.SendFromClient(handler, resp);
                       return;
                   }
                   Wrapper.GoalField gf = gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.GoalField;
                    // remove piece and goal
                    if (gf.Type == GoalFieldType.goal)
                   {
                       gf.Type = GoalFieldType.nongoal;
                   }
                   gameMaster.Pieces.Remove(carriedPiece);

                   bool blueWon = false;

                   MakeDecision.EndGame(gameMaster.Board, TeamColour.blue);
                   if (MakeDecision.endGame)
                       blueWon = true;
                   else
                       MakeDecision.EndGame(gameMaster.Board, TeamColour.red);

                   if (MakeDecision.endGame)
                   {
                       if (blueWon)
                           ConsoleDebug.Good("Blue team won!");
                       else
                           ConsoleDebug.Good("Red team won!");
                       BoardPrinter.Print(gameMaster.Board);

                       foreach (var player in gameMaster.Players)
                       {
                           string endGameResponse = new DataMessageBuilder(player.Id, MakeDecision.endGame)
                                .SetWrapperTaskFields(gameMaster.Board.GetTaskFields())
                                .SetWrapperGoalFields(gameMaster.Board.GetGoalFields())
                                .SetWrapperPieces(gameMaster.Pieces)
                                .SetPlayerLocation(player.Location)
                                .GetXml();

                           gameMaster.Connection.SendFromClient(handler, endGameResponse);
                       }

                       gameMaster.CancelToken.Cancel();
                   }

                   resp = new DataMessageBuilder(currentPlayer.Id, MakeDecision.endGame)
                    .AddGoalField(gf.SchemaField as GoalField)
                    .GetXml();
               }
               gameMaster.Connection.SendFromClient(handler, resp);

        }

        public static async Task HandleMessage(AuthorizeKnowledgeExchange message, GameMasterClient gameMaster, Socket handler)
        {
            var playerFrom = gameMaster.Players.Where(p => p.Guid == message.playerGuid).Single();
            var playerTo = gameMaster.Players.Where(p => p.Id == message.withPlayerId).SingleOrDefault();

            await Task.Delay((int)gameMaster.Settings.ActionCosts.KnowledgeExchangeDelay);
               if (playerTo == null)
               {
                   var permanentReject = new RejectKnowledgeExchange()
                   {
                       permanent = true,
                       playerId = playerFrom.Id
                   };
                   gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(permanentReject));
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
                   gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(accept));
                   return;
               }

               var response = new KnowledgeExchangeRequest()
               {
                   playerId = playerTo.Id,
                   senderPlayerId = playerFrom.Id
               };

               playerFrom.OpenExchangeRequests.Add(playerTo.Id);
               gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(response));
        }

        public static void HandleMessage(object message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}