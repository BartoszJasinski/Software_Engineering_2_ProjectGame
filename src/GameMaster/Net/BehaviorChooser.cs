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
                        PlayerLocation = new Location() {x = player.X, y = player.Y},
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
                     player.Location.y + dy >= gameMaster.Board.Height)
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
                gameMaster.Connection.SendFromClient(handler, XmlMessageConverter.ToXml(resp));
            });
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

        public static void HandleMessage(Discover message, GameMasterClient gameMaster, Socket handler)
        {
            Data resp = new Data();
            Task.Delay((int) gameMaster.Settings.ActionCosts.DiscoverDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Where(p => p.Guid == message.playerGuid).Single();
                gameMaster.Logger.Log(message, currentPlayer);
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
            });
        }

        public static void HandleMessage(PickUpPiece message, GameMasterClient gameMaster, Socket handler)
        {
            string resp = "";
            Task.Delay((int) gameMaster.Settings.ActionCosts.PickUpDelay).ContinueWith(_ =>
            {
                Wrapper.Player currentPlayer = gameMaster.Players.Single(p => p.Guid == message.playerGuid);
                gameMaster.Logger.Log(message, currentPlayer);
                Wrapper.Piece piece =
                    gameMaster.Pieces.FirstOrDefault(
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
                    resp = new DataMessageBuilder(currentPlayer.Id)
                        .AddPiece(new Common.Schema.Piece()
                        {
                            id = piece.Id,
                            timestamp = piece.TimeStamp,
                            playerId = currentPlayer.Id,
                            playerIdSpecified = true,
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
                gameMaster.Logger.Log(message, currentPlayer);
                Wrapper.Piece piece =
                    gameMaster.Pieces.SingleOrDefault(
                        pc =>                            
                            pc.PlayerId == currentPlayer.Id);
                if (piece == null) // not carrying anything
                {
                    piece = gameMaster.Pieces.SingleOrDefault(pc => pc.Location.Equals(currentPlayer.Location));
                }
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
                gameMaster.Logger.Log(message, currentPlayer);
                var dmb = new DataMessageBuilder(currentPlayer.Id);
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
                        gameMaster.Board.UpdateDistanceToPiece(new[] {carriedPiece}.ToList());
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


                resp = new DataMessageBuilder(currentPlayer.Id)
                    .AddGoalField(gf.SchemaField as GoalField)
                    .GetXml();

                gameMaster.Connection.SendFromClient(handler, resp);
            });
        }

        public static void HandleMessage(AuthorizeKnowledgeExchange message, GameMasterClient gameMaster, Socket handler)
        {
            var playerFrom = gameMaster.Players.Where(p => p.Guid == message.playerGuid).Single();
            var playerTo = gameMaster.Players.Where(p => p.Id == message.withPlayerId).SingleOrDefault();

            Task.Delay((int) gameMaster.Settings.ActionCosts.KnowledgeExchangeDelay).ContinueWith(_ =>
            {
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
            });
        }

        public static void HandleMessage(object message, GameMasterClient gameMaster, Socket handler)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}