using Common.DebugUtils;
using Common.Schema;

namespace Player.Net
{
    static class BehaviorChooser/*: IMessageHandler<ConfirmGameRegistration>*/
    {
        
        public static void HandleMessage(GetGames message)
        {

        }



        public static void HandleMessage(object message)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        
    }
}
