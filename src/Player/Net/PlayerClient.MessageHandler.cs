using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Player.Net
{
    public partial class PlayerClient
    {
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
                    Player.RegisterForNextGameAfterGameEnd();
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
                builder.SetPlayerLocation(Player.game.Location);

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
    } 
} //namespace