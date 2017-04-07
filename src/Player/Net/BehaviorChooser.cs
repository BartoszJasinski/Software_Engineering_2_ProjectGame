using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using System.Net.Sockets;

namespace Player.Net
{
    public static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
    {
        public static void HandleMessage(RegisteredGames message, PlayerMessageHandleArgs args)
        {
            if (message.GameInfo == null || message.GameInfo.Length == 0)
            {
                Task.Run(() =>
                {
                    Thread.Sleep((int) args.Settings.RetryJoinGameInterval);
                    string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                    args.Connection.SendFromClient(args.Socket, xmlMessage);
                });
            }
            else
            {
                ConsoleDebug.Good("Games available");
                if (args.Options.GameName == null)
                {
                    ConsoleDebug.Warning("Game name not specified");
                    return;
                }
                if (message.GameInfo.Count(info => info.gameName == args.Options.GameName) == 1)
                {
                    string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                    {
                        gameName = args.Options.GameName,
                        playerIdSpecified = false,
                        preferredRole = args.Options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                        teamColour = args.Options?.PreferredColor == "red" ? TeamColour.red : TeamColour.blue
                    });
                    args.Connection.SendFromClient(args.Socket, xmlMessage);
                }
            }
        }

        public static void HandleMessage(ConfirmJoiningGame message, PlayerMessageHandleArgs args)
        {
            if (message == null)
                return;

            args.PlayerClient.Id = message.playerId;
            args.PlayerClient.GameId = message.gameId;
            args.PlayerClient.Guid = message.privateGuid;
            return;
        }

        public static void HandleMessage(RejectJoiningGame message, PlayerMessageHandleArgs args)
        {
            if (message == null)
                return;
            //try to connect again
            args.Connection.SendFromClient(args.Socket, XmlMessageConverter.ToXml(new GetGames()));
            
            return;
        }

        public static void HandleMessage(Game message, PlayerMessageHandleArgs args)
        {
            args.PlayerClient.Players = message.Players;
            args.PlayerClient.Board = message.Board;
            args.PlayerClient.Location = message.PlayerLocation;
            args.PlayerClient.TaskFields = new Common.SchemaWrapper.TaskField[message.Board.width, message.Board.tasksHeight];
            args.PlayerClient.GoalFields = new Common.SchemaWrapper.GoalField[message.Board.width, message.Board.goalsHeight];
            ConsoleDebug.Good("Game started");
            args.PlayerClient.Play();
            
        }

        public static void HandleMessage(Data message, PlayerMessageHandleArgs args)
        {
            if (message.PlayerLocation != null)
            {
                args.PlayerClient.Location = message.PlayerLocation;
                ConsoleDebug.Message($"My location: ({message.PlayerLocation.x}, {message.PlayerLocation.y})");
            }
            if (message.TaskFields != null)
            {
                Common.SchemaWrapper.TaskField[] taskFields = new Common.SchemaWrapper.TaskField[message.TaskFields.Length];
                for (int i = 0; i < taskFields.Length; i++)
                    taskFields[i] = new Common.SchemaWrapper.TaskField(message.TaskFields[i]);
                FieldsUpdater(args.PlayerClient.TaskFields, taskFields); ;
            }
            if (message.GoalFields != null)
            {
                Common.SchemaWrapper.GoalField[] goalFields = new Common.SchemaWrapper.GoalField[message.GoalFields.Length];
                for (int i = 0; i < goalFields.Length; i++)
                    goalFields[i] = new Common.SchemaWrapper.GoalField(message.GoalFields[i]);
                FieldsUpdater(args.PlayerClient.GoalFields, goalFields);
            }
            if (message.Pieces != null)
            {
                foreach (Piece piece in message.Pieces)
                {
                    args.PlayerClient.TaskFields[args.PlayerClient.Location.x, args.PlayerClient.Location.y].PieceId = piece.id;
                    args.PlayerClient.TaskFields[args.PlayerClient.Location.x, args.PlayerClient.Location.y].Timestamp = piece.timestamp;
                    args.PlayerClient.TaskFields[args.PlayerClient.Location.x, args.PlayerClient.Location.y].PlayerId = piece.playerId;
                    args.PlayerClient.TaskFields[args.PlayerClient.Location.x, args.PlayerClient.Location.y].DistanceToPiece = 0;
                }
            }
            if (message.gameFinished == true)
            {
                args.PlayerClient.Disconnect();
            }

            args.PlayerClient.Play();
        }

        public static void HandleMessage(object message, PlayerMessageHandleArgs args)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        private static void FieldsUpdater(Common.SchemaWrapper.Field[,] oldTaskFields, Common.SchemaWrapper.Field[] newTaskFields)
        {
            foreach (Common.SchemaWrapper.Field taskField in newTaskFields)
            {
                oldTaskFields[taskField.X, taskField.Y] = taskField;
            }
        }

    }
}