using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;
using System.Net.Sockets;

namespace Player.Net
{
    static class BehaviorChooser /*: IMessageHandler<ConfirmGameRegistration>*/
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
            ConsoleDebug.Good("Game started");
            args.PlayerClient.Play();
            
        }

        public static void HandleMessage(Data message, PlayerMessageHandleArgs args)
        {
            
            args.PlayerClient.Location = message.PlayerLocation;
            ConsoleDebug.Message($"My location: ({message.PlayerLocation.x}, {message.PlayerLocation.y})");
            args.PlayerClient.Play();
        }

        public static void HandleMessage(object message, PlayerMessageHandleArgs args)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}