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
        public static void HandleMessage(RegisteredGames message, PlayerClient client, Socket socket)
        {
            if (message.GameInfo == null || message.GameInfo.Length == 0)
            {
                Task.Run(() =>
                {
                    Thread.Sleep((int) client.Settings.RetryJoinGameInterval);
                    string xmlMessage = XmlMessageConverter.ToXml(new GetGames());
                    client.Connection.SendFromClient(socket, xmlMessage);
                });
            }
            else
            {
                ConsoleDebug.Good("Games available");
                if (client.Options.GameName == null)
                {
                    ConsoleDebug.Warning("Game name not specified");
                    return;
                }
                if (message.GameInfo.Count(info => info.gameName == client.Options.GameName) == 1)
                {
                    string xmlMessage = XmlMessageConverter.ToXml(new JoinGame()
                    {
                        gameName = client.Options.GameName,
                        playerIdSpecified = false,
                        preferredRole = client.Options?.PreferredRole == "player" ? PlayerType.member : PlayerType.leader,
                        preferredTeam = client.Options?.PreferredColor == "red" ? TeamColour.red : TeamColour.blue
                    });
                    client.Connection.SendFromClient(socket, xmlMessage);
                }
            }
        }

        public static void HandleMessage(ConfirmJoiningGame message, PlayerClient client, Socket socket)
        {
            if (message == null)
                return;

            client.Id = message.playerId;

            return;
        }


        public static void HandleMessage(object message, PlayerClient client, Socket socket)
        {
            ConsoleDebug.Warning("Unknown Type");
        }
    }
}