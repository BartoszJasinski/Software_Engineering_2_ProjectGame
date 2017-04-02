using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;

namespace GameMaster.Net
{
    static class BehaviorChooser/*: IMessageHandler<ConfirmGameRegistration>*/
    {
        

        public static void HandleMessage(ConfirmGameRegistration message)
        {
            ConsoleDebug.Good("I get gameId = " + message.gameId);
        }

        public static void HandleMessage(JoinGame message)
        {

        }



        public static void HandleMessage(object message)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        
    }
}
