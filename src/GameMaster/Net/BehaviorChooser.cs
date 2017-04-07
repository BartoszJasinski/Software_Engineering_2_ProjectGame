using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using Wrapper = Common.SchemaWrapper;
using System.Net.Sockets;
using System.Threading;
using GameMaster.Logic;
using GameMaster.Logic.Board;

namespace GameMaster.Net
{
    static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
    {
        public static void HandleMessage(ConfirmGameRegistration message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
            gameMaster.Id = message.gameId;
        }

        public static void HandleMessage(JoinGame message, GameMasterClient gameMaster, Socket handler)
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

            if (role == PlayerType.leader)
                selectedTeam.AddLeader(new Wrapper.Leader(message.playerId, guid, selectedTeam));
            else
                selectedTeam.Players.Add(new Wrapper.Player(message.playerId, guid, selectedTeam));

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

            if (gameMaster.IsReady)
            {
                //TODO Load Board from config file
                var boardGenerator = new RandomGoalBoardGenerator(5, 5, 3, 123);
                var board = boardGenerator.CreateBoard();
                gameMaster.Board = board;
                var players = gameMaster.Players.Select(p => p.SchemaPlayer).ToArray();
                foreach (var player in gameMaster.Players)
                {
                    var startGame = new Game()
                    {
                        Board = board.SchemaBoard,
                        playerId = player.Id,
                        PlayerLocation = new Location() {x = (uint) player.Id, y = 0},
                        Players = players
                    };
                    var gameString = XmlMessageConverter.ToXml(startGame);
                    //ConsoleDebug.Message(gameString);
                    gameMaster.Connection.SendFromClient(handler, gameString);
                }
                //place first pieces
                gameMaster.PlaceNewPieces((int) gameMaster.Settings.GameDefinition.InitialNumberOfPieces);
                //start infinite Piece place loop
                gameMaster.GeneratePieces();
            }
        }

