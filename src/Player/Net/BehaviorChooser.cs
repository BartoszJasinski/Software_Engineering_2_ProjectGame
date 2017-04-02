using System.Threading;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;

namespace Player.Net
{
    static class BehaviorChooser/*: IMessageHandler<ConfirmGameRegistration>*/
    {
        
        public static void HandleMessage(RegisteredGames message, PlayerMessageHandleArgs args)
        {
            if (message.GameInfo==null || message.GameInfo.Length == 0)
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
            }
        }



        public static void HandleMessage(object message, PlayerMessageHandleArgs args)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        
    }
}
