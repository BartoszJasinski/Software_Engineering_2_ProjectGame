using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;

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
                        preferredTeam = args.Options?.PreferredColor == "red" ? TeamColour.red : TeamColour.blue
                    });
                    args.Connection.SendFromClient(args.Socket, xmlMessage);
                }
            }
        }

        public static void HandleMessage(ConfirmJoiningGame message, PlayerMessageHandleArgs args)
        {

        }


        public static void HandleMessage(object message, PlayerMessageHandleArgs args)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}