        public static void HandleMessage(Move message, GameMasterClient gameMaster, Socket handler)
        {
            int dx, dy;
            Data resp = new Data();

            Task.Delay(new TimeSpan(0, 0, 0, 0, (int) gameMaster.Settings.ActionCosts.MoveDelay)).ContinueWith(_ =>
            {
                Wrapper.Player player = gameMaster.Players.First(p => message.playerGuid == p.Guid);
                gameMaster.Logger.Log(message, player);
                resp.playerId = player.Id;
                dx = message.direction == MoveType.right ? 1 : (message.direction == MoveType.left ? -1 : 0);
                dy = message.direction == MoveType.up ? 1 : (message.direction == MoveType.down ? -1 : 0);
                //if moving behind borders, dont move
                //also don't move where another player is
                //well, that escalated quickly  
                if (!message.directionSpecified ||
                    (player.Location.x + dx < 0 || player.Location.x + dx >= gameMaster.Board.Width) ||
                    (player.Location.y + dy < 0 ||
                     player.Location.y + dy >= gameMaster.Board.TasksHeight * 2 + gameMaster.Board.GoalsHeight)
                    ||
                    gameMaster.Players.Where(
                        p => p.Location.x == player.Location.x + dx && p.Location.y == player.Location.y + dy).Any())
                {
                    resp.PlayerLocation = player.Location;
                    gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
                    return;
                }
                resp.PlayerLocation = new Location()
                {
                    x = (uint) (player.Location.x + dx),
                    y = (uint) (player.Location.y + dy)
                };
                player.Location = resp.PlayerLocation;
                //TODO refactor into seperate class maybe
                //add info about new field
                var newField = gameMaster.Board.Fields[player.Location.x, player.Location.y];
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
                    if (pieceList.Count > 0)
                        resp.Pieces = pieceList.ToArray();
                }
                gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
            });
        }

        public static void HandleMessage(Discover message, GameMasterClient gameMaster, Socket handler)
        {
            Data resp = new Data();
            Task.Delay((int) gameMaster.Settings.ActionCosts.DiscoverDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Where(p => p.Guid == message.playerGuid).Single();
                var taskFields = new List<TaskField>();
                var pieceList = new List<Piece>();

                for (int i = (int) currentPlayer.Location.x - 1; i <= (int) currentPlayer.Location.x + 1; i++)
                {
                    if (i < 0 || i >= gameMaster.Board.Width) continue;
                    for (int j = (int) currentPlayer.Location.y - 1; j <= (int) currentPlayer.Location.y + 1; j++)
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
                if (taskFields.Count > 0)
                    resp.TaskFields = taskFields.ToArray();
                if (pieceList.Count > 0)
                    resp.Pieces = pieceList.ToArray();
                gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
            });
        }

        public static void HandleMessage(PickUpPiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            Task.Delay((int) gameMaster.Settings.ActionCosts.PickUpDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
                Wrapper.Piece piece =
                    gameMaster.Pieces.SingleOrDefault(
                        pc =>
                            pc.Location.x == currentPlayer.Location.x && pc.Location.y == currentPlayer.Location.y &&
                            !pc.PlayerId.HasValue);
                if (piece == null)
                {
                    //send empty piece collection
                    resp = new DataMessageBuilder(currentPlayer.Id)
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
                            (gameMaster.Board.Fields[currentPlayer.X + 1, currentPlayer.Y] as Wrapper.TaskField)
                            ?.DistanceToPiece,
                            (gameMaster.Board.Fields[currentPlayer.X - 1, currentPlayer.Y] as Wrapper.TaskField)
                            ?.DistanceToPiece,
                            (gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y + 1] as Wrapper.TaskField)
                            ?.DistanceToPiece,
                            (gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y - 1] as Wrapper.TaskField)
                            ?.DistanceToPiece
                        }.Where(u => u.HasValue).Select(u => u.Value).Min() + 1;
                    }
                    resp = new DataMessageBuilder(currentPlayer.Id)
                        .AddPiece(new Piece()
                        {
                            id = piece.Id,
                            timestamp = piece.TimeStamp,
                            playerId = currentPlayer.Id,
                            type = PieceType.unknown
                        })
                        .GetXml();
                }
                gameMaster.Connection.SendFromClient(handler, resp);
            });
        }

        public static void HandleMessage(TestPiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            Task.Delay((int) gameMaster.Settings.ActionCosts.TestDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
                Wrapper.Piece piece =
                    gameMaster.Pieces.SingleOrDefault(
                        pc =>
                            pc.Location.x == currentPlayer.Location.x && pc.Location.y == currentPlayer.Location.y &&
                            pc.PlayerId == currentPlayer.Id);
                if (piece == null)
                {
                    //send empty piece collection
                    resp = new DataMessageBuilder(currentPlayer.Id)
                        .SetPieces(new Piece[0])
                        .GetXml();
                }
                else
                {
                    resp = new DataMessageBuilder(currentPlayer.Id)
                        .AddPiece(piece.SchemaPiece)
                        .GetXml();
                }
                gameMaster.Connection.SendFromClient(handler, resp);
            });
        }

        public static void HandleMessage(PlacePiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            Task.Delay((int) gameMaster.Settings.ActionCosts.PlacingDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
                Wrapper.Piece carriedPiece =
                    gameMaster.Pieces.SingleOrDefault(
                        pc =>
                            pc.PlayerId == currentPlayer.Id);
                if (carriedPiece == null || !gameMaster.IsPlayerInGoalArea(currentPlayer) || carriedPiece.Type==PieceType.sham)
                {
                    //send empty piece collection
                    resp = new DataMessageBuilder(currentPlayer.Id)
                        .SetGoalFields(new GoalField[0])
                        .GetXml();
                    gameMaster.Connection.SendFromClient(handler, resp);
                    return;
                }
                Wrapper.GoalField gf = gameMaster.Board.Fields[currentPlayer.X, currentPlayer.Y] as Wrapper.GoalField;
                // remove piece and goal
                if (gf.Type == GoalFieldType.goal)
                {
                    gameMaster.Pieces.Remove(carriedPiece);
                    gf.Type = GoalFieldType.nongoal;
                }

                resp = new DataMessageBuilder(currentPlayer.Id)
                    .AddGoalField(gf.SchemaField as GoalField)
                    .GetXml();

                gameMaster.Connection.SendFromClient(handler, resp);
            });
        }


        public static void HandleMessage(object message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}